using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace LightControl.Internal
{
    public class NetworkAvailabilty
    {
        public event Func<object, bool, Task> NetworkAvailabilityChanged;

        private static NetworkAvailabilty _networkAvailabilty;
        private bool _lastAvailability = false;

        public static NetworkAvailabilty Instance
        {
            get { return _networkAvailabilty ?? (_networkAvailabilty = new NetworkAvailabilty()); }
        }

        public bool IsNetworkAvailable
        {
            get
            {
                return CheckInternetAccess();
            }
        }

        private NetworkAvailabilty()
        {
            _lastAvailability = CheckInternetAccess();
            NetworkInformation.NetworkStatusChanged += NetworkInformationOnNetworkStatusChanged;
        }

        private bool CheckInternetAccess()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();

            return (connectionProfile != null &&
                                 connectionProfile.GetNetworkConnectivityLevel() ==
                                 NetworkConnectivityLevel.InternetAccess);
        }

        private void NetworkInformationOnNetworkStatusChanged(object sender)
        {
            lock (this)
            {
 
                bool available = CheckInternetAccess();

                System.Diagnostics.Debug.WriteLine("NetworkInformationOnNetworkStatusChanged:" + available);

                if(NetworkAvailabilityChanged != null)
                {
                    Delegate[] invocationList = NetworkAvailabilityChanged.GetInvocationList();
                    Task[] handlerTasks = new Task[invocationList.Length];

                    for (int i = 0; i < invocationList.Length; i++)
                    {
                        handlerTasks[i] = ((Func<object, bool, Task>)invocationList[i])(this, available);
                    }

                    Task.WhenAll(handlerTasks).Wait();

                }

                _lastAvailability = available;
            }
        }

    }
}
