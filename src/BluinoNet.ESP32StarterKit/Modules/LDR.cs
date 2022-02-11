using nanoFramework.Hardware.Esp32;
using System;
using System.Diagnostics;
using System.Device.Adc;
using System.Text;
//using Windows.Devices.Adc;

namespace BluinoNet.Modules
{
	public class LDR
	{

		/// <summary>The maximum amount of lux the sensor can detect before becoming saturated.</summary>
		public const double MAX_ILLUMINANCE = 1000;
		private AdcChannel input;

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public LDR(int PinNumber, int ChannelNumber, DeviceFunction Adc)
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

		/// <summary>Returns the current sensor reading in lux.</summary>
		/// <returns>A reading in lux between 0 and MAX_ILLUMINANCE.</returns>
		public double GetIlluminance()
		{

			return (1-this.input.ReadRatio()) * LDR.MAX_ILLUMINANCE;
		}
	}
}
