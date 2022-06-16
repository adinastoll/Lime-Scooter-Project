// <copyright file="ConnectionStringManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace DeviceManagement
{
    using System;
    using System.Threading.Tasks;
    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;
    using Microsoft.Azure.Devices;

    /// <summary>
    /// A singleton to hold the connection string to IoT Hub and device.
    /// </summary>
    public class ConnectionStringManager
    {
        private static ConnectionStringManager instance = null;

        private ConnectionStringManager()
        {
            this.IotHubConnectionString = null;
            this.DeviceConnectionString = null;
        }

        /// <summary>
        /// Gets or sets the IoT Hub Connection String.
        /// </summary>
        public string IotHubConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Connection String of the device.
        /// </summary>
        public string DeviceConnectionString { get; set; }

        /// <summary>
        /// Gets the IoTHubConnectionString instance.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<ConnectionStringManager> GetInstance()
        {
            if (instance == null)
            {
                instance = new ConnectionStringManager();
                string iotHubConnectionString = await GetConnectionStringAsync("LimeScooterIoTHubConnectionString");
                ValidateIotHubConnectionString(iotHubConnectionString);
                string deviceConnectionString = await GetConnectionStringAsync("FirstScooterConnectionString");

                instance.IotHubConnectionString = iotHubConnectionString;
                instance.DeviceConnectionString = deviceConnectionString;
            }

            return instance;
        }

        /// <summary>
        /// Gets the connection string from Key Vault.
        /// </summary>
        /// <param name="secretName">The secret name.</param>
        /// <returns>A <see cref="string"/> representing the Connection String found at the given key in the Key Vault.</returns>
        private static async Task<string> GetConnectionStringAsync(string secretName)
        {
            // Get the IoT device connection string
            string keyVaultUrl = "https://limescooterkeyvault.vault.azure.net/";
            var client = new SecretClient(vaultUri: new Uri(keyVaultUrl), credential: new DefaultAzureCredential());
            KeyVaultSecret keyVaultSecret = await client.GetSecretAsync(secretName);
            return keyVaultSecret?.Value;
        }

        /// <summary>
        /// Validates that the given connection string is in the right format.
        /// </summary>
        /// <param name="hubConnectionString">The connection string to be validated.</param>
        private static void ValidateIotHubConnectionString(string hubConnectionString)
        {
            try
            {
                _ = IotHubConnectionStringBuilder.Create(hubConnectionString);
            }
            catch (Exception)
            {
                Console.WriteLine("An IoT Hub connection string needs to be specified, " +
                    "please set the environment variable \"IOTHUB_CONNECTION_STRING\" " +
                    "or pass in \"-s | --HubConnectionString\" through command line.");
                Environment.Exit(1);
            }
        }
    }
}
