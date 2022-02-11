using System;
using System.Device.Gpio;
using System.Text;
using System.Threading;

namespace BluinoNet.Modules
{
    public class TouchSensor
    {
        bool IsTouching = false;
        public delegate void TouchedEventHandler(object source, TouchedArgs args);
        public event TouchedEventHandler? Touched;
        public class TouchedArgs : EventArgs
        {
            public DateTime TimeCaptured { get; set; }
        }
        bool IsListen = false;
        Thread Listener;
        GpioPin TouchPin;
        public TouchSensor(int PinNumber)
        {
            var gpio = new GpioController();
            TouchPin = gpio.OpenPin(PinNumber, PinMode.Input);
        }

        public void StartListener()
        {
            if (IsListen) return;
            Listener = new Thread(new ThreadStart(DoListen));
            Listener.Start();
            IsListen = true;
        }

        void DoListen()
        {
            while (true)
            {
                if(TouchPin.Read() == PinValue.High && !IsTouching)
                {
                    IsTouching = true;
                    Touched?.Invoke(this, new TouchedArgs() { TimeCaptured = DateTime.UtcNow });
                }
                else
                {
                    IsTouching = false;
                }
                Thread.Sleep(250);
            }
        }

        public bool IsTouched()
        {
            return TouchPin.Read() == PinValue.High;
        }
    }
}
