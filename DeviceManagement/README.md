# Azure Functions for [Lime App Project](https://dev.azure.com/cseonboarding/TechOnboarding/_git/TechOnboarding?path=%2Fcookbook%2Fcases%2Flime.md&_a=preview)

This project contains the Azure Functions responsible with updating the device properties.
1. Initially, the device properties (Battery, Status, Latitude, Longitude) were stored in memory. In that case, the update was handled through [Direct Methods](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-direct-methods). Rent/Return/RechargeDevice Azure Functions were created for that purpose. 
1. Currently, the device properties (Battery, Status, Latitude, Longitude) are stored as [Device Twins](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-device-twins). In this case, UpdateDeviceTwinStatusProperty Azure Function is used to update the reported property (Status). Unit tests were created to cover this scenario.

> **_NOTE:_**  "Status" is the only property we are updating through Azure Functions.