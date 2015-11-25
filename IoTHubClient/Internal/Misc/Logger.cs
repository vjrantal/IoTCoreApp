using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Threading;
using Windows.Web.Http;


namespace IoTHubClient.Internal.Misc
{
    public sealed class Logger
    {
        private static Logger _instance;
        private List<string> _logEntries = new List<string>();
        private List<string> _logEntriesBuffer = new List<string>();
        private StorageFile _logFile = null;
        private Semaphore _semaphoreForLogs = new Semaphore(1, 1); //Is used to synchronize read/writes to _logEntries 
        AutoResetEvent _asyncWaiter = new AutoResetEvent(true); //Ensures that writeEntriesToFileAsync/sendToServerAsync are running only one by time
        bool _local = false;
        string _name;
        string _url;

        /// <summary>
        /// Retursn a Logger instance
        /// </summary>
        public static Logger Instance
        {
            get
            {
                return _instance;
            }
        }
        public static bool PrintOut
        {
            get; set;
        }

        /// <summary>
        /// Initializes logger for local usage. This will store logged data to application local folder. 
        /// </summary>
        /// 
        public static void InitLocal(string fileName)
        {
            if (_instance == null)
                _instance = new Logger(true, fileName);
        }

        /// <summary>
        /// Initializes logger for remote use. Data will be sent to server.
        /// </summary>
        public static void InitRemote(string name, string url)
        {
            if (_instance == null)
                _instance = new Logger(false, name, url);
        }

        /// <summary>
        /// Writes a log entry. The entry will be stored into local folder or remote server 
        /// </summary>
        /// <param name="dataSection">A log entry.</param>
        public void Write(string logEntry, bool dontPrint = false)
        {
            string now = DateTime.Now.ToString();
            if (_semaphoreForLogs.WaitOne(1000))
            {
                _logEntries.Add(now + ":" + logEntry);
                _semaphoreForLogs.Release();
            }
            if (PrintOut && !dontPrint)
            {
                System.Diagnostics.Debug.WriteLine(now + ":" + logEntry);
            }
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        private Logger(bool local, string name, string url = "")
        {
            _local = local;
            _name = name;
            _url = url;

            int seconds = 5; //To local storage we save every second

            if (!local) //To remote storage we save every 10 seconds
                seconds = 10;

            TimeSpan period = TimeSpan.FromSeconds(seconds);

            ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                if (_local)
                    await writeEntriesToFileAsync();
                else
                    await sendToServerAsync();
            }, period);
        }

        /// <summary>
        /// If local logging is enabled this function is called every second to store data to application local storage.
        /// </summary>
        private async Task writeEntriesToFileAsync()
        {
            try
            {
                _asyncWaiter.WaitOne();
                if (_logFile == null)
                {
                    _logFile = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(_name, CreationCollisionOption.OpenIfExists);
                }

                if (_semaphoreForLogs.WaitOne(1000))
                {
                    _logEntriesBuffer.AddRange(_logEntries);
                    _logEntries.Clear();
                    _semaphoreForLogs.Release();
                }

                await Windows.Storage.FileIO.AppendLinesAsync(_logFile, _logEntriesBuffer);
                _logEntriesBuffer.Clear();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                _asyncWaiter.Set();
            }
        }

        /// <summary>
        /// If remote logging is enabled this function is called every 10 second. And it will send logs to the server.
        /// </summary>
        private async Task sendToServerAsync()
        {
            try
            {
                _asyncWaiter.WaitOne();
                if (_semaphoreForLogs.WaitOne(1000))
                {
                    _logEntriesBuffer.AddRange(_logEntries);
                    _logEntries.Clear();
                    _semaphoreForLogs.Release();
                }

                if (_logEntriesBuffer.Count > 0)
                {
                    var uri = new Uri(_url);
                    HttpClient client = new HttpClient();
                    HttpResponseMessage response;
                    var data = string.Empty;
                    foreach (string obj in _logEntriesBuffer)
                    {
                        data += obj;
                    }
                    var keyValues = new List<KeyValuePair<string, string>>();
                    keyValues.Add(new KeyValuePair<string, string>("data", data));
                    keyValues.Add(new KeyValuePair<string, string>("name", _name));
                    HttpFormUrlEncodedContent formContent = new HttpFormUrlEncodedContent(keyValues);

                    response = await client.PostAsync(uri, formContent);
                    _logEntriesBuffer.Clear();
                }
 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                _asyncWaiter.Set();
            }
        }


    }
}
