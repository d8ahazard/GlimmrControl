using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;

namespace Glimmr
{
    class NetUtility
    {
        //we just assume we are connected to embedded AP if:
        //1. the IP is in 10.41.0.0/24 subnet
        //2. the device IP is between 2 and 5 (ESP8266 DHCP range) 
        public static bool IsConnectedToGlimmrAP()
        {
            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            var ip = addrInfo.Address;
                            string ips = ip.ToString();

                            if (ips.StartsWith("10.41.0."))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}