// /* eslint-disable max-classes-per-file */
// /* eslint-disable no-restricted-globals */
// /* eslint-disable no-undef */
function InitMapAndSignalR() {
    const protocol = document.location.protocol.startsWith('https') ? 'wss://' : 'ws://';
    const webSocket = new WebSocket(protocol + location.host);

    var map;
    var mapsDataSource;

    const deviceCount = document.getElementById('deviceCount');
    const trackedDevices = new TrackedDevices();
    const deviceStatus = document.getElementById('status');
    const rentScooterButton = document.getElementById('rentScooterButton');
    const returnScooterButton = document.getElementById('returnScooterButton');

    listOfDevices.addEventListener('change', OnSelectionChange, false);
    deviceStatus.addEventListener('change', OnStatusChanged, false);

    function OnSelectionChange() {
        const device = trackedDevices.findDevice(listOfDevices[listOfDevices.selectedIndex].text);
        chartData.labels = device.timeData;
        chartData.datasets[0].data = device.batteryData;
        chartData.datasets[1].data = device.locationData;
        myLineChart.update();
    }

    function OnStatusChanged() {
        const device = trackedDevices.findDevice(listOfDevices[listOfDevices.selectedIndex].text);

        let innerHTML = 'Return Scooter';
        let color = 'Red';
        let deviceStatusText = 'Undefined';

        switch (device.deviceStatus) {
            case 0:
                innerHTML = 'Rent Scooter';
                color = 'Green';
                deviceStatusText = 'Available';
                returnScooterButton.style.display = "none";
                rentScooterButton.style.display = "block";
                break;
            case 1:
                innerHTML = 'Return Scooter';
                color = 'Red';
                deviceStatusText = 'Rented';
                returnScooterButton.style.display  = "block";
                rentScooterButton.style.display = "none";
                break;
            case 2:
                innerHTML = 'Recharging'
                rentScooterButton.style.display = "none";
                returnScooterButton.style.display = "none"
                deviceStatusText = 'Recharging';
                break;
            case 3:
                rentScooterButton.display = false;
                deviceStatusText = 'Unavailable';
                rentScooterButton.style.display = "none";
                returnScooterButton.style.display = "none";
                break;
            case 4:
                rentScooterButton.display = false;
                deviceStatusText = 'Undefined';
                rentScooterButton.style.display = "none";
                returnScooterButton.style.display = "none";
                break;
        }
        rentScooterButton.innerHTML = innerHTML;
        rentScooterButton.style.color = color;
        deviceStatus.innerText = deviceStatusText;
    }

    map = new atlas.Map('myMap', {
        center: [5.55, 51.5],
        zoom: 10,
        view: "Auto",
        language: 'en-US',
        authOptions: {
            authType: 'subscriptionKey',
            subscriptionKey: 'F9PF06n9Ym07aC_5LR1BQut5C86elVL4aTpZzrUyJwE'
        }
    });

    //Construct a zoom control and add it to the map.
    map.controls.add(new atlas.control.ZoomControl(), {
        position: 'bottom-right'
    });

    map.events.add('ready', function () {

        // Create a data source and add it to the map.
        mapsDataSource = new atlas.source.DataSource();
        map.sources.add(mapsDataSource);

        // Create a symbol layer to render icons and/or text at points on the map.
        var layer = new atlas.layer.SymbolLayer(mapsDataSource);

        // Add the layer to the map.
        map.layers.add(layer);

        const apiBaseUrl = window.location.origin;

        webSocket.onmessage = function onMessage(message) {
            try {
                const messageData = JSON.parse(message.data);
                // time and either battery or location are required
                // if (!messageData.MessageDate || (!messageData.IotData.battery && !messageData.IotData.location)) {
                //     return;
                // }
                const obj = messageData.IotData;

                let text = "Latitude: " + obj.Latitude + " Longitude: " + obj.Longitude
                document.getElementById("messages").innerHTML = text;

                // Replace the pin so only latest position is shown.
                mapsDataSource.clear();
                mapsDataSource.add(new atlas.data.Point([obj.Longitude, obj.Latitude]));

                const existingDeviceData = trackedDevices.findDevice(messageData.DeviceId);

                console.log(messageData.MessageDate, messageData.IotData.Battery, messageData.IotData.Location, messageData.IotData.status)
                if (existingDeviceData) {
                    existingDeviceData.updateData(messageData.MessageDate, messageData.IotData.Battery, messageData.IotData.Location, messageData.IotData.Status);
                    OnStatusChanged();
                } else {
                    const newDeviceData = new DeviceData(messageData.DeviceId);
                    trackedDevices.devices.push(newDeviceData);
                    const numDevices = trackedDevices.getDevicesCount();
                    deviceCount.innerText = numDevices === 1 ? `${numDevices} device` : `${numDevices} devices`;
                    deviceStatus.innerText = messageData.IotData.status;

                    // add device to the UI list
                    const node = document.createElement('option');
                    const nodeText = document.createTextNode(messageData.DeviceId);
                    node.appendChild(nodeText);
                    listOfDevices.appendChild(node);

                    OnStatusChanged();
                }

            }
            catch (err) {
                console.error(err);
            }
        };
    })
}

function rentScooter() {
    const xhr = new XMLHttpRequest()
    //open a get request with the remote server URL
    xhr.open("GET", "http://localhost:7071/api/RentDevice?deviceId=FirstScooter")
    //send the Http request
    xhr.send()
}

function rechargeScooter() {
    const xhr = new XMLHttpRequest()
    //open a get request with the remote server URL
    xhr.open("GET", "http://localhost:7071/api/RechargeDevice?deviceId=FirstScooter")
    //send the Http request
    xhr.send()
}