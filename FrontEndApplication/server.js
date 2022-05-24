const express = require('express')
const http = require('http')
const WebSocket = require('ws')
const path = require('path')
const EventHubReader = require('./scripts/event-hub-reader')
const { DefaultAzureCredential } = require("@azure/identity");
const { SecretClient } = require("@azure/keyvault-secrets");

async function getSecrets(keyVaultName, secretName) {
    const keyVaultUri = `https://${keyVaultName}.vault.azure.net`;

    const credential = new DefaultAzureCredential();
    const secretClient = new SecretClient(keyVaultUri, credential);

    const iotHubConnectionString = await secretClient.getSecret(secretName);
    return iotHubConnectionString
}

async function main() {
    const iotHubConnectionStringSecret = await getSecrets(process.env["LimeScooterKeyVault"], process.env["LimeScooterIoTHubConnectionStringSecretName"]);

    if (!iotHubConnectionStringSecret || !iotHubConnectionStringSecret.value) {
        console.error('The Connection String of the IoT Hub must be specified');
        return;
    }
    const iotHubConnectionString = iotHubConnectionStringSecret.value;

    const eventHubConsumerGroupSecret = await getSecrets(process.env["LimeScooterKeyVault"], process.env["EventHubConsumerGroupSecretName"]); //process.env["EventHubConsumerGroup"];
    if (!eventHubConsumerGroupSecret || !eventHubConsumerGroupSecret.value) {
        console.error('Environment varialble EventHubConsumerGroup must be specified.');
        return;
    }

    const eventHubConsumerGroup = eventHubConsumerGroupSecret.value;

    // Redirect requests to the public subdirectory to the root
    const app = express();
    app.use(express.static(path.join(__dirname, 'client')));
    app.use((req, res) => {
        res.redirect('/')
    })

    // Create a Web Server using http module
    const server = http.createServer(app);

    // Create a Web Socket Server using ws npm package
    // The Web Socket Client will be created in the code on the client side
    const wss = new WebSocket.Server({ server });

    // Broadcast the message to everyone listening
    wss.broadcast = (data) => {
        wss.clients.forEach((client) => {
            if (client.readyState === WebSocket.OPEN) {
                try {
                    console.log(`Broadcasting data: ${data}`);
                    client.send(data);
                } catch (e) {
                    console.error(e);
                }
            }
        })
    }

    // Bind the server to a network address
    server.listen(process.env.PORT || '3000', () => {
        console.log('Listening on %d', server.address().port);
    });

    // Create the EventHubReader and broadcast the messages to the web socket
    const eventHubReader = new EventHubReader(iotHubConnectionString, eventHubConsumerGroup);

    (async () => {
        await eventHubReader.startReadMessage((message, date, deviceId) => {
            try {
                const payload = {
                    IotData: message,
                    MessageDate: date || Date.now().toISOString(),
                    DeviceId: deviceId,
                };

                wss.broadcast(JSON.stringify(payload));
            } catch (err) {
                console.error('Error broadcasting: [%s] from [%s].', err, message);
            }
        })
    })().catch();
}

main()