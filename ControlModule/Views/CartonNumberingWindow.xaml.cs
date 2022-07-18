using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;

using ControlModule.Models;
using ControlModule.Controllers;
using System.ComponentModel;
using System.IO.Ports;
using ControlModule.Helpers;
using System.Threading;
namespace ControlModule.Views
{
    /// <summary>
    /// Interaction logic for CartonNumberingCRUDWindow.xaml
    /// </summary>
    public partial class CartonNumberingWindow : Window
    {
        BackgroundWorker threadSearch;
        string productNo;
        BackgroundWorker threadSave;
        List<CartonNumberingModel> cartonNumberingList;

        string portReceive;
        SerialPort serialPortReceive;

        ElectricScaleProfile electricScaleProfile;
        public CartonNumberingWindow()
        {
            InitializeComponent();

            threadSearch = new BackgroundWorker();
            threadSearch.DoWork += new DoWorkEventHandler(threadSearch_DoWork);
            threadSearch.RunWorkerCompleted += new RunWorkerCompletedEventHandler(threadSearch_RunWorkerCompleted);

            cartonNumberingList = new List<CartonNumberingModel>();

            threadSave = new BackgroundWorker();
            threadSave.DoWork += new DoWorkEventHandler(threadSave_DoWork);
            threadSave.RunWorkerCompleted += new RunWorkerCompletedEventHandler(threadSave_RunWorkerCompleted);

            electricScaleProfile = new ElectricScaleProfile();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int profileId = 0;
            int.TryParse(AppSettingsHelper.ReadSetting("ElectricScaleProfile"), out profileId);
            electricScaleProfile = ElectricScaleProfileHelper.ElectricScaleProfileList().Where(p => p.ProfileId == profileId).FirstOrDefault();
            if (electricScaleProfile == null)
            {
                this.Close();
            }
            serialPortReceive = new SerialPort();
            serialPortReceive.BaudRate = electricScaleProfile.BaudRate;
            serialPortReceive.DataReceived += new SerialDataReceivedEventHandler(serialPortReceive_DataReceived);
            portReceive = AppSettingsHelper.ReadSetting("ReceivePort");
            serialPortReceive.PortName = portReceive;
            txtProductNo.Focus();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (threadSearch.IsBusy == true)
            {
                return;
            }

            //Format Production No.
            txtProductNo.Text = txtProductNo.Text.ToUpper();

            productNo = txtProductNo.Text;
            if (string.IsNullOrEmpty(productNo) == true)
            {
                return;
            }

            btnLoading.Content = string.Format("Loading\n{0}", productNo);
            this.Cursor = Cursors.Wait;
            wrapPanelSizeWeight.Children.Clear();
            btnSearch.IsEnabled = false;            
            threadSearch.RunWorkerAsync();
        }

        private void threadSearch_DoWork(object sender, DoWorkEventArgs e)
        {
            List<CartonNumberingModel> cartonNumberingList = CartonNumberingController.Get(productNo);
            e.Result = cartonNumberingList;
        }

        private void threadSearch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<CartonNumberingModel> cartonNumberingList = e.Result as List<CartonNumberingModel>;
            if (cartonNumberingList.Count > 0)
            {
                foreach (CartonNumberingModel cartonNumbering in cartonNumberingList)
                {
                    StackPanel stackPanel = new StackPanel();
                    stackPanel.Orientation = Orientation.Vertical;
                    stackPanel.Margin = new Thickness(20, 10, 20, 10);
                    Button btnSize = new Button();
                    btnSize.Content = cartonNumbering.SizeNo;
                    btnSize.MinWidth = 75;
                    btnSize.FontSize = 45;
                    //btnSize.FontWeight = FontWeights.Bold;                    

                    TextBox txtWeight = new TextBox();
                    txtWeight.MinWidth = 75;
                    txtWeight.Margin = new Thickness(0, 5, 0, 0);
                    txtWeight.TextAlignment = TextAlignment.Center;
                    txtWeight.FontSize = 45;
                    txtWeight.Foreground = Brushes.Blue;
                    txtWeight.Text = string.Format("{0}", cartonNumbering.GrossWeight);

                    btnSize.Tag = txtWeight;
                    txtWeight.Tag = cartonNumbering.SizeNo;
                    btnSize.Click += new RoutedEventHandler(btnSize_Click);

                    stackPanel.Children.Add(btnSize);
                    stackPanel.Children.Add(txtWeight);

                    wrapPanelSizeWeight.Children.Add(stackPanel);
                }
            }
            this.Cursor = null;
            btnSearch.IsEnabled = true;
            btnSave.IsEnabled = true;
            btnLoading.IsEnabled = true;
            btnDelete.IsEnabled = true;
        }

