namespace SimulatedScootersUnitTests
{
    using Microsoft.Azure.Devices.Client;
    using Moq;
    using SimulatedDeviceScooters;
    using SimulatedDeviceScooters.IoTHubDeviceClient;
    using SimulatedDeviceScooters.DeviceProperties;
    using System.Text;
    using System.Text.Json;

    [TestClass]
    public class DeviceToCloudTelemetryTests
    {
        private Mock<IDeviceClient> deviceClientMock = new Mock<IDeviceClient>();
        private List<TelemetryMessage> deviceInformationMessages;

        double defaultBatteryLevel = 100;
        double defaultLatitude = 47.192480;
        double defaultLongitude = 8.851230;
        Status defaultStatus = Status.Available;

        private void Setup()
        {
            this.deviceInformationMessages = new List<TelemetryMessage>();

            // Mock the device client to add the messages in a list on SendEventAsync
            this.deviceClientMock.Setup(x => x.SendEventAsync(It.IsAny<Message>()))
                           .Callback((Message msg) =>
                           {
                               byte[] messageBytes = msg.GetBytes();
                               string messageString = Encoding.UTF8.GetString(messageBytes);
                               var telemetryMessage = JsonSerializer.Deserialize<TelemetryMessage>(messageString);
                               if (telemetryMessage != null)
                               {
                                   deviceInformationMessages.Add(telemetryMessage);
                               }
                           })
                           .Returns(Task.CompletedTask);
        }

        [TestMethod]
        public void SendDeviceToCloudTelemetry()
        {
            // Arrange
            this.Setup();

            // Create a device property manager that takes the properties from memory
            IDevicePropertiesManager devicePropertiesHandler = new InMemoryDevicePropertiesManager();

            // Act - Send a message from the device with is properties
            Task ctask = DeviceToCloudCommunication.SendDeviceToCloudTelemetryAsync(deviceClientMock.Object, devicePropertiesHandler);

            // Assert
            Assert.AreEqual(this.deviceInformationMessages.Count, 1);
            Assert.AreEqual(this.deviceInformationMessages[0].Status, this.defaultStatus);
            Assert.AreEqual(this.deviceInformationMessages[0].Battery, this.defaultBatteryLevel);
            Assert.AreEqual(this.deviceInformationMessages[0].Latitude, this.defaultLatitude);
            Assert.AreEqual(this.deviceInformationMessages[0].Longitude, this.defaultLongitude);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void IotHubConnectionStringValidationTest()
        {
            string connectionString = "test";

            ConnectionStringHandler.ValidateIotHubConnectionString(connectionString);
        }
    }
}