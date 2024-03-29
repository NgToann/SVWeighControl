﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControlModule.Helpers
{
    class ElectricScaleProfileHelper
    {
        public static List<ElectricScaleProfile> ElectricScaleProfileList()
        {
            List<ElectricScaleProfile> electricScaleProfileList = new List<ElectricScaleProfile>();
            ElectricScaleProfile electricScaleProfile1 = new ElectricScaleProfile
            {
                ProfileId = 1,
                BaudRate = 2400,
                StabilityKey = 'S',
                BeginKey = 'S',
                EndKey = 'g',
                SubjectMail = "SECTION A",
            };
            electricScaleProfileList.Add(electricScaleProfile1);
            ElectricScaleProfile electricScaleProfile2 = new ElectricScaleProfile
            {
                ProfileId = 2,
                BaudRate = 9600,
                StabilityKey = 'B',
                BeginKey = '=',
                EndKey = '\r',
                SubjectMail = "SECTION Anex",
            };
            ElectricScaleProfile electricScaleProfile3 = new ElectricScaleProfile
            {
                ProfileId = 3,
                BaudRate = 9600,
                StabilityKey = 'B',
                BeginKey = '=',
                EndKey = '\r',
                SubjectMail = "Mobile Scale",
            };
            electricScaleProfileList.Add(electricScaleProfile3);

            return electricScaleProfileList;
        }

        public static string ConvertWeight(string dataReceived, ElectricScaleProfile electricScaleProfile)
        {
            char[] charNumberArray = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', '-' };
            if (dataReceived.Contains(electricScaleProfile.StabilityKey) == true)
            {
                if (dataReceived.Contains(electricScaleProfile.BeginKey) == true && dataReceived.Contains(electricScaleProfile.EndKey) == true)
                {
                    dataReceived = dataReceived.Substring(dataReceived.IndexOf(electricScaleProfile.BeginKey));
                    if (dataReceived.Contains(electricScaleProfile.StabilityKey) == true && dataReceived.Contains(electricScaleProfile.EndKey) == true)
                    {
                        dataReceived = dataReceived.Substring(0, dataReceived.IndexOf(electricScaleProfile.EndKey) + 1);
                        if (dataReceived.Contains(electricScaleProfile.StabilityKey) == true)
                        {
                            return new String(dataReceived.Where(d => charNumberArray.Contains(d)).ToArray());
                        }
                    }
                }
            }
            return null;
        }
    }

    class ElectricScaleProfile
    {        
        public int ProfileId { get; set; }
        public int BaudRate { get; set; }
        public char StabilityKey { get; set; }
        public char BeginKey { get; set; }
        public char EndKey { get; set; }
        public string SubjectMail { get; set; }
        public string Location { get; set; }
    }
}
