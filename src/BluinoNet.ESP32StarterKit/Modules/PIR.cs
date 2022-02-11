using System;
using System.Device.Gpio;
using System.Text;

namespace BluinoNet.Modules
{
    public  class PIR
    {
        public delegate void MotionCapturedEventHandler(object source, MotionCapturedArgs args);
        public event MotionCapturedEventHandler? MotionCaptured; 
        public class MotionCapturedArgs : EventArgs
        {   
            public DateTime TimeCaptured { get; set; }
        }

        GpioPin PinInterupt;
        public bool IsCaptureMovement { get; set; } = false;

        public PIR(int PinNumber)
        {
            var gpio = new GpioController();
            PinInterupt = gpio.OpenPin(PinNumber, PinMode.InputPullUp);
            PinInterupt.ValueChanged += PinInterupt_ValueChanged;
        }

        private void PinInterupt_ValueChanged(object sender, PinValueChangedEventArgs e)
        {
           if(e.ChangeType == PinEventTypes.Rising)
            {
                IsCaptureMovement = true;
                MotionCaptured?.Invoke(this, new MotionCapturedArgs() { TimeCaptured = DateTime.UtcNow });
            }
            else
            {
                IsCaptureMovement = false;
            }
        }
    }
}
