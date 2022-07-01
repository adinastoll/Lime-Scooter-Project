namespace AzureFunctionsTests
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;
    using DeviceManagement;
    using Moq;
    using DeviceManagement.DevicePropertiesManager;
    using Microsoft.Azure.Devices.Client;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using System.Text.Json;
    using System.Threading;
    using System;

    public class DeviceManagementTest
    {
        readonly DeviceManagement deviceManagement;
        readonly ILogger logger;

        public DeviceManagementTest()
        {
            // Mock the IDeviceTwinManagement which is responsible with updating the property on the Device Twin
            var deviceTwinManagement = new Mock<IDeviceTwinManagement>();
            
            var methodResponse = new MethodResponse(Encoding.UTF8.GetBytes(""), 200);
            deviceTwinManagement.Setup(x => x.UpdateDeviceTwinStatusPropertyAsync(It.IsAny<string>(), It.IsAny<Status>()))
                .ReturnsAsync(methodResponse);

            deviceManagement = new DeviceManagement(deviceTwinManagement.Object);
            
            // Initialize a null logger
            logger = NullLoggerFactory.Instance.CreateLogger("Null Logger"); 
        }

        [Fact]
        public void TestUpdateDeviceTwinStatusPropertySuccess()
        {
            var queryStringValue = "firstScooter";
            var newStatusValue = "Available";

            // Arrange
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Query = new QueryCollection
                (
                    new System.Collections.Generic.Dictionary<string, StringValues>()
                    {
                        { "deviceId", queryStringValue },
                        { "newStatus", newStatusValue }
                    }
                )
            };

            // Act
            var response = this.deviceManagement.UpdateDeviceTwinStatusProperty(request, logger);
            response.Wait();

            // Assert - Check that the response is an "OK" response
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);

            // Assert - Check that the contents of the response are the expected contents
            var result = (OkObjectResult)response.Result;
            var expectedResult = $"Updated the device twin property on the device {queryStringValue} with new stats {newStatusValue}";
            Assert.Equal(expectedResult, result.Value);
        }

        [Fact]
        public void TestUpdateDeviceTwinStatusPropertyFailureNoDeviceIdQueryParameter()
        {
            var newStatusValue = "Available";

            // Arrange - missing "deviceId" parameter in the HttpRequest
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Query = new QueryCollection
                (
                    new System.Collections.Generic.Dictionary<string, StringValues>()
                    {
                        { "newStatus", newStatusValue },
                    }
                )
            };

            // Act
            var response = this.deviceManagement.UpdateDeviceTwinStatusProperty(request, logger);
            response.Wait();

            // Assert - Check that the response is a "BadRequest" response
            Assert.IsAssignableFrom<BadRequestObjectResult>(response.Result);

            // Assert - Check that the contents of the response are the expected contents
            var result = (BadRequestObjectResult)response.Result;
            var expectedResult = $"A device id and a new status are missing on the query string or in the request body.";
            Assert.Equal(expectedResult, result.Value);
        }

        [Fact]
        public void TestUpdateDeviceTwinStatusPropertyFailureNoStatusQueryParameter()
        {
            var queryStringValue = "firstScooter";

            // Arrange - missing "newStatus" parameter in the HttpRequest
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Query = new QueryCollection
                (
                    new System.Collections.Generic.Dictionary<string, StringValues>()
                    {
                        { "deviceId", queryStringValue },
                    }
                )
            };

            // Act
            var response = this.deviceManagement.UpdateDeviceTwinStatusProperty(request, logger);
            response.Wait();

            // Assert - Check that the response is a "BadRequest" response
            Assert.IsAssignableFrom<BadRequestObjectResult>(response.Result);

            // Assert - Check that the contents of the response are the expected contents
            var result = (BadRequestObjectResult)response.Result;
            var expectedResult = $"A device id and a new status are missing on the query string or in the request body.";
            Assert.Equal(expectedResult, result.Value);
        }

        [Fact]
        public void TestRentScooterFailureNoQueryParameter()
        {
            var queryStringValue = "abc";

            // Arrange - missing "newStatus" and "deviceId" parameters in the HttpRequest
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Query = new QueryCollection
                (
                    new System.Collections.Generic.Dictionary<string, StringValues>()
                    {
                        { "random-query-param", queryStringValue }
                    }
                )
            };

            // Act
            var response = this.deviceManagement.UpdateDeviceTwinStatusProperty(request, logger);
            response.Wait();

            // Assert - Check that the response is a "BadRequest" response
            Assert.IsAssignableFrom<BadRequestObjectResult>(response.Result);

            // Assert - Check that the contents of the response are the expected contents
            var result = (BadRequestObjectResult)response.Result;
            var expectedResult = $"A device id and a new status are missing on the query string or in the request body.";
            Assert.Equal(expectedResult, result.Value);
        }

        [Fact]
        public void TestBatteryAlertCustomEndpoint()
        {
            // Arrange
            bool testPassed = true;
            var deviceInformation = new TelemetryMessage
            {
                DeviceId = "FirstScooter",
                Status = Status.Available,
                Battery = 20,
                Latitude = 0,
                Longitude = 0,
            };

            string telemetryMessageBody = JsonSerializer.Serialize(deviceInformation);

            SemaphoreSlim semaphore = new SemaphoreSlim(0);
            var deviceTwinManagement = new Mock<IDeviceTwinManagement>();

            var methodResponse = new MethodResponse(Encoding.UTF8.GetBytes(""), 200);
            deviceTwinManagement.Setup(x => x.UpdateDeviceTwinStatusPropertyAsync(It.IsAny<string>(), It.IsAny<Status>()))
                .Callback((string deviceId, Status newStatus) => {
                    semaphore.Release();
                })
                .ReturnsAsync(methodResponse);

            var deviceManagement = new DeviceManagement(deviceTwinManagement.Object);

            // Act
            deviceManagement.RunBatteryAlert(telemetryMessageBody, 1,DateTime.Now, "Id", logger);

            if (!semaphore.Wait(TimeSpan.FromSeconds(5)))
            {
                testPassed = false;
            }

            // Assert
            Assert.True(testPassed);
        }

    }
}
