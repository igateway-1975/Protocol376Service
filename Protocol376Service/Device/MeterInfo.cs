using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service
{
    public class MeterInfo
    {
        /// <summary>
        /// 仿真总线的ID号
        /// </summary>
        public int Channel = 1;
        /// <summary>
        /// 表具类型
        /// </summary>
        public int MeterType = Protocol376Device.ElectricMeter;
        /// <summary>
        /// Modbus地址
        /// </summary>
        public int ModbusAddress = 1;
        /// <summary>
        /// 原数据库中的Meter_ID       
        /// </summary>
        public int MeterId = 0;
        /// <summary>
        /// 测点
        /// </summary>
        public int MeasurePoint = 1;
        /// <summary>
        /// 集中器地址
        /// </summary>
        public int ConcentratorAddress = 0;
        /// <summary>
        /// 倍率
        /// </summary>
        public int Rate = 1;
    }
}
