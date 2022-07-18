using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using UsbHid;

using ControlModule.Models;
using ControlModule.Controllers;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;
using ControlModule.Helpers;
using System.Net.Mail;
using System.Net;
namespace ControlModule.Views
{
    /// <summary>
    /// Interaction logic for CompareWeightWindow.xaml
    /// </summary>
    public partial class CompareWeightWindow : Window
    {
        List<CartonNumberingModel> cartonNumberingList;
        List<CartonNumberingDetailModel> cartonNumberingDetailList;
        List<PackingModel> packingList;
        BackgroundWorker threadLogin;
        BackgroundWorker threadBarcode;
        BackgroundWorker bwMetalDetect;
        string portReceive;
        SerialPort serialPortReceive;
        string portWrite;
        PackingModel packingInsert;
        List<CartonNoUIElementModel> cartonNoUIElementList;
        CartonNoUIElementModel cartonNoUIElementClicked;
        string productNo;
        List<MailAddressReceiveMessageModel> mailAddressReceiveMessageList;
        SmtpClient smtpClient;
        MailMessage mailMessage;
        bool flagSending;
        string checkby;
        string reason;
        ElectricScaleProfile electricScaleProfile;
        List<StoringModel> storingList;
        List<PORepackingModel> poRepackingList;
        FcatPOModel fcatPOCheck;
        LoadMode loadMode;
        LoadMode cartonMode;
        LoadMode barcodeMode;

        UsbHidDevice hidDevice;
        string location = "";

        public CompareWeightWindow(string _productNo)
        {
            InitializeComponent();
            productNo = _productNo;
            cartonNumberingList = new List<CartonNumberingModel>();
            cartonNumberingDetailList = new List<CartonNumberingDetailModel>();
            storingList = new List<StoringModel>();
            poRepackingList = new List<PORepackingModel>();
            packingList = new List<PackingModel>();
            threadLogin = new BackgroundWorker();
            threadLogin.DoWork += new DoWorkEventHandler(threadLogin_DoWork);
            threadLogin.RunWorkerCompleted += new RunWorkerCompletedEventHandler(threadLogin_RunWorkerCompleted);
            threadBarcode = new BackgroundWorker();
            threadBarcode.DoWork += new DoWorkEventHandler(threadBarcode_DoWork);
            threadBarcode.RunWorkerCompleted += new RunWorkerCompletedEventHandler(threadBarcode_RunWorkerCompleted);
            packingInsert = new PackingModel();
            cartonNoUIElementList = new List<CartonNoUIElementModel>();
            fcatPOCheck = new FcatPOModel();
            mailAddressReceiveMessageList = new List<MailAddressReceiveMessageModel>();
            MailAddress mailAddressSend = new MailAddress("saovietqc.com@gmail.com", "Sao Viet");
            smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mailAddressSend.Address, "S@0v13tqc.c0m"),
                Timeout = 10 * 1000,
            };

            //MailAddress mailAddressSend = new MailAddress("it02@dl.chungphi.com", "Sao Viet");
            //smtpClient = new SmtpClient
            //{
            //    Host = "10.2.3.2",
            //    Port = 25,
            //    EnableSsl = false,
            //    DeliveryMethod = SmtpDeliveryMethod.Network,
            //    UseDefaultCredentials = true,
            //    Credentials = new NetworkCredential(mailAddressSend.Address, "Happy03"),
            //    Timeout = 10 * 1000,
            //};

            smtpClient.SendCompleted += new SendCompletedEventHandler(smtpClient_SendCompleted);
            mailMessage = new MailMessage
            {
                From = mailAddressSend,
                IsBodyHtml = true,
                Subject = "Loading System",
            };

            // Test sending email
            //var sendTo = new MailAddress("nguyentoan712@gmail.com", "Toann");
            //mailMessage.To.Add(sendTo);
            //mailMessage.Body = "Hello Toan";
            //smtpClient.SendAsync(mailMessage, mailMessage);

            barcodeMode = new LoadMode
            {
                Type = 0,
                Name = "Barcode Mode",
                Min = 0.97,
                Max = 1.03
            };
            cartonMode = new LoadMode
            {
                Type = 1,
                Name = "Carton Mode",
                Min = 0.97,
                Max = 1.03
            };

            flagSending = false;
            checkby = null;
            reason = null;
            electricScaleProfile = new ElectricScaleProfile();
            location = AppSettingsHelper.ReadSetting("Location");

            hidDevice = new UsbHidDevice(0x0462, 0x0026);

            hidDevice.OnConnected += HidDevice_OnConnected;
            hidDevice.OnDisConnected += HidDevice_OnDisConnected;
            hidDevice.DataReceived += HidDevice_DataReceived;

