using IoTHubClient;
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
        public ObservableCollection<string> Messages
        {
            get; set;
        }

        public MainPage()
        {
            this.InitializeComponent();
            Messages = new ObservableCollection<string>();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            IotHubSettings settings = new IotHubSettings();
            if(await settings.LoadSettingsFromFileAsync())
            {
                await IotHubClient.Instance.ConnectAsync(settings);
                IotHubClient.Instance.NewMessageReceived += OnNewMessageReceived;
                Messages.Add("Connected succesfully");

            } else

            {
                System.Diagnostics.Debug.WriteLine("Failed to load settings..");
            }
        }

        private async void OnNewMessageReceived(object sender, string e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Messages.Add(e);
            });
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            await IotHubClient.Instance.SendMessageAsync("hello world");
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            await IotHubClient.Instance.DisconnectAsync();
        }
    }
}
