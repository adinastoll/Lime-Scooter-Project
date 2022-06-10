// <copyright file="IDeviceClient.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SimulatedDeviceScooters.IoTHubDeviceClient
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;

    /// <summary>
    /// Device Client interface.
    /// </summary>
    public interface IDeviceClient
    {
        /// <summary>
        /// Sends an event from the device to IoT Hub in asynchronous way.
        /// </summary>
        /// <param name="message">The message to be sent to the IoT Hub.</param>
        /// <returns>A task.</returns>
        public Task SendEventAsync(Message message);

        /// <summary>
        /// Closes the device client.
        /// </summary>
        /// <returns>A task.</returns>
        public Task CloseAsync();

        /// <summary>
        /// Disposes the resources used by the device client.
        /// </summary>
        public void Dispose();
    }
}
