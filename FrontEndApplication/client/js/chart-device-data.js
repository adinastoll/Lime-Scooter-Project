/* eslint-disable max-classes-per-file */
/* eslint-disable no-restricted-globals */
/* eslint-disable no-undef */
$(document).ready(() => {
  const protocol = document.location.protocol.startsWith('https') ? 'wss://' : 'ws://';
  const webSocket = new WebSocket(protocol + location.host);

  const trackedDevices = new TrackedDevices();

  // Define the chart axes
  const chartData = {
    datasets: [
      {
        fill: false,
        label: 'battery',
        yAxisID: 'battery',
        borderColor: 'rgba(255, 204, 0, 1)',
        pointBoarderColor: 'rgba(255, 204, 0, 1)',
        backgroundColor: 'rgba(255, 204, 0, 0.4)',
        pointHoverBackgroundColor: 'rgba(255, 204, 0, 1)',
        pointHoverBorderColor: 'rgba(255, 204, 0, 1)',
        spanGaps: true,
      }
    ]
  };

  const chartOptions = {
    scales: {
      yAxes: [{
        id: 'battery',
        type: 'linear',
        scaleLabel: {
          labelString: 'battery (%)',
          display: true,
        },
        position: 'left',
      }],
    }
  }

  // Get the context of the canvas element we want to select
  const ctx = document.getElementById('iotChart').getContext('2d');
  const myLineChart = new Chart(
    ctx,
    {
      type: 'line',
      data: chartData,
      options: chartOptions,
    });

  // Manage a list of devices in the UI, and update which device data the chart is showing
  // based on selection
  let needsAutoSelect = true;
  const deviceCount = document.getElementById('deviceCount');
  const deviceStatus = document.getElementById('status');
  const listOfDevices = document.getElementById('listOfDevices');
  const rentScooterButton = document.getElementById('rentScooterButton');
  const rechargeScooterButton = document.getElementById('rechargeScooterButton');

  function OnSelectionChange() {
    const device = trackedDevices.findDevice(listOfDevices[listOfDevices.selectedIndex].text);
    chartData.labels = device.timeData;
    chartData.datasets[0].data = device.batteryData;
    chartData.datasets[1].data = device.locationData;
    myLineChart.update();
  }
  function OnStatusChanged() {
    const device = trackedDevices.findDevice(listOfDevices[listOfDevices.selectedIndex].text);
    deviceStatus.innerText = device.deviceStatus;
    console.log("Device status: ", device.deviceStatus);

    let innerHTML = 'Return Scooter';
    let color = 'Red';
    let deviceStatusText = 'Undefined';

    switch (device.deviceStatus) {
      case 0:
        innerHTML = 'Rent Scooter';
        color = 'Green';
        deviceStatusText = 'Available';
        rechargeScooterButton.style.display = "block";
        rentScooterButton.style.display = "block";
        break;
      case 1:
        innerHTML = 'Return Scooter';
        color = 'Red';
        deviceStatusText = 'Rented';
        rechargeScooterButton.style.display = "none";
        rentScooterButton.style.display = "none";
        break;
      case 2:
        innerHTML = 'Recharging'
        rechargeScooterButton.style.display = "block"
        rentScooterButton.style.display = "none";
        deviceStatusText = 'Recharging';
        break;
      case 3:
        rentScooterButton.display = false;
        deviceStatusText = 'Unavailable';
        rentScooterButton.style.display = "none";
        rechargeScooterButton.style.display = "none"
        break;
      case 4:
        rentScooterButton.display = false;
        deviceStatusText = 'Undefined';
        rentScooterButton.style.display = "none";
        rechargeScooterButton.style.display = "none";
        break;
    }
    rentScooterButton.innerHTML = innerHTML;
    rentScooterButton.style.color = color;
    deviceStatus.innerText = deviceStatusText;
    deviceStatus.style.color = color;
  }

  listOfDevices.addEventListener('change', OnSelectionChange, false);
  deviceStatus.addEventListener('change', OnStatusChanged, false);

  // When a web socket message arrives:
  // 1. Unpack it
  // 2. Validate it has date/time and battery
  // 3. Find or create a cached device to hold the telemetry data
  // 4. Append the telemetry data
  // 5. Update the chart UI
  webSocket.onmessage = function onMessage(message) {
    try {
      const messageData = JSON.parse(message.data);
      console.log("Received Data: ", messageData);
      // console.log("messageData.IotData.latitude: ", messageData.IotData.Latitude);

      console.log("messageData.IotData.location: ", messageData.IotData.Location);

      // find or add device to list of tracked devices
      const existingDeviceData = trackedDevices.findDevice(messageData.DeviceId);

      if (existingDeviceData) {
        existingDeviceData.addData(messageData.MessageDate, messageData.IotData.Battery, messageData.IotData.Location, messageData.IotData.Status);
      } else {
        const newDeviceData = new DeviceData(messageData.DeviceId);
        trackedDevices.devices.push(newDeviceData);
        const numDevices = trackedDevices.getDevicesCount();
        deviceCount.innerText = numDevices === 1 ? `${numDevices} device` : `${numDevices} devices`;
        deviceStatus.innerText = messageData.IotData.Status;
        newDeviceData.addData(messageData.MessageDate, messageData.IotData.Battery, messageData.IotData.Location, messageData.IotData.Status);

        // add device to the UI list
        const node = document.createElement('option');
        const nodeText = document.createTextNode(messageData.DeviceId);
        node.appendChild(nodeText);
        listOfDevices.appendChild(node);

        // if this is the first device being discovered, auto-select it
        if (needsAutoSelect) {
          needsAutoSelect = false;
          listOfDevices.selectedIndex = 0;
          OnSelectionChange();
        }
      }

      myLineChart.update();
      OnStatusChanged();
    } catch (err) {
      console.error(err);
    }
  };
});