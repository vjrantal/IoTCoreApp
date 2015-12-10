using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightControl.Internal
{

    public class Message
    {
        public enum MessageType { Control, Unknown, Start, Stop };

        public MessageType Type
        {
            get; set;
        }

        public DateTime Timestamp
        {
            get; set;
        }

        public Message(JObject obj)
        {
            Type = MessageType.Unknown;
            if(obj != null)
            {
                JToken value;
                if (obj.TryGetValue("Timestamp", out value))
                {
                    Timestamp = new DateTime((long)value);
                } 
                else
                {
                    Timestamp = DateTime.UtcNow;
                }
            }
            else
            {
                Timestamp = DateTime.UtcNow;
            }
            
        }
    }
}
