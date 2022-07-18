using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ControlModule.Entities;
using ControlModule.Models;
using System.Data.SqlClient;

namespace ControlModule.Controllers
{
    public class StoringController
    {
        //spm_SelectStoringByPO
        private static SaovietWeightControlEntities db = ConnectTo.SaovietWeightControlEntities();
        public static List<StoringModel> Get(string productNo)
        {
            var @ProductNo = new SqlParameter("@ProductNo", productNo);
            return db.ExecuteStoreQuery<StoringModel>("spm_SelectStoringByPO @ProductNo", @ProductNo).ToList();
        }

        public static List<StoringModel> GetAll()
        {
            return db.ExecuteStoreQuery<StoringModel>("spm_SelectStoringAvailable").ToList();
        }
    }
}
