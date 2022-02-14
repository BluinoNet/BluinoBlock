using BluinoNet;
using BluinoNet.Modules;
using nanoFramework.SignalR.Client;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using Windows.Devices.WiFi;

namespace BluinoBlockDevice
{
    public class Program
    {
        static ESP32StarterKit board;

        const string SignalRHubUrl = "http://192.168.1.1/devicehub";
        public static void Main()
        {
            SetupBoard();
            SetupAndConnectNetwork();
            StartSignalRBlock();

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }


        static void SetupBoard()
        {
            board = new ESP32StarterKit();
            board.SetupLed(ESP32Pins.IO23, ESP32Pins.IO02, ESP32Pins.IO04, ESP32Pins.IO05);
            board.SetupButton(ESP32Pins.IO12, ESP32Pins.IO14);
            board.SetupLedRgb(ESP32Pins.IO02, ESP32Pins.IO04, ESP32Pins.IO05);
            board.SetupDisplay();
            board.SetupBMP180();
            board.SetupBuzzer(ESP32Pins.IO23);
            board.SetupRelay(ESP32Pins.IO22);
            board.SetupLightSensor(ESP32Pins.IO36, 0, ESP32ADCs.ADC0);
            board.SetupPIR(ESP32Pins.IO22);
            board.SetupMpu6050();
            board.SetupPotentiometer(ESP32Pins.IO36, 0, ESP32ADCs.ADC0);
            board.SetupServo(ESP32Pins.IO05);
            //MicroSd.SetupMicroSd(ESP32Pins.IO13);
            //board.SetupHCSR04(ESP32Pins.IO13, ESP32Pins.IO12);



        }
        static void StartSignalRBlock()
        {  //setup connection
            var options = new HubConnectionOptions() { Reconnect = true };
            HubConnection hubConnection = new HubConnection(SignalRHubUrl, options: options);

            hubConnection.Closed += HubConnection_Closed;

            hubConnection.On("ReceiveMessage", new Type[] { typeof(string), typeof(string), typeof(string) }, (sender, args) =>
            {
                var MethodName = (string)args[0];
                var Param1 = (string)args[1];
                var Param2 = (string)args[2];

                Console.WriteLine($"{MethodName} : {Param1}, {Param2}");
                switch (MethodName)
                {
                    case COMMANDS.LED:
                        switch (int.Parse(Param1))
                        {
                            case 1:
                                if(Param2 == bool.TrueString)
                                    board.BoardLed1.TurnOn();
                                else
                                    board.BoardLed1.TurnOff();
                                break;
                            case 2:
                                if (Param2 == bool.TrueString)
                                    board.BoardLed2.TurnOn();
                                else
                                    board.BoardLed2.TurnOff();
                                break;
                            case 3:
                                if (Param2 == bool.TrueString)
                                    board.BoardLed3.TurnOn();
                                else
                                    board.BoardLed3.TurnOff();
                                break;
                            case 4:
                                if (Param2 == bool.TrueString)
                                    board.BoardLed4.TurnOn();
                                else
                                    board.BoardLed4.TurnOff();
                                break;
                        }
                        break; 
                    case COMMANDS.RGBLED:
                        break;
                    case COMMANDS.DISPLAY:
                        break;
                    case COMMANDS.BUTTON:
                        break;
                    case COMMANDS.DISTANCE:
                        break;
                    case COMMANDS.SERVO:
                        break;
                    case COMMANDS.TEMP:
                        break;
                    case COMMANDS.RELAY:
                        break;
                    case COMMANDS.ACCEL:
                        break;
                    case COMMANDS.BUZZER:
                        break;
                    case COMMANDS.POTENSIO:
                        break;
                    case COMMANDS.PRESSURE:
                        break;
                    case COMMANDS.LIGHT:
                        break;
                    case COMMANDS.HUMID:
                        break;
                    case COMMANDS.PIR:
                        break;
                  
                }
            
            });

            //start connection
            hubConnection.Start();

            AsyncResult dashboardClientConnected = hubConnection.InvokeCoreAsync("CheckConnection", typeof(bool), new object[] { }, -1);

            int seconds = 0;

            while (!dashboardClientConnected.Completed)
            {
                Debug.WriteLine($"Waited {seconds} for client to open webapp");
                seconds++;
                Thread.Sleep(1000);
            }

            if ((bool)dashboardClientConnected.Value)
            {
                hubConnection.SendCore("ReportStatus", new object[] { "Client Connected" });

                int count = 0;
                while (hubConnection.State == HubConnectionState.Connected)
                {
                    hubConnection.InvokeCore("SendMessage", null, new object[] { "counter", $"pesan {count}", DateTime.UtcNow.ToString() });
                    count++;
                    Thread.Sleep(1000);
                }
            }
            else
            {
                hubConnection.Stop("client failed to connect");
            }
           
           
        }

        private static void HubConnection_Closed(object sender, SignalrEventMessageArgs message)
        {
            Debug.WriteLine($"closed received with message: {message.Message}");
        }
        private static void SetupAndConnectNetwork()
        {
            // Get the first WiFI Adapter
            var wifiAdapter = WiFiAdapter.FindAllAdapters()[0];

            // Begin network scan.
            wifiAdapter.ScanAsync();

            // While networks are being scan, continue on configuration. If networks were set previously, 
            // board may already be auto-connected, so reconnection is not even needed.
            var wiFiConfiguration = Wireless80211Configuration.GetAllWireless80211Configurations()[0];
            var ipAddress = NetworkInterface.GetAllNetworkInterfaces()[0].IPv4Address;
            var needToConnect = string.IsNullOrEmpty(ipAddress) || (ipAddress == "0.0.0.0");
            while (needToConnect)
            {
                foreach (var network in wifiAdapter.NetworkReport.AvailableNetworks)
                {
                    // Show all networks found
                    Debug.WriteLine($"Net SSID :{network.Ssid},  BSSID : {network.Bsid},  rssi : {network.NetworkRssiInDecibelMilliwatts},  signal : {network.SignalBars}");

                    // If its our Network then try to connect
                    if (network.Ssid == wiFiConfiguration.Ssid)
                    {

                        var result = wifiAdapter.Connect(network, WiFiReconnectionKind.Automatic, wiFiConfiguration.Password);

                        if (result.ConnectionStatus == WiFiConnectionStatus.Success)
                        {
                            Debug.WriteLine($"Connected to Wifi network {network.Ssid}.");
                            needToConnect = false;
                        }
                        else
                        {
                            Debug.WriteLine($"Error {result.ConnectionStatus} connecting to Wifi network {network.Ssid}.");
                        }
                    }
                }

                Thread.Sleep(10000);
            }

            ipAddress = NetworkInterface.GetAllNetworkInterfaces()[0].IPv4Address;
            Debug.WriteLine($"Connected to Wifi network with IP address {ipAddress}");

        }
    }

    public static class COMMANDS
    {
        public const string LED = "LED";
        public const string BUZZER = "BUZZER";
        public const string TEMP = "TEMP";
        public const string HUMID = "HUMID";
        public const string DISTANCE = "DISTANCE";
        public const string LIGHT = "LIGHT";
        public const string SERVO = "SERVO";
        public const string ACCEL = "ACCEL";
        public const string PIR = "PIR";
        public const string BUTTON = "BUTTON";
        public const string RELAY = "RELAY";
        public const string RGBLED = "RGBLED";
        public const string PRESSURE = "PRESSURE";
        public const string POTENSIO = "POTENSIO";
        public const string DISPLAY = "DISPLAY";

    }
}
