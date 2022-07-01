# Front End Application for Lime App Project

This project contains code for a web application which can read battery data from IoT Hub and show the real-time data in a line chart on the web page

This project, inspired from [here](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-live-data-visualization-in-web-apps?msclkid=e75184b0d04511ec81546d3edfd4f024), shows how to set up a nodejs website to visualize device data streaming to an [Azure IoT Hub](https://azure.microsoft.com/en-us/services/iot-hub) using the event [event hub SDK](https://www.npmjs.com/package/@azure/event-hubs). Prerequisites:
- Create an Azure IoT Hub
- Configure your IoT Hub with a device, a consumer group, and use that info for connecting a device and a service application
- On a website, register for device telemetry and broadcast it over a web socket to attached clients
- In a web page, display device data in a chart

To run:
1. Run '.\init.ps1'
2. Run 'npm install'.
3. Run 'npm run start'.