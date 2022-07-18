using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.ComponentModel;
using Microsoft.Win32;
using ControlModule.Models;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using ControlModule.Controllers;
using System.Threading;
namespace ControlModule.Views
{
    /// <summary>
    /// Interaction logic for InputPackingListWindow.xaml
    /// </summary>
    public partial class NewImportPackingListWindow : Window
    {
        string[] filePathArray;
        BackgroundWorker bwReadExcel;
        List<CartonNumberingModel> cartonNumberingList;
        List<CartonNumberingDetailModel> cartonNumberingDetailList;
        BackgroundWorker bwImport;
        public NewImportPackingListWindow()
        {
            InitializeComponent();
            bwReadExcel = new BackgroundWorker();
            bwReadExcel.WorkerSupportsCancellation = true;
            bwReadExcel.DoWork += new DoWorkEventHandler(bwReadExcel_DoWork);
            bwReadExcel.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwReadExcel_RunWorkerCompleted);

            cartonNumberingList = new List<CartonNumberingModel>();
            cartonNumberingDetailList = new List<CartonNumberingDetailModel>();

            bwImport = new BackgroundWorker();
            bwImport.DoWork += new DoWorkEventHandler(bwImport_DoWork);
            bwImport.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwImport_RunWorkerCompleted);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open Original Packing List File";
            openFileDialog.Filter = "EXCEL Files (*.xls, *.xlsx)|*.xls;*.xlsx";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                filePathArray = openFileDialog.FileNames;
                if (bwReadExcel.IsBusy == false)
                {
                    this.Cursor = Cursors.Wait;
                    cartonNumberingList.Clear();
                    cartonNumberingDetailList.Clear();
                    bwReadExcel.RunWorkerAsync();
                }
            }
            else
            {
                this.Close();
            }
        }

        private void bwReadExcel_DoWork(object sender, DoWorkEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                txtStatus.Text = "Reading ...";
                prgStatus.Maximum = filePathArray.Count();
            }));
            int filePathIndex = 1;
            foreach (string filePath in filePathArray)
            {
                Excel.Application excelApplication = new Excel.Application();
                Excel.Workbook excelWorkbook = excelApplication.Workbooks.Open(filePath);
                //excelApplication.Visible = true;
                Excel.Worksheet excelWorksheet;
                Excel.Range excelRange;
                try
                {
                    excelWorksheet = (Excel.Worksheet)excelWorkbook.Worksheets[1];
                    excelRange = excelWorksheet.UsedRange;
                    var productNoCell = (excelRange.Cells[1, 1] as Excel.Range).Value2;
                    if (productNoCell != null)
                    {
                        string productNo = productNoCell.ToString().Split('/')[0].Replace("PROD. NO:", "").Trim();
                        if (string.IsNullOrEmpty(productNo) == false)
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                txtStatus.Text = String.Format("Reading PO: {0}    total: {1} / {2}", productNo, filePathIndex, filePathArray.Count());
                                prgStatus.Value = filePathIndex;
                            }));
                            int i = 1;
                            for (int x = 2; x <= excelRange.Columns.Count; x++)
                            {
                                var sizeNoCell = (excelRange.Cells[5, x] as Excel.Range).Value2;
                                if (sizeNoCell != null)
                                {
                                    string sizeNo = sizeNoCell.ToString();
                                    if (string.IsNullOrEmpty(sizeNo) == false)
                                    {
                                        if (cartonNumberingList.Where(c => c.ProductNo.ToLower() == productNo.ToLower() && c.SizeNo.ToLower() == sizeNo.ToLower()).Count() > 0)
                                        {
                                            sizeNo = string.Format("{0}'", sizeNo);
                                        }
                                        var qtyCell = (excelRange.Cells[7, x] as Excel.Range).Value2;
                                        int qty = 0;
                                        int.TryParse(qtyCell.ToString(), out qty);

                                        CartonNumberingModel cartonNumbering = new CartonNumberingModel
                                        {
                                            ProductNo = productNo,
                                            SizeNo = sizeNo,
                                            Quantity = qty,
                                        };
                                        if (string.IsNullOrEmpty(cartonNumbering.ProductNo) == false
                                            && string.IsNullOrEmpty(cartonNumbering.SizeNo) == false
                                            && cartonNumbering.Quantity > 0)
                                        {
                                            cartonNumberingList.Add(cartonNumbering);
                                        }
                                        //for (int y = 8; y <= excelRange.Rows.Count; y++)
                                        for (int y = i; y <= i + qty - 1; y++)
                                        {
                                            //var cartonNoCell = (excelRange.Cells[y, x] as Excel.Range).Value2;
                                            //if (cartonNoCell != null)
                                            //{
                                            //int cartonNo = 0;
                                            int cartonNo = y;
                                            //int.TryParse(cartonNoCell.ToString(), out cartonNo);

                                            CartonNumberingDetailModel cartonNumberingDetail = new CartonNumberingDetailModel
                                            {
                                                ProductNo = productNo,
                                                SizeNo = sizeNo,
                                                CartonNo = cartonNo,
                                            };
                                            if (string.IsNullOrEmpty(cartonNumberingDetail.ProductNo) == false
                                                && string.IsNullOrEmpty(cartonNumberingDetail.SizeNo) == false
                                                && cartonNumberingDetail.CartonNo > 0)
                                            {
                                                cartonNumberingDetailList.Add(cartonNumberingDetail);
                                            }
                                            //}
                                        }
                                        i = qty + i;
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    excelWorkbook.Close(false, Missing.Value, Missing.Value);
                    excelApplication.Quit();
                }
                filePathIndex++;
            }
        }

        private void bwReadExcel_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = null;
            if (e.Cancelled == true)
            {
                return;
            }
            if (e.Error != null)
            {
                MessageBox.Show(string.Format("Error\n{0}", e.Error.Message), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            if (cartonNumberingList.Count <= 0 || cartonNumberingDetailList.Count <= 0)
            {
                MessageBox.Show("Cannot Read Excel File. Try Again!", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }

            txtStatus.Text = String.Format("Read completed: {0} POs", cartonNumberingList.Select(s => s.ProductNo).Distinct().Count());
            prgStatus.Value = 0;

            dgMain.ItemsSource = cartonNumberingDetailList;
            MessageBoxResult result = MessageBox.Show(string.Format("Read Completed, {0} Size & {1} Carton. Do You Want Clear Old Packing List Before Import?", cartonNumberingList.Count, cartonNumberingDetailList.Count), this.Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Information, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes || result == MessageBoxResult.No)
            {
                if (bwImport.IsBusy == false)
                {
                    this.Cursor = Cursors.Wait;
                    bwImport.RunWorkerAsync(result);
                }
            }
            else
            {
                this.Close();
            }
        }

        private void bwImport_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1500);
            List<String> productNoList = cartonNumberingList.Select(c => c.ProductNo).Distinct().ToList();
            MessageBoxResult? result = e.Argument as MessageBoxResult?;

            Dispatcher.Invoke(new Action(() => {
                txtStatus.Text = "Deleting PO ...";
                prgStatus.Maximum = productNoList.Count();
            }));

            if (result.Value == MessageBoxResult.Yes)
            {
                int productNoIndex = 1;
                foreach (string productNo in productNoList)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        txtStatus.Text = String.Format("Deleting PO: {0}", productNo);
                        prgStatus.Value = productNoIndex;
                    }));
                    CartonNumberingController.Delete(productNo);
                    CartonNumberingDetailController.Delete(productNo);
                    productNoIndex++;
                }
            }

            Dispatcher.Invoke(new Action(() =>
            {
                txtStatus.Text = "Creating CartonNumbering ...";
                prgStatus.Maximum = cartonNumberingList.Count();
            }));

            int cartonNumberingIndex = 1;
            foreach (CartonNumberingModel model in cartonNumberingList)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    txtStatus.Text = String.Format("Creating CartonNumbering for: PO: {0} Size: {1}", model.ProductNo, model.SizeNo);
                    prgStatus.Value = cartonNumberingIndex;
                }));
                CartonNumberingController.Create(model);
                cartonNumberingIndex++;
            }

            Dispatcher.Invoke(new Action(() =>
            {
                txtStatus.Text = "Creating CartonNumberingDetail ...";
                prgStatus.Maximum = cartonNumberingDetailList.Count();
            }));

            int cartonNumberingDetailIndex = 1;
            foreach (CartonNumberingDetailModel model in cartonNumberingDetailList)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    txtStatus.Text = String.Format("Creating CartonNumberingDetail for : PO: {0} Size: {1} Carton: {2}", model.ProductNo, model.SizeNo, model.CartonNo);
                    prgStatus.Value = cartonNumberingDetailIndex;

                    dgMain.SelectedItem = model;
                    dgMain.ScrollIntoView(model);
                }));
                CartonNumberingDetailController.Create(model);
                cartonNumberingDetailIndex++;
            }
        }

        private void bwImport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = null;
            if (e.Error != null)
            {
                MessageBox.Show(string.Format("Error\n{0}", e.Error.Message), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            MessageBox.Show("Saved!", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
            txtStatus.Text = "Finished !";
            prgStatus.Value = 0;

            Thread.Sleep(1500);
            this.Close();
        }
    }
}
