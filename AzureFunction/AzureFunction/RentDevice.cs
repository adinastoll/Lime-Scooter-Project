using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;

namespace AzureFunction
{
    public static class RentDevice
    {
        private static ServiceClient s_serviceClient;

        [FunctionName("RentDevice")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request");

            string deviceId = req.Query["deviceId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceId = deviceId ?? data?.deviceId;

            // Create a ServiceClient to communicate with service-facing endpoint on your hub.
            string connectionString = "";
            s_serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            await InvokeDirectMethodAsync(deviceId);

            //string responseMessage = string.IsNullOrEmpty(deviceId)
                                //? "This HTTP triggered function executed successfully. Pass a device id in the query string or in the request body for a personalized response."
                                //: $"Hello. Renting Scooter: {deviceId}.";

            //string HubConnectionString = "";

            // This sample accepts the service connection string as a parameter, if present
            //ValidateConnectionString(HubConnectionString);

            return new OkObjectResult("");
        }

        private static async Task InvokeDirectMethodAsync(string deviceId)
        {
            var methodInvocation = new CloudToDeviceMethod("RentScooter")
            {
                ResponseTimeout = TimeSpan.FromSeconds(30)
            };
            methodInvocation.SetPayloadJson("10");

            Console.WriteLine($"\nInvoking direct method for device: {deviceId}");

            // Invoke the direct method asynchronous and get the response from the simulated device.
            var response = await s_serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);
            Console.WriteLine($"\nResponse status: {response.Status}, payload:\n\t{response.GetPayloadAsJson()}");
        }

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
