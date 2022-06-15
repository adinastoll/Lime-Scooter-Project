// <copyright file="DeviceClientWrapper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SimulatedDeviceScooters.IoTHubDeviceClient
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;

    /// <summary>
    /// Implements <see cref="IDeviceClient"/>.
    /// </summary>
    public class DeviceClientWrapper : IDeviceClient
    {
        private DeviceClient deviceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceClientWrapper"/> class.
        /// </summary>
        /// <param name="deviceClient">The Device Client instance.</param>
        public DeviceClientWrapper(DeviceClient deviceClient)
        {
            this.deviceClient = deviceClient;
        }

        /// <inheritdoc/>
        public Task SendEventAsync(Message message)
        {
            return this.deviceClient.SendEventAsync(message);
        }

        /// <inheritdoc/>
        public Task CloseAsync()
        {
            return this.deviceClient.CloseAsync();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.deviceClient.Dispose();
        }
    }
}