        private void btnSize_Click(object sender, RoutedEventArgs e)
        {
            Button btnSize = sender as Button;
            if (btnSize == null || btnSize.Tag == null)
            {
                return;
            }
            TextBox txtWeight = btnSize.Tag as TextBox;
            if (txtWeight == null || txtWeight.Tag == null)
            {
                return;
            }
            txtWeight.Text = string.Format("{0}", tblGetWeight.Tag);
        }

        private void btnGetWeight_Click(object sender, RoutedEventArgs e)
        {            
            if (serialPortReceive.IsOpen == false)
            {
                serialPortReceive.Close();
                tblGetWeight.Text = "0";
                tblGetWeight.Tag = 0;
            }
            try
            {
                serialPortReceive.Open();
                btnGetWeight.IsEnabled = false;
            }
            catch
            {
                MessageBox.Show(string.Format("Có Lỗi Xảy Ra Khi Lấy Cân Nặng."), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                serialPortReceive.Close();
                tblGetWeight.Text = "0";
                tblGetWeight.Tag = 0;
            }
        }

        private void serialPortReceive_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(100);
            if (serialPortReceive.IsOpen == true)
            {
                string dataReceived = ElectricScaleProfileHelper.ConvertWeight(serialPortReceive.ReadExisting(), electricScaleProfile);
                if (string.IsNullOrEmpty(dataReceived) == false)
                {                    
                    double actualWeight = 0;
                    if (double.TryParse(dataReceived, out actualWeight) == false)
                    {
                        //Alert Here.
                    }
                    if (actualWeight > 0.5 && actualWeight < 20)
                    {
                        tblGetWeight.Dispatcher.Invoke(new Action(() =>
                        {
                            tblGetWeight.Text = string.Format("{0}", actualWeight);
                            tblGetWeight.Tag = actualWeight;
                        }));

                        btnGetWeight.Dispatcher.Invoke(new Action(() => btnGetWeight.IsEnabled = true));
                        serialPortReceive.Close();
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (wrapPanelSizeWeight.Children.Count <= 0)
            {
                return;
            }
            if (MessageBox.Show("Confirm Save?", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            if (threadSave.IsBusy == true)
            {
                return;
            }
            List<CartonNumberingModel> cartonNumberingUpdateList = new List<CartonNumberingModel>();
            foreach (StackPanel stackPanel in wrapPanelSizeWeight.Children)
            {
                TextBox txtWeight = stackPanel.Children[1] as TextBox;
                if (txtWeight == null)
                {
                    return;
                }
                string sizeNo = txtWeight.Tag.ToString();
                double grossWeight = 0;
                double.TryParse(txtWeight.Text, out grossWeight);
                CartonNumberingModel cartonNumbering = new CartonNumberingModel
                {
                    ProductNo = productNo,
                    SizeNo = sizeNo,
                    GrossWeight = grossWeight,
                };
                if (string.IsNullOrEmpty(cartonNumbering.ProductNo) == false
                    && string.IsNullOrEmpty(cartonNumbering.SizeNo) == false
                    && cartonNumbering.GrossWeight > 0)
                {
                    cartonNumberingUpdateList.Add(cartonNumbering);
                }
            }
            if (cartonNumberingUpdateList.Count <= 0)
            {
                return;
            }
            this.Cursor = Cursors.Wait;
            btnSave.IsEnabled = false;
            threadSave.RunWorkerAsync(cartonNumberingUpdateList);
        }

        private void threadSave_DoWork(object sender, DoWorkEventArgs e)
        {
            List<CartonNumberingModel> cartonNumberingUpdateList = e.Argument as List<CartonNumberingModel>;
            foreach (CartonNumberingModel cartonNumbering in cartonNumberingUpdateList)
            {
                CartonNumberingController.Update(cartonNumbering);
            }
        }

        private void threadSave_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = null;
            btnSave.IsEnabled = true;
            MessageBox.Show("OK.", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            serialPortReceive.Close();
        }

        private void btnLoading_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(productNo) == true)
            {
                return;
            }
            CompareWeightWindow window = new CompareWeightWindow(productNo);
            window.Show();
            this.Close();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //if (MessageBox.Show(string.Format("Confirm Delete ProductNo ?"), this.Title, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            //{
            //    return;
            //}

            DeletePOWindow window = new DeletePOWindow(productNo);
            window.ShowDialog();
        }
    }
}
