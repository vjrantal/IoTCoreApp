using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightControl.Internal
{
    public class BlinkMessage
        : Message
    {
        public BlinkMessage(JObject obj) :
            base(obj)
        {
            Type = MessageType.Blink;
        }
    }
}
