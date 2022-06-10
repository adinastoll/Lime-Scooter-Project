A Console Application which simulates battery changes and location movements for the [Lime App Project](https://dev.azure.com/cseonboarding/TechOnboarding/_git/TechOnboarding?path=%2Fcookbook%2Fcases%2Flime.md&_a=preview)

# Description
A console application which sends telemetry data to IoT Hub with information regarding a scooter. 

A telemetry message contains the following properties: Battery, Latitude, Longitude, Status, DeviceId.

The telemetry is sent on a scheduled interval. The project covers two scenarios:
1. When the device properties were stored in memory. This was the initial approach and *InMemoryDevicePropertiesManager* class was used to handle that scenario. It is currently used only for unit testing purposes. For that case, method handlers were configured for *RentScooter, ReturnScooter, RechargeScooter* direct methods. The methods were invoked from the web application and updated the properties in memory.
2. When the device properties are stored as Device Twin. This is the current approach and it uses *DeviceTwinsManager*, a class which gets/updates the properties.

# Non-functional description:
- The project uses StyleCop.
- It retrieves the Connection String for the device and for the IoT Hub from a secret stored as Key Vault.

>**_NOTE:_** Currently it is working only for one device, which was manually configured.