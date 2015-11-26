using IoTHubClient;
using IoTHubClient.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
                await _iotHubClient.ConnectAsync(_settings);
                _iotHubClient.NewMessageReceived += OnNewMessageReceivedFromIotHub;
                
                InactivityTimer.Instance.ResetTimer();
                SendNewEvent("Initialized");
                return true;
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
            if(evt.Length > 50)
            {
                evt = evt.Substring(0, 50);
            }

            if(NewEventReceived != null)
            {
                NewEventReceived(this, evt);
            }
        }

    }
}
