// The class for holding the last N points of telemetry from a device
class DeviceData {
    constructor(deviceId) {
        this.deviceId = deviceId;
        this.maxLen = 50;
        this.timeData = new Array(this.maxLen);
        this.batteryData = new Array(this.maxLen);
        this.deviceStatus = 'Unknown';
    }

    addData(time, battery, location, status) {
        this.timeData.push(time);
        this.batteryData.push(battery);

        var deviceStatus = 'Undefined';
        switch(status){
            case 0:
                deviceStatus = 'Available'
                break;
            case 1:
                deviceStatus = 'Rented'
                break;
            case 2: 
                deviceStatus = 'Recharging'
                break;
            case 3:
                deviceStatus = 'Unavailable'
                break;
        }
        
        this.deviceStatus = status;

        if (this.timeData.length > this.maxLen) {
            this.timeData.shift();
            this.batteryData.shift();
        }
    }

    updateData(time, battery, location, status){
        this.deviceStatus = status;
    }
}