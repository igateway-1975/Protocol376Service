using Protocol376Service.Modbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service
{
    public class Protocol376Device
    {
        public const int ElectricMeter = 0;
        public const int WaterMeter = 1;
        public const int ColdHeatMeter = 2;

        public MeterInfo Meter;
        public int MeterType = 0; 
        /// <summary>
        /// 水表和电表上次的数据
        /// </summary>
        public float MeterOldValue = 0.0f;
        /// <summary>
        /// 能量表上次的数据
        /// </summary>   
        public float MeterCoolValue = 0.0f;
        public float MeterHotValue = 0.0f;     
        public float MeterSumFlowValue = 0.0f;

        public DateTime LastUpdateTime = new DateTime();
        public ModbusDataAccess DataAccess = new ModbusDataAccess();
        public bool CommunicationIsOK = false;

        public Protocol376Device(MeterInfo meter, int meterType)
        {
            Meter = meter;
            MeterType = meterType;
        }

        //public void SetMeterValue(float value)
        //{
        //    ushort[] usdata = new ushort[2];

        //    float realValue = value * Meter.Rate;
        //    if (realValue < MeterOldValue)
        //    {
        //        realValue = MeterOldValue;
        //    }
        //    else
        //    {
        //        MeterOldValue = realValue;
        //    }

        //    UInt32 data = (UInt32)(realValue * 10);
        //    usdata[0] = (ushort)(data >> 16);
        //    usdata[1] = (ushort)(data);

        //    switch (MeterType)
        //    {
        //        case 0:
        //            DataAccess.WriteRegisters(54, usdata);                   
        //            break;
        //        case 1:
        //            DataAccess.WriteRegisters(0, usdata);
        //            break;

        //    }
        //}

        public void SetMeterValue(string strdata)
        {
            float realValue = 0.0f;
            UInt32 data = 0;

            ushort[] usdata = new ushort[2];

            switch(MeterType)
            {
                case Protocol376Device.ElectricMeter:
                    realValue = Utility.GetPowerValue(strdata, 12) * Meter.Rate;
                    if (realValue < MeterOldValue)
                    {
                        realValue = MeterOldValue;
                    }
                    else
                    {
                        MeterOldValue = realValue;
                    }
                    data = (UInt32)(realValue * 10);
                    usdata[0] = (ushort)(data >> 16);
                    usdata[1] = (ushort)(data);
                    DataAccess.WriteRegisters(54, usdata);
                    break;

                case Protocol376Device.WaterMeter:
                    realValue = Utility.GetPowerValue(strdata, 12) * Meter.Rate;
                    if (realValue < MeterOldValue)
                    {
                        realValue = MeterOldValue;
                    }
                    else
                    {
                        MeterOldValue = realValue;
                    }
                    data = (UInt32)(realValue * 10);
                    usdata[0] = (ushort)(data >> 16);
                    usdata[1] = (ushort)(data);
                    DataAccess.WriteRegisters(0, usdata);
                    break;

                case Protocol376Device.ColdHeatMeter:
                    if (strdata.Length < 12 + 10 * 5 + 6 * 2) 
                        break;
                    float coolEnergy = Utility.GetEnergyValue(strdata, 12) * Meter.Rate;
                    float hotEnergy = Utility.GetEnergyValue(strdata, 12 + 10) * Meter.Rate;                 
                    float flow = Utility.GetEnergyValue(strdata, 12 + 10 * 3); 
                    float sumFlow = Utility.GetEnergyValue(strdata, 12 + 10 * 4);
                    float supplyWaterTmper = Utility.GetTemperatureValue(strdata, 12 + 10 * 5);
                    float returnWaterTmper = Utility.GetTemperatureValue(strdata, 12 + 10 * 5 + 6);
                    MeterCoolValue = coolEnergy > MeterCoolValue ? coolEnergy : MeterCoolValue;
                    MeterHotValue = hotEnergy > MeterHotValue ? hotEnergy : MeterHotValue;
                    MeterSumFlowValue = sumFlow > MeterSumFlowValue ? sumFlow : MeterSumFlowValue;

                    usdata[0] = (ushort)(supplyWaterTmper * 100);
                    DataAccess.WriteRegister(2, usdata[0]);  //供水温度

                    usdata[0] = (ushort)(returnWaterTmper * 100);
                    DataAccess.WriteRegister(3, usdata[0]);  //回水温度

                    data = (UInt32)(MeterHotValue);
                    usdata[0] = (ushort)(data >> 16);
                    usdata[1] = (ushort)(data);
                    DataAccess.WriteRegisters(8, usdata);    //累计热量               

                    data = (UInt32)(coolEnergy);
                    usdata[0] = (ushort)(data >> 16);
                    usdata[1] = (ushort)(data);
                    DataAccess.WriteRegisters(11, usdata);   //累计冷量                

                    data = (UInt32)(flow);
                    usdata[0] = (ushort)(data >> 16);
                    usdata[1] = (ushort)(data);
                    DataAccess.WriteRegisters(0, usdata);   //瞬时流量

                    data = (UInt32)(sumFlow);
                    usdata[0] = (ushort)(data >> 16);
                    usdata[1] = (ushort)(data);
                    DataAccess.WriteRegisters(5, usdata);   //累计流量

                    break;
            }
        }
    }
}
