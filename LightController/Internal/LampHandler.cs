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
        LampValue _defaulsBeforeBlink = null;
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

            _defaulsWhenOff = new LampValue() { On = false, Brightness = 100, ColorTemp = 9000, Hue = 260, Saturation = 100 };
            _defaulsWhenOn = new LampValue() { On = true, Brightness = 100, ColorTemp = 9000, Hue = 260, Saturation = 100 };
            _defaulsBeforeBlink = new LampValue() { On = true, Brightness = 100, ColorTemp = 9000, Hue = 260, Saturation = 100 };
            _lastValues = null;
        }

        public async Task StartLightsAsync()
        {
            foreach (var consumer in Consumers)
            {
                if (consumer.Value != null)
                {
                    await SetValuesAsync(consumer.Value, _defaulsWhenOn);
                }
            }
            _lastValues = _defaulsWhenOn;
        }

        public async Task BlinkLightsAsync()
        {
            foreach (var consumer in Consumers)
            {
                if (consumer.Value != null)
                {
                    await SetValuesAsync(consumer.Value, _defaulsBeforeBlink);
                }
            }

            foreach (var consumer in Consumers)
            {
                if (consumer.Value != null)
                {
                    await BlinkLightsAsync(consumer.Value);
                }
            }

            await Task.Delay(500);

            foreach (var consumer in Consumers)
            {
                if (consumer.Value != null)
                {
                    await BlinkLightsAsync(consumer.Value);
                }
            }

            await Task.Delay(500);

            if(_lastValues != null)
            {
                foreach (var consumer in Consumers)
                {
                    if (consumer.Value != null)
                    {
                        await SetValuesAsync(consumer.Value, _lastValues);
                    }
                }
            }
        }

        public async Task StopLightsAsync()
        {
            foreach (var consumer in Consumers)
            {
                if (consumer.Value != null)
                {
                    await SetValuesAsync(consumer.Value, _defaulsWhenOff);
                }
            }
            _lastValues = _defaulsWhenOff;
        }

        public async Task ControlLightsAsync(ControlMessage controlMsg)
        {
            foreach (var consumer in Consumers)
            {
                if (consumer.Value != null)
                {
                    await SetValuesAsync(consumer.Value, controlMsg.LampValue);
                }
            }
            _lastValues = controlMsg.LampValue;
        }

        private async Task SetValuesAsync(LampStateConsumer consumer, LampValue values)
        {
            if (_lastValues == null || _lastValues.On != values.On) await consumer.SetOnOffAsync(values.On);
            if (_lastValues == null || _lastValues.Brightness != values.Brightness) await consumer.SetBrightnessAsync(getAbsoluteValue(values.Brightness));
            if (_lastValues == null || _lastValues.ColorTemp != values.ColorTemp) await consumer.SetColorTempAsync(getAbsoluteColorTemperatureValue(values.ColorTemp));
            if (_lastValues == null || _lastValues.Hue != values.Hue) await consumer.SetHueAsync(getAbsoluteHueValue(values.Hue));
            if (_lastValues == null || _lastValues.Saturation != values.Saturation) await consumer.SetSaturationAsync(getAbsoluteValue(values.Saturation));
        }

        public async Task BlinkLightsAsync(LampStateConsumer consumer)
        {
            Dictionary<String, object> from = new Dictionary<string, object>();
            from.Add("Hue", 200);
            Dictionary<String, object> to = new Dictionary<string, object>();
            to.Add("Hue", 300);
            await consumer.ApplyPulseEffectAsync(from, to, 100, 200, 2, 0);
        }

        private async void OnWatcherAdded(LampStateWatcher sender, AllJoynServiceInfo args)
        {
            LampStateJoinSessionResult joinSessionResult = await LampStateConsumer.JoinSessionAsync(args, sender);

            if (joinSessionResult.Status == AllJoynStatus.Ok)
            {
                Consumers.Add(args.UniqueName,joinSessionResult.Consumer);
                joinSessionResult.Consumer.SessionLost += OnConsumerSessionLost;
                if(_lastValues != null)
                {
                    await SetValuesAsync(joinSessionResult.Consumer, _lastValues);
                }
                Debug.WriteLine("New Lamp joined. ID:" + args.UniqueName + " Lamp count: " + Consumers.Count);
                if (NewEventReceived != null)
                {
                    NewEventReceived(this, "New Lamp joined. ID:" + args.UniqueName + " Lamp count: " + Consumers.Count);
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
            return Convert.ToUInt32((double)value / 100.0 * UInt32.MaxValue);
        }

        private uint getAbsoluteHueValue(uint value)
        {
            return Convert.ToUInt32((double)value / 360.0 * UInt32.MaxValue);
        }

        private uint getAbsoluteColorTemperatureValue(uint value)
        {
            // Minimum color temperature for Lifx bulbs is 2500
            if (value < 2500) value = 2500;
            return Convert.ToUInt32((double)value / 9000.0 * UInt32.MaxValue);
        }
    }
}
