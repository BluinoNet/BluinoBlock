using System;
using System.Collections;
using System.Device.Gpio;
using System.Device.I2c;
using System.Text;
using System.Threading;

namespace BluinoNet.Modules
{
    
    public class MpuSensorValue
    {
        public float AccelerationX { get; set; }
        public float AccelerationY { get; set; }
        public float AccelerationZ { get; set; }
        public float GyroX { get; set; }
        public float GyroY { get; set; }
        public float GyroZ { get; set; }
    }

    public class MpuSensorEventArgs : EventArgs
    {
        public byte Status { get; set; }
        public float SamplePeriod { get; set; }
        public MpuSensorValue[] Values { get; set; }
    }
    public delegate void MpuSensorEventHandler(object source, MpuSensorEventArgs args);
    
    public partial class Mpu6050 : IDisposable
    {
        public event MpuSensorEventHandler SensorInterruptEvent;

        #region Constants
        const int DelayTime = 100;
        public const byte ADDRESS = 0x68;
        private const byte PWR_MGMT_1 = 0x6B;
        private const byte SMPLRT_DIV = 0x19;
        private const byte CONFIG = 0x1A;
        private const byte GYRO_CONFIG = 0x1B;
        private const byte ACCEL_CONFIG = 0x1C;
        private const byte FIFO_EN = 0x23;
        private const byte INT_ENABLE = 0x38;
        private const byte INT_STATUS = 0x3A;
        private const byte USER_CTRL = 0x6A;
        private const byte FIFO_COUNT = 0x72;
        private const byte FIFO_R_W = 0x74;

        private const int SensorBytes = 12;

        #endregion

        private const Int32 INTERRUPT_PIN = 18;
        I2cDevice _mpu6050Device = null;
        private GpioController IoController;

        #region 12c

        private byte ReadByte(byte regAddr)
        {
            byte[] buffer = new byte[1];
            buffer[0] = regAddr;
            byte[] value = new byte[1];
            _mpu6050Device.WriteRead(buffer, value);
            return value[0];
        }

        private byte[] ReadBytes(byte regAddr, int length)
        {
            byte[] values = new byte[length];
            byte[] buffer = new byte[1];
            buffer[0] = regAddr;
            _mpu6050Device.WriteRead(buffer, values);
            return values;
        }

        public ushort ReadWord(byte address)
        {
            byte[] buffer = ReadBytes(FIFO_COUNT, 2);
            return (ushort)(((int)buffer[0] << 8) | (int)buffer[1]);
        }

        void WriteByte(byte regAddr, byte data)
        {
            byte[] buffer = new byte[2];
            buffer[0] = regAddr;
            buffer[1] = data;
            _mpu6050Device.Write(buffer);
        }

        void writeBytes(byte regAddr, byte[] values)
        {
            byte[] buffer = new byte[1 + values.Length];
            buffer[0] = regAddr;
            Array.Copy(values, 0, buffer, 1, values.Length);
            _mpu6050Device.Write(buffer);
        }

        #endregion
        public Mpu6050(int I2cBusId=1)
        {
            InitHardware(I2cBusId);
        }
        public void InitHardware(int I2cBusId)
        {
            try
            {
                IoController = new GpioController();
                

                var settings = new I2cConnectionSettings(I2cBusId, ADDRESS, I2cBusSpeed.FastMode); //The slave's address and the bus speed.
                _mpu6050Device = new I2cDevice(settings);

               

                Thread.Sleep(3); // wait power up sequence

                WriteByte(PWR_MGMT_1, 0x80);// reset the device
                Thread.Sleep(100);
                WriteByte(PWR_MGMT_1, 0x2);
                WriteByte(USER_CTRL, 0x04); //reset fifo

                WriteByte(PWR_MGMT_1, 1); // clock source = gyro x
                WriteByte(GYRO_CONFIG, 0); // +/- 250 degrees sec
                WriteByte(ACCEL_CONFIG, 0); // +/- 2g

                WriteByte(CONFIG, 1); // 184 Hz, 2ms delay
                WriteByte(SMPLRT_DIV, 19);  // set rate 50Hz
                WriteByte(FIFO_EN, 0x78); // enable accel and gyro to read into fifo
                WriteByte(USER_CTRL, 0x40); // reset and enable fifo
                WriteByte(INT_ENABLE, 0x1);
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
            }
        }

