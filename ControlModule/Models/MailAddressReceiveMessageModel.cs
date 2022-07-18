using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControlModule.Models
{
    public class MailAddressReceiveMessageModel
    {
        public string MailAddress { get; set; }
        public string Name { get; set; }
        public bool HidUSBSend { get; set; }
        public bool CartonIssuesSend { get; set; }
    }
}
