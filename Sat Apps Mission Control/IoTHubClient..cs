using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

using Microsoft.Azure.Devices.Client;
//using Newtonsoft.Json;
using System.Threading;
using Newtonsoft.Json;

namespace Sat_Apps_Mission_Control
{
    public sealed partial class MainPage : Page
    {
        int totalMessagesSent = 0;
        static DeviceClient deviceClient;

        static string iotHubUri = "<your iotHub URI here.azure-devices.net>";
        static string deviceKey = "<your device key here>";

        private void IoTHubSetup()
        {
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("myFirstDevice", deviceKey));

            //SendDeviceToCloudMessagesAsync();
        }

        private static async void SendDeviceToCloudMessagesAsync(SIKData dataset)
        {

            var messageString = JsonConvert.SerializeObject(dataset);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);


        }



    }


}
