using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;

namespace Luminary
{
    public class ControlHub : Hub
    {
        // send RGB data over network using ARTNET protocol
        private static ArtNet.Engine ArtNetEngine;

        // keep track of how long our ARTNET calls take 
        private Stopwatch ArtNetStopWatch;

        public ControlHub()
        {
            ArtNetEngine = null;
            ArtNetStopWatch = new Stopwatch();
        }

        // SignalR client callback
        // turns on/off power to LEDs and train
        public void SetPower(Boolean on)
        {
            if (on)
            {
                // set pin mode to output 
                String SetPinModeToOutputUrl = ArduinoHelper.Property.GetModeOutputUrl(ArduinoHelper.Property.PowerPin);
                var content = ArduinoHelper.SendToArduino(SetPinModeToOutputUrl);
                Clients.All.debug(String.Format("Power: url={0}, content={1}", SetPinModeToOutputUrl, content));

                // set pin high
                String SetPinHighUrl = ArduinoHelper.Property.GetDigitalWriteUrl(ArduinoHelper.Property.PowerPin, ArduinoHelper.Property.High);
                content = ArduinoHelper.SendToArduino(SetPinHighUrl);
                Clients.All.debug(String.Format("Power: url={0}, content={1}", SetPinHighUrl, content));
            }
            else
            {
                // set pin low
                String SetPinLowUrl = ArduinoHelper.Property.GetDigitalWriteUrl(ArduinoHelper.Property.PowerPin, ArduinoHelper.Property.Low);
                var content = ArduinoHelper.SendToArduino(SetPinLowUrl);
                Clients.All.debug(String.Format("Power: url={0}, content={1}", SetPinLowUrl, content));
            }

            // cache power value
            String SetPowerValueUrl = ArduinoHelper.Property.GetDataPutUrl(ArduinoHelper.Property.PowerKey, on ? "true" : "false");
            var content2 = ArduinoHelper.SendToArduino(SetPowerValueUrl);
            Clients.All.debug(String.Format("RedGreenBlue: url={0}, content={1}", SetPowerValueUrl, content2));

            // send status to clients
            RequestStatus();
        }

        // SignalR client callback
        // sets the color of the LEDs
        public void SetRgb(int red, int green, int blue)
        {
            // create new RGB
            RGB rgb = new RGB();
            rgb.red = red;
            rgb.green = green;
            rgb.blue = blue;

            // send RGB to Arduino
            SendDXM(rgb);

            // notify clients
            Clients.All.debug("SetRgb: " + red + "," + green + "," + blue);    
        }

        // SignalR client callback
        // records the position of the RGB slider
        public void SetRgbSlider(float value)
        {
            // cache color slider value
            String sliderValue = ""+value;
            String SetColorSliderValueUrl = ArduinoHelper.Property.GetDataPutUrl(ArduinoHelper.Property.RgbSliderKey, sliderValue);
            var content = ArduinoHelper.SendToArduino(SetColorSliderValueUrl);
            Clients.All.debug(String.Format("SetRgbSlider: url={0}, content={1}", SetColorSliderValueUrl, content));

            // send status to clients
            RequestStatus();
        }

        // SignalR client callback
        // sets the speed and direction of the train
        public void SetSpeed(float value)
        {
            // get current speed slider value
            String GetSpeedStatusUrl = ArduinoHelper.Property.GetDataGetUrl(ArduinoHelper.Property.SpeedSliderKey);
            var SpeedJSon = ArduinoHelper.SendToArduino(GetSpeedStatusUrl);
            Clients.All.debug(String.Format("SetSpeed: url={0}, content={1}", GetSpeedStatusUrl, SpeedJSon));
            
            // cache speed slider value
            String sliderValue = ""+value;
            String SetSpeedSliderValueUrl = ArduinoHelper.Property.GetDataPutUrl(ArduinoHelper.Property.SpeedSliderKey, sliderValue);
            var content = ArduinoHelper.SendToArduino(SetSpeedSliderValueUrl);
            Clients.All.debug(String.Format("SetSpeed: url={0}, content={1}", SetSpeedSliderValueUrl, content));

            // Forward=[0..1.0], Backwards=[-1.0..0], Stop=[0]
            int speed;
            String direction;
            if (value < 0) // backward
            {
                speed = (int)(255.0 * (value * -1.0)); // 0-255
                direction = ArduinoHelper.Property.Backward;
            }
            else if (value > 0) // forward
            {
                speed = (int)(255.0 * value); // 0-255
                direction = ArduinoHelper.Property.Forward;
            }
            else // stop
            {
                speed = 0;
                direction = ArduinoHelper.Property.Release;
            }
            String SetSpeedUrl = ArduinoHelper.Property.GetMotorWriteUrl(speed, direction);
            content = ArduinoHelper.SendToArduino(SetSpeedUrl);
            Clients.All.debug(String.Format("SetSpeed: url={0}, content={1}", SetSpeedUrl, content));

            // send status to clients
            RequestStatus();
        }