        Thread Loop;
        bool IsUpdating = false;

        public void Looping()
        {
            while (true)
            {
                if (_mpu6050Device != null)
                {
                    int interrupt_status = ReadByte(INT_STATUS);
                    if ((interrupt_status & 0x10) != 0)
                    {
                        WriteByte(USER_CTRL, 0x44); // reset and enable fifo
                    }
                    if ((interrupt_status & 0x1) != 0)
                    {
                        MpuSensorEventArgs ea = new MpuSensorEventArgs();
                        ea.Status = (byte)interrupt_status;
                        ea.SamplePeriod = 0.02f;
                        ArrayList l = new ArrayList(); //new List<MpuSensorValue>();

                        int count = ReadWord(FIFO_COUNT);

                        while (count >= SensorBytes)
                        {
                            byte[] data = ReadBytes(FIFO_R_W, (byte)SensorBytes);
                            count -= SensorBytes;

                            short xa = (short)((int)data[0] << 8 | (int)data[1]);
                            short ya = (short)((int)data[2] << 8 | (int)data[3]);
                            short za = (short)((int)data[4] << 8 | (int)data[5]);

                            short xg = (short)((int)data[6] << 8 | (int)data[7]);
                            short yg = (short)((int)data[8] << 8 | (int)data[9]);
                            short zg = (short)((int)data[10] << 8 | (int)data[11]);

                            MpuSensorValue sv = new MpuSensorValue();
                            sv.AccelerationX = (float)xa / (float)16384;
                            sv.AccelerationY = (float)ya / (float)16384;
                            sv.AccelerationZ = (float)za / (float)16384;
                            sv.GyroX = (float)xg / (float)131;
                            sv.GyroY = (float)yg / (float)131;
                            sv.GyroZ = (float)zg / (float)131;
                            l.Add(sv);
                        }
                        ea.Values = (MpuSensorValue[])l.ToArray(typeof(MpuSensorValue));

                        if (SensorInterruptEvent != null)
                        {
                            if (ea.Values.Length > 0)
                            {
                                SensorInterruptEvent(this, ea);
                            }
                        }
                    }
                }
                Thread.Sleep(DelayTime);
                if (!IsUpdating) break;
            }
           
        }
        public void StartUpdating()
        {
           if(!IsUpdating)
            {
                Loop = new Thread(new ThreadStart(Looping));
                IsUpdating = true;
                Loop.Start();
            }
        }

        public void StopUpdating()
        {
            IsUpdating = false;
         
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (_mpu6050Device != null)
                {
                    _mpu6050Device.Dispose();
                    _mpu6050Device = null;
                }
                disposedValue = true;

            }
        }

