using System;
using System.Device.Gpio;

namespace BluinoNet.Modules
{
    public class Led
    {
        GpioPin _led;
        public Led(int LedPin)
        {
            var gpio = new GpioController();
            _led = gpio.OpenPin(LedPin, PinMode.Output);
        }

        public void TurnOn()
        {
            _led.Write(PinValue.High);
        }
        public void Toggle()
        {
            var state = _led.Read();
            _led.Write(state == PinValue.High ? PinValue.Low : PinValue.High);
        }
        
        public void TurnOff()
        {
            _led.Write(PinValue.Low);
        }
    }
}
