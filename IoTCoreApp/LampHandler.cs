using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Windows.Devices.AllJoyn;
using org.allseen.LSF.LampState;

namespace IoTCoreApp
{
    class LampHandler
    {
        private LampStateConsumer consumer = null;
        private bool toggleState = false;

        public LampHandler()
        {
            AllJoynBusAttachment busAttachment = new AllJoynBusAttachment();
            LampStateWatcher watcher = new LampStateWatcher(busAttachment);
            watcher.Added += Watcher_Added;
            watcher.Start();
        }

        private async void toggle()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            await consumer.SetBrightnessAsync(getAbsoluteValue(50));
            toggleState = !toggleState;
            await consumer.SetOnOffAsync(toggleState);
            toggle();
        }

        private async void Watcher_Added(LampStateWatcher sender, AllJoynServiceInfo args)
        {
            LampStateJoinSessionResult joinSessionResult = await LampStateConsumer.JoinSessionAsync(args, sender);

            if (joinSessionResult.Status == AllJoynStatus.Ok)
            {
                Debug.WriteLine("LampState join session succeeded.");
                consumer = joinSessionResult.Consumer;
                consumer.Signals.LampStateChangedReceived += Signals_LampStateChangedReceived;
                consumer.SessionLost += Consumer_SessionLost;

                toggle();
            }
        }

        private void Consumer_SessionLost(LampStateConsumer sender, AllJoynSessionLostEventArgs args)
        {
            Debug.WriteLine("LampState session lost.");
        }

        private async void Signals_LampStateChangedReceived(LampStateSignals sender, LampStateLampStateChangedReceivedEventArgs args)
        {
            LampStateGetBrightnessResult brightnessResult = await consumer.GetBrightnessAsync();
            Debug.WriteLine("The brightness was " + getRelativeValue(brightnessResult.Brightness) + "%.");
        }

        private uint getAbsoluteValue(uint value)
        {
            return Convert.ToUInt32(value * ((0xFFFFFFFF - 1) / 100));
        }
        private uint getRelativeValue(uint value)
        {
            return Convert.ToUInt32(value / ((0xFFFFFFFF - 1) / 100));
        }
    }
}
