// <copyright file="TelemetryMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Newtonsoft.Json;

/// <summary>
/// Scooter state.
/// </summary>
public enum Status
{
    /// <summary>
    ///  Scooter is available for rental.
    /// </summary>
    Available,

    /// <summary>
    ///  Scooter is rented.
    /// </summary>
    Rented,

    /// <summary>
    ///  Scooter is recharging.
    /// </summary>
    Recharging,

    /// <summary>
    ///  Scooter unavailable for rental.
    /// </summary>
    Unavailable,

    /// <summary>
    ///  Scooter state unintialized
    /// </summary>
    Undefined,
}

/// <summary>
/// A class representing a scooter device.
/// </summary>
public class TelemetryMessage
{
    /// <summary>
    /// Gets or sets the scooter id.
    /// </summary>
    [JsonProperty("deviceId")]
    public string DeviceId { get; set; }

    /// <summary>
    /// Gets or sets the scooter battery level.
    /// </summary>
    [JsonProperty("battery")]
    public double Battery { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    [JsonProperty("status")]
    public Status Status { get; set; }

    /// <summary>
    /// Gets or sets the latitude.
    /// </summary>
    [JsonProperty("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude.
    /// </summary>
    [JsonProperty("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there is a battery alert.
    /// </summary>
    [JsonProperty("batteryAlert")]
    public bool BatteryAlert { get; set; }
}