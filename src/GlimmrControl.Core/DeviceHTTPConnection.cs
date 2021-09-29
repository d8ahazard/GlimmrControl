using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Glimmr
{
    class DeviceHttpConnection
    {
        private static DeviceHttpConnection Instance;

        private HttpClient Client;

        private DeviceHttpConnection ()
        {
            Client = new HttpClient();
            Client.Timeout = TimeSpan.FromSeconds(5);
        }
        
        public static DeviceHttpConnection GetInstance()
        {
            if (Instance == null) Instance = new DeviceHttpConnection();
            return Instance;
        }

        public async Task<string> Send_Glimmr_API_Call(string DeviceURI, string API_Call)
        {
            try
            {
                string apiCommand = "/api/DreamData"; //Glimmr http API URI
                if (API_Call != null && API_Call.Length > 0)
                {
                    apiCommand += API_Call;
                }
                Debug.WriteLine("API Command: " + DeviceURI + apiCommand);
                var result = await Client.GetAsync(DeviceURI + apiCommand);
                if (result.IsSuccessStatusCode)
                {
                    return await result.Content.ReadAsStringAsync();
                } else //404 or other non-success status codes, indicates that target is not Glimmr device
                {
                    return "err";
                }
            } catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e.Message);
                return null; //time-out or other connection error
            }
        }
    }
}
