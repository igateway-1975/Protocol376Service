using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service
{
    public class Protocol376Platform
    {
        public int idleTimeout = 0;
        public Dictionary<int, Protocol376Device> devices = new Dictionary<int, Protocol376Device>();
        public Dictionary<string, Protocol376Device> devices2 = new Dictionary<string, Protocol376Device>();

        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// key = meter.Channel*256 + meter.ModbusAddress
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddDevice(int key, Protocol376Device value)
        {
            if (!devices.ContainsKey(key))
            {
                devices.Add(key, value);
                string strKey = value.Meter.ConcentratorAddress.ToString("D8") + value.Meter.MeasurePoint.ToString("D4");
                if (devices2.ContainsKey(strKey)){
                    log.Error("配置文件中有重复: ConcentratorAddress = " + value.Meter.ConcentratorAddress.ToString("D8") + " MeasurePoint:" + value.Meter.MeasurePoint.ToString("D4"));
                }
                else 
                    devices2.Add(strKey, value);
            }           
        }

        /// <summary>
        ///  key = meter.Channel*256 + meter.ModbusAddress
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Protocol376Device GetDevice(int key)
        {
            if (devices.ContainsKey(key))
            {
                return devices[key];
            }
            return null;
        }

        /// <summary>
        ///  key = String.Format(value.Meter.ConcentratorAddress.ToString("8D") + value.Meter.MeasurePoint.ToString("4D"), value);
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Protocol376Device GetDevice(string key)
        {
            if (devices2.ContainsKey(key))
            {
                return devices2[key];
            }
            return null;
        }

    }
}
