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
        deviceStatus.innerText = device.deviceStatus;
        if (device.deviceStatus === false) {
            rentScooterButton.innerHTML = 'Return Scooter';
            rentScooterButton.style.color = 'Red';
        }
        else {
            rentScooterButton.innerHTML = 'Rent Scooter';
            rentScooterButton.style.color = 'Green';
        }
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

                let text = "Latitude: " + obj.latitude + " Longitude: " + obj.longitude
                document.getElementById("messages").innerHTML = text;

                // Replace the pin so only latest position is shown.
                mapsDataSource.clear();
                mapsDataSource.add(new atlas.data.Point([obj.longitude, obj.latitude]));

                const existingDeviceData = trackedDevices.findDevice(messageData.DeviceId);

                console.log(messageData.MessageDate, messageData.IotData.battery, messageData.IotData.location, messageData.IotData.status)
                if (existingDeviceData) {
                    existingDeviceData.updateData(messageData.MessageDate, messageData.IotData.battery, messageData.IotData.location, messageData.IotData.status);
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