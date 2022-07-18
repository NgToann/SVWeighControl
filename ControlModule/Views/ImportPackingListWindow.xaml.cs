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
namespace ControlModule.Views
{
    /// <summary>
    /// Interaction logic for InputPackingListWindow.xaml
    /// </summary>
    public partial class ImportPackingListWindow : Window
    {
        string[] filePathArray;
        BackgroundWorker bwReadExcel;
        List<CartonNumberingModel> cartonNumberingList;
        List<CartonNumberingDetailModel> cartonNumberingDetailList;
        BackgroundWorker bwImport;
        public ImportPackingListWindow()
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
            openFileDialog.Title = "Open Packing List File";
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
                            for (int x = 2; x <= excelRange.Columns.Count; x++)
                            {
                                var sizeNoCell = (excelRange.Cells[5, x] as Excel.Range).Value2;
                                if (sizeNoCell != null)
                                {
                                    string sizeNo = sizeNoCell.ToString();
                                    if (string.IsNullOrEmpty(sizeNo) == false)
                                    {
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
                                        for (int y = 8; y <= excelRange.Rows.Count; y++)
                                        {
                                            var cartonNoCell = (excelRange.Cells[y, x] as Excel.Range).Value2;
                                            if (cartonNoCell != null)
                                            {
                                                int cartonNo = 0;
                                                int.TryParse(cartonNoCell.ToString(), out cartonNo);

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
                                            }
                                        }
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
            List<String> productNoList = cartonNumberingList.Select(c => c.ProductNo).Distinct().ToList();
            MessageBoxResult? result = e.Argument as MessageBoxResult?;
            if (result.Value == MessageBoxResult.Yes)
            {
                foreach (string productNo in productNoList)
                {
                    CartonNumberingController.Delete(productNo);
                    CartonNumberingDetailController.Delete(productNo);
                }
            }
            foreach (CartonNumberingModel model in cartonNumberingList)
            {
                CartonNumberingController.Create(model);
            }
            foreach (CartonNumberingDetailModel model in cartonNumberingDetailList)
            {
                CartonNumberingDetailController.Create(model);
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
            this.Close();
        }
    }
}
