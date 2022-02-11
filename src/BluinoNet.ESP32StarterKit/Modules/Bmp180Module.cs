using Iot.Device.Bmp180;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Text;

namespace BluinoNet.Modules
{
    public class Bmp180Module
    {
        Bmp180 BMP180;
        public Bmp180Module()
        {
            Configuration.SetPinFunction(ESP32Pins.IO22, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(ESP32Pins.IO21, DeviceFunction.I2C1_DATA);
            var i2c = new I2cConnectionSettings(1, Bmp180.DefaultI2cAddress, I2cBusSpeed.StandardMode);
            var i2cDevice = new I2cDevice(i2c);
            BMP180 = new Bmp180(i2cDevice);




        }

        /// <summary>
        /// in celcius
        /// </summary>
        /// <returns></returns>
        public double ReadTemperature()
        {
            var readResult = BMP180.ReadTemperature();

            return readResult.DegreesCelsius;

            //return 0;
        }
        /// <summary>
        /// in atmosphere
        /// </summary>
        /// <returns></returns>
        public double ReadPressure()
        {
            var readResult = BMP180.ReadPressure();

            return readResult.Atmospheres;

        }
        /// <summary>
        /// in atmosphere
        /// </summary>
        /// <returns></returns>
        public double ReadSeaLevelPressure()
        {
            var readResult = BMP180.ReadSeaLevelPressure();

            return readResult.Atmospheres;

        }
        
        /// <summary>
        /// in meter
        /// </summary>
        /// <returns></returns>
        public double ReadAltitude()
        {
            var readResult = BMP180.ReadAltitude();

            return readResult.Meters;

        }
    }
}