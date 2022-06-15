// <copyright file="DeviceToCloudCommunication.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SimulatedDeviceScooters
{
    using System;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using SimulatedDeviceScooters.DeviceProperties;
    using SimulatedDeviceScooters.IoTHubDeviceClient;

    /// <summary>
    /// This class is responsible with sending telemetry to the IoT Hub.
    /// </summary>
    public class DeviceToCloudCommunication
    {
        private static readonly string BatteryAlert = "batteryAlert";
        private static readonly double MinimumBatteryLevel = 20;

        /// <summary>
        /// Sends device to cloud telemetry.
        /// </summary>
        /// <param name="deviceClient">The Device Client.</param>
        /// <param name="devicePropertiesHandler">The handler for device properties.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public static async Task SendDeviceToCloudTelemetryAsync(IDeviceClient deviceClient, IDevicePropertiesManager devicePropertiesHandler)
        {
            var battery = await devicePropertiesHandler.GetBatteryLevelAsync();

            var deviceInformation = new TelemetryMessage
            {
                DeviceId = devicePropertiesHandler.GetDeviceId(),
                Status = await devicePropertiesHandler.GetDeviceStatusAsync(),
                Battery = battery,
                Latitude = await devicePropertiesHandler.GetLatitudeAsync(),
                Longitude = await devicePropertiesHandler.GetLongitudeAsync(),
            };

            // Create JSON Message
            string telemetryMessageBody = JsonSerializer.Serialize(deviceInformation);
            using Message telemetryMessage = new (Encoding.ASCII.GetBytes(telemetryMessageBody))
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
            };

            // Add a custom application property to the message.
            // An IoT hub can filter on these properties without access to the message body.
            telemetryMessage.Properties.Add(BatteryAlert, (battery < MinimumBatteryLevel) ? "true" : "false");

            // Send the telemetry message
            await deviceClient.SendEventAsync(telemetryMessage);
            Console.WriteLine($"{DateTime.Now} > Sending message: {telemetryMessageBody}");
        }
    }
}
