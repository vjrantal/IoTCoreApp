using Amqp;
using Amqp.Framing;
using IoTHubClient.Internal;
using IoTHubClient.Internal.Misc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace IoTHubClient
{
    class IotHubEngine
    {
        public event EventHandler<string> NewMessageReceived;

        private AMQPClient _amqpClient;
        private bool _connecting = false;

        public IotHubEngine()
        {
            Logger.InitLocal("IotHubEngine.txt");
            Logger.PrintOut = true;
            Trace.TraceLevel = TraceLevel.Verbose;
            Trace.TraceListener = (f, a) => Logger.Instance.Write(string.Format(f, a),false);
        }

        public async Task<bool> ConnectAsync(IotHubSettings settings = null)
        {
            bool connected = false;
            int retryCount = 0;

            await DisconnectAsync();

            while (retryCount < 5 && !connected)
            {
                System.Diagnostics.Debug.WriteLine("Try to connect.. Attempts:" + retryCount);
                _amqpClient = new AMQPClient();
                connected = await _amqpClient.ConnectAsync(settings);
                retryCount++;
            }

            if (connected)
            {
                _amqpClient.NewMessageReceived += OnNewMessageReceived;
                System.Diagnostics.Debug.WriteLine("Connection successful");
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Connection failed");
                return false;
            }
        }

        private void OnNewMessageReceived(object sender, string msg)
        {
            if(NewMessageReceived != null)
            {
                NewMessageReceived(this, msg);
            }
        }

        public Task<bool> SendMessageAsync(string msg)
        {
            return Task.Run(() =>
            {
                if (_amqpClient != null)
                {
                    return _amqpClient.SendMsg(msg);
                }
                else
                {
                    return false;
                }
            });
        }

        public async Task DisconnectAsync()
        {
            if (_amqpClient != null)
            {
                await _amqpClient.DisconnectAsync();
                _amqpClient.NewMessageReceived -= OnNewMessageReceived;
                _amqpClient = null;
            }
        }
    }
}