            hidDevice.Connect();
            bwMetalDetect = new BackgroundWorker();
            bwMetalDetect.DoWork += BwMetalDetect_DoWork;
            bwMetalDetect.RunWorkerCompleted += BwMetalDetect_RunWorkerCompleted;
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
            portWrite = AppSettingsHelper.ReadSetting("WritePort");
            string[] portList = SerialPort.GetPortNames();
            if (portList.Count() > 0)
            {
                if (string.IsNullOrEmpty(portReceive) == true || portList.Contains(portReceive) == false)
                {
                    portReceive = portList[0];
                }
                if (string.IsNullOrEmpty(portWrite) == true || portList.Contains(portWrite) == false)
                {
                    portWrite = portList[0];
                }
                serialPortReceive.PortName = portReceive;
                foreach (string port in portList)
                {
                    MenuItem miPortReceive = new MenuItem();
                    miPortReceive.Header = port;
                    miPortReceive.Tag = port;
                    miPortReceive.Click += new RoutedEventHandler(miPortReceive_Click);
                    miSelectPortReceive.Items.Add(miPortReceive);
                    if (port == portReceive)
                    {
                        miPortReceive.IsChecked = true;
                    }

                    MenuItem miPortWrite = new MenuItem();
                    miPortWrite.Header = port;
                    miPortWrite.Tag = port;
                    miPortWrite.Click += new RoutedEventHandler(miPortWrite_Click);
                    miSelectPortWrite.Items.Add(miPortWrite);
                    if (port == portWrite)
                    {
                        miPortWrite.IsChecked = true;
                    }
                }
            }
            if (string.IsNullOrEmpty(productNo) == true)
            {
                txtProductNo.Focus();
                return;
            }

