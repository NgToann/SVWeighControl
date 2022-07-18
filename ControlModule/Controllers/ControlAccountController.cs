using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ControlModule.Entities;
using ControlModule.Models;
using System.Data.SqlClient;
namespace ControlModule.Controllers
{
    class ControlAccountController
    {
        static SaovietWeightControlEntities db = ConnectTo.SaovietWeightControlEntities();
        public static ControlAccountModel Find(string securityCode)
        {
            var @SecurityCode = new SqlParameter("@SecurityCode", securityCode);
            return db.ExecuteStoreQuery<ControlAccountModel>("spm_SelectControlAccountBySecurityCode @SecurityCode", @SecurityCode).FirstOrDefault();
        }
    }
}
