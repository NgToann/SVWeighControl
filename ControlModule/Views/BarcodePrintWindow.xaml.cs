using System.Windows;
using System.Windows.Media.Imaging;

using BarcodeLib;
using drawing = System.Drawing;
using System.IO;
namespace ControlModule.Views
{
    /// <summary>
    /// Interaction logic for BarcodePrintWindow.xaml
    /// </summary>
    public partial class BarcodePrintWindow : Window
    {
        string barcode;
        int quantity;
        drawing.Image image;
        public BarcodePrintWindow(string _barcode, int _quantity)
        {
            InitializeComponent();
            barcode = _barcode;
            quantity = _quantity;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BarcodeLib.Barcode barcodeLib = new BarcodeLib.Barcode();
            barcodeLib.IncludeLabel = true;
            image = barcodeLib.Encode(TYPE.CODE128, barcode, System.Drawing.Color.Black, System.Drawing.Color.White, 250, 75);
            
            MemoryStream memoryStream = new MemoryStream();
            image.Save(memoryStream, drawing.Imaging.ImageFormat.Png);
            memoryStream.Position = 0;
            BitmapImage itmapImage = new BitmapImage();
            itmapImage.BeginInit();
            itmapImage.StreamSource = memoryStream;
            itmapImage.EndInit();

            imageBarcode.Source = itmapImage;

            txtQuantity.Text = string.Format("{0}", quantity);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(txtQuantity.Text, out quantity);
            drawing.Printing.PrintDocument printDocument = new drawing.Printing.PrintDocument();
            printDocument.DocumentName = string.Format("Barcode Print {0}", barcode);
            printDocument.PrintPage += new drawing.Printing.PrintPageEventHandler(printDocument_PrintPage);
            printDocument.EndPrint += new drawing.Printing.PrintEventHandler(printDocument_EndPrint);
            printDocument.Print();
        }
        
        private void printDocument_PrintPage(object sender, drawing.Printing.PrintPageEventArgs e)
        {
            quantity = quantity - 1;
            int marginLeft = 0;
            int.TryParse(txtMarginLeft.Text, out marginLeft);
            int marginTop = 0;
            int.TryParse(txtMarginTop.Text, out marginTop);
            drawing.Point point = new drawing.Point(marginLeft, marginTop);
            e.Graphics.DrawImage(image, point);
            if (quantity > 0)
            {
                e.HasMorePages = true;
                return;
            }
        }

        private void printDocument_EndPrint(object sender, drawing.Printing.PrintEventArgs e)
        {
            this.Close();
        }
    }
}
