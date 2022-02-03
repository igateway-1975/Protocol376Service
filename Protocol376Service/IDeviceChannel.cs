using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service.Device
{
    interface IDeviceChannel
    {
        void start();
        DateTime LastUpdateDateTime { get; }
        string ConnectionString { get; set; }
        int ModbusPort { get; set; }
        int FreshSQLTimeOfSecond { get; set; }
        List<MeterDevice> MeterList { get; set; }
    }
}
