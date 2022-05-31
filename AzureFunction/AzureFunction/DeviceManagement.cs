// <copyright file="DeviceManagement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace AzureFunction
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is responsible with cloud to device communication.
    /// </summary>
    public static class DeviceManagement
    {
        private static ServiceClient serviceClient;

        /// <summary>
        /// Invokes the RentScooter direct method on the device.
        /// </summary>
        /// <param name="req">The request.</param>
        /// <param name="log">The ILogger.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [FunctionName("RentDevice")]
        public static async Task<IActionResult> RunRentDevice(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request");

            string deviceId = req.Query["deviceId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceId = deviceId ?? data?.deviceId;

            if (deviceId != null)
            {
                // Create a ServiceClient to communicate with service-facing endpoint on your hub.
                var connectionStringInstance = await IoTHubConnectionString.GetInstance();
                serviceClient = ServiceClient.CreateFromConnectionString(connectionStringInstance.ConnectionString);

                await InvokeDirectMethodAsync(deviceId, "RentScooter");

                return new OkObjectResult($"Invoked direct method on device with Id, {deviceId}");
            }
            else
            {
                return new BadRequestObjectResult("Please pass a device id on the query string or in the request body");
            }
        }

        /// <summary>
        /// Invokes the RechargeScooter direct method on the device.
        /// </summary>
        /// <param name="req">The request.</param>
        /// <param name="log">The ILogger.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [FunctionName("RechargeDevice")]
        public static async Task<IActionResult> RunReturnDevice(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request");

            string deviceId = req.Query["deviceId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceId = deviceId ?? data?.deviceId;

            if (deviceId != null)
            {
                // Create a ServiceClient to communicate with service-facing endpoint on your hub.
                var connectionStringInstance = await IoTHubConnectionString.GetInstance();
                serviceClient = ServiceClient.CreateFromConnectionString(connectionStringInstance.ConnectionString);

                await InvokeDirectMethodAsync(deviceId, "RechargeScooter");

                return new OkObjectResult($"Invoked direct method on device with Id, {deviceId}");
            }
            else
            {
                return new BadRequestObjectResult("Please pass a device id on the query string or in the request body");
            }
        }

        private static async Task InvokeDirectMethodAsync(string deviceId, string methodName)
        {
            var methodInvocation = new CloudToDeviceMethod(methodName)
            {
                ResponseTimeout = TimeSpan.FromSeconds(30),
            };
            methodInvocation.SetPayloadJson("10");

            Console.WriteLine($"\nInvoking direct method for device: {deviceId}");

            // Invoke the direct method asynchronous and get the response from the simulated device.
            var response = await serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);
            Console.WriteLine($"\nResponse status: {response.Status}, payload:\n\t{response.GetPayloadAsJson()}");
        }
    }
}
