using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ControlModule.Views;
using ControlModule.Helpers;
namespace ControlModule
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void miExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void miCartonNumbering_Click(object sender, RoutedEventArgs e)
        {
            CartonNumberingWindow window = new CartonNumberingWindow();
            window.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            this.Title = String.Format("{0} - Version: {1}", this.Title, ProgramHelper.GetVersion());            
        }

        private void miAbout_Click(object sender, RoutedEventArgs e)
        {
            string about = "*Software: Loading System (Only for Saoviet Corporation)\n"
                + String.Format("*Version: {0} - Scan Barcode Only\n\n", ProgramHelper.GetVersion())
                + "*Created by: Mr.Tuấn & Mr.Vũ\n"
                + "*Contact:\n"
                + " -Phone & WhatsApp & Zalo: 0973148429\n"
                + " -Skype: vuvd_it";
            MessageBox.Show(about, "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void miCompareWeight_Click(object sender, RoutedEventArgs e)
        {
            CompareWeightWindow window = new CompareWeightWindow(null);
            window.Show();
        }

        private void miInputPackingList_Click(object sender, RoutedEventArgs e)
        {
            ImportPackingListWindow window = new ImportPackingListWindow();
            window.Show();
        }

        private void miPackingReport_Click(object sender, RoutedEventArgs e)
        {
            PackingReportWindow window = new PackingReportWindow();
            window.Show();
        }

        private void miNewInputPackingList_Click(object sender, RoutedEventArgs e)
        {
            NewImportPackingListWindow window = new NewImportPackingListWindow();
            window.Show();
        }

        private void miInputPORepacking_Click(object sender, RoutedEventArgs e)
        {
            ImportPORepackingWindow window = new ImportPORepackingWindow();
            window.Show();
        }

        private void miImportPackingListDIESELPO_Click(object sender, RoutedEventArgs e)
        {
            ImportPackingListDIESELPOWindow window = new ImportPackingListDIESELPOWindow();
            window.Show();
        }

        private void miImportFCATPO_Click(object sender, RoutedEventArgs e)
        {
            ImportFCATPOWindow window = new ImportFCATPOWindow();
            window.Show();
        }

        private void miDeletePO_Click(object sender, RoutedEventArgs e)
        {
            //DeletePOWindow window = new DeletePOWindow();
            //window.ShowDialog();
        }
    }
}
