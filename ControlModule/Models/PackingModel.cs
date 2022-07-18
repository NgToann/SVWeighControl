using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControlModule.Models
{
    public class PackingModel
    {
        public string ProductNo { get; set; }
        public string SizeNo { get; set; }
        public int CartonNo { get; set; }
        public double ActualWeight { get; set; }
        public double DifferencePercent { get; set; }
        public bool IsPass { get; set; }
        public bool IsFirstPass { get; set; }
        public string CreatedAccount { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Barcode { get; set; }
    }
}
