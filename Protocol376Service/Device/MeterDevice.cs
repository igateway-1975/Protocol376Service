using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service.Device
{
    public class MeterDevice
    {  
        private int modbusId;
        private int meterIdInDB;

        public MeterDevice(int modbusId, int meterIdInDB)
        {
            this.modbusId = modbusId;
            this.meterIdInDB = meterIdInDB;
        }

        public int ModbusId
        {
            get => modbusId;
            set
            {
                modbusId = value < 256 ? value : 0;
            }
        }
        public int MeterIdInDB { get => meterIdInDB; set => meterIdInDB = value; }
    }
}