            txtProductNo.Text = productNo;
            popupLogin.IsOpen = true;

        }

        private bool _usbHidWorking = false;
        private void HidDevice_DataReceived(byte[] data)
        {
            if (String.IsNullOrEmpty(packingInsert.ProductNo)
                || packingInsert.CartonNo == 0 
                )
                return;
            hidDevice.Disconnect();
            Dispatcher.Invoke(new Action(() =>
            {
                if (bwMetalDetect.IsBusy == false && _usbHidWorking == false)
                {
                    _usbHidWorking = true;
                    txtBarcode.IsEnabled = false;
                    txtCartonNo.IsEnabled = false;
                    this.Cursor = Cursors.Wait;
                    bwMetalDetect.RunWorkerAsync();
                }
            }));
        }

        private void BwMetalDetect_DoWork(object sender, DoWorkEventArgs e)
        {
            var code = new Guid();
            code = Guid.NewGuid();
            Dispatcher.Invoke(new Action(() =>
            {
                ControlProblemWindow window = new ControlProblemWindow(true, packingInsert.CartonNo, code.ToString());
                window.ShowDialog();
                checkby = window.CheckBy;
                reason = window.Reason;
            }));
            Dispatcher.Invoke(new Action(() =>
            {
                lblHidUSBStatus.Foreground = Brushes.Blue;
                lblHidUSBStatus.Text = String.Format("{0} - USB Hid: sending ...", location);
            }));
            string mailBody = "<html><p style='color:red'>Loading System Easement Alarm</p><br /><br /><br /><br /><table border='1' style='width:100%'>"
                    + "<tr><td>Location</td><td>Production No.</td><td>CartonNo</td><td>Reason</td><td>CheckBy</td><td>Time</td></tr>"
                    + "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td></tr>"
                    + "</table></html>";
            string logBody = "Location.:{0} Production No.:{1} CartonNo.:{2} Reason.:{3} CheckBy.:{4} Code.:{5} Time:{6}";

            mailMessage.Subject = string.Format("Loading System Easement Alarm ({0})", location);
            mailMessage.Body = string.Format(mailBody, location, productNo, packingInsert.CartonNo, reason, checkby, DateTime.Now.ToString());

            // Testing
            mailMessage.To.Clear();
            foreach (MailAddressReceiveMessageModel mailAddressReceiveMessage in mailAddressReceiveMessageList)
            {
                MailAddress mailAddressReceive = new MailAddress(mailAddressReceiveMessage.MailAddress, mailAddressReceiveMessage.Name);
                if (mailAddressReceiveMessage.HidUSBSend)
                    mailMessage.To.Add(mailAddressReceive);
            }
            if (flagSending == false && mailMessage.To.Count > 0)
            {
                LogHelper.CreateLog(string.Format(logBody, location, productNo, packingInsert.CartonNo, reason, checkby, code.ToString(), DateTime.Now.ToString()));
                smtpClient.SendAsync(mailMessage, mailMessage);
                flagSending = true;
            }

            Thread.Sleep(2000);
            Dispatcher.Invoke(new Action(() =>
            {
                lblHidUSBStatus.Text = String.Format("{0} - USB Hid: Done", location);
            }));
            Thread.Sleep(3000);
        }
        private void BwMetalDetect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = null;
            txtCartonNo.IsEnabled = true;
            txtBarcode.IsEnabled = true;
            txtCartonNo.Focus();
            txtBarcode.Focus();
            _usbHidWorking = false;
            hidDevice.Connect();
        }

        private void HidDevice_OnDisConnected()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                lblHidUSBStatus.Foreground = Brushes.Red;
                lblHidUSBStatus.Text = String.Format("{0} - USB Hid: Disconnected !", location);
            }));
        }

        private void HidDevice_OnConnected()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                lblHidUSBStatus.Foreground = Brushes.Blue;
                lblHidUSBStatus.Text = String.Format("{0} - USB Hid: Connected !", location);
            }));
        }

        private void miPortReceive_Click(object sender, RoutedEventArgs e)
        {
            MenuItem miPortClick = sender as MenuItem;
            string port = (string)miPortClick.Tag;
            serialPortReceive.Close();
            AppSettingsHelper.AddUpdateAppSettings("ReceivePort", port);
            serialPortReceive.PortName = port;
            foreach (MenuItem miPort in miSelectPortReceive.Items)
            {
                miPort.IsChecked = false;
            }
            miPortClick.IsChecked = true;
        }

        private void miPortWrite_Click(object sender, RoutedEventArgs e)
        {
            MenuItem miPortClick = sender as MenuItem;
            string port = (string)miPortClick.Tag;
            AppSettingsHelper.AddUpdateAppSettings("WritePort", port);
            foreach (MenuItem miPort in miSelectPortWrite.Items)
            {
                miPort.IsChecked = false;
            }
            miPortClick.IsChecked = true;
        }

        private void txtProductNo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtProductNo.Text = txtProductNo.Text.ToUpper();
                if (string.IsNullOrEmpty(txtProductNo.Text) == false)
                {
                    popupLogin.IsOpen = true;
                }
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            txtUserName.Text = txtUserName.Text.ToUpper();
            string userName = txtUserName.Text;
            if (string.IsNullOrEmpty(userName) == true)
            {
                return;
            }
            string password = txtPassword.Password;
            if (string.IsNullOrEmpty(password) == true)
            {
                return;
            }
            if (threadLogin.IsBusy == true)
            {
                return;
            }
            btnLogin.IsEnabled = false;
            this.Cursor = Cursors.Wait;
            object[] arguments = { userName, password };
            threadLogin.RunWorkerAsync(arguments);
        }

        private void threadLogin_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] arguments = e.Argument as object[];
            string userName = arguments[0].ToString();
            string password = arguments[1].ToString();
            e.Result = AccountController.Find(userName, password);
            poRepackingList = PORepackingController.GetAll();
        }

        private void threadLogin_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnLogin.IsEnabled = true;
            this.Cursor = null;
            AccountModel account = e.Result as AccountModel;

            if (account != null)
            {
                packingInsert.CreatedAccount = account.UserName;
                this.Title = string.Format("Saoviet - Loading System - Loading - User: {0} ({1})", account.FullName.ToUpper(), account.UserName);
                txtPassword.Clear();
                popupLogin.IsOpen = false;

                mailMessage.To.Clear();
                gridSizeNoList.Children.Clear();
                gridSizeNoList.ColumnDefinitions.Clear();
                gridSizeNoList.RowDefinitions.Clear();

                gridCartonNoList.Children.Clear();
                gridCartonNoList.ColumnDefinitions.Clear();
                gridCartonNoList.RowDefinitions.Clear();
                cartonNoUIElementList.Clear();
                txtCartonNo.IsEnabled = false;

                serialPortReceive.Close();
                Reset();
                ComPortHelper.WriteToPort(portWrite, "DIO[0]:VALUE=0\r\n");
                ComPortHelper.WriteToPort(portWrite, "DIO[3]:VALUE=0\r\n");
                productNo = txtProductNo.Text;

                if (string.IsNullOrEmpty(productNo) == true)
                {
                    return;
                }
                packingInsert.ProductNo = productNo;

                if (threadBarcode.IsBusy == false)
                {
                    this.Cursor = Cursors.Wait;
                    txtProductNo.IsEnabled = false;
                    threadBarcode.RunWorkerAsync();
                }
                // Testing
                //bwMetalDetect.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Đăng Nhập Thất Bại.", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void threadBarcode_DoWork(object sender, DoWorkEventArgs e)
        {
            mailAddressReceiveMessageList = MailAddressReceiveMessageController.Get();
            cartonNumberingList = CartonNumberingController.Get(productNo);
            cartonNumberingDetailList = CartonNumberingDetailController.Get(productNo);

            packingList = PackingController.Get(productNo);
            storingList = StoringController.Get(productNo);
            fcatPOCheck = FCatPOController.GetToLoading(productNo);
        }

        private void threadBarcode_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = null;
            txtProductNo.IsEnabled = true;
            //if (fcatPOCheck == null)
            //{
            //    MessageBox.Show(String.Format("This PO {0} not yet upload F.CAT PO, Contact QC to check again !\n\nĐơn hàng {1} chưa được Upload F.CAT PO, liên hệ QC để kiểm tra lại !", productNo, productNo), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            //else if (!fcatPOCheck.StatusCurrent.ToUpper().Equals("PASSED"))
            //{
            //    MessageBox.Show(String.Format("PO: {0} status is {1} not PASSED; can not Loading now !\n\nĐơn hàng: {2} hiện tại chưa PASS ở F.CAT, Liên hệ QC kiểm tra lại đơn hàng !", productNo, fcatPOCheck.StatusCurrent, productNo), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            foreach (MailAddressReceiveMessageModel mailAddressReceiveMessage in mailAddressReceiveMessageList)
            {
                MailAddress mailAddressReceive = new MailAddress(mailAddressReceiveMessage.MailAddress, mailAddressReceiveMessage.Name);
                if (mailAddressReceiveMessage.CartonIssuesSend)
                    mailMessage.To.Add(mailAddressReceive);
            }

            if (cartonNumberingList.Count <= 0 || cartonNumberingDetailList.Count <= 0)
            {
                MessageBox.Show("Mã Đơn Hàng Không Đúng.", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int columnCount = cartonNumberingList.Count + 1;
            int rowCount = 1 + cartonNumberingDetailList.GroupBy(c => c.SizeNo).Select(c => c.Count()).Max();

            for (int x = 1; x <= columnCount; x++)
            {
                ColumnDefinition cdSizeNo = new ColumnDefinition();
                cdSizeNo.Width = new GridLength(1, GridUnitType.Auto);
                gridSizeNoList.ColumnDefinitions.Add(cdSizeNo);

                ColumnDefinition cdCartonNo = new ColumnDefinition();
                cdCartonNo.Width = new GridLength(1, GridUnitType.Auto);
                gridCartonNoList.ColumnDefinitions.Add(cdCartonNo);

            }

            for (int y = 1; y <= rowCount; y++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(1, GridUnitType.Auto);
                gridCartonNoList.RowDefinitions.Add(rd);
            }

            for (int y = 2; y <= rowCount; y++)
            {
                TextBlock tblNumberOf = new TextBlock();
                Grid.SetColumn(tblNumberOf, 0);
                Grid.SetRow(tblNumberOf, y - 1);
                tblNumberOf.Text = string.Format("#{0}", y - 1);
                tblNumberOf.Width = 77;
                tblNumberOf.Height = 44;
                tblNumberOf.FontSize = 25;
                tblNumberOf.Foreground = Brushes.Gray;
                tblNumberOf.TextAlignment = TextAlignment.Center;
                gridCartonNoList.Children.Add(tblNumberOf);
            }

            TextBlock tblSizeHeader = new TextBlock();
            tblSizeHeader.Width = 77;
            tblSizeHeader.Height = 44;
            gridSizeNoList.Children.Add(tblSizeHeader);
            for (int x = 0; x <= cartonNumberingList.Count - 1; x++)
            {
                CartonNumberingModel cartonNumbering = cartonNumberingList[x];
                List<CartonNumberingDetailModel> cartonNumberingDetailList_D1 = cartonNumberingDetailList.Where(c => c.SizeNo == cartonNumbering.SizeNo).ToList();

                TextBlock tblSize = new TextBlock();
                Grid.SetColumn(tblSize, x + 1);
                Grid.SetRow(tblSize, 0);
                tblSize.Text = cartonNumbering.SizeNo;
                tblSize.Width = 77;
                tblSize.MinHeight = 44;
                tblSize.FontSize = 25;
                tblSize.FontWeight = FontWeights.Bold;
                tblSize.TextAlignment = TextAlignment.Center;
                tblSize.TextWrapping = TextWrapping.Wrap;
                gridSizeNoList.Children.Add(tblSize);

                for (int y = 0; y <= cartonNumberingDetailList_D1.Count - 1; y++)
                {
                    CartonNumberingDetailModel cartonNumberingDetail = cartonNumberingDetailList_D1[y];
                    PackingModel packing = packingList.Where(p => p.CartonNo == cartonNumberingDetail.CartonNo).FirstOrDefault();

                    TextBlock tblCartonNo = new TextBlock();
                    Grid.SetColumn(tblCartonNo, x + 1);
                    Grid.SetRow(tblCartonNo, y + 1);
                    tblCartonNo.Text = string.Format("{0}", cartonNumberingDetail.CartonNo);
                    tblCartonNo.MinWidth = 77;
                    tblCartonNo.MinHeight = 44;
                    tblCartonNo.FontSize = 25;
                    tblCartonNo.TextAlignment = TextAlignment.Center;
                    if (packing != null)
                    {
                        if (packing.IsPass == true)
                        {
                            tblCartonNo.Foreground = Brushes.Green;
                            tblCartonNo.TextDecorations = TextDecorations.Strikethrough;
                        }
                        else
                        {
                            tblCartonNo.Foreground = Brushes.Red;
                        }
                        if (packing.IsFirstPass == true)
                        {
                            tblCartonNo.Foreground = Brushes.Green;
                        }
                        else
                        {
                            tblCartonNo.Foreground = Brushes.Red;
                        }
                    }
                    gridCartonNoList.Children.Add(tblCartonNo);

                    CartonNoUIElementModel cartonNoUIElement = new CartonNoUIElementModel
                    {
                        CartonNo = cartonNumberingDetail.CartonNo,
                        TextBlock = tblCartonNo,
                        CartonNumbering = cartonNumbering,
                        Packing = packing,
                    };
                    cartonNoUIElementList.Add(cartonNoUIElement);
                }
            }
            // default load mode
            loadMode = barcodeMode;
            if (poRepackingList.Where(w => w.ProductNo.Contains(productNo)).Count() > 0)
            {
                MessageBox.Show(string.Format("This PO : {0} is Repacking PO", productNo), this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                loadMode = cartonMode;
                stkpnCartonNo.Visibility = Visibility.Visible;
                stkpnBarcode.Visibility = Visibility.Collapsed;
                txtCartonNo.IsEnabled = true;
                txtCartonNo.Focus();
            }
            else
            {
                stkpnCartonNo.Visibility = Visibility.Collapsed;
                stkpnBarcode.Visibility = Visibility.Visible;
                // barcode
                txtBarcode.Text = "";
                txtBarcode.IsEnabled = true;
                txtBarcode.Focus();
                btnBarcode.IsEnabled = true;
                btnBarcode.IsDefault = true;
            }
        }

        double minActualWeight = 0;
        double maxActualWeight = 0;
        private void txtCartonNo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                updateCartonNumbering = false;
                serialPortReceive.Close();
                Reset();
                ComPortHelper.WriteToPort(portWrite, "DIO[0]:VALUE=0\r\n");
                ComPortHelper.WriteToPort(portWrite, "DIO[3]:VALUE=0\r\n");

                if (double.TryParse(txtMinActualWeight.Text, out minActualWeight) == false)
                {
                    txtMinActualWeight.BorderBrush = Brushes.Red;
                }
                else
                {
                    txtMinActualWeight.ClearValue(TextBox.BorderBrushProperty);
                }

                if (double.TryParse(txtMaxActualWeight.Text, out maxActualWeight) == false)
                {
                    txtMaxActualWeight.BorderBrush = Brushes.Red;
                }
                else
                {
                    txtMaxActualWeight.ClearValue(TextBox.BorderBrushProperty);
                }

                int cartonNo = 0;
                int.TryParse(txtCartonNo.Text, out cartonNo);
                if (cartonNo <= 0)
                {
                    MessageBox.Show("Mã Thùng Không Đúng.", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                cartonNoUIElementClicked = cartonNoUIElementList.Where(c => c.CartonNo == cartonNo).FirstOrDefault();
                if (cartonNoUIElementClicked == null || (cartonNoUIElementClicked.Packing != null && cartonNoUIElementClicked.Packing.IsPass == true))
                {
                    MessageBox.Show("Mã Thùng Không Đúng Hoặc Thùng Đã Được Xuất.", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Point point = cartonNoUIElementClicked.TextBlock.TranslatePoint(new Point(), gridCartonNoList);
                scrollViewerCartonNo.ScrollToHorizontalOffset(point.X);
                scrollViewerCartonNo.ScrollToVerticalOffset(point.Y);

                packingInsert.SizeNo = cartonNoUIElementClicked.CartonNumbering.SizeNo;
                tblGrossWeight.Text = string.Format("{0}", cartonNoUIElementClicked.CartonNumbering.GrossWeight);
                tblGrossWeight.Tag = cartonNoUIElementClicked.CartonNumbering.GrossWeight;
                packingInsert.CartonNo = cartonNo;

                // Carton Mode
                if (loadMode == cartonMode)
                {
                    stkpnCartonNo.Visibility = Visibility.Visible;
                    stkpnBarcode.Visibility = Visibility.Collapsed;
                    btnBarcode.IsDefault = false;
                    txtBarcode.IsEnabled = false;

                    txtCartonNo.IsEnabled = true;
                    txtCartonNo.Focus();
                    txtCartonNo.SelectAll();
                }
                // Barcode Mode
                else
                {
                    stkpnCartonNo.Visibility = Visibility.Collapsed;
                    stkpnBarcode.Visibility = Visibility.Visible;

                    btnBarcode.IsDefault = false;
                    txtBarcode.IsEnabled = false;
                    txtBarcode.Focus();
                    txtBarcode.SelectAll();

                    txtCartonNo.IsEnabled = false;
                }

                serialPortReceive.Open();
            }
        }

        bool updateCartonNumbering;
        private void btnBarcode_Click(object sender, RoutedEventArgs e)
        {
            serialPortReceive.Close();
            updateCartonNumbering = false;
            Reset();
            ComPortHelper.WriteToPort(portWrite, "DIO[0]:VALUE=0\r\n");
            ComPortHelper.WriteToPort(portWrite, "DIO[3]:VALUE=0\r\n");

            if (double.TryParse(txtMinActualWeight.Text, out minActualWeight) == false)
            {
                txtMinActualWeight.BorderBrush = Brushes.Red;
            }
            else
            {
                txtMinActualWeight.ClearValue(TextBox.BorderBrushProperty);
            }

            if (double.TryParse(txtMaxActualWeight.Text, out maxActualWeight) == false)
            {
                txtMaxActualWeight.BorderBrush = Brushes.Red;
            }
            else
            {
                txtMaxActualWeight.ClearValue(TextBox.BorderBrushProperty);
            }

            int cartonNo = 0;
            string barcodeScan = "";
            barcodeScan = txtBarcode.Text.Trim().ToString();
            var storingByBarcode = storingList.Where(w => w.Barcode == barcodeScan && w.IsPass == true && w.IsComplete == true).FirstOrDefault();
            if (storingByBarcode == null)
            {
                //MessageBox.Show("Barcode không đúng hoặc thùng chưa được cân ở INPUT(Nhập Hàng)\nNhập số Carton để cân thùng này !", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBox.Show("Barcode không đúng hoặc Thùng chưa được cân ở INPUT(Nhập Hàng)\nKiểm tra lại Barcode, Cân thùng giày ở StoringSystem trước khi xuất hàng !", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                stkpnBarcode.Visibility = Visibility.Collapsed;
                stkpnCartonNo.Visibility = Visibility.Visible;
                txtCartonNo.IsEnabled = true;
                txtCartonNo.Focus();
                //MessageBox.Show("Barcode không đúng hoặc Thùng chưa được cân ở INPUT(Nhập Hàng)\nKiểm tra lại Barcode, Cân thùng giày ở StoringSystem trước khi xuất hàng !", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                //txtBarcode.Focus();
                //txtBarcode.SelectAll();
                return;
            }

            cartonNo = storingByBarcode.CartonNo;
            var firstCartonOfSizeOfPO = cartonNoUIElementList.Where(w => w.CartonNumbering.SizeNo == storingByBarcode.SizeNo && w.CartonNumbering.GrossWeight > 0).ToList();
            if (firstCartonOfSizeOfPO.Count == 0)
            {
                updateCartonNumbering = true;
            }

            cartonNoUIElementClicked = cartonNoUIElementList.Where(c => c.CartonNo == cartonNo).FirstOrDefault();
            if (cartonNoUIElementClicked == null || (cartonNoUIElementClicked.Packing != null && cartonNoUIElementClicked.Packing.IsPass == true))
            {
                MessageBox.Show("Mã Thùng Không Đúng Hoặc Thùng Đã Được Xuất.", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Point point = cartonNoUIElementClicked.TextBlock.TranslatePoint(new Point(), gridCartonNoList);
            scrollViewerCartonNo.ScrollToHorizontalOffset(point.X);
            scrollViewerCartonNo.ScrollToVerticalOffset(point.Y);

            packingInsert.SizeNo = cartonNoUIElementClicked.CartonNumbering.SizeNo;
            tblGrossWeight.Text = string.Format("{0}", storingByBarcode.ActualWeight);
            tblGrossWeight.Tag = storingByBarcode.ActualWeight;
            packingInsert.CartonNo = cartonNo;
            packingInsert.Barcode = storingByBarcode.Barcode;

            serialPortReceive.Open();
            txtBarcode.IsEnabled = false;
            btnBarcode.IsEnabled = false;
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
                    if (actualWeight > minActualWeight && actualWeight < maxActualWeight)
                    {
                        packingInsert.ActualWeight = actualWeight;

                        tblActualWeight.Dispatcher.Invoke(new Action(() =>
                        {
                            tblActualWeight.Text = string.Format("{0}", actualWeight);
                            tblActualWeight.Tag = actualWeight;
                        }));

                        serialPortReceive.Close();
                        CompareWeight();
                    }
                }
            }
        }

        private void CompareWeight()
        {
            double grossWeight = 0;
            tblGrossWeight.Dispatcher.Invoke(new Action(() => grossWeight = (double)tblGrossWeight.Tag));

            double actualWeight = 0;
            tblActualWeight.Dispatcher.Invoke(new Action(() => actualWeight = (double)tblActualWeight.Tag));

            if (grossWeight <= 0 || updateCartonNumbering == true)
            {
                if (grossWeight <= 0)
                {
                    grossWeight = actualWeight;
                }
                List<CartonNoUIElementModel> cartonNoUIElementList_D1 = cartonNoUIElementList.Where(c => c.CartonNumbering.SizeNo == packingInsert.SizeNo).ToList();
                foreach (CartonNoUIElementModel cartonNoUIElement in cartonNoUIElementList_D1)
                {
                    cartonNoUIElement.CartonNumbering.GrossWeight = actualWeight;
                }

                CartonNumberingModel cartonNumbering = new CartonNumberingModel
                {
                    ProductNo = packingInsert.ProductNo,
                    SizeNo = packingInsert.SizeNo,
                    GrossWeight = actualWeight,
                    CartonNoBasic = packingInsert.CartonNo,
                };
                CartonNumberingController.Update_2(cartonNumbering);
            }

            double percentDiffence = actualWeight / grossWeight;
            packingInsert.DifferencePercent = Math.Round(100 * (percentDiffence - 1), 2);
            tblDifferencePercent.Dispatcher.Invoke(new Action(() => tblDifferencePercent.Text = string.Format("{0}", Math.Round(100 * (percentDiffence - 1), 2))));

            PackingModel packingClicked = cartonNoUIElementClicked.Packing;
            if (percentDiffence >= loadMode.Min && percentDiffence <= loadMode.Max)
            {
                tblResult.Dispatcher.Invoke(new Action(() =>
                {
                    tblResult.Foreground = Brushes.White;
                    tblResult.Background = Brushes.Green;
                    tblResult.Text = string.Format("{0} - Pass", packingInsert.CartonNo);
                }));

                if (packingClicked == null)
                {
                    cartonNoUIElementClicked.TextBlock.Dispatcher.Invoke(new Action(() =>
                    {
                        cartonNoUIElementClicked.TextBlock.Foreground = Brushes.Green;
                        cartonNoUIElementClicked.TextBlock.TextDecorations = TextDecorations.Strikethrough;
                    }));
                    packingInsert.IsFirstPass = true;
                    packingInsert.IsPass = true;
                    cartonNoUIElementClicked.Packing = packingInsert;
                }
                else
                {
                    if (packingClicked.IsPass == false)
                    {
                        cartonNoUIElementClicked.TextBlock.Dispatcher.Invoke(new Action(() =>
                        {
                            cartonNoUIElementClicked.TextBlock.Foreground = Brushes.Red;
                            cartonNoUIElementClicked.TextBlock.TextDecorations = TextDecorations.Strikethrough;
                        }));

                        string mailBody = "<html><table border='1' style='width:100%'>"
                            + "<tr><td>Production No.</td><td>Size No.</td><td>Carton No.</td><td>Gross Weight</td><td>Actual Weight</td><td>Difference Percent</td><td>Check by</td><td>Reason</td><td>Repacking PO</td></tr>"
                            + "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}kg</td><td>{4}kg</td><td>{5}%</td><td>{6}</td><td>{7}</td><td>{8}</td></tr>"
                            + "</table></html>";
                        string logBody = "Production No.:{0} Size No.:{1} Carton No.:{2} Gross Weight:{3}kg Actual Weight: {4}kg Difference Percent:{5}% Check by:{6} Reason:{7} Repacking PO: {8}";

                        if (string.IsNullOrEmpty(checkby) == false && string.IsNullOrEmpty(reason) == false)
                        {
                            mailMessage.Subject = string.Format("Saoviet Loading System(Re-check)({0})", electricScaleProfile.SubjectMail);
                            mailMessage.Body = string.Format(mailBody, packingInsert.ProductNo, packingInsert.SizeNo, packingInsert.CartonNo, grossWeight, packingInsert.ActualWeight, packingInsert.DifferencePercent, checkby, reason, loadMode.Type);
                            LogHelper.CreateLog(string.Format(logBody, packingInsert.ProductNo, packingInsert.SizeNo, packingInsert.CartonNo, grossWeight, packingInsert.ActualWeight, packingInsert.DifferencePercent, checkby, reason, loadMode.Type));
                            if (flagSending == false && mailMessage.To.Count > 0)
                            {

                                smtpClient.SendAsync(mailMessage, mailMessage);
                                flagSending = true;
                                Dispatcher.Invoke(new Action(() =>
                                {
                                    MessageBox.Show(String.Format("Email sent to {0} users!", mailMessage.To.Count()), this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                                }));
                            }
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                ControlProblemWindow window = new ControlProblemWindow(false, 0, "");
                                window.ShowDialog();
                                checkby = window.CheckBy;
                                reason = window.Reason;

                                mailMessage.Subject = string.Format("Saoviet Loading System(Re-check)({0})", electricScaleProfile.SubjectMail);
                                mailMessage.Body = string.Format(mailBody, packingInsert.ProductNo, packingInsert.SizeNo, packingInsert.CartonNo, grossWeight, packingInsert.ActualWeight, packingInsert.DifferencePercent, checkby, reason, loadMode.Type);
                                LogHelper.CreateLog(string.Format(logBody, packingInsert.ProductNo, packingInsert.SizeNo, packingInsert.CartonNo, grossWeight, packingInsert.ActualWeight, packingInsert.DifferencePercent, checkby, reason, loadMode.Type));
                                if (flagSending == false && mailMessage.To.Count > 0)
                                {
                                    smtpClient.SendAsync(mailMessage, mailMessage);
                                    flagSending = true;
                                    Dispatcher.Invoke(new Action(() =>
                                    {
                                        MessageBox.Show(String.Format("Email sent to {0} users!", mailMessage.To.Count()), this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                                    }));
                                }
                            }));
                        }
                    }
                    packingInsert.IsPass = true;
                    packingClicked.IsPass = true;
                }
            }
            else
            {
                if (percentDiffence < loadMode.Min)
                {
                    tblResult.Dispatcher.Invoke(new Action(() =>
                    {
                        tblResult.Foreground = Brushes.Black;
                        tblResult.Background = Brushes.Yellow;
                        tblResult.Text = string.Format("{0} - Low", packingInsert.CartonNo);
                    }));
                }
                else
                {
                    tblResult.Dispatcher.Invoke(new Action(() =>
                    {
                        tblResult.Foreground = Brushes.White;
                        tblResult.Background = Brushes.Red;
                        tblResult.Text = string.Format("{0} - Hi", packingInsert.CartonNo);
                    }));
                }
                ComPortHelper.WriteToPort(portWrite, "DIO[0]:VALUE=1\r\n");
                ComPortHelper.WriteToPort(portWrite, "DIO[3]:VALUE=1\r\n");
                cartonNoUIElementClicked.TextBlock.Dispatcher.Invoke(new Action(() =>
                {
                    cartonNoUIElementClicked.TextBlock.Foreground = Brushes.Red;
                }));
                packingInsert.IsFirstPass = false;
                packingInsert.IsPass = false;
                cartonNoUIElementClicked.Packing = packingInsert;

                string mailBody = "<html><table border='1' style='width:100%'>"
                    + "<tr><td>Production No.</td><td>Size No.</td><td>Carton No.</td><td>Gross Weight</td><td>Actual Weight</td><td>Difference Percent</td><td>Repacking PO</td></tr>"
                    + "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}kg</td><td>{4}kg</td><td>{5}%</td><td>{6}</td></tr>"
                    + "</table></html>";
                string logBody = "Production No.:{0} Size No.:{1} Carton No.:{2} Gross Weight:{3}kg Actual Weight: {4}kg Difference Percent:{5}% Repacking PO: {6}";

                mailMessage.Subject = string.Format("Saoviet Loading System({0})", electricScaleProfile.SubjectMail);
                mailMessage.Body = string.Format(mailBody, packingInsert.ProductNo, packingInsert.SizeNo, packingInsert.CartonNo, grossWeight, packingInsert.ActualWeight, packingInsert.DifferencePercent, loadMode.Type);
                LogHelper.CreateLog(string.Format(logBody, packingInsert.ProductNo, packingInsert.SizeNo, packingInsert.CartonNo, grossWeight, packingInsert.ActualWeight, packingInsert.DifferencePercent, loadMode.Type));
                if (flagSending == false && mailMessage.To.Count > 0)
                {
                    smtpClient.SendAsync(mailMessage, mailMessage);
                    flagSending = true;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        MessageBox.Show(String.Format("Email sent to {0} users !", mailMessage.To.Count()), this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    }));
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ControlProblemWindow window = new ControlProblemWindow(false, 0, "");
                    window.ShowDialog();
                    checkby = window.CheckBy;
                    reason = window.Reason;
                }));
            }

            if (string.IsNullOrEmpty(packingInsert.ProductNo) == false
                            && string.IsNullOrEmpty(packingInsert.SizeNo) == false
                            && packingInsert.CartonNo > 0
                            && packingInsert.ActualWeight > 0)
            {
                PackingController.CreateUpdate(packingInsert);
            }

            if (loadMode == cartonMode)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    stkpnBarcode.Visibility = Visibility.Collapsed;
                    stkpnCartonNo.Visibility = Visibility.Visible;

                    txtCartonNo.IsEnabled = true;
                    txtCartonNo.Focus();
                    txtCartonNo.SelectAll();

                    btnBarcode.IsEnabled = false;
                    txtBarcode.IsEnabled = false;
                }));
            }
            else
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    stkpnBarcode.Visibility = Visibility.Visible;
                    stkpnCartonNo.Visibility = Visibility.Collapsed;
                    txtBarcode.IsEnabled = true;
                    txtBarcode.Focus();
                    txtBarcode.SelectAll();

                    btnBarcode.IsEnabled = true;
                    btnBarcode.IsDefault = true;
                }));
            }
        }

        private void smtpClient_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            flagSending = false;
        }

        private void txtProductNo_GotMouseCapture(object sender, MouseEventArgs e)
        {
            txtProductNo.SelectAll();
        }

        private void txtProductNo_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txtProductNo.SelectAll();
        }

        private void txtCartonNo_GotMouseCapture(object sender, MouseEventArgs e)
        {
            txtCartonNo.SelectAll();
        }

        private void txtCartonNo_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txtCartonNo.SelectAll();
        }

        private void scrollViewerCartonNo_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.HorizontalChange != 0)
            {
                scrollViewerSizeNo.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }

        private void btnClosePopup_Click(object sender, RoutedEventArgs e)
        {
            popupLogin.IsOpen = false;
        }

        private void popupLogin_Opened(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserName.Text) == true)
            {
                txtUserName.Focus();
            }
            else
            {
                txtPassword.Focus();
            }
        }

        private void Reset()
        {
            tblGrossWeight.Text = "0";
            tblGrossWeight.Tag = 0;
            tblActualWeight.Text = "0";
            tblActualWeight.Tag = 0;
            tblDifferencePercent.Text = "0";
            tblResult.Text = "...";
            tblResult.Foreground = Brushes.Black;
            tblResult.Background = Brushes.Transparent;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            serialPortReceive.Close();
            ComPortHelper.WriteToPort(portWrite, "DIO[0]:VALUE=0\r\n");
            ComPortHelper.WriteToPort(portWrite, "DIO[3]:VALUE=0\r\n");
            smtpClient.SendAsyncCancel();
        }

        private void txtCartonNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            Reset(); ;
        }

        private void txtBarcode_GotMouseCapture(object sender, MouseEventArgs e)
        {
            txtBarcode.SelectAll();
        }

        private void txtBarcode_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            btnBarcode.IsEnabled = true;
            btnBarcode.IsDefault = true;
            txtBarcode.SelectAll();
        }

        private void txtBarcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            Reset();
        }
    }

    class CartonNoUIElementModel
    {
        public int CartonNo { get; set; }
        public TextBlock TextBlock { get; set; }
        public CartonNumberingModel CartonNumbering { get; set; }
        public PackingModel Packing { get; set; }
    }

    class LoadMode
    {
        public int Type { get; set; }
        public string Name { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
    }
}
