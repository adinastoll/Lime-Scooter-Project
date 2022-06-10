// <copyright file="DeviceTwinManagement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace DeviceManagement.DevicePropertiesManager
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared;

    /// <inheritdoc/>
    public class DeviceTwinManagement : IDeviceTwinManagement
    {
        /// <inheritdoc/>
        public async Task<MethodResponse> UpdateDeviceTwinStatusPropertyAsync(string deviceId, Status newStatus)
        {
            // Get the connection string of the device
            var connectionStringManager = await ConnectionStringManager.GetInstance();

            // Get the connection string of the IoT Hub
            string iotHubConnectionString = connectionStringManager.IotHubConnectionString;

            string firstScooterConnectionString = connectionStringManager.DeviceConnectionString;

            var registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            var deviceClient = DeviceClient.CreateFromConnectionString(firstScooterConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);

            var query = registryManager.CreateQuery($"SELECT * FROM devices where deviceId={deviceId}");
            var device = await query.GetNextAsTwinAsync();

            string result;
            try
            {
                if (device != null)
                {
                    TwinCollection reportedProperties = new TwinCollection();
                    reportedProperties["status"] = newStatus;

                    // Update the reported property on the scooter
                    deviceClient.UpdateReportedPropertiesAsync(reportedProperties).Wait();

                    // Acknowlege the update was invoked with a 200 success message
                    result = $"{{\"result\":\"Updated the status to {newStatus} for device {deviceId}\"}}";
                    return new MethodResponse(Encoding.UTF8.GetBytes(result), 200);
                }
                else
                {
                    // Could not find the device with the given ID, return 404
                    result = $"{{\"result\":\"Tried to update the status to {newStatus} on {deviceId}. Device not found.\"}}";
                    return new MethodResponse(Encoding.UTF8.GetBytes(result), 404);
                }
            }
            catch (Exception ex)
            {
                result = $"{{\"result\":\"Tried to update the status to {newStatus} on {deviceId}. An internal error occured.\"}}";
                return new MethodResponse(Encoding.UTF8.GetBytes(ex.Message), 505);
            }
        }
    }
}
