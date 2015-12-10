using LightControl.Internal;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightControl.Internal
{
    public class ControlMessage
        : Message
    {
        public LampValue LampValue
        {
            get; set;
        }

        public ControlMessage(JObject obj) :
            base(obj)
        {
            Type = MessageType.Control;
            LampValue = new LampValue();
            LampValue.On = (bool)obj["On"];
            LampValue.Brightness = (uint)obj["Brightness"];
            LampValue.ColorTemp = (uint)obj["ColorTemp"];
            LampValue.Hue = (uint)obj["Hue"];
            LampValue.Saturation = (uint)obj["Saturation"];
        }
    }
}
