using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ControlModule.Entities;
using ControlModule.Models;
using System.Data.SqlClient;
namespace ControlModule.Controllers
{
    class AccountController
    {
        static SaovietWeightControlEntities db = ConnectTo.SaovietWeightControlEntities();
        public static AccountModel Find(string userName, string password)
        {
            var @UserName = new SqlParameter("@UserName", userName);
            var @Password = new SqlParameter("@Password", password);

            return db.ExecuteStoreQuery<AccountModel>("spm_SelectAccountByUserNamePassword @UserName, @Password", @UserName, @Password).FirstOrDefault();
        }
    }
}
