using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ControlModule.Entities;
using ControlModule.Models;
using System.Data.SqlClient;
namespace ControlModule.Controllers
{
    class CartonNumberingController
    {
        static SaovietWeightControlEntities db = ConnectTo.SaovietWeightControlEntities();
        public static List<CartonNumberingModel> Get(string productNo)
        {
            var @ProductNo = new SqlParameter("@ProductNo", productNo);
            return db.ExecuteStoreQuery<CartonNumberingModel>("spm_SelectCartonNumberingByProductNo @ProductNo", @ProductNo).ToList();
        }        

        public static bool Delete(string productNo)
        {
            var @ProductNo = new SqlParameter("@ProductNo", productNo);
            if (db.ExecuteStoreCommand("spm_DeleteCartonNumberingByProductNo @ProductNo", @ProductNo) > 0)
            {
                return true;
            }
            return false;
        }

        public static bool Create(CartonNumberingModel model)
        {
            var @ProductNo = new SqlParameter("@ProductNo", model.ProductNo);
            var @SizeNo = new SqlParameter("@SizeNo", model.SizeNo);
            var @Quantity = new SqlParameter("@Quantity", model.Quantity);
            if (db.ExecuteStoreCommand("spm_InsertCartonNumbering  @ProductNo, @SizeNo, @Quantity", @ProductNo, @SizeNo, @Quantity) > 0)
            {
                return true;
            }
            return false;
        }

        public static bool Update(CartonNumberingModel model)
        {
            var @ProductNo = new SqlParameter("@ProductNo", model.ProductNo);
            var @SizeNo = new SqlParameter("@SizeNo", model.SizeNo);
            var @GrossWeight = new SqlParameter("@GrossWeight", model.GrossWeight);
            if (db.ExecuteStoreCommand("spm_UpdateCartonNumbering  @ProductNo, @SizeNo, @GrossWeight", @ProductNo, @SizeNo, @GrossWeight) > 0)
            {
                return true;
            }
            return false;
        }

        public static bool Update_2(CartonNumberingModel model)
        {
            var @ProductNo = new SqlParameter("@ProductNo", model.ProductNo);
            var @SizeNo = new SqlParameter("@SizeNo", model.SizeNo);
            var @GrossWeight = new SqlParameter("@GrossWeight", model.GrossWeight);
            var @CartonNoBasic = new SqlParameter("@CartonNoBasic", model.CartonNoBasic);
            if (db.ExecuteStoreCommand("spm_UpdateCartonNumbering_2  @ProductNo, @SizeNo, @GrossWeight, @CartonNoBasic", @ProductNo, @SizeNo, @GrossWeight, @CartonNoBasic) > 0)
            {
                return true;
            }
            return false;
        }
    }
}
