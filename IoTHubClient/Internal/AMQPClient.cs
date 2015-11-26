using Amqp;
using Amqp.Framing;
using IoTHubClient.Internal.Misc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

//Part of the functionality is taken from these OS projects: 
//https://github.com/ppatierno/codesamples/blob/master/IoTHubAmqp/IoTHubAmqp/Program.cs
//https://github.com/Azure/azure-iot-sdks/blob/75107fa7b0e614a83dfcd81aff4727541d81fa28/csharp/service/Microsoft.Azure.Devices/Common/Security/SharedAccessSignatureBuilder.cs

namespace IoTHubClient.Internal
{
    public class AMQPClient
    {
        public event EventHandler<string> NewMessageReceived;

        private Address _address = null;
        private Connection _connection = null;
        private Session _session = null;
        private ReceiverLink _receiveLink = null;
        private IotHubSettings _settings;
        private static readonly DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public bool Connect(IotHubSettings settings)
        {
            try
            {
                if(settings != null)
                {
                    _settings = settings;
                }
                    
                _address = new Address(_settings.Host, _settings.Port, null, null);
                _connection = new Connection(_address);

                string audience = Fx.Format("{0}/devices/{1}", _settings.Host, _settings.DeviceId);
                string resourceUri = Fx.Format("{0}/devices/{1}", _settings.Host, _settings.DeviceId);

                string sasToken = GetSharedAccessSignature(null, _settings.DeviceKey, resourceUri, new TimeSpan(1, 0, 0));

                bool cbs = PutCbsToken(_connection, _settings.Host, sasToken, audience);

                if (cbs)
                {
                    _session = new Session(_connection);
                    StartToListen();
                    return true;
                };
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Connect Failed:" + e.Message);
            }

            return false;
        }

        public void Disconnect()
        {
            if(_receiveLink != null)
            {
                _receiveLink.Close(1);
                _receiveLink = null;
            }
            if(_session != null)
            {
                _session.Close(1);
                _session = null;
            }
            if(_connection != null)
            {
                _connection.Close(1);
                _connection = null;
            }
        }

        public bool SendMsg(string msg)
        {
            try
            {
                string entity = Fx.Format("/devices/{0}/messages/events", _settings.DeviceId);

                SenderLink senderLink = new SenderLink(_session, "sender-link", entity);

                var messageValue = Encoding.UTF8.GetBytes(msg);
                Message message = new Message()
                {
                    BodySection = new Data() { Binary = messageValue }
                   
                };

                senderLink.Send(message);
                senderLink.Close(1);

                return true;

            } catch (Exception ex)
            {
                Logger.Instance.Write("Sending failed:" + ex.Message);
            }
            return false;
        }

        private void StartToListen()
        {
            string entity = Fx.Format("/devices/{0}/messages/deviceBound", _settings.DeviceId);

            _receiveLink = new ReceiverLink(_session, "receive-link", entity);

            _receiveLink.Start(5, OnMessageCallback);
        }

        private string GetSharedAccessSignature(string keyName, string sharedAccessKey, string resource, TimeSpan tokenTimeToLive)
        {
            // http://msdn.microsoft.com/en-us/library/azure/dn170477.aspx
            // the canonical Uri scheme is http because the token is not amqp specific
            // signature is computed from joined encoded request Uri string and expiry string

            string expiry = BuildExpiresOn(TimeSpan.FromHours(12));
            string encodedUri = WebUtility.UrlEncode(resource);
            string sig = Sign(encodedUri + "\n" + expiry, sharedAccessKey);

            if (keyName != null)
            {
                return string.Format(
                    "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}",
                    encodedUri,
                    WebUtility.UrlEncode(sig),
                    WebUtility.UrlEncode(expiry),
                    WebUtility.UrlEncode(keyName));
            }
            else
            {
                return string.Format(
                    "SharedAccessSignature sr={0}&sig={1}&se={2}",
                    encodedUri,
                    WebUtility.UrlEncode(sig),
                    WebUtility.UrlEncode(expiry));
            }
        }

        private bool PutCbsToken(Connection connection, string host, string shareAccessSignature, string audience)
        {
            bool result = true;
            Session session = new Session(connection);

            string cbsReplyToAddress = "cbs-reply-to";
            var cbsSender = new SenderLink(session, "cbs-sender", "$cbs");
            var cbsReceiver = new ReceiverLink(session, cbsReplyToAddress, "$cbs");
            // construct the put-token message
            var request = new Message(shareAccessSignature);
            request.Properties = new Properties();
            request.Properties.MessageId = Guid.NewGuid().ToString();
            request.Properties.ReplyTo = cbsReplyToAddress;
            request.ApplicationProperties = new ApplicationProperties();
            request.ApplicationProperties["operation"] = "put-token";
            request.ApplicationProperties["type"] = "azure-devices.net:sastoken";
            request.ApplicationProperties["name"] = audience;
            cbsSender.Send(request,2000);

            // receive the response
            var response = cbsReceiver.Receive(2000);
            if (response == null || response.Properties == null || response.ApplicationProperties == null)
            {
                result = false;
            }
            else
            {
                int statusCode = (int)response.ApplicationProperties["status-code"];
                string statusCodeDescription = (string)response.ApplicationProperties["status-description"];

                if (statusCode != (int)202 && statusCode != (int)200) // !Accepted && !OK
                {
                    result = false;
                }
            }
            
            cbsSender.Close(1000);
            cbsReceiver.Close(1000);
            session.Close(1000);
            session = null;
            return result;
        }


        private string BuildExpiresOn(TimeSpan timeToLive)
        {
            DateTime expiresOn = DateTime.UtcNow.Add(timeToLive);
            TimeSpan secondsFromBaseTime = expiresOn.Subtract(EpochTime);
            long seconds = Convert.ToInt64(secondsFromBaseTime.TotalSeconds, CultureInfo.InvariantCulture);
            return Convert.ToString(seconds, CultureInfo.InvariantCulture);
        }

        private string Sign(string requestString, string key)
        {
            var algo = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
            var keyMaterial = Convert.FromBase64String(key).AsBuffer();
            var hash = algo.CreateHash(keyMaterial);
            hash.Append(CryptographicBuffer.ConvertStringToBinary(requestString, BinaryStringEncoding.Utf8));

            var sign = CryptographicBuffer.EncodeToBase64String(hash.GetValueAndReset());
            return sign;
        }

        private void OnMessageCallback(ReceiverLink receiver, Message message)
        {
            byte[] b = (byte[])message.Body;
            string msg = Encoding.UTF8.GetString(b);

            System.Diagnostics.Debug.WriteLine(msg);
            Logger.Instance.Write("Incoming:" + msg);
            // Do something with the value
            receiver.Accept(message);
            receiver.SetCredit(5);

            if (String.Equals(msg, "ping") )
            {
                Task.Run(() =>
                {
                    Logger.Instance.Write("sending ping back..");
                    SendMsg("ping");
                });
            } else
            {
                if(NewMessageReceived != null)
                {
                    Task.Run(() =>
                    {
                        NewMessageReceived(this, msg);
                    });
                }
            }
        }
    }
}
