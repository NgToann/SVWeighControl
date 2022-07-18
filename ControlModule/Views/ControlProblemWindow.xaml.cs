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
using ControlModule.Models;
using ControlModule.Controllers;
namespace ControlModule.Views
{
    /// <summary>
    /// Interaction logic for ControlProblemWindow.xaml
    /// </summary>
    public partial class ControlProblemWindow : Window
    {
        public string CheckBy;
        public string Reason;
        BackgroundWorker threadLoad;
        bool metalDetected;
        bool canClose;
        int cartonNo;
        string codeDetected;
        public ControlProblemWindow(bool metalDetected, int cartonNo, string codeDetected)
        {
            InitializeComponent();
            this.metalDetected = metalDetected;
            this.cartonNo = cartonNo;
            this.codeDetected = codeDetected;
            canClose = false;
            threadLoad = new BackgroundWorker();
            threadLoad.DoWork += new DoWorkEventHandler(threadLoad_DoWork);
            threadLoad.RunWorkerCompleted += new RunWorkerCompletedEventHandler(threadLoad_RunWorkerCompleted);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtSecurityCode.Focus();
            if (metalDetected)
            {
                panelChooseReason.Visibility = Visibility.Collapsed;
                brMetalDetected.Visibility = Visibility.Visible;
                tblCartonNo.Text = String.Format("CartonNo: {0}", cartonNo);
                radMetalCorrect.IsChecked = true;
                tblCodeDetect.Text = codeDetected;
            }
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            string securityCode = txtSecurityCode.Password;
            if (string.IsNullOrEmpty(securityCode) == true)
            {
                return;
            }
            if (threadLoad.IsBusy == true)
            {
                return;
            }
            btnAccept.IsEnabled = false;
            canClose = false;
            btnReport.IsEnabled = false;
            this.Cursor = Cursors.Wait;
            threadLoad.RunWorkerAsync(securityCode);
        }

        private void threadLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            string securityCode = e.Argument as string;
            e.Result = ControlAccountController.Find(securityCode);
        }

        private void threadLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnAccept.IsEnabled = true;            
            this.Cursor = null;
            if (e.Error != null)
            {
                MessageBox.Show(string.Format("Error\n{0}", e.Error.Message), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ControlAccountModel controlAccount = e.Result as ControlAccountModel;
            if (controlAccount == null)
            {
                MessageBox.Show("Security Code Un-correct.", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            txtSecurityCode.Clear();
            CheckBy = controlAccount.FullName;
            tblWelcome.Text = controlAccount.FullName;
            btnReport.IsEnabled = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (canClose == false)
            {
                e.Cancel = true;
            }
        }

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            string reason = null;
            foreach (UIElement uiElement in panelChooseReason.Children)
            {
                RadioButton rbtnReason = uiElement as RadioButton;
                if (rbtnReason.IsChecked == true)
                {
                    reason = rbtnReason.Content as string;
                }
            }
            if (!metalDetected)
            {
                if (string.IsNullOrEmpty(reason) == true)
                {
                    return;
                }
                Reason = reason;
            }
            canClose = true;
            this.Close();
        }

        private void txtSecurityCode_KeyDown(object sender, KeyEventArgs e)
        {
            btnAccept.IsDefault = true;
        }

        private void radMetalCorrect_Checked(object sender, RoutedEventArgs e)
        {
            Reason = radMetalCorrect.Content.ToString();
        }

        private void radMetalInCorrect_Checked(object sender, RoutedEventArgs e)
        {
            Reason = radMetalInCorrect.Content.ToString();
        }
    }
}
