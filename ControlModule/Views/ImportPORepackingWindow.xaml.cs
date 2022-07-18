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

using ControlModule.Controllers;
using ControlModule.Models;
using System.ComponentModel;

namespace ControlModule.Views
{
    /// <summary>
    /// Interaction logic for ImportPORepackingWindow.xaml
    /// </summary>
    public partial class ImportPORepackingWindow : Window
    {
        List<PORepackingModel> poRepackingInsertList;
        List<PORepackingModel> poRepackingLoadList;
        List<PORepackingModel> poRepackingReLoadList;
        BackgroundWorker bwLoad;

        public ImportPORepackingWindow()
        {
            poRepackingInsertList = new List<PORepackingModel>();
            poRepackingLoadList = new List<PORepackingModel>();
            poRepackingReLoadList = new List<PORepackingModel>();

            bwLoad = new BackgroundWorker();
            bwLoad.DoWork +=new DoWorkEventHandler(bwLoad_DoWork);
            bwLoad.RunWorkerCompleted +=new RunWorkerCompletedEventHandler(bwLoad_RunWorkerCompleted);

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (bwLoad.IsBusy == true)
            {
                return;
            }
            this.Cursor = Cursors.Wait;
            bwLoad.RunWorkerAsync();
        }

        private void bwLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            poRepackingLoadList = PORepackingController.GetAll();
        }

        private void bwLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dgPORepacking.ItemsSource = poRepackingLoadList;
            this.Cursor = null;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            poRepackingReLoadList = dgPORepacking.Items.OfType<PORepackingModel>().ToList();
            PORepackingModel currentPO = new PORepackingModel();
            currentPO = dgPORepacking.CurrentItem as PORepackingModel;

            poRepackingReLoadList.RemoveAll(r => r.ProductNo == currentPO.ProductNo);
            ReLoad();
        }

        private void ReLoad()
        {
            dgPORepacking.ItemsSource = null;
            dgPORepacking.ItemsSource = poRepackingReLoadList;
        }

        private void btnAddPORepacking_Click(object sender, RoutedEventArgs e)
        {
            poRepackingReLoadList = dgPORepacking.Items.OfType<PORepackingModel>().ToList();

            string productNo = "";
            productNo= txtPORepacking.Text.ToUpper().Trim();
            if (productNo == "")
            {
                txtPORepacking.Focus();
                return;
            }

            var productNoExist = poRepackingReLoadList.Where(w => w.ProductNo == productNo).ToList();
            if (productNoExist.Count > 0)
            {
                MessageBox.Show(string.Format("PO: {0} exist !", productNo), this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            PORepackingModel newPO = new PORepackingModel();
            newPO.ProductNo = productNo;
            newPO.CreatedTime = DateTime.Now;

            poRepackingReLoadList.Add(newPO);

            ReLoad();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            popConfirm.IsOpen = true;
            txtSecurity.Clear();
            txtSecurity.Focus();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string security = "";
            security = txtSecurity.Password;
            if (security == "")
            {
                txtSecurity.Focus();
                return;
            }

            var controlAccount = ControlAccountController.Find(security);
            if (controlAccount == null)
            {
                MessageBox.Show("Wrong Security Code !", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                txtSecurity.Focus();
                txtSecurity.SelectAll();
                return;
            }

            poRepackingInsertList = dgPORepacking.Items.OfType<PORepackingModel>().ToList();
            if (poRepackingInsertList.Count == 0)
            {
                MessageBox.Show("PO Repacking List Is Empty !", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                popConfirm.IsOpen = false;
                return;
            }

            var poDBList = poRepackingLoadList.Select(s=>s.ProductNo).ToList();
            poRepackingInsertList.RemoveAll(r => poDBList.Contains(r.ProductNo));

            foreach (var poRepacking in poRepackingInsertList)
            {
                PORepackingController.Insert(poRepacking);
                dgPORepacking.Dispatcher.Invoke((Action)(() =>
                {
                    dgPORepacking.SelectedItem = poRepacking;
                    dgPORepacking.ScrollIntoView(poRepacking);
                }));
            }

            MessageBox.Show(string.Format("{0} PO Imported !", poRepackingInsertList.Count), this.Title, MessageBoxButton.OK, MessageBoxImage.Information);

            txtSecurity.Clear();
            popConfirm.IsOpen = false;
        }
    }
}
