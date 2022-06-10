// <copyright file="IDeviceTwinManagement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace DeviceManagement.DevicePropertiesManager
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;

    /// <summary>
    /// This interface exposes functions that allow updating the Device Twin properties of a scooter.
    /// </summary>
    public interface IDeviceTwinManagement
    {
        /// <summary>
        /// Updates the Device Twin property (status) on the scooter.
        /// </summary>
        /// <param name="deviceId">The ID of the device to be updated.</param>
        /// <param name="newStatus">The new status of the device to be used.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<MethodResponse> UpdateDeviceTwinStatusPropertyAsync(string deviceId, Status newStatus);
    }
}