        // SignalR client callback
        // send status of power, RGB slider, and speed slider
        public void RequestStatus()
        {
            // power
            String GetPowerStatusUrl = ArduinoHelper.Property.GetDataGetUrl(ArduinoHelper.Property.PowerKey);
            var PowerStatusJson = ArduinoHelper.SendToArduino(GetPowerStatusUrl);
            Clients.All.debug(String.Format("Status: url={0}, content={1}", GetPowerStatusUrl, PowerStatusJson));
            Clients.All.status(PowerStatusJson); 

            // RGB slider
            String GetColorSliderValueUrl = ArduinoHelper.Property.GetDataGetUrl(ArduinoHelper.Property.RgbSliderKey);
            var RgbSliderStatusJson = ArduinoHelper.SendToArduino(GetColorSliderValueUrl);
            Clients.All.debug(String.Format("Status: url={0}, content={1}", GetColorSliderValueUrl, RgbSliderStatusJson));
            Clients.All.status(RgbSliderStatusJson); 

            // speed slider
            String GetSpeedSliderValueUrl = ArduinoHelper.Property.GetDataGetUrl(ArduinoHelper.Property.SpeedSliderKey);
            var SpeedSliderStatusJson = ArduinoHelper.SendToArduino(GetSpeedSliderValueUrl);
            Clients.All.debug(String.Format("Status: url={0}, content={1}", GetSpeedSliderValueUrl, SpeedSliderStatusJson));
            Clients.All.status(SpeedSliderStatusJson);
        }

        private void SendDXM(RGB rgb) {
            // initialize the engine if needed
            if (ArtNetEngine == null)
            {
                System.Diagnostics.Debug.WriteLine("Staring ArtEngine...");
                ArtNetStopWatch.Restart();
                ArtNetEngine = new ArtNet.Engine("ArtNet Engine", "");
                ArtNetEngine.Start();
                ArtNetStopWatch.Stop();
                System.Diagnostics.Debug.WriteLine( String.Format("...took {0}ms", ArtNetStopWatch.ElapsedMilliseconds) );
            }

            // build DMX data
            System.Diagnostics.Debug.WriteLine("Building DMXData...");
            ArtNetStopWatch.Restart();
            byte[] DMXData = new byte[512];
            for(int i=0;i<DMXData.Length;i+=3) {
                DMXData[i] = (byte)rgb.red;
                if(i+1<DMXData.Length)
                    DMXData[i + 1] = (byte)rgb.green;
                if(i+2<DMXData.Length)
                    DMXData[i + 2] = (byte)rgb.blue;
            }
            ArtNetStopWatch.Stop();
            System.Diagnostics.Debug.WriteLine(String.Format("...took {0}ms", ArtNetStopWatch.ElapsedMilliseconds));

            // send DMX data
            System.Diagnostics.Debug.WriteLine("Sending ArtEngine data...");
            ArtNetStopWatch.Restart();
            ArtNetEngine.SendDMX(0, DMXData, DMXData.Length);
            ArtNetStopWatch.Stop();
            System.Diagnostics.Debug.WriteLine(String.Format("...took {0}ms", ArtNetStopWatch.ElapsedMilliseconds));
        }

        public class RGB
        {
            private static byte MIN = (byte)0;
            private static byte MAX = (byte)255;

            public int red { get; set; }
            public int green { get; set; }
            public int blue { get; set; }

            public RGB()
            {
                init(MIN, MIN, MIN);
            }

            public RGB(int red, int green, int blue)
            {
                init(red, green, blue);
            }

            private void init(int red, int green, int blue)
            {
                this.red = red;
                this.green = green;
                this.blue = blue;
            }
        }
    }
}