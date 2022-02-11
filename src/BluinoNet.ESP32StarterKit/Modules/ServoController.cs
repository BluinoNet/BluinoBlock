using nanoFramework.Hardware.Esp32;
using System;
//
using System.Device.Pwm;
using System.Text;

namespace BluinoNet.Modules
{
    public class ServoController
    {
        private PwmChannel servo;
        //private PwmController controller;
        private ServoController.ServoType type;
        private bool invertServo;
        private double minPulseLength;
        private double maxPulseLength;

        //public ServoController(PwmController pwmController, PwmChannel pwnChannel)
        public ServoController(int pinNumber, DeviceFunction PwmDeviceFunction=DeviceFunction.PWM1)
        {
            this.invertServo = false;

            Configuration.SetPinFunction(pinNumber, PwmDeviceFunction);

           this.servo = PwmChannel.CreateFromPin(pinNumber);

            this.ConfigurePulseParameters(0.5, 2.4);
            this.ConfigureAsPositional(false);
            this.EnsureFrequency();
        }

        public void ConfigureAsPositional(bool inverted)
        {
            this.type = ServoController.ServoType.Positional;
            this.invertServo = inverted;
        }

        public void ConfigureAsContinuous(bool inverted)
        {
            this.type = ServoController.ServoType.Continuous;
            this.invertServo = inverted;
        }

        public void ConfigurePulseParameters(double minimumPulseWidth, double maximumPulseWidth)
        {
            if (minimumPulseWidth > 1.5 || minimumPulseWidth < 0.1)
                throw new ArgumentOutOfRangeException("Must be between 0.1 and 1.5 ms");
            if (maximumPulseWidth > 3.0 || maximumPulseWidth < 1.6)
                throw new ArgumentOutOfRangeException("Must be between 1.6 and 3 ms");
            this.minPulseLength = minimumPulseWidth;
            this.maxPulseLength = maximumPulseWidth;
        }

        public void Set(double value)
        {
            if (this.type == ServoController.ServoType.Positional)
                this.FixedSetPosition(value);
            else
                this.ContiniousSetSpeed(value);
        }

        private void FixedSetPosition(double position)
        {
            if (position < 0.0 || position > 180.0)
                throw new ArgumentOutOfRangeException("degrees", "degrees must be between 0 and 180.");
            this.EnsureFrequency();
            if (this.invertServo)
                position = 180.0 - position;
            this.servo.DutyCycle = (position / 180.0 * (this.maxPulseLength / 20.0 - this.minPulseLength / 20.0) + this.minPulseLength / 20.0);
            this.servo.Start();
        }

        private void ContiniousSetSpeed(double speed)
        {
            if (speed < -100.0 || speed > 100.0)
                throw new ArgumentOutOfRangeException(nameof(speed), "degrees must be between -100 and 100.");
            speed += 100.0;
            this.FixedSetPosition(speed / 200.0 * 180.0);
        }

        private void EnsureFrequency() => this.servo.Frequency = (50);

        public void Stop() => this.servo.Stop();

        private enum ServoType
        {
            Positional,
            Continuous,
        }
    }
}
