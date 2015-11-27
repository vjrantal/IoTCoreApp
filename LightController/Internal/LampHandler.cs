using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Windows.Devices.AllJoyn;
using org.allseen.LSF.LampState;
using LightControl.Internal;

namespace LightControl
{
    class LampHandler
    {
        private LampStateConsumer _consumer = null;
        private bool toggleState = false;

        public LampHandler()
        {
            AllJoynBusAttachment busAttachment = new AllJoynBusAttachment();
            LampStateWatcher watcher = new LampStateWatcher(busAttachment);
            watcher.Added += OnWatcherAdded;
            watcher.Start();
        }

        public  async void ControlLights(ControlMessage controlMsg)
        {
            if(_consumer != null)
            {
                await _consumer.SetBrightnessAsync(getAbsoluteValue(controlMsg.Brightness));
                await _consumer.SetColorTempAsync(getAbsoluteColorTemperatureValue(controlMsg.ColorTemp));
                await _consumer.SetHueAsync(getAbsoluteHueValue(controlMsg.Hue));
                await _consumer.SetOnOffAsync(controlMsg.On);
                await _consumer.SetSaturationAsync(getAbsoluteValue(controlMsg.Saturation));
            }
        }

        private async void OnWatcherAdded(LampStateWatcher sender, AllJoynServiceInfo args)
        {
            LampStateJoinSessionResult joinSessionResult = await LampStateConsumer.JoinSessionAsync(args, sender);

            if (joinSessionResult.Status == AllJoynStatus.Ok)
            {
                Debug.WriteLine("LampState join session succeeded.");
                _consumer = joinSessionResult.Consumer;
                _consumer.Signals.LampStateChangedReceived += OnSignalsLampStateChangedReceived;
                _consumer.SessionLost += OnConsumerSessionLost;
            }
        }

        private void OnConsumerSessionLost(LampStateConsumer sender, AllJoynSessionLostEventArgs args)
        {
            Debug.WriteLine("LampState session lost.");
        }

        private async void OnSignalsLampStateChangedReceived(LampStateSignals sender, LampStateLampStateChangedReceivedEventArgs args)
        {
            LampStateGetBrightnessResult brightnessResult = await _consumer.GetBrightnessAsync();
            Debug.WriteLine("The brightness was " + getRelativeValue(brightnessResult.Brightness) + "%.");
        }

        private uint getAbsoluteValue(uint value)
        {
            return Convert.ToUInt32(value * ((0xFFFFFFFF - 1) / 100));
        }
        private uint getAbsoluteHueValue(uint value)
        {
            return Convert.ToUInt32(value * ((0xFFFFFFFF - 1) / 360));
        }
        private uint getAbsoluteColorTemperatureValue(uint value)
        {
            return Convert.ToUInt32(value * ((0xFFFFFFFF - 1) / 10000));
        }

        private uint getRelativeValue(uint value)
        {
            return Convert.ToUInt32(value / ((0xFFFFFFFF - 1) / 100));
        }
    }
}
