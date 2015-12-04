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

        public ControlMessage() :
            base()
        {
            Type = MessageType.Control;
        }

        public static ControlMessage Create(JObject obj)
        {
            ControlMessage ctrlMsg = new ControlMessage();
            ctrlMsg.LampValue.On = (bool)obj["On"];
            ctrlMsg.LampValue.Brightness = (uint)obj["Brightness"];
            ctrlMsg.LampValue.ColorTemp = (uint)obj["ColorTemp"];
            ctrlMsg.LampValue.Hue = (uint)obj["Hue"];
            ctrlMsg.LampValue.Saturation = (uint)obj["Saturation"];
            return ctrlMsg;
        }
    }
}
