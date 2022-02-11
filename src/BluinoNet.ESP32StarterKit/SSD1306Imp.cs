using BluinoNet.Modules;
using BMC.Drivers.BasicGraphics;
using System;
//
using System.Device.I2c;
using System.Text;

namespace BluinoNet
{
    public class SSD1306Imp : BasicGraphics, IDisposable
    {

        //SSD1306 screen;
        SSD1306 screen;
        public SSD1306Imp(int I2cBusId=1) : base(128, 64, ColorFormat.OneBpp)
        {
            var con = SSD1306.GetConnectionSettings(I2cBusId);
            I2cDevice dev = new I2cDevice(con);
            screen = new SSD1306(dev); 
        }
       
        public void Flush()
        {
            screen.DrawBufferNative(this.Buffer);
            //do nothing
        }

        public void Dispose()
        {
            screen.Dispose();
            //do nothing
        }
    }
}
