// <copyright file="DeviceTwinsManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SimulatedDeviceScooters.DeviceProperties
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements the <see cref="IDevicePropertiesManager"/> for the scenario where the device information are stored as Device Twins.
    /// </summary>
    internal class DeviceTwinsManager : IDevicePropertiesManager
    {
        private DeviceClient deviceClient;
        private string deviceId;
        private RegistryManager registryManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTwinsManager"/> class.
        /// </summary>
        /// <param name="deviceClient">The device client.</param>
        /// <param name="deviceId">The ID of the device.</param>
        /// <param name="registryManager">The registry manager.</param>
        public DeviceTwinsManager(DeviceClient deviceClient, string deviceId, RegistryManager registryManager)
        {
            this.registryManager = registryManager;
            this.deviceId = deviceId;
            this.deviceClient = deviceClient;

            this.InitializeDevice();
        }

        /// <inheritdoc/>
        public async Task<double> GetBatteryLevelAsync()
        {
            Twin twin = await this.registryManager.GetTwinAsync(this.deviceId);

            var reportedJsonProperties = twin.Properties.Reported.ToJson();
            var scooter = JsonConvert.DeserializeObject<TelemetryMessage>(reportedJsonProperties);
            return scooter.Battery;
        }

        /// <inheritdoc/>
        public string GetDeviceId()
        {
            return this.deviceId;
        }

        /// <inheritdoc/>
        public async Task<Status> GetDeviceStatusAsync()
        {
            Twin twin = await this.registryManager.GetTwinAsync(this.deviceId);

            var reportedJsonProperties = twin.Properties.Reported.ToJson();
            var scooter = JsonConvert.DeserializeObject<TelemetryMessage>(reportedJsonProperties);
            return scooter.Status;
        }

        /// <inheritdoc/>
        public async Task<double> GetLatitudeAsync()
        {
            Twin twin = await this.registryManager.GetTwinAsync(this.deviceId);

            var reportedJsonProperties = twin.Properties.Reported.ToJson();
            var scooter = JsonConvert.DeserializeObject<TelemetryMessage>(reportedJsonProperties);
            return scooter.Latitude;
        }

        /// <inheritdoc/>
        public async Task<double> GetLongitudeAsync()
        {
            Twin twin = await this.registryManager.GetTwinAsync(this.deviceId);

            var reportedJsonProperties = twin.Properties.Reported.ToJson();
            var scooter = JsonConvert.DeserializeObject<TelemetryMessage>(reportedJsonProperties);
            return scooter.Longitude;
        }

        /// <inheritdoc/>
        public void SetDirectMethodAsync()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Simulated device battery and location.
        /// </summary>
        public async void SimulateBatteryAndLocationChanges()
        {
            var status = await this.GetDeviceStatusAsync();
            double battery = await this.GetBatteryLevelAsync();
            double latitude = await this.GetLatitudeAsync();

            switch (status)
            {
                case Status.Rented:
                    battery *= 0.9;
                    latitude -= 0.0002;
                    this.UpdateDeviceTwinProperties(battery, latitude);
                    break;
                case Status.Recharging:
                    battery += 0.1;
                    this.UpdateDeviceTwinProperties(battery, latitude);
                    break;
            }
        }

        private void UpdateDeviceTwinProperties(double battery, double latitude)
        {
            TwinCollection reportedProperties = new TwinCollection();
            reportedProperties["battery"] = battery;
            reportedProperties["latitude"] = latitude;

            this.deviceClient.UpdateReportedPropertiesAsync(reportedProperties).Wait();
        }

        private void InitializeDevice()
        {
            TwinCollection reportedProperties = new TwinCollection();
            reportedProperties["battery"] = 100;
            reportedProperties["latitude"] = 47.192480;
            reportedProperties["longitude"] = 8.851230;
            reportedProperties["status"] = Status.Available;

            this.deviceClient.UpdateReportedPropertiesAsync(reportedProperties).Wait();
        }
    }
}
