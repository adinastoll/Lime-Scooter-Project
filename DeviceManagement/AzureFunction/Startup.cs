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
    /// The Startup class.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// The Configuration function.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IDeviceTwinManagement>((s) =>
            {
                return new DeviceTwinManagement();
            });
        }
    }
}
