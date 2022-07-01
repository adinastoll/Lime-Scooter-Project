// <copyright file="DeviceManagement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace DeviceManagement
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using global::DeviceManagement.DevicePropertiesManager;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is responsible with cloud to device communication.
    /// It contains Azure Functions to update the Device Twin properties - for the scenario in which the properties are stored as Device Twins,
    /// as well as Azure Functions which invoke Direct Methods on the device - for the scenario in which the properties of the device are stored in memory.
    /// </summary>
    public class DeviceManagement
    {
        private static ServiceClient serviceClient;
        private IDeviceTwinManagement deviceTwinManagement;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceManagement"/> class.
        /// </summary>
        /// <param name="deviceTwinManagement">dsakdshadsa.</param>
        public DeviceManagement(IDeviceTwinManagement deviceTwinManagement)
        {
            this.deviceTwinManagement = deviceTwinManagement;
        }

        /// <summary>
        /// Updates the Device Twin property (status).
        /// </summary>
        /// <param name="req">The request.</param>
        /// <param name="log">The ILogger.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [FunctionName("UpdateDeviceTwinStatusProperty")]
        public async Task<IActionResult> UpdateDeviceTwinStatusProperty(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request");

            string deviceId = req.Query["deviceId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceId = deviceId ?? data?.deviceId;

            string newStatus = req.Query["newStatus"];
            newStatus = newStatus ?? data?.newStatus;

            if (deviceId != null && newStatus != null)
            {
                Enum.TryParse(newStatus, out Status newDeviceStatus);
                await this.deviceTwinManagement.UpdateDeviceTwinStatusPropertyAsync(deviceId, newDeviceStatus);

                return new OkObjectResult($"Updated the device twin property on the device {deviceId} with new stats {newStatus}");
            }
            else
            {
                return new BadRequestObjectResult("A device id and a new status are missing on the query string or in the request body.");
            }
        }

        /// <summary>
        /// Automaically update the Status of the scooter on the Device Twin when a battery allert comes in.
        /// </summary>
        /// <param name="myQueueItem">The request.</param>
        /// <param name="deliveryCount">The ILogger.</param>
        /// <param name="enqueuedTimeUtc">The enqueued time in UTC.</param>
        /// <param name="messageId">The message Id.</param>
        /// <param name="log">The log.</param>
        [FunctionName("BatteryAlertServiceBusQueueTrigger")]
        public void RunBatteryAlert(
            [ServiceBusTrigger("battery-alerts", Connection = "ServiceBusConnection")]
            string myQueueItem,
            int deliveryCount,
            DateTime enqueuedTimeUtc,
            string messageId,
            ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            log.LogInformation($"EnqueuedTimeUtc={enqueuedTimeUtc}");
            log.LogInformation($"DeliveryCount={deliveryCount}");
            log.LogInformation($"MessageId={messageId}");

            try
            {
                var telemetryMessage = JsonConvert.DeserializeObject<TelemetryMessage>(myQueueItem);
                if (telemetryMessage == null || telemetryMessage.DeviceId == null)
                {
                    log.LogInformation("Could not identify the scooter. The update of its status was not performed.");
                    return;
                }

                this.deviceTwinManagement.UpdateDeviceTwinStatusPropertyAsync(telemetryMessage.DeviceId, Status.Unavailable);
            }
            catch (Exception e)
            {
                log.LogError("Could not deserialize the message into a TelemetryMessage. Exception details: ", e);
            }
        }

        /// <summary>
        /// Invokes the RentScooter direct method on the device.
        /// </summary>
        /// <param name="req">The request.</param>
        /// <param name="log">The ILogger.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [FunctionName("RentDevice")]
        public async Task<IActionResult> RunRentDevice(
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
                await this.InvokeDirectMethodAsync(deviceId, "RentScooter");

                return new OkObjectResult($"Invoked direct method RentScooter on device with Id, {deviceId}");
            }
            else
            {
                return new BadRequestObjectResult("The device id is mandatory on the query string or in the request body.");
            }
        }

        /// <summary>
        /// Invokes the RechargeScooter direct method on the device.
        /// </summary>
        /// <param name="req">The request.</param>
        /// <param name="log">The ILogger.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [FunctionName("RechargeDevice")]
        public async Task<IActionResult> RunRechargeDevice(
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
                await this.InvokeDirectMethodAsync(deviceId, "RechargeScooter");

                return new OkObjectResult($"Invoked direct method on device with Id, {deviceId}");
            }
            else
            {
                return new BadRequestObjectResult("The device id is mandatory on the query string or in the request body.");
            }
        }

        /// <summary>
        /// Invokes the ReturnDevice direct method on the device.
        /// </summary>
        /// <param name="req">The request.</param>
        /// <param name="log">The ILogger.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [FunctionName("ReturnDevice")]
        public async Task<IActionResult> RunReturnDevice(
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
                await this.InvokeDirectMethodAsync(deviceId, "ReturnScooter");
                return new OkObjectResult($"Invoked direct method on device with Id, {deviceId}");
            }
            else
            {
                return new BadRequestObjectResult("The device id is mandatory on the query string or in the request body.");
            }
        }

        /// <summary>
        /// Invokes the the direct method on the device.
        /// </summary>
        /// <param name="deviceId">The ID of the device.</param>
        /// <param name="methodName">The name of the method to be invoked.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<CloudToDeviceMethodResult> InvokeDirectMethodAsync(string deviceId, string methodName)
        {
            // Create a ServiceClient to communicate with service-facing endpoint on your hub.
            var connectionStringInstance = await ConnectionStringManager.GetInstance();

            // The main entry point for communicating with IoT Hub
            serviceClient = ServiceClient.CreateFromConnectionString(connectionStringInstance.IotHubConnectionString);

            var methodInvocation = new CloudToDeviceMethod(methodName)
            {
                ResponseTimeout = TimeSpan.FromSeconds(30),
            };
            methodInvocation.SetPayloadJson("10");

            Console.WriteLine($"\nInvoking direct method for device: {deviceId}");

            // Invoke the direct method asynchronous and get the response from the simulated device.
            var response = await serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);
            Console.WriteLine($"\nResponse status: {response.Status}, payload:\n\t{response.GetPayloadAsJson()}");
            return response;
        }
    }
}
