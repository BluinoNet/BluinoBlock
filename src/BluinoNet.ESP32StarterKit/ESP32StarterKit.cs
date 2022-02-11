using BluinoNet.Modules;
using Iot.Device.DHTxx.Esp32;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Pwm;
using System.Device.Spi;

namespace BluinoNet
{
    public class ESP32StarterKit
    {
        bool IsInitRelay = false;
        bool IsInitButton = false;
        bool IsInitBuzzer = false;
        bool IsInitLed = false;
        bool IsInitRgb = false;
        bool IsInitMicroSd = false;
        bool IsInitLight = false;
        bool IsInitPir = false;
        bool IsInitTouch = false;
        bool IsInitPotentiometer = false;
        bool IsInitBME = false;
        bool IsInitDS18 = false;
        bool IsInitDHT11 = false;
        bool IsInitDisplay = false;
        bool IsInitMpu = false;
        bool IsInitServo = false;

        GpioController controller;
        // Browse our samples repository: https://github.com/nanoframework/samples
        // Check our documentation online: https://docs.nanoframework.net/
        // Join our lively Discord community: https://discord.gg/gCyBu8T
        public Relay BoardRelay { get; set; }
        public Button BoardButton1 { get; set; }
        public Button BoardButton2 { get; set; }
        public Tunes BoardBuzzer { get; set; }
        public Led BoardLed1 { get; set; }
        public Led BoardLed2 { get; set; }
        public Led BoardLed3 { get; set; }
        public Led BoardLed4 { get; set; }
        public RgbLedPwm BoardLedRgb { get; set; }
        public MicroSd BoardMicroSd { get; set; }
        public LDR BoardLightSensor { get; set; }
        public PIR BoardPIR { get; set; }
        public TouchSensor BoardTouchSensor { get; set; }
        public Potentiometer BoardPotentiometer { get; set; }
        public Bmp180Module BoardBMP180 { get; set; }
        public DS18B20 BoardDS18B20 { get; set; }
        public DHT11Module BoardDHT11 { get; set; }
        public SSD1306Imp BoardDisplay { get; set; }
        public Mpu6050 BoardMpu6050 { get; set; }
        public ServoController BoardServo { get; set; }
        public HCSR04 BoardHCSR04 { get; set; }

        /// <summary>
        /// Get SPI Interface for module communication
        /// </summary>
        /// <param name="ChipSelectPin">Chip Select pin</param>
        /// <param name="ClockFrequency">Frekunsi SPI</param>
        /// <param name="DataBitLength">Databit length</param>
        /// <param name="Mode">SPI Mode</param>
        /// <param name="SpiBus">Default = 1</param>
        /// <returns></returns>
        public SpiDevice GetSpi(int ChipSelectPin, int ClockFrequency, int DataBitLength, SpiMode Mode, int SpiBus = 1)
        {
            Configuration.SetPinFunction(ESP32Pins.IO23, DeviceFunction.SPI1_MOSI);
            Configuration.SetPinFunction(ESP32Pins.IO19, DeviceFunction.SPI1_CLOCK);
            Configuration.SetPinFunction(ESP32Pins.IO25, DeviceFunction.SPI1_MISO);

            var spiSettings = new SpiConnectionSettings(SpiBus, ChipSelectPin);
            spiSettings.ClockFrequency = ClockFrequency;
            spiSettings.DataBitLength = DataBitLength;
            spiSettings.Mode = Mode;

            var spi = SpiDevice.Create(spiSettings);
            return spi;
        }

