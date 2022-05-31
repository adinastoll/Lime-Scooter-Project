// <copyright file="IoTHubConnectionString.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace AzureFunction
{
    using System;
    using System.Threading.Tasks;
    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;
    using Microsoft.Azure.Devices;

    /// <summary>
    /// A singleton to hold the connection string to IoT Hub.
    /// </summary>
    public class IoTHubConnectionString
    {
        private static IoTHubConnectionString instance = null;

        private IoTHubConnectionString()
        {
            this.ConnectionString = null;
        }

        /// <summary>
        /// Gets or sets the IoT Hub Connection String.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets the IoTHubConnectionString instance.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<IoTHubConnectionString> GetInstance()
        {
            if (instance == null)
            {
                instance = new IoTHubConnectionString();
                string iotHubConnectionString = await GetConnectionStringAsync("LimeScooterIoTHubConnectionString");
                ValidateConnectionString(iotHubConnectionString);
                instance.ConnectionString = iotHubConnectionString;
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
        private static void ValidateConnectionString(string hubConnectionString)
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
