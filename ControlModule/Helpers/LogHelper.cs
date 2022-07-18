using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ControlModule.Helpers
{
    class LogHelper
    {
        public static void CreateLog(string log)
        {
            log = string.Format("{0:yyyy-MM-dd hh:mm:ss} {1}{2}", DateTime.Now, log, Environment.NewLine);
            File.AppendAllText(@"Log.txt", log, Encoding.UTF8);
        }
    }
}
