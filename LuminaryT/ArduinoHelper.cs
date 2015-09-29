using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Luminary
{
    public class ArduinoHelper
    {
        public static String SendToArduino(String url)
        {
            System.Diagnostics.Debug.WriteLine( String.Format("SendToArduino: url={0}", url) );
            System.Net.WebClient ArduinoWebClient = null;
            try
            {
                ArduinoWebClient = new System.Net.WebClient();
                var content = ArduinoWebClient.DownloadString(url);
                return content;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("SendToArduino: error={0}", e));
                return "error"+e;
            }
            finally
            {
                if (ArduinoWebClient != null)
                    ArduinoWebClient.Dispose();
            }
        }

        public class Property
        {
            public static readonly int PowerPin = 12;
            public static readonly int High = 1;
            public static readonly int Low = 0;

            public static readonly String PowerKey = "power";
            public static readonly String RgbSliderKey = "rgbSlider";
            public static readonly String SpeedSliderKey = "speedSlider";

            public static readonly String Forward = "forward";
            public static readonly String Backward = "backward";
            public static readonly String Release = "release";

            public static readonly string IP = "192.168.1.165";
            public static readonly string BaseUrl = "http://" + IP;
            public static readonly string ArduinoUrl = BaseUrl + "/arduino";
            public static readonly string DataUrl = BaseUrl + "/data";

            public static string GetDigitalReadUrl(int pin)
            {
                return ArduinoUrl + "/digital/" + pin;
            }

            public static string GetDigitalWriteUrl(int pin, int value)
            {
                return ArduinoUrl + "/digital/" + pin + "/" + value;
            }

            public static string GetAnalogReadUrl(int pin)
            {
                return ArduinoUrl + "/analog/" + pin;
            }

            public static string GetAnalogWriteUrl(int pin, int value)
            {
                return ArduinoUrl + "/analog/" + pin + "/" + value;
            }

            public static string GetModeInputUrl(int pin)
            {
                return ArduinoUrl + "/mode/" + pin + "/input";
            }

            public static string GetModeOutputUrl(int pin)
            {
                return ArduinoUrl + "/mode/" + pin + "/output";
            }

            public static string GetDataPutUrl(String key, String value)
            {
                return DataUrl + "/put/" + key + "/" + value;
            }

            public static string GetDataGetUrl(String key)
            {
                return DataUrl + "/get/" + key;
            }

            public static string GetDigitalKey(int pin)
            {
                return "D" + pin;
            }

            public static string getAnalogKey(int pin)
            {
                return "A" + pin;
            }

            public static string GetMotorWriteUrl(int speed, String direction)
            {
                return ArduinoUrl + "/motor/" + speed + "/" + direction;
            }
        }
    }
}