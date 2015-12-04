using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightControl.Internal
{
    public class StartMessage
        : Message
    {
 
        public StartMessage() :
            base()
        {
            Type = MessageType.Start;
        }

        public static StartMessage Create(JObject obj)
        {
            StartMessage msg = new StartMessage();
            return msg;
        }
    }
}
