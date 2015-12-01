using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

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
        /// Tries to load the settings first from the app local folder and then from application package. 
        /// Changing IotHubSettings.txt content in application local folder makes it possible
        /// to overwrite the app default settings
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
                bool localFileExists = true;
                string fileText;

                var file = await  Windows.Storage.ApplicationData.Current.LocalFolder.TryGetItemAsync("IotHubSettings.txt");

                if(file == null)
                {
                    localFileExists = false;
                    file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///Settings/IotHubSettings.txt"));
                }
 
                var stream = await ((StorageFile)file).OpenStreamForReadAsync();

                
                using (StreamReader reader = new StreamReader(stream))
                {
                    fileText = reader.ReadToEnd();
                    JObject o = JObject.Parse(fileText);

                    Host = (string)o["Host"];
                    Port = (int)o["Port"];
                    DeviceId = (string)o["DeviceId"];
                    DeviceKey = (string)o["DeviceKey"];

                    if(!localFileExists)
                    {
                        //Copy file from application Uri to local Folder
                        await CopySettingsFileAsync(fileText);
                    }

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

        public async Task CopySettingsFileAsync(string settingsText)
        {
            Windows.Storage.StorageFile settingsFile = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("IotHubSettings.txt", CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(settingsFile, settingsText);
        }
    }
}
