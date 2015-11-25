using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTHubClient
{
    /// <summary>
    /// The main interface to IoTHub.
    /// </summary>
    public class IotHubClient
    {
        private IotHubEngine _engine;

        /// <summary>
        /// Fired when new message is received from the IoTHub.
        /// </summary>
        public event EventHandler<string> NewMessageReceived
        {
            add
            {
                _engine.NewMessageReceived += value;
            }
            remove
            {
                _engine.NewMessageReceived -= value;
            }
        }

        /// <summary>
        /// The IotHubClient instance.
        /// </summary>
        static public IotHubClient Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new IotHubClient();
                }

                return _instance;
            }
        }
        static IotHubClient _instance = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        private IotHubClient()
        {
            _engine = new IotHubEngine();
        }

        /// <summary>
        /// Connects to IotHub and starts to listen incoming events.
        /// </summary>
        public Task<bool> ConnectAsync(IotHubSettings settings)
        {
            return _engine.ConnectAsync(settings);
        }

        /// <summary>
        /// Sends a message to IoTHub
        /// </summary>
        public async Task SendMessageAsync(string msg)
        {
            await _engine.SendMessageAsync(msg);
        }

        /// <summary>
        /// Disconnects from the IotHub
        /// </summary>
        public async Task DisconnectAsync()
        {
            await _engine.DisconnectAsync();
        }

    }
}
