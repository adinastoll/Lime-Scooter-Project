// <copyright file="InMemoryDevicePropertiesManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SimulatedDeviceScooters.DeviceProperties
{
    using System;
    using System.Configuration;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;

    /// <summary>
    /// Implements the <see cref="IDevicePropertiesManager"/> for the scenario where the properties are stored in memory.
    /// </summary>
    public class InMemoryDevicePropertiesManager : IDevicePropertiesManager
    {
        private readonly string rechargeScooterMethodName = "RechargeScooter";
        private readonly string rentScooterMethodName = "RentScooter";
        private readonly string returnScooterMethodName = "ReturnScooter";
        private readonly string takeScooterOfflineMethodName = "TakeScooterOffline";

        private string deviceId;
        private double battery;
        private Status status;
        private double latitude;
        private double longitude;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDevicePropertiesManager"/> class.
        /// </summary>
        /// <param name="deviceId">The ID of the device.</param>
        /// <param name="defaultBatteryLevel">The battery level of the scooter.</param>
        /// <param name="defaultStatus">The status of the scooter.</param>
        /// <param name="defaultLatitude">The latitude of the scooter's position.</param>
        /// <param name="defaultLongitude">The longitude of the scooter's position.</param>
        public InMemoryDevicePropertiesManager(string deviceId = "firstScooter", double defaultBatteryLevel = 100, Status defaultStatus = Status.Available, double defaultLatitude = 47.192480, double defaultLongitude = 8.851230)
        {
            this.deviceId = deviceId;
            this.battery = defaultBatteryLevel;
            this.status = defaultStatus;
            this.latitude = defaultLatitude;
            this.longitude = defaultLongitude;
        }

        /// <inheritdoc/>
        public Task<double> GetBatteryLevelAsync()
        {
            switch (this.status)
            {
                case Status.Rented:
                    this.battery *= 0.9;
                    break;
                case Status.Recharging:
                    this.battery += 0.5;
                    break;
            }

            return Task.FromResult(this.battery);
        }

        /// <inheritdoc/>
        public string GetDeviceId()
        {
            return this.deviceId;
        }

        /// <inheritdoc/>
        public Task<Status> GetDeviceStatusAsync()
        {
            return Task.FromResult(this.status);
        }

        /// <inheritdoc/>
        public Task<double> GetLatitudeAsync()
        {
            if (this.status == Status.Rented)
            {
                this.latitude -= 0.0002;
            }

            return Task.FromResult(this.latitude);
        }

        /// <inheritdoc/>
        public Task<double> GetLongitudeAsync()
        {
            return Task.FromResult(this.longitude);
        }

        /// <inheritdoc/>
        public async void SetDirectMethodAsync()
        {
            // Get the connection string of the device
            string firstScooterConnectionStringSecretName = ConfigurationManager.AppSettings.Get("firstScooterConnectionStringSecretName");
            string firstScooterConnectionString = await ConnectionStringHandler.GetConnectionStringAsync(firstScooterConnectionStringSecretName);
            ConnectionStringHandler.ValidateIotHubConnectionString(firstScooterConnectionString);

            // Connect to the IoT Hub using the MQTT protocol
            var deviceClient = DeviceClient.CreateFromConnectionString(firstScooterConnectionString, TransportType.Mqtt);

            // Create a handler for the direct method RentScooter
            await deviceClient.SetMethodHandlerAsync(this.rentScooterMethodName, this.RentScooter, null);

            // Create a handler for the direct method RechargeScooter
            await deviceClient.SetMethodHandlerAsync(this.rechargeScooterMethodName, this.RechargeScooter, null);

            // Create a handler for the direct method Return
            await deviceClient.SetMethodHandlerAsync(this.returnScooterMethodName, this.ReturnScooter, null);

            // Create a handler for a direct method to take the scooter offline
            await deviceClient.SetMethodHandlerAsync(this.takeScooterOfflineMethodName, this.TakeScooterOffline, null);
        }

        private async Task<MethodResponse> RechargeScooter(MethodRequest methodRequest, object userContext)
        {
            return await this.UpdateDeviceStatus(this.deviceId, methodRequest.Name, Status.Recharging);
        }

        private async Task<MethodResponse> RentScooter(MethodRequest methodRequest, object userContext)
        {
            return await this.UpdateDeviceStatus(this.deviceId, methodRequest.Name, Status.Rented);
        }

        private async Task<MethodResponse> ReturnScooter(MethodRequest methodRequest, object userContext)
        {
            return await this.UpdateDeviceStatus(this.deviceId, methodRequest.Name, Status.Available);
        }

        private async Task<MethodResponse> TakeScooterOffline(MethodRequest methodRequest, object userContext)
        {
            return await this.UpdateDeviceStatus(this.deviceId, methodRequest.Name, Status.Unavailable);
        }

        private Task<MethodResponse> UpdateDeviceStatus(string deviceId, string methodName, Status newStatus)
        {
            string result;
            try
            {
                this.status = newStatus;

                // Acknowlege the direct method call with a 200 success message
                result = $"{{\"result\":\"Executed direct method: {methodName} for device {deviceId}\"}}";

                // Acknowlege the direct method call with a 200 success message
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
            }
            catch (Exception ex)
            {
                result = $"{{\"result\":\"Tried to execute direct method: {methodName} for device {deviceId}. An internal error occured.\"}}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(ex.Message), 505));
            }
        }
    }
}
