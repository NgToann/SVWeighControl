using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControlModule.Models
{
    public class ElectricScaleSource
    {
        public int ProfileId { get; set; }
        public int BaudRate { get; set; }
        public char StabilityKey { get; set; }
        public char BeginKey { get; set; }
        public char EndKey { get; set; }
        public string SubjectMail { get; set; }
        public string Location { get; set; }
    }
}
