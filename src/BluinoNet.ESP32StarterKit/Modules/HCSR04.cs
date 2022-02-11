using System;
using System.Device.Gpio;
using System.Text;
using System.Threading;

namespace BluinoNet.Modules
{
    public class DistanceEventArgs : EventArgs
    {
        public float Distance { get; set; }

        public DistanceEventArgs(float distance)
        {
            Distance = distance;
        }
    }

    public delegate void DistanceDetectedEventHandler(object sender, DistanceEventArgs e);
    public class HCSR04 
    {
        #region Properties

        public float CurrentDistance { get; private set; } = -1;

        public float MinimumDistance => 2; //in cm
        public float MaximumDistance => 400; //in cm

        public event DistanceDetectedEventHandler DistanceDetected = delegate { };

        #endregion

        #region Member variables / fields

        /// <summary>
        ///     Trigger Pin.
        /// </summary>
        protected GpioPin _triggerPort;

        /// <summary>
        ///     Echo Pin.
        /// </summary>
        protected GpioPin _echoPort;

        protected long _tickStart;

        #endregion

        #region Constructors

        /// <summary>
        ///     Default constructor is private to prevent it being called.
        /// </summary>
        private HCSR04()
        {
        }

        /// <summary>
        ///     Create a new HCSR04 object and hook up the interrupt handler.
        /// </summary>
        /// <param name="triggerPin"></param>
        /// <param name="echoPin"></param>
        public HCSR04(int triggerPin, int echoPin)
        {
            var gpio = new GpioController();
            _triggerPort = gpio.OpenPin(triggerPin, PinMode.Output);
            _triggerPort.Write(PinValue.Low);

            _echoPort = gpio.OpenPin(echoPin, PinMode.InputPullUp);
            _echoPort.ValueChanged += EchoPortOnInterrupt;
        }


        #endregion

        public void MeasureDistance()
        {
            CurrentDistance = -1;

            // Raise trigger port to high for 10+ micro-seconds
            _triggerPort.Write(true);
            Thread.Sleep(1); //smallest amount of time we can wait

            // Start Clock
            _tickStart = DateTime.UtcNow.Ticks;
            // Trigger device to measure distance via sonic pulse
            _triggerPort.Write(false);
        }

        void EchoPortOnInterrupt(object sender, PinValueChangedEventArgs e)
        {
            if (_echoPort.Read() == PinValue.High) //echo is high
            {
                _tickStart = DateTime.UtcNow.Ticks;
                return;
            }

            // Calculate Difference
            float elapsed = DateTime.UtcNow.Ticks - _tickStart;

            // Return elapsed ticks
            // x10 for ticks to micro sec
            // divide by 58 for cm (assume speed of sound is 340m/s)
            CurrentDistance = elapsed / 580f;

            if (CurrentDistance < MinimumDistance || CurrentDistance > MaximumDistance)
                CurrentDistance = -1;

            this.DistanceDetected(this, new DistanceEventArgs(CurrentDistance));
        }
    }
}
