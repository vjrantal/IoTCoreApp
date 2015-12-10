using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightControl.Internal
{
    public class MessageParser
    {

        public static Message ParseMessage(string message)
        {
            Message returnMessage;

            try
            {
                JObject o = JObject.Parse(message);

                var type = (string)o["Type"];

                if (type.Equals("Control"))
                {
                    returnMessage = new ControlMessage(o);
                }
                else if (type.Equals("Start"))
                {
                    returnMessage = new StartMessage(o);
                }
                else if (type.Equals("Stop"))
                {
                    returnMessage = new StopMessage(o);
                }
                else
                {
                    returnMessage = new Message(null);
                }
            }
            catch (Exception e)
            {
                returnMessage = new Message(null);
            }

            return returnMessage;
        }
    }
}
