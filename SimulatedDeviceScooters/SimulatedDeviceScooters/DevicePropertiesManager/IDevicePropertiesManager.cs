// <copyright file="IDevicePropertiesManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SimulatedDeviceScooters.DeviceProperties
{
    using System.Threading.Tasks;

    /// <summary>
    /// An interface for device properties.
    /// </summary>
    public interface IDevicePropertiesManager
    {
        /// <summary>
        /// Gets the device ID.
        /// </summary>
        /// <returns>The device ID.</returns>
        public string GetDeviceId();

        /// <summary>
        /// Gets the device status.
        /// </summary>
        /// <returns>The device status.</returns>
        public Task<Status> GetDeviceStatusAsync();

        /// <summary>
        /// Gets the device battery level.
        /// </summary>
        /// <returns>The device battery level.</returns>
        public Task<double> GetBatteryLevelAsync();

        /// <summary>
        /// Gets the latitude.
        /// </summary>
        /// <returns>The latitude of the device location.</returns>
        public Task<double> GetLatitudeAsync();

        /// <summary>
        /// Gets the device longitude.
        /// </summary>
        /// <returns>The longitude of the device location.</returns>
        public Task<double> GetLongitudeAsync();

        /// <summary>
        /// Sets a method handler for the direct method call.
        /// </summary>
        public void SetDirectMethodAsync();
    }
}
