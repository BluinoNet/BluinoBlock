using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Threading;

namespace BluinoNet.Modules
{
    public class RgbLedPwm
    {
        PwmChannel redLed { set; get; }
        PwmChannel greenLed { set; get; }
        PwmChannel blueLed { set; get; }

        readonly double dutyCycleMax = .3; // RGB Led doesn't seem to get much brighter than at 30%

        public RgbLedPwm(int PinR, int PinG, int PinB)
        {
            Configuration.SetPinFunction(PinR, ESP32Pwms.PWM1);
            Configuration.SetPinFunction(PinG, ESP32Pwms.PWM2);
            Configuration.SetPinFunction(PinB, ESP32Pwms.PWM3);
            redLed = PwmChannel.CreateFromPin(PinR);
            greenLed = PwmChannel.CreateFromPin(PinG);
            blueLed = PwmChannel.CreateFromPin(PinB);

        }

        public void SetColorRGB(int R, int G, int B)
        {
            if (R < 0 || R > 255)
            {
                throw new ArgumentException("R must be 0-255");
            }
            if (G < 0 || G > 255)
            {
                throw new ArgumentException("G must be 0-255");
            }
            if (B < 0 || B > 255)
            {
                throw new ArgumentException("B must be 0-255");
            }
            redLed.Start();
            greenLed.Start();
            blueLed.Start();

            double r = R / 255f;
            double g = R / 255f;
            double b = R / 255f;

            redLed.DutyCycle = (r * dutyCycleMax);
            greenLed.DutyCycle = (g * dutyCycleMax);
            blueLed.DutyCycle = (b * dutyCycleMax);
        }

        public void SetColorHSV(int Hue, int Saturation = 1, int Brightness = 1)
        {

            if (Hue < 0 || Hue >= 360)
            {
                throw new ArgumentException("Hue must be 0-359");
            }
            if (Saturation < 0 || Saturation > 1)
            {
                throw new ArgumentException("Saturation must be 0-1");
            }
            if (Brightness < 0 || Brightness > 1)
            {
                throw new ArgumentException("Brightness must be 0-1");
            }
            redLed.Start();
            greenLed.Start();
            blueLed.Start();

            double r, g, b;


            HsvToRgb(Hue, Saturation, Brightness, out r, out g, out b);


            redLed.DutyCycle = (r * dutyCycleMax);
            greenLed.DutyCycle = (g * dutyCycleMax);
            blueLed.DutyCycle = (b * dutyCycleMax);
        }
        /// <summary>
        /// HSV to rgb. Note that for RGB LED use, you probably want a constant 100% brightness. This doesn't do that. 
        /// For that Algorithm, check out: https://blog.adafruit.com/2012/03/14/constant-brightness-hsb-to-rgb-algorithm/
        /// </summary>
        /// <param name="hue">Hue in degress (0-359º).</param>
        /// <param name="saturation">Saturation.</param>
        /// <param name="brightValue">Brightness value.</param>
        /// <param name="r">The red component. (0-1)</param>
        /// <param name="g">The green component. (0-1)</param>
        /// <param name="b">The blue component. (0-1)</param>
        static void HsvToRgb(double hue, double saturation, double brightValue, out double r, out double g, out double b)
        {
            double H = hue;
            double R, G, B;

            // hue parameter checking/fixing
            if (H < 0 || H >= 360)
            {
                H = 0;
            }
            // if Brightness is turned off, then everything is zero.
            if (brightValue <= 0)
            {
                R = G = B = 0;
            }

            // if saturation is turned off, then there is no color/hue. it's grayscale.
            else if (saturation <= 0)
            {
                R = G = B = brightValue;
            }
            else // if we got here, then there is a color to create.
            {
                double hf = H / 60.0;
                int i = (int)System.Math.Floor(hf);
                double f = hf - i;
                double pv = brightValue * (1 - saturation);
                double qv = brightValue * (1 - saturation * f);
                double tv = brightValue * (1 - saturation * (1 - f));

                switch (i)
                {

                    // Red Dominant
                    case 0:
                        R = brightValue;
                        G = tv;
                        B = pv;
                        break;

                    // Green Dominant
                    case 1:
                        R = qv;
                        G = brightValue;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = brightValue;
                        B = tv;
                        break;

                    // Blue Dominant
                    case 3:
                        R = pv;
                        G = qv;
                        B = brightValue;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = brightValue;
                        break;

                    // Red Red Dominant
                    case 5:
                        R = brightValue;
                        G = pv;
                        B = qv;
                        break;

                    // In case the math is out of bounds, this is a fix.
                    case 6:
                        R = brightValue;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = brightValue;
                        G = pv;
                        B = qv;
                        break;

                    // If the color is not defined, go grayscale
                    default:
                        R = G = B = brightValue;
                        break;
                }
            }
            r = Clamp(R);
            g = Clamp(G);
            b = Clamp(B);
        }

        /// <summary>
        /// Clamp a value to 0 to 1
        /// </summary>
        static double Clamp(double i)
        {
            if (i < 0) return 0;
            if (i > 1) return 1;
            return i;
        }
    }
}