        /// <summary>
        /// Get i2c device for communication
        /// </summary>
        /// <param name="DeviceAddress">address of device</param>
        /// <param name="BusSpeed">standard or fast</param>
        /// <param name="i2cBus">default = 1</param>
        /// <returns></returns>
        public I2cDevice GetI2C(int DeviceAddress, I2cBusSpeed BusSpeed, int i2cBus = 1)
        {
            Configuration.SetPinFunction(ESP32Pins.IO22, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(ESP32Pins.IO21, DeviceFunction.I2C1_DATA);
            var i2c = new I2cConnectionSettings(i2cBus, DeviceAddress, BusSpeed);

            var device = new I2cDevice(i2c);
            return device;
        }

        /// <summary>
        /// Get GPIO for Specific Pin 
        /// </summary>
        /// <param name="Pin">pin number</param>
        /// <param name="Mode">input, output or interupt</param>
        /// <returns></returns>
        public GpioPin GetGpio(int Pin, PinMode Mode)
        {
            var device = controller.OpenPin(Pin, Mode);
            return device;
        }

        /// <summary>
        /// Get PWM Channel by PinNumber
        /// </summary>
        /// <param name="pinNumber"></param>
        /// <param name="PwmId">Default is PWM 1</param>
        /// <returns></returns>
        public PwmChannel GetPwm(int pinNumber, DeviceFunction PwmId = ESP32Pwms.PWM1)
        {
            Configuration.SetPinFunction(pinNumber, PwmId);
            var pwm = PwmChannel.CreateFromPin(pinNumber);
            return pwm;
        }
        public ESP32StarterKit(int PinButton1,int PinButton2, int PinBuzzer, int PinLed1,int PinLed2,int PinLed3,int PinLed4, 
            int PinRelay) : this()
        {
            SetupRelay(PinRelay);

            SetupButton(PinButton1,PinButton2);

            SetupBuzzer(PinBuzzer);

            SetupLed(PinLed1, PinLed2, PinLed3, PinLed4);

        }

        public ESP32StarterKit()
        {
            controller = new GpioController();
        }
        public void SetupLed(int PinLed1, int PinLed2=-1, int PinLed3=-1, int PinLed4=-1)
        {
            if (PinLed1 > 0)
                this.BoardLed1 = new Led(PinLed1);
            if(PinLed2 > 0)
                this.BoardLed2 = new Led(PinLed2);
            if(PinLed3 > 0)
                this.BoardLed3 = new Led(PinLed3);
            if(PinLed4>0)
                this.BoardLed4 = new Led(PinLed4);
            IsInitLed = true;
        }

        public void SetupBuzzer(int PinBuzzer)
        {
            this.BoardBuzzer = new Tunes(PinBuzzer);
            IsInitBuzzer = true;

        } 
        
        public void SetupHCSR04(int PinTrigger,int PinEcho)
        {
            this.BoardHCSR04 = new HCSR04 (PinTrigger,PinEcho);

        }
        public void SetupPIR(int PinPIR)
        {
            this.BoardPIR = new PIR(PinPIR);
            IsInitPir = true;

        }
        public void SetupButton(int PinButton1,int PinButton2)
        {
            this.BoardButton1 = new Button(PinButton1);
            this.BoardButton2 = new Button(PinButton2);
            IsInitButton = true;

        }
        public void SetupRelay(int PinRelay)
        {
            this.BoardRelay = new Relay(PinRelay);
            IsInitRelay = true;

        }

        public void SetupServo(int PinServo)
        {
            this.BoardServo = new ServoController(PinServo);
            IsInitServo = true;

        }

        public void SetupLedRgb(int PinR,int PinG,int PinB)
        {
            this.BoardLedRgb = new RgbLedPwm(PinR,PinG,PinB);
            IsInitRgb = true;

        }

        public void SetupLightSensor(int PinNumber, int ChannelNumber, int AdcDev)
        {
            this.BoardLightSensor = new LDR (PinNumber,ChannelNumber,(DeviceFunction)AdcDev);
            IsInitLight = true;

        }
        public void SetupTouchSensor(int PinNumber)
        {
            this.BoardTouchSensor = new TouchSensor(PinNumber);
            IsInitTouch = true;

        }
        public void SetupPotentiometer(int PinNumber, int ChannelNumber, int AdcDev)
        {
            this.BoardPotentiometer = new Potentiometer (PinNumber, ChannelNumber, (DeviceFunction)AdcDev);
            IsInitPotentiometer = true;

        }

        public void SetupDS18B20(int OneWirePin)
        {
            this.BoardDS18B20 = new DS18B20(OneWirePin);
            IsInitDS18 = true;

        }

        public void SetupMicroSD(int CSPin)
        {
            Modules.FatFsNano.MicroSdConfiguration.Setup(CSPin,1);
        }
        public void SetupDHT11(int PinEcho, int PinTrigger)
        {
          
            BoardDHT11 = new  DHT11Module(PinTrigger,PinEcho);   
            IsInitDHT11 = true;

        }

        public void SetupDisplay()
        {
            Configuration.SetPinFunction(ESP32Pins.IO22, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(ESP32Pins.IO21, DeviceFunction.I2C1_DATA);
            BoardDisplay = new SSD1306Imp();
            IsInitDisplay = true;

        }

        public void SetupBMP180()
        {
            BoardBMP180 = new Bmp180Module();
            IsInitBME = true;

        }
        public void SetupMpu6050()
        {
            BoardMpu6050 = new Mpu6050();
            IsInitMpu = true;

        }
    }
    public class ESP32Pwms
    {
        public const DeviceFunction PWM1 = DeviceFunction.PWM1;
        public const DeviceFunction PWM2 = DeviceFunction.PWM2;
        public const DeviceFunction PWM3 = DeviceFunction.PWM3;
        public const DeviceFunction PWM4 = DeviceFunction.PWM4;
        public const DeviceFunction PWM5 = DeviceFunction.PWM5;
        public const DeviceFunction PWM6 = DeviceFunction.PWM6;
        public const DeviceFunction PWM7 = DeviceFunction.PWM7;
        public const DeviceFunction PWM8 = DeviceFunction.PWM8;
        public const DeviceFunction PWM9 = DeviceFunction.PWM9;
        public const DeviceFunction PWM10 = DeviceFunction.PWM10;
        public const DeviceFunction PWM11 = DeviceFunction.PWM11;
        public const DeviceFunction PWM12 = DeviceFunction.PWM12;
        public const DeviceFunction PWM13 = DeviceFunction.PWM13;
        public const DeviceFunction PWM14 = DeviceFunction.PWM14;
        public const DeviceFunction PWM15 = DeviceFunction.PWM15;
        public const DeviceFunction PWM16 = DeviceFunction.PWM16;

    }
    
    public class ESP32ADCs
    {
        public const int ADC0 = (int)DeviceFunction.ADC1_CH0;
        public const int ADC1 = (int)DeviceFunction.ADC1_CH1;
        public const int ADC2 = (int)DeviceFunction.ADC1_CH2;
        public const int ADC3 = (int)DeviceFunction.ADC1_CH3;
        public const int ADC4 = (int)DeviceFunction.ADC1_CH4;
        public const int ADC5 = (int)DeviceFunction.ADC1_CH5;
        public const int ADC6 = (int)DeviceFunction.ADC1_CH6;
        public const int ADC7 = (int)DeviceFunction.ADC1_CH7;
        public const int ADC8 = (int)DeviceFunction.ADC1_CH8;
        public const int ADC9 = (int)DeviceFunction.ADC1_CH9;
        public const int ADC10 =(int)DeviceFunction.ADC1_CH10;
        public const int ADC11 =(int)DeviceFunction.ADC1_CH11;
        public const int ADC12 =(int)DeviceFunction.ADC1_CH12;
        public const int ADC13 =(int)DeviceFunction.ADC1_CH13;
        public const int ADC14 =(int)DeviceFunction.ADC1_CH14;
        public const int ADC15 =(int)DeviceFunction.ADC1_CH15;
        public const int ADC16 =(int)DeviceFunction.ADC1_CH16;
        public const int ADC17 =(int)DeviceFunction.ADC1_CH17;
        public const int ADC18 =(int)DeviceFunction.ADC1_CH18;
        public const int ADC19 = (int)DeviceFunction.ADC1_CH19;

    }
    public class ESP32Pins
    {
        public const int IO00 = 0;

        //
        // Summary:
        //     Gpio IO01 (UART0 TXD)
        public const int IO01 = 1;

        //
        // Summary:
        //     Gpio IO02
        public const int IO02 = 2;

        //
        // Summary:
        //     Gpio IO03 (UART0 RXD)
        public const int IO03 = 3;

        //
        // Summary:
        //     Gpio IO04
        public const int IO04 = 4;

        //
        // Summary:
        //     Gpio IO05
        public const int IO05 = 5;

        //
        // Summary:
        //     Gpio IO06 (Reserved for SPI flash)
        public const int IO06 = 6;

        //
        // Summary:
        //     Gpio IO07 (Reserved for SPI flash)
        public const int IO07 = 7;

        //
        // Summary:
        //     Gpio IO08 (Reserved for SPI flash)
        public const int IO08 = 8;

        //
        // Summary:
        //     Gpio IO09 (Reserved for SPI flash)
        public const int IO09 = 9;

        //
        // Summary:
        //     Gpio IO10 (Reserved for SPI flash)
        public const int IO10 = 10;

        //
        // Summary:
        //     Gpio IO11 (Reserved for SPI flash)
        public const int IO11 = 11;

        //
        // Summary:
        //     Gpio IO12 (also used for JTAG TDI)
        public const int IO12 = 12;

        //
        // Summary:
        //     Gpio IO13 (also used for JTAG TCK)
        public const int IO13 = 13;

        //
        // Summary:
        //     Gpio IO14 (also used for JTAG TMS)
        public const int IO14 = 14;

        //
        // Summary:
        //     Gpio IO15 (also used for JTAG TDO)
        public const int IO15 = 15;

        //
        // Summary:
        //     Gpio IO16
        public const int IO16 = 16;

        //
        // Summary:
        //     Gpio IO17
        public const int IO17 = 17;

        //
        // Summary:
        //     Gpio IO18
        public const int IO18 = 18;

        //
        // Summary:
        //     Gpio IO19
        public const int IO19 = 19;

        //
        // Summary:
        //     Gpio IO20, No Physical pin for IO20
        public const int IO20 = 20;

        //
        // Summary:
        //     Gpio IO21
        public const int IO21 = 21;

        //
        // Summary:
        //     Gpio IO22
        public const int IO22 = 22;

        //
        // Summary:
        //     Gpio IO23
        public const int IO23 = 23;

        //
        // Summary:
        //     Gpio IO24, No Physical pin for IO24
        public const int IO24 = 24;

        //
        // Summary:
        //     Gpio IO25
        public const int IO25 = 25;

        //
        // Summary:
        //     Gpio IO26
        public const int IO26 = 26;

        //
        // Summary:
        //     Gpio IO27
        public const int IO27 = 27;

        //
        // Summary:
        //     Gpio IO28, No Physical pin for IO28
        public const int IO28 = 28;

        //
        // Summary:
        //     Gpio IO29, No Physical pin for IO29
        public const int IO29 = 29;

        //
        // Summary:
        //     Gpio IO30, No Physical pin for IO30
        public const int IO30 = 30;

        //
        // Summary:
        //     Gpio IO31, No Physical pin for IO31
        public const int IO31 = 31;

        //
        // Summary:
        //     Gpio IO32
        public const int IO32 = 32;

        //
        // Summary:
        //     Gpio IO33
        public const int IO33 = 33;

        //
        // Summary:
        //     Gpio IO34 (Input Only, no software pullup/pulldown functions)
        public const int IO34 = 34;

        //
        // Summary:
        //     Gpio IO35 (Input Only, no software pullup/pulldown functions)
        public const int IO35 = 35;

        //
        // Summary:
        //     Gpio IO36 SENSOR_VP (Input Only, no software pullup/pulldown functions)
        public const int IO36 = 36;

        //
        // Summary:
        //     Gpio IO37 (Input Only, no software pullup/pulldown functions)
        public const int IO37 = 37;

        //
        // Summary:
        //     Gpio IO38 (Input Only, no software pullup/pulldown functions)
        public const int IO38 = 38;

        //
        // Summary:
        //     Gpio IO39 SENSOR_VN (Input Only, no software pullup/pulldown functions)
        public const int IO39 = 39;

    }
}
