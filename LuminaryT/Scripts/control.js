(function (window, undefined) {
    function Control(ui) {
        // declare a proxy to reference the hub
        var serverConnection = $.connection.controlHub;

        // server -> client : debug
        serverConnection.client.debug = function (text) {
            appendToTextArea(text);
        };

        // server -> client : status
        serverConnection.client.status = function (json) {
            refreshComponentsJson(json);
        };

        function refreshComponentsJson(json) {
            var status = $.parseJSON(json);
            appendToTextArea("refreshComponents: status=" + status);
            refreshComponents(status.key, status.value);
        }

        function refreshComponents(key, value) {
            appendToTextArea("refreshComponents: key=" + key + ", value=" + value);
            if (key == "power") {
                ui.setPowerSwitch(value == "true" ? true : false);
            }
            else if (key == "rgbSlider") {
                ui.setRgbSlider(value);
            }
            else if (key == "speedSlider") {
                ui.setSpeedSlider(value);
            }
        }

        function appendToTextArea(text) {
            ui.appendToTextArea(text);
        }

        this.sendRgbRequestUsingCanvasAtX = function sendRgbRequestUsingCanvasAtX(canvas, x) {
            var y = canvas.height / 2;
            var context = canvas.getContext('2d');
            var img_data = context.getImageData(x, y, 1, 1).data;
            var r = img_data[0];
            var g = img_data[1];
            var b = img_data[2];
            this.sendRgbRequest(r, g, b);
        }

        this.sendRgbRequest = function sendRgbRequest(r, g, b) {
            console.log("sendRgbRequest: " + r + "," + g + "," + b);
            serverConnection.server.setRgb(r, g, b);
        }

        this.sendRgbSliderValue = function sendRgbSliderValue(x) {
            console.log("sendRgbSliderValue: " + x);
            serverConnection.server.setRgbSlider(x);
        }

        this.sendSpeedRequest = function sendSpeedRequest(x) {
            console.log("sendSpeedRequest: " + x);
            serverConnection.server.setSpeed(x);
        }

        this.sendPowerRequest = function sendPowerRequest(powerOn) {
            console.log("sendPowerRequest: " + powerOn);
            serverConnection.server.setPower(powerOn);
        }

        // start the connection
        $.connection.hub.start().done(function () {
            // request status from server
            serverConnection.server.requestStatus();
        });  
    }

    // expose access
    window.Control = Control;

})(window);