        ~Mpu6050()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
    /*
    public class AccelerationConditionChangeResult
    {
        protected AccelerationConditions _newValue = new AccelerationConditions();
        protected AccelerationConditions _oldValue = new AccelerationConditions();

        public AccelerationConditions New
        {
            get => this._newValue;
            set
            {
                this._newValue = value;
                this.RecalcDelta();
            }
        }

        public AccelerationConditions Old
        {
            get => this._oldValue;
            set
            {
                this._oldValue = value;
                this.RecalcDelta();
            }
        }

        public AccelerationConditions Delta { get; protected set; } = new AccelerationConditions();

        public AccelerationConditionChangeResult(
          AccelerationConditions newValue,
          AccelerationConditions oldValue)
        {
            this.New = newValue;
            this.Old = oldValue;
        }

        protected void RecalcDelta()
        {
            AccelerationConditions accelerationConditions1 = new AccelerationConditions();
            AccelerationConditions accelerationConditions2 = accelerationConditions1;
            float xacceleration = this.New.XAcceleration;
            float nullable1 = this.Old.XAcceleration;
            float nullable2 = xacceleration != 0 & nullable1 != 0 ? (xacceleration - nullable1) : 0;
            accelerationConditions2.XAcceleration = nullable2;
            AccelerationConditions accelerationConditions3 = accelerationConditions1;
            nullable1 = this.New.YAcceleration;
            float nullable3 = this.Old.YAcceleration;
            float nullable4 = nullable1 != 0 & nullable3 != 0 ? (nullable1 - nullable3) : 0;
            accelerationConditions3.YAcceleration = nullable4;
            AccelerationConditions accelerationConditions4 = accelerationConditions1;
            nullable3 = this.New.ZAcceleration;
            nullable1 = this.Old.ZAcceleration;
            float nullable5 = nullable3 != 0 & nullable1 != 0 ? (nullable3 - nullable1) : 0;
            accelerationConditions4.ZAcceleration = nullable5;
            AccelerationConditions accelerationConditions5 = accelerationConditions1;
            nullable1 = this.New.XGyroscopicAcceleration;
            nullable3 = this.Old.XGyroscopicAcceleration;
            float nullable6 = nullable1 != 0 & nullable3 != 0 ? (nullable1 - nullable3) : 0;
            accelerationConditions5.XGyroscopicAcceleration = nullable6;
            AccelerationConditions accelerationConditions6 = accelerationConditions1;
            nullable3 = this.New.YGyroscopicAcceleration;
            nullable1 = this.Old.YGyroscopicAcceleration;
            float nullable7 = nullable3 != 0 & nullable1 != 0 ? (nullable3 - nullable1) : 0;
            accelerationConditions6.YGyroscopicAcceleration = nullable7;
            AccelerationConditions accelerationConditions7 = accelerationConditions1;
            nullable1 = this.New.ZGyroscopicAcceleration;
            nullable3 = this.Old.ZGyroscopicAcceleration;
            float nullable8 = nullable1 != 0 & nullable3 != 0 ? (nullable1 - nullable3) : 0;
            accelerationConditions7.ZGyroscopicAcceleration = nullable8;
            this.Delta = accelerationConditions1;
        }
    }
    public delegate void AccelerationConditionChangeResultHandler(object sender, AccelerationConditionChangeResult e);
    public class AccelerationConditions
    {
        public float XAcceleration { get; set; }

        public float YAcceleration { get; set; }

        public float ZAcceleration { get; set; }

        public float XGyroscopicAcceleration { get; set; }

        public float YGyroscopicAcceleration { get; set; }

        public float ZGyroscopicAcceleration { get; set; }

        public AccelerationConditions()
        {
        }

        public AccelerationConditions(
          float xAcceleration,
          float yAcceleration,
          float zAcceleration,
          float xGyroAcceleration,
          float yGyroAcceleration,
          float zGyroAcceleration)
        {
            this.XAcceleration = xAcceleration;
            this.YAcceleration = yAcceleration;
            this.ZAcceleration = zAcceleration;
            this.XGyroscopicAcceleration = xGyroAcceleration;
            this.YGyroscopicAcceleration = yGyroAcceleration;
            this.ZGyroscopicAcceleration = zGyroAcceleration;
        }

        public static AccelerationConditions From(
          AccelerationConditions conditions) => new AccelerationConditions(conditions.XAcceleration, conditions.YAcceleration, conditions.ZAcceleration, conditions.XGyroscopicAcceleration, conditions.YGyroscopicAcceleration, conditions.ZGyroscopicAcceleration);
    }
    public interface IAccelerometer
    {
        AccelerationConditions Conditions { get; }

        event AccelerationConditionChangeResultHandler Updated;
    }
    public class Mpu6050 :
         IAccelerometer, IDisposable
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x68,
            Address1 = 0x69,
            Default = Address0
        }

        private enum Register : byte
        {
            Config = 0x1a,
            GyroConfig = 0x1b,
            AccelConfig = 0x1c,
            InterruptConfig = 0x37,
            InterruptEnable = 0x38,
            InterruptStatus = 0x3a,
            PowerManagement = 0x6b,
            AccelerometerX = 0x3b,
            AccelerometerY = 0x3d,
            AccelerometerZ = 0x3f,
            Temperature = 0x41,
            GyroX = 0x43,
            GyroY = 0x45,
            GyroZ = 0x47
        }

        public event AccelerationConditionChangeResultHandler Updated;

        /// <summary>
        ///     Acceleration along the X-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float AccelerationX
        {
            get
            {
                if (IsSampling) { return Conditions.XAcceleration; } else { return ReadRegisterInt16(Register.AccelerometerX) * (1 << AccelerometerScale) / AccelScaleBase; }
            }
        }

        /// <summary>
        ///     Acceleration along the Y-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float AccelerationY
        {
            get
            {
                if (IsSampling) { return Conditions.YAcceleration; } else { return ReadRegisterInt16(Register.AccelerometerY) * (1 << AccelerometerScale) / AccelScaleBase; }
            }
        }

        /// <summary>
        ///     Acceleration along the Z-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float AccelerationZ
        {
            get
            {
                if (IsSampling) { return Conditions.ZAcceleration; } else { return ReadRegisterInt16(Register.AccelerometerZ) * (1 << AccelerometerScale) / AccelScaleBase; }
            }
        }

        public AccelerationConditions Conditions { get; protected set; } = new AccelerationConditions();

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        private const float GyroScaleBase = 131f;
        private const float AccelScaleBase = 16384f;

        // internal thread lock
        private object _lock = new object();
        //private CancellationTokenSource SamplingTokenSource;

        private float _temp;

        private float _lastTemp;

        private int GyroScale { get; set; }
        private int AccelerometerScale { get; set; }
        private I2cDevice Device { get; set; }

        public float GyroChangeThreshold { get; set; }
        public float AccelerationChangeThreshold { get; set; }
        public byte Address { get; private set; }

        public Mpu6050(int I2cbus=1, byte address = 0x68)
        {
            var settings = new I2cConnectionSettings(I2cbus,address, I2cBusSpeed.StandardMode); //The slave's address and the bus speed.
            Device = new I2cDevice(settings);

            Initialize(address);
        }

        public Mpu6050(int I2cBus, Addresses address)
            : this(I2cBus, (byte)address)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopUpdating();
            }
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        ///// <summary>
        ///// Starts continuously sampling the sensor.
        /////
        ///// This method also starts raising `Changed` events and IObservable
        ///// subscribers getting notified.
        ///// </summary>
        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock)
            {
                if (IsSampling) { return; }

                // state muh-cheen
                IsSampling = true;

                //SamplingTokenSource = new CancellationTokenSource();
                //CancellationToken ct = SamplingTokenSource.Token;

                AccelerationConditions oldConditions;
                AccelerationConditionChangeResult result;
                var task1 = new Thread(new ThreadStart(() => {
                    while (true)
                    {
                        if (!IsSampling)
                        {
                            // do task clean up here
                            //_observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = AccelerationConditions.From(Conditions);

                        // read
                        Update();

                        // build a new result with the old and new conditions
                        result = new AccelerationConditionChangeResult(Conditions, oldConditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        Thread.Sleep(standbyDuration);
                    }
                }));
                task1.Start();
            }
        }

        protected void RaiseChangedAndNotify(AccelerationConditionChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            //base.NotifyObservers(changeResult);
        }

        ///// <summary>
        ///// Stops sampling the temperature.
        ///// </summary>
        public void StopUpdating()
        {
            lock (_lock)
            {
                //if (!IsSampling) { return; }

                //SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

        private void Initialize(byte address)
        {
            switch (address)
            {
                case 0x68:
                case 0x69:
                    // valid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("MPU6050 device address must be either 0x68 or 0x69");
            }

            Address = address;

            Wake();
        }

        /// <summary>
        /// Gyroscope X measurement, in degrees per second
        /// </summary>
        public float XGyroscopicAcceleration
        {
            get
            {
                if (IsSampling)
                {
                    return Conditions.XGyroscopicAcceleration;
                }
                return ReadRegisterInt16(Register.GyroX) * (1 << GyroScale) / GyroScaleBase;
            }
        }

        /// <summary>
        /// Gyroscope Y measurement, in degrees per second
        /// </summary>
        public float YGyroscopicAcceleration
        {
            get
            {
                if (IsSampling)
                {
                    return Conditions.YGyroscopicAcceleration;
                }
                return ReadRegisterInt16(Register.GyroY) * (1 << GyroScale) / GyroScaleBase;
            }
        }

        /// <summary>
        /// Gyroscope Z measurement, in degrees per second
        /// </summary>
        public float ZGyroscopicAcceleration
        {
            get
            {
                if (IsSampling)
                {
                    return Conditions.ZGyroscopicAcceleration;
                }
                return ReadRegisterInt16(Register.GyroZ) * (1 << GyroScale) / GyroScaleBase;
            }
        }

        /// <summary>
        /// Temperature of sensor
        /// </summary>
        public float TemperatureC
        {
            get
            {
                if (IsSampling)
                {
                    return _temp;
                }
                return ReadRegisterInt16(Register.Temperature) * (1 << GyroScale) / GyroScaleBase;
            }
        }

        public void Wake()
        {
            Device.Write(new byte[] { Address, (byte)Register.PowerManagement, 0x00 });

            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            // read all 3 config bytes
            var data = new byte[3];
            Device.WriteRead(new byte[] { Address, (byte)Register.Config }, data);

            //var data = Device.WriteReadData(Address, 3, (byte)Register.Config);

            GyroScale = (data[1] & 0b00011000) >> 3;
            AccelerometerScale = (data[2] & 0b00011000) >> 3;
        }

        private short ReadRegisterInt16(Register register)
        {
            return ReadRegisterInt16((byte)register);
        }

        private short ReadRegisterInt16(byte register)
        {
            var data = new byte[2];
            Device.WriteRead(new byte[] { Address, register }, data);
            //var data = Device.WriteReadData(Address, 2, register);
            unchecked
            {
                return (short)(data[0] << 8 | data[1]); ;
            }
        }

        private void Update()
        {
            lock (_lock)
            {
                // we'll just read 14 bytes (7 registers), starting at 0x3b
                var data = new byte[14];
                Device.WriteRead(new byte[] { Address, (byte)Register.AccelerometerX }, data);
                //var data = Device.WriteReadData(Address, 14, (byte)Register.AccelerometerX);

                var a_scale = (1 << AccelerometerScale) / AccelScaleBase;
                var g_scale = (1 << GyroScale) / GyroScaleBase;
                Conditions.XAcceleration = ScaleAndOffset(data, 0, a_scale);
                Conditions.YAcceleration = ScaleAndOffset(data, 2, a_scale);
                Conditions.ZAcceleration = ScaleAndOffset(data, 4, a_scale);
                _temp = ScaleAndOffset(data, 6, 1 / 340f, 36.53f);
                Conditions.XGyroscopicAcceleration = ScaleAndOffset(data, 8, g_scale);
                Conditions.YGyroscopicAcceleration = ScaleAndOffset(data, 10, g_scale);
                Conditions.ZGyroscopicAcceleration = ScaleAndOffset(data, 12, g_scale);
            }
        }

        private float ScaleAndOffset(byte[] data, int index, float scale, float offset = 0)
        {
            // convert to a signed number
            unchecked
            {
                var s = (short)(data[index] << 8 | data[index + 1]);
                return (s * scale) + offset;
            }
        }
    }*/
}
