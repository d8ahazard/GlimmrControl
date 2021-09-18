using Glimmr.Models;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xamarin.Forms;

namespace Glimmr
{
    enum DeviceStatus { Default, Unreachable, Error };

    //Data Model. Represents a Glimmr light with a network address, name, and some current light values.
    [XmlType("dev")]
    public class GlimmrDevice : INotifyPropertyChanged, IComparable
    {
        private string networkAddress = "10.41.0.1";                          //device IP (can also be hostname if applicable)
        private string name = "";                                               //device display name ("Server Description")
        private DeviceStatus status = DeviceStatus.Default;                     //Current connection status
        private bool isEnabled = true;
        private int deviceMode = 0;                     

        [XmlElement("url")]
        public string NetworkAddress
        {
            set
            {
                if (value == null || value.Length < 3) return; //More elaborate checking for URL syntax could be added here
                networkAddress = value;
            }
            get { return networkAddress; }
        }

        [XmlElement("name")]
        public string Name
        { 
            set
            {
                if (value == null || name.Equals(value)) return; //Make sure name is not set to null
                name = value;
                OnPropertyChanged("Name");
            }
            get { return name; }
        }

        internal DeviceStatus CurrentStatus
        {
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
            get { return status; }
        }

        [XmlElement("ncustom")]
        public bool NameIsCustom { get; set; } = true; //If the light name is custom, the name returned by the API response will be ignored

        [XmlElement("en")]
        public bool IsEnabled
        {
            set
            {
                isEnabled = value;
                OnPropertyChanged("Status");
                OnPropertyChanged("ListHeight");
                OnPropertyChanged("TextColor");
                OnPropertyChanged("IsEnabled");
            }
            get { return isEnabled; }
        }

        
        [XmlIgnore]
        public int DeviceMode
        {
            get { return deviceMode; }
            set {
                OnPropertyChanged("PowerColor");
                OnPropertyChanged("VideoColor");
                OnPropertyChanged("AudioColor");
                OnPropertyChanged("AvColor");
                OnPropertyChanged("AmbientColor");
                OnPropertyChanged("StreamColor");
                deviceMode = value; 
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [XmlIgnore]
        public Color PowerColor { get { return DeviceMode == 0 ? Color.FromHex("#666") : Color.FromHex("#222"); } } //button background color
        
        [XmlIgnore]
        public Color VideoColor { get { return DeviceMode == 1 ? Color.FromHex("#666") : Color.FromHex("#222"); } } //button background color

        [XmlIgnore]
        public Color AudioColor { get { return DeviceMode == 2 ? Color.FromHex("#666") : Color.FromHex("#222"); } } //button background color

        [XmlIgnore]
        public Color AmbientColor { get { return DeviceMode == 3 ? Color.FromHex("#666") : Color.FromHex("#222"); } } //button background color

        [XmlIgnore]
        public Color AvColor { get { return DeviceMode == 4 ? Color.FromHex("#666") : Color.FromHex("#222"); } } //button background color

        [XmlIgnore]
        public Color StreamColor { get { return DeviceMode == 5 ? Color.FromHex("#666") : Color.FromHex("#222"); } } //button background color

        [XmlIgnore]
        public string ListHeight { get { return isEnabled ? "-1" : "0"; } } //height of one view cell (set to 0 to hide device)

        [XmlIgnore]
        public string TextColor { get { return isEnabled ? "#FFF" : "#999"; } } //text color for modification page

        [XmlIgnore]
        public string Status //string containing IP and current status, second label in list viewcell
        {
            get
            {
                string statusText = "";
                if (IsEnabled)
                {
                    switch (status)
                    {
                        case DeviceStatus.Default: statusText = ""; break;
                        case DeviceStatus.Unreachable: statusText = " (Offline)"; break;
                        case DeviceStatus.Error: statusText = " (Error)"; break;
                    }
                }
                else
                {
                    statusText = " (Hidden)";
                }
                return string.Format("{0}{1}", networkAddress, statusText);
            }
        }

        //constructors
        public GlimmrDevice() { }

        public GlimmrDevice(string nA, string name)
        {
            NetworkAddress = nA;
            Name = name;
        }

        //member functions

        //send a call to this device's Glimmr HTTP API
        public async Task<bool> SendApiCall(string call, string p = "")
        {
            string url = "http://" + networkAddress;
            if (networkAddress.StartsWith("https://"))
            {
                url = networkAddress;
            }
            Debug.WriteLine("URL: " + url);
            string response = await DeviceHttpConnection.GetInstance().Send_Glimmr_API_Call(url, call + p);
            if (response == null)
            {
                Debug.WriteLine("NO RESPONSE.");
                CurrentStatus = DeviceStatus.Unreachable;
                return false;
            }

            if (response.Equals("err")) //404 or other non-success http status codes, indicates that target is not a Glimmr device
            {
                Debug.WriteLine("Response error.");
                CurrentStatus = DeviceStatus.Error;
                return false;
            }

            SystemData deviceResponse = JsonConvert.DeserializeObject<SystemData>(response);
            if (deviceResponse == null) //could not parse XML API response
            {
                Debug.WriteLine("Response is null.");
                CurrentStatus = DeviceStatus.Error;
                return false;
            } else
            {
                Debug.WriteLine("We have a valid response: " + JsonConvert.SerializeObject(deviceResponse));
            }

            CurrentStatus = DeviceStatus.Default; //the received response was valid

            if (!NameIsCustom) Name = deviceResponse.DeviceName;           

            DeviceMode = deviceResponse.DeviceMode;
            Debug.WriteLine("Returning true?");
            return true;
        }

        public async Task<bool> Refresh() //fetches updated values from Glimmr device
        {
            if (!IsEnabled) return false;
            return await SendApiCall("");
        }

        public int CompareTo(object comp) //compares devices in alphabetic order based on name
        {
            GlimmrDevice c = comp as GlimmrDevice;
            if (c == null || c.Name == null) return 1;
            int result = (name.CompareTo(c.name));
            if (result != 0) return result;
            return (networkAddress.CompareTo(c.networkAddress));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
