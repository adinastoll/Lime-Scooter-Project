// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using DeviceManagement.DevicePropertiesManager;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(DeviceManagement.Startup))]

namespace DeviceManagement
{
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;

    /// <summary>
    /// fkjsdfjsf.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// dsaljdlksajdsa.
        /// </summary>
        /// <param name="builder">fkdsjflkdsjf.</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IDeviceTwinManagement>((s) =>
            {
                return new DeviceTwinManagement();
            });
        }
    }
}
