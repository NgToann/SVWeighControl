using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ControlModule.Entities;
using System.Data.SqlClient;
using ControlModule.Models;

namespace ControlModule.Controllers
{
    public class PORepackingController
    {
        //spm_InsertPORepacking
        static SaovietWeightControlEntities db = ConnectTo.SaovietWeightControlEntities();
        public static bool Insert(PORepackingModel model)
        {
            var @ProductNo = new SqlParameter("@ProductNo", model.ProductNo);
            if (db.ExecuteStoreCommand("spm_InsertPORepacking @ProductNo", @ProductNo) > 0)
            {
                return true;
            }
            return false;
        }
        public static List<PORepackingModel> GetAll()
        {
            return db.ExecuteStoreQuery<PORepackingModel>("spm_SelectPORepacking").ToList();
        }
    }
}
