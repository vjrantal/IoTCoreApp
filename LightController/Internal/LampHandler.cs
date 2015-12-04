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
    public class LampHandler
    {
        public event EventHandler<string> NewEventReceived;

        private AllJoynBusAttachment _busAttachment;
        private LampStateWatcher _watcher;
        LampValue _defaulsWhenOff = null;
        LampValue _defaulsWhenOn = null;
        LampValue _lastValues = null;

        public IDictionary<string, LampStateConsumer> Consumers
        {
            get;
            protected set;
        }

        public LampHandler()
        {
            Consumers = new Dictionary<string, LampStateConsumer>();
            _busAttachment = new AllJoynBusAttachment();
            _watcher = new LampStateWatcher(_busAttachment);
            _watcher.Added += OnWatcherAdded;
            _watcher.Start();

            _defaulsWhenOff = new LampValue() { On = false, Brightness = 90, ColorTemp = 5000, Hue = 180, Saturation = 60 };
            _defaulsWhenOn = new LampValue() { On = true, Brightness = 90, ColorTemp = 5000, Hue = 180, Saturation = 60 };

        }

        public async Task StartLightsAsync()
        {
            foreach (var consumer in Consumers)
            {
                if (consumer.Value != null)
                {
                    SetValues(consumer.Value, _defaulsWhenOn);
                }
            }
        }

        public async Task StopLightsAsync()
        {
            foreach (var consumer in Consumers)
            {
                if (consumer.Value != null)
                {
                    SetValues(consumer.Value, _defaulsWhenOff);
                }
            }
        }

        public async Task ControlLights(ControlMessage controlMsg)
        {
            _lastValues = controlMsg.LampValue;

            foreach (var consumer in Consumers)
            {
                if (consumer.Value != null)
                {
                    SetValues(consumer.Value, controlMsg.LampValue);
                }
            }
        }

        private async Task SetValues(LampStateConsumer consumer, LampValue values)
        {
            consumer.SetBrightnessAsync(getAbsoluteValue(values.Brightness));
            consumer.SetColorTempAsync(getAbsoluteColorTemperatureValue(values.ColorTemp));
            consumer.SetHueAsync(getAbsoluteHueValue(values.Hue));
            consumer.SetOnOffAsync(values.On);
            consumer.SetSaturationAsync(values.Saturation);
        }


        private async void OnWatcherAdded(LampStateWatcher sender, AllJoynServiceInfo args)
        {
            LampStateJoinSessionResult joinSessionResult = await LampStateConsumer.JoinSessionAsync(args, sender);

            if (joinSessionResult.Status == AllJoynStatus.Ok)
            {
                if(NewEventReceived != null)
                {
                    NewEventReceived(this, "New Lamp joined. ID:" + args.UniqueName);
                }

                Debug.WriteLine("LampState join session succeeded.");

                Consumers.Add(args.UniqueName,joinSessionResult.Consumer);
                
                joinSessionResult.Consumer.SessionLost += OnConsumerSessionLost;

                if(_lastValues != null)
                {
                    SetValues(joinSessionResult.Consumer, _lastValues);
                }
                
            }
        }

        private void OnConsumerSessionLost(LampStateConsumer sender, AllJoynSessionLostEventArgs args)
        {
            
            string id = string.Empty;

            foreach(var consumer in Consumers)
            {
                if(consumer.Value == sender)
                {
                    id = consumer.Key;
                }
            }

            if(id != string.Empty)
            {
                Consumers[id].SessionLost -= OnConsumerSessionLost;
                Consumers[id] = null;
                Consumers.Remove(id);
            }

            Debug.WriteLine("LampState session lost. ID:" + id);
            if (NewEventReceived != null)
            {
                NewEventReceived(this, "LampState session lost. ID:" + id);
            }
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
