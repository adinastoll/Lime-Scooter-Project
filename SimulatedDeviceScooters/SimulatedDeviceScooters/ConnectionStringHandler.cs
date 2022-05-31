// <copyright file="ConnectionStringHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SimulatedDeviceScooters
{
    using System;
    using System.Threading.Tasks;
    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;
    using Microsoft.Azure.Devices.Client;

    /// <summary>
    /// Handler class for Connection Strings.
    /// </summary>
    internal static class ConnectionStringHandler
    {
        /// <summary>
        /// Gets the connection string from Key Vault.
        /// </summary>
        /// <param name="secretName">The secret name.</param>
        /// <returns>A <see cref="string"/> representing the Connection String found at the given key in the Key Vault.</returns>
        public static async Task<string> GetConnectionStringAsync(string secretName)
        {
            // Get the IoT device connection string
            string keyVaultUrl = "https://limescooterkeyvault.vault.azure.net/";
            var client = new SecretClient(vaultUri: new Uri(keyVaultUrl), credential: new DefaultAzureCredential());
            KeyVaultSecret keyVaultSecret = await client.GetSecretAsync(secretName);
            return keyVaultSecret?.Value;
        }

        /// <summary>
        /// Validates that the given string is a valid Connection String.
        /// </summary>
        /// <param name="connectionString">The connection string to validate.</param>
        public static void ValidateConnectionString(string connectionString)
        {
            try
            {
                _ = IotHubConnectionStringBuilder.Create(connectionString);
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
