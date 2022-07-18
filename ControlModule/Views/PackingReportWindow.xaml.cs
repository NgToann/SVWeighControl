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

using ControlModule.DataSets;
using Microsoft.Reporting.WinForms;
using System.Data;
using System.ComponentModel;
using ControlModule.Models;
using ControlModule.Controllers;
namespace ControlModule.Views
{
    /// <summary>
    /// Interaction logic for CartonNumberingDetailReportWindow.xaml
    /// </summary>
    public partial class PackingReportWindow : Window
    {
        BackgroundWorker threadReport;
        public PackingReportWindow()
        {
            InitializeComponent();

            threadReport = new BackgroundWorker();
            threadReport.DoWork += new DoWorkEventHandler(threadReport_DoWork);
            threadReport.RunWorkerCompleted += new RunWorkerCompletedEventHandler(threadReport_RunWorkerCompleted);
        }        

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            string productNo = txtProductNo.Text;
            if (string.IsNullOrEmpty(productNo) == true)
            {
                return;
            }

            if (threadReport.IsBusy == true)
            {
                return;
            }

            btnReport.IsEnabled = false;
            this.Cursor = Cursors.Wait;
            threadReport.RunWorkerAsync(productNo);            
        }

        private void threadReport_DoWork(object sender, DoWorkEventArgs e)
        {
            string productNo = e.Argument.ToString();
            List<CartonNumberingModel> cartonNumberingList = CartonNumberingController.Get(productNo);
            List<CartonNumberingDetailModel> cartonNumberingDetailList = CartonNumberingDetailController.Get(productNo);
            List<PackingModel> packingList = PackingController.Get(productNo);
            DataTable dtCartonNumbering = new CartonNumberingDataSet().Tables[0];
            DataTable dtPacking = new PackingDataSet().Tables[0];
            foreach (CartonNumberingModel cartonNumbering in cartonNumberingList)
            {
                DataRow drCartonNumbering = dtCartonNumbering.NewRow();
                drCartonNumbering["ProductNo"] = cartonNumbering.ProductNo;
                drCartonNumbering["SizeNo"] = cartonNumbering.SizeNo;
                drCartonNumbering["Quantity"] = cartonNumbering.Quantity;
                drCartonNumbering["GrossWeight"] = Math.Round(cartonNumbering.GrossWeight, 2);
                dtCartonNumbering.Rows.Add(drCartonNumbering);

                List<CartonNumberingDetailModel> cartonNumberingDetail_D1 = cartonNumberingDetailList.Where(c => c.SizeNo == cartonNumbering.SizeNo).ToList();
                for (int i = 1; i <= cartonNumberingDetail_D1.Count(); i++)
                {
                    CartonNumberingDetailModel cartonNumberingDetail = cartonNumberingDetail_D1[i - 1];
                    PackingModel packing = packingList.Where(p => p.CartonNo == cartonNumberingDetail.CartonNo).FirstOrDefault();
                    
                    DataRow drPacking = dtPacking.NewRow();
                    drPacking["ProductNo"] = cartonNumbering.ProductNo;
                    drPacking["SizeNo"] = cartonNumbering.SizeNo;
                    drPacking["NumberOf"] = i;
                    drPacking["PackingValue"] = string.Format("{0} |", cartonNumberingDetail.CartonNo);
                    drPacking["IsFirstPass"] = true;
                    if (packing != null)
                    {
                        drPacking["PackingValue"] = string.Format("{0} | {1}", cartonNumberingDetail.CartonNo, Math.Round(packing.ActualWeight, 1));
                        drPacking["IsFirstPass"] = packing.IsFirstPass;
                    }
                    drPacking["IsCartonNoBasic"] = false;
                    if (cartonNumberingDetail.CartonNo == cartonNumbering.CartonNoBasic)
                    {
                        drPacking["IsCartonNoBasic"] = true;
                    }
                    dtPacking.Rows.Add(drPacking);
                }
            }
            string packingDate = "";
            if (packingList.Count > 0)
            {
                packingDate = string.Format("{0:dd/MM/yyyy}", packingList.FirstOrDefault().CreatedTime);
            }
            double totalWeight = packingList.Sum(p => p.ActualWeight);
            object[] results = { productNo, packingDate, dtCartonNumbering, dtPacking, totalWeight };
            e.Result = results;
        }

        private void threadReport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = null;
            btnReport.IsEnabled = true;

            object[] results = e.Result as object[];
            string productNo = results[0] as string;
            string packingDate = results[1] as string;
            double totalWeight = (results[4] as double?).Value;
            DataTable dtCartonNumbering = results[2] as DataTable;
            DataTable dtPacking = results[3] as DataTable;

            ReportParameter rpProductNo = new ReportParameter("ProductNo", productNo);
            ReportParameter rpPackingDate = new ReportParameter("PackingDate", packingDate);
            ReportParameter rpTotalWeight = new ReportParameter("TotalWeight", totalWeight.ToString());

            ReportDataSource rds = new ReportDataSource();
            rds.Name = "CartonNumbering";
            rds.Value = dtCartonNumbering;
            ReportDataSource rds1 = new ReportDataSource();
            rds1.Name = "Packing";
            rds1.Value = dtPacking;
            //Debug
            //reportViewer.LocalReport.ReportPath = @"C:\Users\IT02\Documents\Visual Studio 2010\Projects\Saoviet Weight Control Solution\ControlModule\Reports\CompareWeightReport.rdlc";
            //Release
            reportViewer.LocalReport.ReportPath = @"Reports\CompareWeightReport.rdlc";
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { rpProductNo, rpPackingDate, rpTotalWeight});
            reportViewer.LocalReport.DataSources.Clear();
            reportViewer.LocalReport.DataSources.Add(rds);
            reportViewer.LocalReport.DataSources.Add(rds1);
            reportViewer.RefreshReport();            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtProductNo.Focus();
        }
    }
}
