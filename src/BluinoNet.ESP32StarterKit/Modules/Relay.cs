using System;
using System.Device.Gpio;

namespace BluinoNet.Modules
{
    public class Relay
    {
        bool IsOn = false;
        GpioPin _Relay;
        public Relay(int RelayPin)
        {
            var gpio = new GpioController();
            _Relay = gpio.OpenPin(RelayPin, PinMode.Output);
            TurnOff();
        }

        public void TurnOn()
        {
            IsOn = true;
            _Relay.Write(PinValue.High);
        }
        public void Toggle()
        {
            IsOn = !IsOn;
            _Relay.Write(IsOn ? PinValue.High : PinValue.Low);
        }

        public void TurnOff()
        {
            IsOn = false;
            _Relay.Write(PinValue.Low);
        }
    }
}
