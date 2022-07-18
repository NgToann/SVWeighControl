using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ControlModule.Entities;
using System.Data.SqlClient;
using ControlModule.Models;

namespace ControlModule.Controllers
{
    class PackingController
    {
        static SaovietWeightControlEntities db = ConnectTo.SaovietWeightControlEntities();
        public static List<PackingModel> Get(string productNo)
        {
            var @ProductNo = new SqlParameter("@ProductNo", productNo);
            return db.ExecuteStoreQuery<PackingModel>("spm_SelectPackingByProductNo @ProductNo", @ProductNo).ToList();
        }

        public static bool CreateUpdate(PackingModel model)
        {
            var @ProductNo              = new SqlParameter("@ProductNo", model.ProductNo);
            var @SizeNo                 = new SqlParameter("@SizeNo", model.SizeNo);
            var @CartonNo               = new SqlParameter("@CartonNo", model.CartonNo);
            var @ActualWeight           = new SqlParameter("@ActualWeight", model.ActualWeight);
            var @DifferencePercent      = new SqlParameter("@DifferencePercent", model.DifferencePercent);
            var @IsPass                 = new SqlParameter("@IsPass", model.IsPass);
            var @IsFirstPass            = new SqlParameter("@IsFirstPass", model.IsFirstPass);
            var @CreatedAccount         = new SqlParameter("@CreatedAccount", model.CreatedAccount);
            var @Barcode                = new SqlParameter("@Barcode", model.Barcode);

            if (db.ExecuteStoreCommand("spm_InsertPacking_3 @ProductNo, @SizeNo, @CartonNo, @ActualWeight, @DifferencePercent, @IsPass, @IsFirstPass, @CreatedAccount, @Barcode", 
                                                            @ProductNo, @SizeNo, @CartonNo, @ActualWeight, @DifferencePercent, @IsPass, @IsFirstPass, @CreatedAccount, @Barcode) > 0)
            {
                return true;
            }
            return false;
        }

        //
        public static bool Delete(string productNo)
        {
            var @ProductNo = new SqlParameter("@ProductNo", productNo);
            if (db.ExecuteStoreCommand("spm_DeletePackingByPO @ProductNo",
                                                            @ProductNo) > 0)
            {
                return true;
            }
            return false;
        }
    }
}
