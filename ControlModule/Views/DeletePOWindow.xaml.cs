using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ControlModule.Views
{
    /// <summary>
    /// Interaction logic for DeletePOWindow.xaml
    /// </summary>
    public partial class DeletePOWindow : Window
    {
        BackgroundWorker bwDelete;
        string productNo;
        public DeletePOWindow(string productNo)
        {
            this.productNo = productNo;
            InitializeComponent();
            bwDelete = new BackgroundWorker();
            bwDelete.DoWork += BwDelete_DoWork;
            bwDelete.RunWorkerCompleted += BwDelete_RunWorkerCompleted;
        }

        private void BwDelete_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string password = e.Argument as String;
                var check = ControlAccountController.Find(password);
                if (check == null)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        MessageBox.Show("Password incorrect ! Please Try Again", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                        txtPassword.Focus();
                        txtPassword.SelectAll();
                        e.Result = false;
                    }));
                }
                else
                {
                    CartonNumberingController.Delete(productNo);
                    PackingController.Delete(productNo);
                    e.Result = true;
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    e.Result = false;
                    MessageBox.Show(ex.Message.ToString());
                }));
            }
        }
        private void BwDelete_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnConfirm.IsEnabled = true;
            this.Cursor = null;
            if (e.Error == null && e.Cancelled == false && (bool)e.Result == true)
            {
                MessageBox.Show("Deleted !", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!bwDelete.IsBusy)
            {
                string password = txtPassword.Password.Trim().ToString();
                this.Cursor = Cursors.Wait;
                btnConfirm.IsEnabled = false;
                bwDelete.RunWorkerAsync(password);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            btnConfirm.IsDefault = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtPassword.Focus();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
