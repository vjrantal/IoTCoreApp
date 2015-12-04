using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightControl.Internal
{
    public class LampValue
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
    }
}
