using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ControlModule.Entities;
using ControlModule.Models;
using System.Data.SqlClient;

namespace ControlModule.Controllers
{
    class FCatPOController
    {
        //
        private static SaovietWeightControlEntities db = ConnectTo.SaovietWeightControlEntities();
        public static FcatPOModel Get(string productNo)
        {
            var @ProductNo = new SqlParameter("@ProductNo", productNo);
            return db.ExecuteStoreQuery<FcatPOModel>("spm_SelectFcatPOByPO @ProductNo", @ProductNo).FirstOrDefault();
        }
        public static FcatPOModel GetByPOOrGBS(string productNo)
        {
            var @ProductNo = new SqlParameter("@ProductNo", productNo);
            return db.ExecuteStoreQuery<FcatPOModel>("spm_SelectFcatPOByPOByGBS @ProductNo", @ProductNo).FirstOrDefault();
        }

        public static FcatPOModel GetLike(string productNo)
        {
            var @ProductNo = new SqlParameter("@ProductNo", productNo);
            return db.ExecuteStoreQuery<FcatPOModel>("spm_SelectFcatPOLikePO @ProductNo", @ProductNo).FirstOrDefault();
        }
        //
        public static FcatPOModel GetToLoading(string productNo)
        {
            var @ProductNo = new SqlParameter("@ProductNo", productNo);
            return db.ExecuteStoreQuery<FcatPOModel>("spm_SelectFcatPOToLoading @ProductNo", @ProductNo).FirstOrDefault();
        }
        public static bool Upload(FcatPOModel model, bool isAddNew, bool isDelete)
        {
            var @ProductNo = new SqlParameter("@ProductNo", model.ProductNo);
            var @GBSNo = new SqlParameter("@GBSNo", model.GBSNo);
            var @StatusCurrent = new SqlParameter("@StatusCurrent", model.StatusCurrent);
            var @IsAddNew = new SqlParameter("@IsAddNew", isAddNew);
            var @IsDelete = new SqlParameter("@IsDelete", isDelete);


            if (db.ExecuteStoreCommand("spm_UploadFcatPO    @ProductNo, @GBSNo, @StatusCurrent, @IsAddNew, @IsDelete", 
                                                            @ProductNo, @GBSNo, @StatusCurrent, @IsAddNew, @IsDelete) > 0)
            {
                return true;
            }
            return false;
        }
    }
}
