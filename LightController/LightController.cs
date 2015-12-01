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

        /// <summary>
        /// Constructor
        /// </summary>
        private LightController()
        {
            _iotHubClient = IotHubClient.Instance;
            _networkAvailability = NetworkAvailabilty.Instance;
            _inactivityTimer = InactivityTimer.Instance;
            _settings = new IotHubSettings();
            _lampHandler = new LampHandler();
            _networkAvailability.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
            _inactivityTimer.InactivityPeriodExceeded += OnInactivityPeriodExceeded;
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
                    InactivityTimer.Instance.ResetTimer();
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
            SendNewEvent(e);
            InactivityTimer.Instance.ResetTimer();

            Message msg = MessageParser.ParseMessage(e);

            switch(msg.Type)
            {
                case Message.MessageType.Control:
                {
                    _lampHandler.ControlLights((ControlMessage)msg);
                }
                break;
            }
        }

        /// <summary>
        /// Event handler for network change events. 
        /// </summary>
        private async void OnNetworkAvailabilityChanged(object sender, bool e)
        {
            //Disconnects iothub client when the network is down
            if (e)
            {
                await _iotHubClient.ConnectAsync(_settings);
            }
            else
            {
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
