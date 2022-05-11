using Microsoft.Azure.Devices.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SimulatedDeviceScooters
{
    class Program
    {
        private static DeviceClient deviceClient;
        private static readonly TransportType transportType = TransportType.Mqtt;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id FirstScooter --output table
        private static string connectionString = "";
        private static double defaultBatteryLevel = 100;
        private static bool defaultAvailability = true;

        private static double currentBatteryLevel = defaultBatteryLevel;
        private static bool currentScooterAvailability = defaultAvailability;

        private static string rentScooterDirectMethod = "RentScooter";
        private static string batteryAlert = "batteryAlert";
        private static double minimumBatteryLevel = 20;
        private static TimeSpan telemetryInterval = TimeSpan.FromSeconds(1); // Seconds

        static async Task Main(string[] args)
        {
            Console.WriteLine("Simulated device started.");

            ValidateConnectionString(connectionString);

            // Connect to the IoT Hub using the MQTT protocol
            deviceClient = DeviceClient.CreateFromConnectionString(connectionString, transportType);

            // Create a handler for the direct method call - To Rent a scooter
            await deviceClient.SetMethodHandlerAsync(rentScooterDirectMethod, RentScooter, null);

            // Set up a condition to quit the sample
            Console.WriteLine("Press control-C to exit.");
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            // Sent telemetry to the IoT Hub
            await SendDeviceToCloudTelemetryAsync(cts.Token);
        }

        private static async Task SendDeviceToCloudTelemetryAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                currentBatteryLevel = currentBatteryLevel * 0.9;

                // Create JSON Message
                string telemetryMessageBody = JsonSerializer.Serialize(
                    new
                    {
                        battery = currentBatteryLevel,
                        status = currentScooterAvailability
                    });

                using var telemetryMessage = new Message(Encoding.ASCII.GetBytes(telemetryMessageBody))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8"
                };

                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                telemetryMessage.Properties.Add(batteryAlert, (currentBatteryLevel < minimumBatteryLevel) ? "true" : "false");

                // Send the telemetry message
                await deviceClient.SendEventAsync(telemetryMessage);
                Console.WriteLine($"{ DateTime.Now} > Sending message: {telemetryMessageBody}");

                try
                {
                    await Task.Delay(telemetryInterval, ct);
                }
                catch (TaskCanceledException)
                {
                    // User canceled
                    return;
                }

            }
        }

        private static void ValidateConnectionString(string hubConnectionString)
        {
            try
            {
                _ = IotHubConnectionStringBuilder.Create(hubConnectionString);
            }
            catch (Exception)
            {
                Console.WriteLine("An IoT Hub connection string needs to be specified, " +
                    "please set the environment variable \"IOTHUB_CONNECTION_STRING\" " +
                    "or pass in \"-s | --HubConnectionString\" through command line.");
                Environment.Exit(1);
            }
        }

        private static Task<MethodResponse> RentScooter(MethodRequest methodRequest, object userContext)
        {
            var deviceId = Encoding.UTF8.GetString(methodRequest.Data);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Telemetry interval set to {telemetryInterval}");
            Console.ResetColor();

            currentScooterAvailability = !currentScooterAvailability;

            // Acknowlege the direct method call with a 200 success message
            string result = $"{{\"result\":\"Executed direct method: {methodRequest.Name} for device {deviceId}\"}}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }
    }
}
