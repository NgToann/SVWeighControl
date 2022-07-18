using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;


using System.ComponentModel;
using Microsoft.Win32;
using ControlModule.Models;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using ControlModule.Controllers;

namespace ControlModule.Views
{
    /// <summary>
    /// Interaction logic for ImportPackingListDIESELPO.xaml
    /// </summary>
    public partial class ImportPackingListDIESELPOWindow : Window
    {
        string[] filePathArray;
        BackgroundWorker bwReadExcel;
        List<CartonNumberingModel> cartonNumberingList;
        List<CartonNumberingDetailModel> cartonNumberingDetailList;
        List<CartonNumberingDetaiTempModel> cartonNumberingDetailTempList;
        BackgroundWorker bwImport;

        public ImportPackingListDIESELPOWindow()
        {
            bwReadExcel = new BackgroundWorker();
            bwReadExcel.WorkerSupportsCancellation = true;
            bwReadExcel.DoWork += new DoWorkEventHandler(bwReadExcel_DoWork);
            bwReadExcel.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwReadExcel_RunWorkerCompleted);

            cartonNumberingList = new List<CartonNumberingModel>();
            cartonNumberingDetailList = new List<CartonNumberingDetailModel>();
            cartonNumberingDetailTempList = new List<CartonNumberingDetaiTempModel>();

            bwImport = new BackgroundWorker();
            bwImport.DoWork += new DoWorkEventHandler(bwImport_DoWork);
            bwImport.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwImport_RunWorkerCompleted);

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open Original Packing List DIESEL ProductNo File";
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
                                        var grossWeightCell = (excelRange.Cells[3, x] as Excel.Range).Value2;
                                        double grossWeight = 0;
                                        Double.TryParse(grossWeightCell.ToString(), out grossWeight);

                                        for (int y = 8; y <= excelRange.Rows.Count; y++)
                                        {
                                            var cartonNoCell = (excelRange.Cells[y, x] as Excel.Range).Value2;
                                            if (cartonNoCell != null)
                                            {
                                                int cartonNo = 0;
                                                int.TryParse(cartonNoCell.ToString(), out cartonNo);

                                                CartonNumberingDetaiTempModel cartonNumberingDetailTemp = new CartonNumberingDetaiTempModel
                                                {
                                                    GrossWeight = grossWeight,
                                                    ProductNo = productNo,
                                                    SizeNo = sizeNo,
                                                    CartonNo = cartonNo,
                                                };
                                                if (string.IsNullOrEmpty(cartonNumberingDetailTemp.ProductNo) == false
                                                    && string.IsNullOrEmpty(cartonNumberingDetailTemp.SizeNo) == false
                                                    && cartonNumberingDetailTemp.CartonNo > 0 && cartonNumberingDetailTemp.GrossWeight > 0)
                                                {
                                                    cartonNumberingDetailTempList.Add(cartonNumberingDetailTemp);
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
            if (cartonNumberingDetailTempList.Count <= 0)
            {
                MessageBox.Show("Cannot Read Excel File. Try Again!", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }

            cartonNumberingDetailTempList.OrderBy(o => o.CartonNo).ToList();

            var sizeNoList = cartonNumberingDetailTempList.OrderBy(o => o.CartonNo).Select(s => new { ProductNo = s.ProductNo, SizeNo = s.SizeNo }).Distinct().ToList();

            foreach (var p in sizeNoList)
            {
                string sizeNo =  p.SizeNo;
                var grossWeightList = cartonNumberingDetailTempList.Where(w => w.ProductNo == p.ProductNo && w.SizeNo == p.SizeNo).Select(s => s.GrossWeight).Distinct().ToList();
                foreach (var grossWeight in grossWeightList)
                {
                    var cartonNumberingPerSizePerGrossWeightList = cartonNumberingDetailTempList.Where(w => w.ProductNo == p.ProductNo && w.SizeNo == p.SizeNo && w.GrossWeight == grossWeight).ToList();
                    CartonNumberingModel cartonNumbering = new CartonNumberingModel()
                    {
                        ProductNo = p.ProductNo,
                        SizeNo = sizeNo,
                        Quantity = cartonNumberingPerSizePerGrossWeightList.Count
                    };
                    cartonNumberingList.Add(cartonNumbering);

                    var cartonNoList = cartonNumberingPerSizePerGrossWeightList.Select(s => s.CartonNo).ToList();
                    foreach (var cartonNo in cartonNoList)
                    {
                        var cartonNoDetailTempModel = cartonNumberingPerSizePerGrossWeightList.Where(w => w.CartonNo == cartonNo).FirstOrDefault();
                        CartonNumberingDetailModel cartonNoDetailModel = new CartonNumberingDetailModel()
                        {
                            ProductNo = p.ProductNo,
                            SizeNo = sizeNo,
                            CartonNo = cartonNo
                        };
                        cartonNumberingDetailList.Add(cartonNoDetailModel);
                    }
                    sizeNo += "'";
                }
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

        class CartonNumberingDetaiTempModel
        {
            public string ProductNo { get; set; }
            public string SizeNo { get; set; }
            public double GrossWeight { get; set; }
            public int CartonNo { get; set; }
        }
    }
}
