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
                    returnMessage = ControlMessage.Create(o);
                }
                else
                {
                    returnMessage = new Message();
                }
            }
            catch (Exception e)
            {
                returnMessage = new Message();
            }

            return returnMessage;
        }
    }
}
