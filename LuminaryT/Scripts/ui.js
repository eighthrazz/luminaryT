(function (window, undefined) {
    function UI() {
        // on/off toggle switch
        $("#on_off").bootstrapSwitch();

        // RGB slider
        var rgbSlider = new Slider("#rgbSlider", { id: "rgbDataSlider", min: 0, max: 1.0, value: 0, step: .01 });
        
        // speed/direction slider
        var speedSlider = new Slider("#speed", { id: "speedDataSlider", min: -1.0, max: 1.0, value: 0, step: .01 });

        // UMS player
        embedPlayer('player-container', {
            flashvars: {
                'rtmp': 'rtmp://192.168.1.67:5119/live/TestCam',
                'autoplay': true,
                'soundIcon': false,
                'playPauseIcon': false,
                'allowzoom': false
            },
            size: { width: 640, height: 360 },
            playerStyle: 'glow'
        });  

        // resize the canvas to fill browser window dynamically
        addEventListener("resize", resizeAllCanvas, false);

        // initial canvas draw
        resizeAllCanvas();

        function resizeAllCanvas() {
            resizeCanvas("color_picker_div", "color_picker", "Images/colorHueBar.jpg");
        }

        function resizeCanvas(canvasDivId, canvasId, imageSrc) {
            var canvasDiv = $("#" + canvasDivId);
            var canvas = document.getElementById(canvasId);
            canvas.width = canvasDiv.innerWidth <= 0 ? canvas.width : canvasDiv.innerWidth();
            //canvas.height = canvasDiv.innerHeight <= 0 ? canvas.height : canvasDiv.innerHeight();
            loadImage(canvasId, imageSrc);
        }

        function loadImage(canvasId, imageSrc) {
            // canvas
            var canvas = document.getElementById(canvasId);
            var context = canvas.getContext('2d');

            // create an image object and get it’s source
            var img = new Image();
            img.src = imageSrc;

            // copy the image to the canvas
            $(img).load(function () {
                context.drawImage(img, 0, 0, canvas.width, canvas.height);
            });
        }

        this.appendToTextArea = function appendToTextArea(text) {
            // append date
            var date = new Date();
            $('#debug').append("[" + date.getHours() + ":" + date.getMinutes() + ":" + date.getSeconds() + ":" + date.getMilliseconds() + "]");

            // append text
            $('#debug').append(text + "\n");

            // auto-scroll to bottom
            $('#debug').scrollTop($('#debug')[0].scrollHeight);
        }

        this.setPowerSwitch = function setPowerSwitch(on) {
            var powerSwitch = $("#on_off");
            this.appendToTextArea("setting power from " + powerSwitch.bootstrapSwitch('state') + " to " + on);

            // enable
            if (powerSwitch.bootstrapSwitch('disabled')) {
                powerSwitch.bootstrapSwitch('toggleDisabled');
            }

            // toggle
            if (on && !powerSwitch.bootstrapSwitch('state')) {
                powerSwitch.bootstrapSwitch('toggleState');
            }
            else if (!on && powerSwitch.bootstrapSwitch('state')) {
                powerSwitch.bootstrapSwitch('toggleState');
            }
        }

        this.setRgbSlider = function setRgbSlider(value) {
            this.appendToTextArea("setting rgbSlider from " + rgbSlider.getValue() + " to " + value);
            if (rgbSlider.getValue() != value) {
                rgbSlider.setValue(parseFloat(value));
            }

            // enable
            rgbSlider.enable();
        }

        this.setSpeedSlider = function setSpeedSlider(value) {
            this.appendToTextArea("setting speedSlider from " + speedSlider.getValue() + " to " + value);
            if (speedSlider.getValue() != value) {
                speedSlider.setValue(parseFloat(value));
            }

            // enable
            speedSlider.enable();
        }

        this.initControlCompEvents = function initControlCompEvents(control) {
            var canvas = document.getElementById("color_picker");

            // on/off toggle switch
            $('input[id="on_off"]').on('switchChange.bootstrapSwitch', function (event, state) {
                control.sendPowerRequest(state);
            });

            // RGB Slider
            rgbSlider.on('slideStart', function (ev) {
                originaRgbSliderVal = rgbSlider.getValue();
            });
            rgbSlider.on('slideStop', function (ev) {
                var newVal = rgbSlider.getValue();
                if (originaRgbSliderVal != newVal) {
                    var x = canvas.width * newVal;
                    control.sendRgbRequestUsingCanvasAtX(canvas, x);
                    control.sendRgbSliderValue(newVal);
                }
            });

            // speed/direction slider
            speedSlider.on('slideStart', function (ev) {
                originalSpeedSliderVal = speedSlider.getValue();
            });
            speedSlider.on('slideStop', function (ev) {
                var newVal = speedSlider.getValue();
                if (originalSpeedSliderVal != newVal) {
                    var x = canvas.width * newVal;
                    control.sendSpeedRequest(newVal);
                }
            });
        }
    }
    
    // expose access
    window.UI = UI;

})(window);
