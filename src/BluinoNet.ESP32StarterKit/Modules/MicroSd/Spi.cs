
using System.Device.Spi;
using System.Diagnostics;

namespace BluinoNet.Modules.FatFsNano
{
    static class Spi
    {
        static SpiDevice device = null;
        
        /* usi.S: Initialize MMC control ports */
        public static void InitSpi()
        {
            if (device == null)
            {
                //DUMMY_CS_PIN_NUM
                var settings = new SpiConnectionSettings(MicroSdConfiguration.SPI_BUS_ID)   // The slave's select pin. Not used. CS is controlled by by GPIO pin
                {
                    Mode = SpiMode.Mode0,
                    ClockFrequency = 15 * 1000 * 1000,       //15 Mhz
                    DataBitLength = 8,
                };
                device = SpiDevice.Create(settings);
                Debug.WriteLine("Spi device successfully created");
            }

        }

        /* usi.S: Send a byte to the MMC */
        public static void XmitSpi(byte d)
        {
            byte[] writeBuf = { d };
            device.Write(writeBuf);
        }

        /* usi.S: Send a 0xFF to the MMC and get the received byte */
        public static byte RcvSpi()
        {
            byte[] writeBuf = { 0xff };
            byte[] readBuf = { 0x00 };

            device.TransferFullDuplex(writeBuf, readBuf);
            return readBuf[0];
        }

    }
}
