using IoTHubClient;
using IoTHubClient.Internal;
using LightControl.Internal;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace LightControl
{
    public class LightController
    {
        /// <summary>
        /// Fired when some new events happens in the controller.
        /// </summary>
        public event EventHandler<string> NewEventReceived;

        private IotHubClient _iotHubClient;
        private NetworkAvailabilty _networkAvailability;
        private IotHubSettings _settings;
        private InactivityTimer _inactivityTimer;
        private InactivityTimer _lampCheckTimer;

        private bool _isHeaded;
        private LampHandler _lampHandler;

        /// <summary>
        /// The LightController instance.
        /// </summary>
        static public LightController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LightController();
                }

                return _instance;
            }
        }
        static LightController _instance = null;

        public LampHandler LampHandler
        {
            get { return _lampHandler; }
            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private LightController()
        {
            _iotHubClient = IotHubClient.Instance;
            _networkAvailability = NetworkAvailabilty.Instance;
            _inactivityTimer = InactivityTimer.CreateTimer(InactivityTimer.InactivityPeriod);
            _lampCheckTimer = InactivityTimer.CreateTimer(InactivityTimer.LampChecker);
            _settings = new IotHubSettings();
            _lampHandler = new LampHandler();

            _networkAvailability.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
            _inactivityTimer.InactivityPeriodExceeded += OnInactivityPeriodExceeded;
            _lampCheckTimer.InactivityPeriodExceeded += OnLampCheckPeriodExceeded;
        }

        /// <summary>
        /// Initializes controller.
        /// </summary>
        public async Task<bool> InitializeAsync(bool isHeaded)
        {
            _isHeaded = isHeaded;

            if (await _settings.LoadSettingsFromFileAsync())
            {
                if(await _iotHubClient.ConnectAsync(_settings))
                {
                    _iotHubClient.NewMessageReceived += OnNewMessageReceivedFromIotHub;
                    _inactivityTimer.ResetTimer();
                    _lampCheckTimer.ResetTimer();
                    SendNewEvent("Initialized");
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Event handler for new messages from the IoT Hub
        /// </summary>
        private void OnNewMessageReceivedFromIotHub(object sender, string e)
        {
            System.Diagnostics.Debug.WriteLine("OnNewMessageReceivedFromIotHub start");

            SendNewEvent(e);
            _inactivityTimer.ResetTimer();
            try
            {
                Message msg = MessageParser.ParseMessage(e);

                switch (msg.Type)
                {

                    case Message.MessageType.Control:
                        {

                            _lampHandler.ControlLightsAsync((ControlMessage)msg).Wait();
                            break;
                        }

                    case Message.MessageType.Stop:
                        {
                            _lampHandler.StopLightsAsync().Wait();
                            break;
                        }

                    case Message.MessageType.Start:
                        {
                            _lampHandler.StartLightsAsync().Wait();
                            break;
                        }
                }
            }
             catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("OnNewMessageReceivedFromIotHub exception:");
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            System.Diagnostics.Debug.WriteLine("OnNewMessageReceivedFromIotHub end");
        }

        /// <summary>
        /// Event handler for network change events. 
        /// </summary>
        private async Task OnNetworkAvailabilityChanged(object sender, bool e)
        {
            
            //Disconnects iothub client when the network is down
            if (e)
            {
                SendNewEvent("Network Connected");
                await _iotHubClient.ConnectAsync(_settings);
            }
            else
            {
                SendNewEvent("Network Disconnected");
                await _iotHubClient.DisconnectAsync();
            }
        }

        /// <summary>
        /// Event handler for inactivity notifications
        /// </summary>
        private async void OnInactivityPeriodExceeded(object sender, EventArgs e)
        {
            SendNewEvent("OnInactivityPeriodExceeded");
            //For now we just disconnect and connect again
            await _iotHubClient.DisconnectAsync();
            await _iotHubClient.ConnectAsync(_settings);
        }

        /// <summary>
        /// Event handler for Lamp status check
        /// </summary>
        private void OnLampCheckPeriodExceeded(object sender, EventArgs e)
        {
            _lampCheckTimer.ResetTimer();

            SendNewEvent("Number of connected lamps:" + _lampHandler.Consumers.Count);

            if (_lampHandler.Consumers.Count != 2)
            {
                //_lampHandler = null;
                //_lampHandler = new LampHandler();
            }
        }

        /// <summary>
        /// Fires event when new event happens in the controller. 
        /// </summary>
        private void SendNewEvent(string evt)
        {
            if(evt.Length > 80)
            {
                evt = evt.Substring(0, 80) + "...";
            }

            if(NewEventReceived != null)
            {
                NewEventReceived(this, evt);
            }
        }

    }
}
