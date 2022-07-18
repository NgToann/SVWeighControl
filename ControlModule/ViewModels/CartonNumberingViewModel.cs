using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ControlModule.ViewModels
{
    public class CartonNumberingViewModel : INotifyPropertyChanged
    {
        private string _ProductNo;
        public string ProductNo
        {
            get { return _ProductNo; }
            set
            {
                _ProductNo = value;
                OnPropertyChanged("ProductNo");
            }
        }

        private string _SizeNo;
        public string SizeNo
        {
            get { return _SizeNo; }
            set
            {
                _SizeNo = value;
                OnPropertyChanged("SizeNo");
            }
        }

        private int _Quantity;
        public int Quantity
        {
            get { return _Quantity; }
            set
            {
                _Quantity = value;
                OnPropertyChanged("Quantity");
            }
        }

        private double _GrossWeight;
        public double GrossWeight
        {
            get { return _GrossWeight; }
            set
            {
                _GrossWeight = value;
                OnPropertyChanged("GrossWeight");
            }
        }

        private string _Barcode;
        public string Barcode
        {
            get { return _Barcode; }
            set
            {
                _Barcode = value;
                OnPropertyChanged("Barcode");
            }
        }        

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
