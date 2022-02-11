using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Adc;
using System.Text;

namespace BluinoNet.Modules
{
	public class Potentiometer
	{

		private AdcChannel input;

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public Potentiometer(int PinNumber, int ChannelNumber, DeviceFunction Adc)
		{
			Configuration.SetPinFunction(PinNumber, Adc);
			AdcController adc1 = new AdcController();

			this.input = adc1.OpenChannel(ChannelNumber);
		}

		/// <summary>The voltage returned from the sensor.</summary>
		/// <returns>The voltage value between 0.0 and 3.3</returns>
		public double ReadVoltage()
		{
			return this.input.ReadValue();
		}

		/// <summary>The proportion returned from the sensor.</summary>
		/// <returns>The value between 0.0 and 1.0</returns>
		public double ReadProportion()
		{
			return this.input.ReadRatio();
		}

	}
}
