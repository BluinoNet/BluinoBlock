using System;

namespace BluinoNet.Modules.FatFsNano
{
    public  class MicroSdConfiguration
    {
        public static int CS_PIN_NUM;
        public static int DUMMY_CS_PIN_NUM;
        public static int SPI_BUS_ID  = 1;
        public static void Setup(int CsPinNumber, int SpiBusId=1)
        {
            CS_PIN_NUM= CsPinNumber;
            SPI_BUS_ID = SpiBusId;
        }
    }
}
