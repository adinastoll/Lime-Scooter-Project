const crypto = require("crypto");
const Buffer = require("buffer").Buffer;
const { Connection, ReceiverEvents, isAmqpError, parseConnectionString } = require("rhea-promise");

// This code is inspired from https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-security#security-tokens
function generateSasToken(resourceUri, signingKey, policyName, expiresInMins) {
    resourceUri = encodeURIComponent(resourceUri);

    const expiresInSeconds = Math.ceil(Date.now() / 1000 + expiresInMins * 60);
    const toSign = resourceUri + "\n" + expiresInSeconds;

    // Use the crypto module to create the hmac.
    const hmac = crypto.createHmac("sha256", Buffer.from(signingKey, "base64"));
    hmac.update(toSign);
    const base64UriEncoded = encodeURIComponent(hmac.digest("base64"));

    // Construct authorization string.
    return `SharedAccessSignature sr=${resourceUri}&sig=${base64UriEncoded}&se=${expiresInSeconds}&skn=${policyName}`;
}

async function convertIoTHubToEventHubConnectionString(connectionString) {
    //HostName=something.azure-devices.net;SharedAccessKeyName=..;SharedAccessKey=..
    const { HostName, SharedAccessKeyName, SharedAccessKey } = parseConnectionString(connectionString);

    if (!HostName || !SharedAccessKeyName || !SharedAccessKey) {
        throw new Error("Invalid IoT Hub Connection string");
    }

    // Extract the IoT Hub name from the hostname
    const [iotHubName] = HostName.split(".");
    if (!iotHubName) {
        throw new Error("Unable to extract the IoT Hub name from the connection string");
    }

    // Generate a token to authenticate to the service.
    // The code for generateSasToken can be found at https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-security#security-tokens
    const token = generateSasToken(
        `${HostName}/messages/events`,
        SharedAccessKey,
        SharedAccessKeyName,
        5 // token expires in 5 minutes
    );
    const connectionOptions = {
        transport: "tls",
        host: HostName,
        hostname: HostName,
        username: `${SharedAccessKeyName}@sas.root.${iotHubName}`,
        port: 5671,
        reconnect: false,
        password: token
    };

    const connection = new Connection(connectionOptions);
    await connection.open();

    // Create the receiver that will trigger a redirect error.
    const receiver = await connection.createReceiver({
        source: { address: `amqps://${HostName}/messages/events/$management` }
    });

    return new Promise((resolve, reject) => {
        receiver.on(ReceiverEvents.receiverError, (context) => {
            const error = context.receiver && context.receiver.error;
            if (isAmqpError(error) && error.condition === "amqp:link:redirect") {
                const hostname = error.info && error.info.hostname;
                const parsedAddress = error.info.address.match(/5671\/(.*)\/\$management/i);

                if (!hostname) {
                    reject(error);
                } else if (parsedAddress == undefined || (parsedAddress && parsedAddress[1] == undefined)) {
                    const msg = `Cannot parse the EventHub name from the given address: ${error.info.address} in the error: ` +
                        `${error.stack}\n${JSON.stringify(error.info)}.\nThe parsed result is: ${JSON.stringify(parsedAddress)}.`;
                    reject(Error(msg));
                } else {
                    const entityPath = parsedAddress[1];
                    resolve(`Endpoint=sb://${hostname}/;EntityPath=${entityPath};SharedAccessKeyName=${SharedAccessKeyName};SharedAccessKey=${SharedAccessKey}`);
                }
            } else {
                reject(error);
            }
            connection.close().catch(() => {
                /* ignore error */
            });
        });
    });
}

module.exports = {
    convertIoTHubToEventHubConnectionString
}