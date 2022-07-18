using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ControlModule.Entities;
using ControlModule.Models;
namespace ControlModule.Controllers
{
    class MailAddressReceiveMessageController
    {
        static SaovietWeightControlEntities db = ConnectTo.SaovietWeightControlEntities();
        public static List<MailAddressReceiveMessageModel> Get()
        {
            return db.ExecuteStoreQuery<MailAddressReceiveMessageModel>("spm_SelectMailAddressReceiveMessage").ToList();
        }
    }
}
