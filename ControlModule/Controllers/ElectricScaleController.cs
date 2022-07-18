using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ControlModule.Entities;
using ControlModule.Models;

namespace ControlModule.Controllers
{
    public class ElectricScaleController
    {
        //

        static SaovietWeightControlEntities db = ConnectTo.SaovietWeightControlEntities();
        public static List<ElectricScaleSource> GetAll()
        {
            return db.ExecuteStoreQuery<ElectricScaleSource>("spm_SelectElectricScale").ToList();
        }
    }
}
