// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SimulatedDeviceScooters
{
    using System;
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using SimulatedDeviceScooters.DeviceProperties;
    using SimulatedDeviceScooters.IoTHubDeviceClient;

    /// <summary>
    /// Simulates an IoT device which sends telemetry every second to the IoT Hub.
    /// </summary>
    internal class Program
    {
        private static readonly TransportType TransportType = TransportType.Mqtt;
        private static readonly TimeSpan TelemetryInterval = TimeSpan.FromSeconds(1); // To be changed
        private static readonly string DeviceId = "FirstScooter";

        private static async Task Main()
        {
            Console.WriteLine($"Simulated device application started for device with id: {DeviceId}.");

            // Get the connection string of the device
            string firstScooterConnectionStringSecretName = ConfigurationManager.AppSettings.Get("firstScooterConnectionStringSecretName");
            string firstScooterConnectionString = await ConnectionStringHandler.GetConnectionStringAsync(firstScooterConnectionStringSecretName);
            ConnectionStringHandler.ValidateIotHubConnectionString(firstScooterConnectionString);

            // Get the connection string of the IoT Hub
            string iotHubConnectionStringSecretName = ConfigurationManager.AppSettings.Get("iotHubConnectionStringSecretName");
            string iotHubConnectionString = await ConnectionStringHandler.GetConnectionStringAsync(iotHubConnectionStringSecretName);

            // Connect to the IoT Hub using the MQTT protocol
            var deviceClient = DeviceClient.CreateFromConnectionString(firstScooterConnectionString, TransportType);
            IDeviceClient customDeviceClient = new DeviceClientWrapper(deviceClient);

            // Set up a condition to quit the sample
            Console.WriteLine("Press control-C to exit.");

            // Set up a manager for reading / updating device properties.
            // For testing purposes we are using the InMemoryDevicePropertiesManager.
            var registryManager = Microsoft.Azure.Devices.RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            var devicePropertiesManager = new DeviceTwinsManager(deviceClient, DeviceId, registryManager);
            devicePropertiesManager.SetDirectMethodAsync();

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
                    await DeviceToCloudCommunication.SendDeviceToCloudTelemetryAsync(customDeviceClient, devicePropertiesManager);
                    await Task.Delay(TelemetryInterval);
                    devicePropertiesManager.SimulateBatteryAndLocationChanges();
                }
            }
        }
    }
}