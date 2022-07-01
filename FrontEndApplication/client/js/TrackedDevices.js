class TrackedDevices {
    constructor() {
        this.devices = []
    }

    // Find a device based on its Id
    findDevice(deviceId) {
        for (let i = 0; i < this.devices.length; ++i) {
            if (this.devices[i].deviceId == deviceId) {
                return this.devices[i];
            }
        }

        return undefined;
    }

    getDevicesCount() {
        return this.devices.length;
    }
}