﻿// <copyright file="DeviceToCloudCommunication.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SimulatedDeviceScooters
{
    using System;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;

    /// <summary>
    /// This class is responsible with the device to cloud communication.
    /// </summary>
    internal class DeviceToCloudCommunication
    {
        private static readonly TimeSpan TelemetryInterval = TimeSpan.FromSeconds(1);
        private static readonly string BatteryAlert = "batteryAlert";
        private static readonly double MinimumBatteryLevel = 20;

        /// <summary>
        /// Sends device to cloud telemetry.
        /// </summary>
        /// <param name="deviceClient">The Device Client.</param>
        /// <param name="scooterIsAvailable">The current availability of the device.</param>
        /// <param name="scooterIsRecharging">Whether the device is currently recharging or not.</param>
        /// <param name="currentBatteryLevel">The current battery level of the device.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public static async Task SendDeviceToCloudTelemetryAsync(DeviceClient deviceClient, bool scooterIsAvailable, bool scooterIsRecharging, double currentBatteryLevel, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (!scooterIsAvailable)
                {
                    currentBatteryLevel *= 0.9;
                }

                if (scooterIsRecharging)
                {
                    currentBatteryLevel += currentBatteryLevel * 0.1;
                }

                // Create JSON Message
                string telemetryMessageBody = JsonSerializer.Serialize(
                    new
                    {
                        battery = currentBatteryLevel,
                        status = scooterIsAvailable,
                    });

                using Message telemetryMessage = new (Encoding.ASCII.GetBytes(telemetryMessageBody))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
                };

                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                telemetryMessage.Properties.Add(BatteryAlert, (currentBatteryLevel < MinimumBatteryLevel) ? "true" : "false");

                // Send the telemetry message
                await deviceClient.SendEventAsync(telemetryMessage);
                Console.WriteLine($"{DateTime.Now} > Sending message: {telemetryMessageBody}");

                try
                {
                    await Task.Delay(TelemetryInterval, ct);
                }
                catch (TaskCanceledException)
                {
                    // User canceled
                    return;
                }
            }
        }
    }
}