using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightControl
{
    /// <summary>
    /// Sends an event when there has been no activity for certain time period
    /// </summary>
    class InactivityTimer
    {
        public event EventHandler InactivityPeriodExceeded;

        private static Timer _timer = null;
        private static readonly int InactivityPeriod = 60 * 60 * 1000; //one hour

        /// <summary>
        /// The InactivityTimer instance.
        /// </summary>
        static public InactivityTimer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new InactivityTimer();
                }

                return _instance;
            }
        }
        static InactivityTimer _instance = null;

        public void ResetTimer()
        {
            if(_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
            
            // Create a timer with one hour interval.
            _timer = new Timer(new TimerCallback(TimerProc),null,InactivityPeriod,Timeout.Infinite);
 
        }

        private void TimerProc(object state)
        {
            if(InactivityPeriodExceeded != null)
            {
                InactivityPeriodExceeded(this, null);
            }

            ResetTimer();
        }

    }
}
