// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SimulatedDeviceScooters
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;

    /// <summary>
    /// Simulates an IoT device which sends telemetry every second to the IoT Hub.
    /// </summary>
    internal class Program
    {
        private static readonly TransportType TransportType = TransportType.Mqtt;
        private static readonly TimeSpan TelemetryInterval = TimeSpan.FromSeconds(1);

        private static readonly bool DefaultAvailability = true;

        private static readonly string RentScooterDirectMethod = "RentScooter";
        private static readonly string RechargeScooterDirectMethod = "RechargeScooter";

        private static DeviceClient deviceClient;

        private static bool scooterIsAvailable = DefaultAvailability;
        private static bool scooterIsRecharging = false;

        private static async Task Main()
        {
            Console.WriteLine("Simulated device started.");

            string connectionString = await ConnectionStringHandler.GetConnectionStringAsync("FirstScooterConnectionString");
            ConnectionStringHandler.ValidateConnectionString(connectionString);

            // Connect to the IoT Hub using the MQTT protocol
            deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType);

            // Create a handler for the direct method call - To Rent a scooter
            await deviceClient.SetMethodHandlerAsync(RentScooterDirectMethod, RentScooter, null);

            // Create a handler for the direct method call - To Rent a scooter
            await deviceClient.SetMethodHandlerAsync(RechargeScooterDirectMethod, RechargeScooter, null);

            // Set up a condition to quit the sample
            Console.WriteLine("Press control-C to exit.");
            using (CancellationTokenSource cancellationTokenSource = new ())
            {
                using CancellationTokenSource cts = cancellationTokenSource;
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    cts.Cancel();
                    Console.WriteLine("Exiting...");
                };

                while (!cts.IsCancellationRequested)
                {
                    // Sent telemetry to the IoT Hub
                    await DeviceToCloudCommunication.SendDeviceToCloudTelemetryAsync(deviceClient, scooterIsAvailable, scooterIsRecharging, cts.Token);
                }
            }
        }

        private static Task<MethodResponse> RentScooter(MethodRequest methodRequest, object userContext)
        {
            var deviceId = Encoding.UTF8.GetString(methodRequest.Data);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Telemetry interval set to {TelemetryInterval}");

            scooterIsRecharging = false;
            scooterIsAvailable = !scooterIsAvailable;

            // Acknowlege the direct method call with a 200 success message
            string result = $"{{\"result\":\"Executed direct method: {methodRequest.Name} for device {deviceId}\"}}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private static Task<MethodResponse> RechargeScooter(MethodRequest methodRequest, object userContext)
        {
            var deviceId = Encoding.UTF8.GetString(methodRequest.Data);
            scooterIsRecharging = !scooterIsRecharging;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Telemetry interval set to {TelemetryInterval}");
            Console.ResetColor();

            scooterIsAvailable = false;

            // Acknowlege the direct method call with a 200 success message
            string result = $"{{\"result\":\"Executed direct method: {methodRequest.Name} for device {deviceId}\"}}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }
    }
}