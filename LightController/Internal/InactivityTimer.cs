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
        public static readonly int InactivityPeriod = 60 * 60 * 1000; //one hour
        public static readonly int LampChecker = 1 * 30 * 1000;  //every minute

        private Timer _timer = null;
        private int _period;

        /// <summary>
        /// The InactivityTimer factory
        /// </summary>
        static public InactivityTimer CreateTimer(int period)
        {
            return new InactivityTimer(period);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public InactivityTimer(int period)
        {
            _period = period;
        }

        public void ResetTimer()
        {
            if(_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
            
            // Create a timer with one hour interval.
            _timer = new Timer(new TimerCallback(TimerProc),null, _period, Timeout.Infinite);
 
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
