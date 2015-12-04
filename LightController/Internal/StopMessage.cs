using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightControl.Internal
{
    public class StopMessage
        : Message
    {
 
        public StopMessage() :
            base()
        {
            Type = MessageType.Stop;
        }

        public static StopMessage Create(JObject obj)
        {
            StopMessage msg = new StopMessage();
            return msg;
        }
    }
}
