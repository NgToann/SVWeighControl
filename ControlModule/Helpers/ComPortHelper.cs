﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;

namespace ControlModule.Helpers
{
    public class ComPortHelper
    {
        public static void WriteToPort(string port, string data)
        {
            try
            {
                SerialPort serialPort = new SerialPort(port);
                if (serialPort.IsOpen == false)
                {
                    serialPort.Open();
                }
                serialPort.Write(data);
                serialPort.Close();
            }
            catch (IOException) { }
        }
    }
}
