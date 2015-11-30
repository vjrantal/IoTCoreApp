using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Diagnostics;
using Windows.ApplicationModel.Background;

using LightControl;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace IoTCoreApp
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            LightController Controller = LightController.Instance;
            Controller.InitializeAsync(false);
            // On purpose not calling deferral.Complete() so that
            // the app keeps running.
        }
    }
}
