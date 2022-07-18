using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControlModule.Entities
{
    class ConnectTo
    {
        public static SaovietWeightControlEntities SaovietWeightControlEntities()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SaovietWeightControlEntities"].ConnectionString;
            return new SaovietWeightControlEntities(string.Format(connectionString, "sa", "sa@123456"));
        }
    }
}
