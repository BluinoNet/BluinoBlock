using Iot.Device.DHTxx.Esp32;
using System;
using System.Device.Gpio;
using System.Text;
using System.Threading;

namespace BluinoNet.Modules
{

    public class DHT11Module
    {
      
        Dht11 sensor;
        /// <summary>
        /// Pin trigger and echo to the same data pin in DHT11/22
        /// </summary>
        /// <param name="PinTrigger"></param>
        /// <param name="PinEcho"></param>
        public DHT11Module(int PinTrigger, int PinEcho)
        {
            sensor = new Dht11(PinEcho, PinTrigger);

        }
        /// <summary>
        /// Return temp in celcius
        /// </summary>
        /// <returns></returns>
        public double ReadTemperature()
        {
           if(sensor.IsLastReadSuccessful)
            {
                return sensor.Temperature.DegreesCelsius;
            }
            return 0;
        }
        
        /// <summary>
        /// return humidity in percent
        /// </summary>
        /// <returns></returns>
        public double ReadHumidity()
        {
           if(sensor.IsLastReadSuccessful)
            {
                return sensor.Humidity.Percent;
            }
            return 0;
        }
    }
            /*
            public class DHT11Module
            {
                public int TemperatureC { get; set; }
                public int Humidity { get; set; }
                GpioPin Pin;
                const int DHTLIB_OK = 0;
                const int DHTLIB_ERROR_CHECKSUM = -1;
                const int DHTLIB_ERROR_TIMEOUT = -2;

                public DHT11Module(int PinNumber)
                {
                    var gpio = new GpioController();
                    Pin = gpio.OpenPin(PinNumber, PinMode.Output);
                }
                public int Read()
                {
                    // BUFFER TO RECEIVE
                    var bits = new int[5];
                    int cnt = 7;
                    int idx = 0;

                    // EMPTY BUFFER
                    for (int i = 0; i < 5; i++) bits[i] = 0;

                    // REQUEST SAMPLE
                    //pinMode(pin, OUTPUT);
                    //digitalWrite(pin, LOW);
                    Pin.SetPinMode(PinMode.Output);
                    Pin.Write(PinValue.Low);
                    Thread.Sleep(18);
                    //digitalWrite(pin, HIGH);
                    Pin.Write(PinValue.High);

                    Thread.Sleep(1);
                    //delayMicroseconds(40);
                    //pinMode(pin, INPUT);
                    Pin.SetPinMode(PinMode.Input);

                    // ACKNOWLEDGE or TIMEOUT
                    int loopCnt = 10000;
                    while (Pin.Read() == PinValue.Low)
                        if (loopCnt-- == 0) return DHTLIB_ERROR_TIMEOUT;

                    loopCnt = 10000;
                    while (Pin.Read() == PinValue.High)
                        if (loopCnt-- == 0) return DHTLIB_ERROR_TIMEOUT;

                    // READ OUTPUT - 40 BITS => 5 BYTES or TIMEOUT
                    for (int i = 0; i < 40; i++)
                    {
                        loopCnt = 10000;
                        while (Pin.Read() == PinValue.Low)
                            if (loopCnt-- == 0) return DHTLIB_ERROR_TIMEOUT;

                        long t = DateTime.UtcNow.Ticks;

                        loopCnt = 10000;
                        while (Pin.Read() == PinValue.High)
                            if (loopCnt-- == 0) return DHTLIB_ERROR_TIMEOUT;

                        if ((DateTime.UtcNow.Ticks - t) > 40) bits[idx] |= (1 << cnt);
                        if (cnt == 0)   // next byte?
                        {
                            cnt = 7;    // restart at MSB
                            idx++;      // next byte!
                        }
                        else cnt--;
                    }

                    // WRITE TO RIGHT VARS
                    // as bits[1] and bits[3] are allways zero they are omitted in formulas.
                    Humidity = bits[0];
                    TemperatureC = bits[2];

                    int sum = bits[0] + bits[2];

                    if (bits[4] != sum) return DHTLIB_ERROR_CHECKSUM;
                    return DHTLIB_OK;
                }
            }*/
        }
