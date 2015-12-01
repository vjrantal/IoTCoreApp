using LightControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SimpleApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public LightController Controller
        {
            get
            {
                return LightController.Instance;
            }
        }

        public ObservableCollection<string> LastMessages
        {
            get; set;
        }

        public MainPage()
        {
            this.InitializeComponent();
            LastMessages = new ObservableCollection<string>();
        }

        override protected void OnNavigatedTo(NavigationEventArgs e)
        {
            InitializeController();
        }

        private async void InitializeController()
        {
            if(await Controller.InitializeAsync(true))
            {
                Controller.NewEventReceived += OnNewMessageReceived;
                LastMessages.Add("Connected..");
            }
        }

        private async void OnNewMessageReceived(object sender, string e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LastMessages.Add(e);
            });
        }


    }
}
