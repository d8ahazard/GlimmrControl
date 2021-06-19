using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Tmds.MDns;

namespace Glimmr
{
    //Discover _http._tcp services via mDNS/Zeroconf and verify they are Glimmr devices by sending an API call
    class DeviceDiscovery
    {
        private static DeviceDiscovery Instance;
        private ServiceBrowser serviceBrowser;
        public event EventHandler<DeviceCreatedEventArgs> ValidDeviceFound;

        private DeviceDiscovery()
        {
            serviceBrowser = new ServiceBrowser();
            serviceBrowser.ServiceAdded += OnServiceAdded;
        }

        public void StartDiscovery()
        {
            serviceBrowser.StartBrowse("_glimmr._tcp");
        }

        public void StopDiscovery()
        {
            serviceBrowser.StopBrowse();
        }

        private async void OnServiceAdded(object sender, ServiceAnnouncementEventArgs e)
        {
            GlimmrDevice toAdd = new GlimmrDevice();
            foreach (var addr in e.Announcement.Addresses)
            {
                toAdd.NetworkAddress = addr.ToString(); break; //only get first address
            }
            toAdd.Name = e.Announcement.Hostname;
            toAdd.NameIsCustom = false;
            if (await toAdd.Refresh()) //check if the service is a valid Glimmr device
            {
                OnValidDeviceFound(new DeviceCreatedEventArgs(toAdd, false));
            }
        }

        public static DeviceDiscovery GetInstance()
        {
            if (Instance == null) Instance = new DeviceDiscovery();
            return Instance;
        }

        protected virtual void OnValidDeviceFound(DeviceCreatedEventArgs e)
        {
            ValidDeviceFound?.Invoke(this, e);
        }
    }
}
