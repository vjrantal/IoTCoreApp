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
        public bool On
        {
            get; set;
        }

        public uint Brightness
        {
            get; set;
        }

        public uint ColorTemp
        {
            get; set;
        }

        public uint Hue
        {
            get; set;
        }

        public uint Saturation
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
            ctrlMsg.On = (bool)obj["On"];
            ctrlMsg.Brightness = (uint)obj["Brightness"];
            ctrlMsg.ColorTemp = (uint)obj["ColorTemp"];
            ctrlMsg.Hue = (uint)obj["Hue"];
            ctrlMsg.Saturation = (uint)obj["Saturation"];
            return ctrlMsg;
        }
    }
}
