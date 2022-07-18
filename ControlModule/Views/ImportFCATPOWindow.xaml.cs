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
    /// Interaction logic for ImportFCATPOWindow.xaml
    /// </summary>
    public partial class ImportFCATPOWindow : Window
    {
        string[] filePathArray;
        BackgroundWorker bwReadExcel;
        List<FcatPOModel> fcatPOList;
        List<FcatPOModel> fcatPOSearchList;
        BackgroundWorker bwLoad;
        BackgroundWorker bwUpload;
        private eAction actionMode = eAction.None;
        public ImportFCATPOWindow()
        {
            fcatPOList = new List<FcatPOModel>();
            fcatPOSearchList = new List<FcatPOModel>();
            bwReadExcel = new BackgroundWorker();
            bwReadExcel.DoWork += BwReadExcel_DoWork;
            bwReadExcel.RunWorkerCompleted += BwReadExcel_RunWorkerCompleted;

            bwLoad = new BackgroundWorker();
            bwLoad.DoWork += BwLoad_DoWork;
            bwLoad.RunWorkerCompleted += BwLoad_RunWorkerCompleted;

            bwUpload = new BackgroundWorker();
            bwUpload.DoWork += BwUpload_DoWork;
            bwUpload.RunWorkerCompleted += BwUpload_RunWorkerCompleted;

            InitializeComponent();
        }
        
        private void BwReadExcel_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
            if (fcatPOList.Count <= 0)
            {
                MessageBox.Show("Cannot Read Excel File. Try Again!", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }

            MessageBox.Show(string.Format("Read Completed, {0} Pos. Do You Want Clear Old Data Before Import?", fcatPOList.Count), this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
            txtStatus.Text = String.Format("Read completed: {0} POs", fcatPOList.Select(s => s.ProductNo).Distinct().Count());
            prgStatus.Value = 0;
            dgMain.ItemsSource = fcatPOList;
        }

        private void BwReadExcel_DoWork(object sender, DoWorkEventArgs e)
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
                Excel.Worksheet excelWorksheet;
                Excel.Range excelRange;
                try
                {
                    excelWorksheet = (Excel.Worksheet)excelWorkbook.Worksheets[1];
                    excelRange = excelWorksheet.UsedRange;
                    for (int r = 7; r <= excelRange.Rows.Count; r++)
                    {
                        var productNoCell = (excelRange.Cells[r, 3] as Excel.Range).Value2;
                        if (productNoCell != null)
                        {
                            var productNo = productNoCell.ToString();
                            var gbsNoCell = (excelRange.Cells[r, 5] as Excel.Range).Value2;
                            var gbsNo = gbsNoCell != null ? gbsNoCell.ToString() : "";

                            var statusCurrentCell = (excelRange.Cells[r, 11] as Excel.Range).Value2;
                            var statusCurrent = statusCurrentCell != null ? statusCurrentCell.ToString() : "";

                            var fcatPOModel = new FcatPOModel
                            {
                                ProductNo = productNo,
                                GBSNo = gbsNo,
                                StatusCurrent = statusCurrent
                            };
                            fcatPOList.Add(fcatPOModel);

                            Dispatcher.Invoke(new Action(() =>
                            {
                                txtStatus.Text = String.Format("Reading PO: {0}    total file: {1} / {2}", productNo, filePathIndex, filePathArray.Count());
                                prgStatus.Value = filePathIndex;
                            }));
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

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            var importList = dgMain.ItemsSource.OfType<FcatPOModel>().ToList();
            if (bwUpload.IsBusy==false)
            {
                this.Cursor = Cursors.Wait;
                btnImport.IsEnabled = false;
                actionMode = eAction.Add;
                bwUpload.RunWorkerAsync(importList);
            }
        }

        private void BwUpload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = null;
            btnImport.IsEnabled = true;
            if (e.Error == null && e.Cancelled == false && (bool)e.Result == true && actionMode == eAction.Add)
            {
                MessageBox.Show(string.Format("Imported !"), this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            if (e.Error == null && e.Cancelled == false && (bool)e.Result == true && actionMode == eAction.Delete)
            {
                MessageBox.Show(string.Format("Deleted !"), this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                dgMain.ItemsSource = fcatPOSearchList;
                dgMain.Items.Refresh();
            }
        }

        private void BwUpload_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var uploadList = e.Argument as List<FcatPOModel>;
                foreach (var item in uploadList)
                {
                    if (actionMode == eAction.Add)
                    {
                        FCatPOController.Upload(item, isAddNew: true, isDelete: false);
                        dgMain.Dispatcher.Invoke(new Action(() =>
                        {
                            dgMain.SelectedItem = item;
                            dgMain.ScrollIntoView(item);
                        }));
                    }
                    else if (actionMode == eAction.Delete)
                    {
                        FCatPOController.Upload(item, isAddNew: false, isDelete: true);
                        fcatPOSearchList.Remove(item);
                    }
                }
                e.Result = true;
            }
            
            catch (Exception ex)
            {
                Dispatcher.Invoke(new Action(() => {
                    MessageBox.Show(ex.InnerException.InnerException.Message.ToString(), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }));
                e.Result = false;
            }
        }

        private void btnOpenExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open F.CAT PO List";
            openFileDialog.Filter = "EXCEL Files (*.xls, *.xlsx)|*.xls;*.xlsx";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                filePathArray = openFileDialog.FileNames;
                if (bwReadExcel.IsBusy == false)
                {
                    this.Cursor = Cursors.Wait;
                    fcatPOList.Clear();
                    bwReadExcel.RunWorkerAsync();
                }
            }
            else
            {
                this.Close();
            }
        }

        private void txtPOSearch_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            btnSearch.IsDefault = true;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            btnSearch.IsDefault = false;
            var searchWhat = txtPOSearch.Text.Trim().ToString();
            List<String> productNoListSearch = new List<string>();
            if (searchWhat.Contains(";"))
            {
                productNoListSearch = searchWhat.Split(';').ToList();
            }
            else
            {
                productNoListSearch.Add(searchWhat);
            }

            if (bwLoad.IsBusy==false)
            {
                txtStatus.Text = "";
                this.Cursor = Cursors.Wait;
                fcatPOSearchList.Clear();
                btnSearch.IsEnabled = false;
                bwLoad.RunWorkerAsync(productNoListSearch);
            }
        }

        private void BwLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            var productNoListSearch = e.Argument as List<String>;
            foreach (var productNo in productNoListSearch)
            {
                var x = FCatPOController.GetByPOOrGBS(productNo.Trim());
                if (x != null)
                    fcatPOSearchList.Add(x);
            }
        }

        private void BwLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = null;
            btnSearch.IsEnabled = true;
            dgMain.ItemsSource = fcatPOSearchList;
            dgMain.Items.Refresh();

            // Create ContextMenu
            dgMain.ContextMenu = null;
            var ctm = new ContextMenu();
            var menuItem = new MenuItem();
            menuItem.Header = "Remove";
            menuItem.Click += MenuItem_Click;
            ctm.Items.Add(menuItem);
            dgMain.ContextMenu = ctm;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dgMain.ItemsSource == null)
                return;
            var itemsSelected = dgMain.SelectedItems.OfType<FcatPOModel>().ToList();

            if (MessageBox.Show(string.Format("Confirm remove {0} item{1} ?", itemsSelected.Count(), itemsSelected.Count() > 1 ? "s" : ""), this.Title, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK
                || itemsSelected.Count() == 0)
            {
                return;
            }
            if (bwUpload.IsBusy==false)
            {
                this.Cursor = Cursors.Wait;
                actionMode = eAction.Delete;
                bwUpload.RunWorkerAsync(itemsSelected);
            }
        }

        enum eAction
        {
            None, Add, Delete
        }

        private void dgMain_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
    }
}
