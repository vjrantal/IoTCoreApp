using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTHubClient
{
    /// <summary>
    /// IoTHub and device related settings
    /// </summary>
    /// 
    public class IotHubSettings
    {
        /// <summary>
        /// Iot Hub Host address
        /// </summary>
        public string Host
        {
            get; set;
        }

        /// <summary>
        /// Iot Hub Host port
        /// </summary>
        public int Port
        {
            get; set;
        }

        /// <summary>
        /// Device ID
        /// </summary>
        public string DeviceId
        {
            get; set;
        }

        /// <summary>
        /// Device Key
        /// </summary>
        public string DeviceKey
        {
            get; set;
        }

        /// <summary>
        /// Tries to load settings from Documents\IotHubSettings.txt
        /// Json structure is following:
        /// {
        /// "Host": "Iot Hub Host address",
        /// "Port": Iot Hub Host port (5671),
        /// "DeviceId": "Device ID",
        /// "DeviceKey": "Device Key"
        /// }
        /// </summary>
        public async Task<bool> LoadSettingsFromFileAsync()
        {
            try
            { 
                var stream = await Windows.Storage.KnownFolders.DocumentsLibrary.OpenStreamForReadAsync("IotHubSettings.txt");

                string text;
                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                    JObject o = JObject.Parse(text);

                    Host = (string)o["Host"];
                    Port = (int)o["Port"];
                    DeviceId = (string)o["DeviceId"];
                    DeviceKey = (string)o["DeviceKey"];

                    if (Host != string.Empty && Port != 0 && DeviceId != string.Empty && DeviceKey != string.Empty)
                        return true;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Can't load settings:");
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            return false;
        }
    }
}
