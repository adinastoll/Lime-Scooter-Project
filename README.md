# Introduction 
This project is based on [Lime Project](https://dev.azure.com/cseonboarding/TechOnboarding/_git/TechOnboarding?path=%2Fcookbook%2Fcases%2Flime.md&_a=preview)

The objectives of this project is to build a foundation of core Azure services.
# Getting Started
The projects consists of:
1.	[SimulatedDeviceScooter](https://dev.azure.com/OnboardingMay2022/_git/OnboardingMay2022?path=/SimulatedDeviceScooters&version=GBadina/spike):

A console application which simulates scooters by sending telemetry data (battery level) to IoT Hub and registers direct method on devices.

2.	[FrontEndApplication](https://dev.azure.com/OnboardingMay2022/_git/OnboardingMay2022?path=/FrontEndApplication&version=GBadina/spike): 

A NodeJs application which displays the battery level in real time. Users can rent/return a scooter. Operators can recharge a scooter.

3.	[DeviceManagement](https://dev.azure.com/OnboardingMay2022/_git/OnboardingMay2022?path=/DeviceManagement&version=GBadina/spike)
Azure Function application with HTTP GET triggers which invokes direct method on devices.

# Build and Test
See the README section from each project mentioned above.
