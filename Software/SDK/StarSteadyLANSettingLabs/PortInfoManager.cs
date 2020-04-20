using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using StarMicronics.StarIO;

namespace StarSteadyLANSettingLabs
{
    public class PortInfoManager
    {
        public PortInfoManager(PortInfo portInfo)
        {
            PortInfo = portInfo;
        }

        public PortInfo PortInfo { get; set; }

        public string ModelName
        {
            get
            {
                return PortInfo.ModelName;
            }
        }

        public string Description
        {
            get
            {
                string description = "";

                if (!PortInfo.MacAddress.Equals(""))
                {
                    description = PortInfo.PortName + " (" + PortInfo.MacAddress + ")";
                }
                else if (!PortInfo.USBSerialNumber.Equals(""))
                {
                    description = PortInfo.PortName + " (" + PortInfo.USBSerialNumber + ")";
                }
                else
                {
                    description = PortInfo.PortName;
                }


                return description;
            }
        }

        public static PortInfoManager[] CreatePortInfoManager(PortInfo[] portInfoArray)
        {
            List<PortInfoManager> managerList = new List<PortInfoManager>();

            foreach (PortInfo portInfo in portInfoArray)
            {
                PortInfoManager manager = new PortInfoManager(portInfo);
                managerList.Add(manager);
            }

            return managerList.ToArray();
        }
    }
}
