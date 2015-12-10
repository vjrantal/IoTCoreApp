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
 
        public StopMessage(JObject obj) :
            base(obj)
        {
            Type = MessageType.Stop;
        }

    }
}
