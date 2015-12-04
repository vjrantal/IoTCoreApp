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

        public Message()
        {
            Type = MessageType.Unknown;
        }
    }
}
