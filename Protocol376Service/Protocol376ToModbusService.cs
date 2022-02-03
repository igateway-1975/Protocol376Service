using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Protocol376Service
{
    partial class Protocol376ToModbusService : ServiceBase
    {     
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static private Dictionary<int, MeterInfo> meters = new Dictionary<int, MeterInfo>();
        static private List<MeterInfo> channelsInfo = new List<MeterInfo>(); 
        static private volatile Protocol376Platform platform = new Protocol376Platform();
        private CancellationTokenSource cts = new CancellationTokenSource();

        private Protocol376ClientService client = null;
        private ModbusService modbusService01 = null;
        private ModbusService modbusService02 = null;
        private ModbusService modbusService03 = null;
        private ModbusService modbusService04 = null;
        public Protocol376ToModbusService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            log.Info("Protocol376ToModbusService service starting...");
            Start();            
            log.Info("Protocol376ToModbusService service started.");
        }

        private void ConfigRate(string fileName)
        {
            Dictionary<int, int> rates = new Dictionary<int, int>();
            try
            {
                DataTable dataTable = CSVFileHelper.OpenCSV(fileName);
                foreach (DataRow row in dataTable.Rows)
                {
                    log.Error(row[0].ToString() + ":" + row[1].ToString());
                    rates.Add(int.Parse(row[0].ToString()), int.Parse(row[1].ToString()));
                }

                log.Error("step1.");

                foreach (KeyValuePair<int, MeterInfo> kv in meters)
                {
                    if (rates.ContainsKey(kv.Value.MeterId))
                    {
                        kv.Value.Rate = rates[kv.Value.MeterId];
                    }
                    else
                    {
                        log.Error("有meterid重复了");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

        }

        protected override void OnStop()
        {
            log.Info("Protocol376ToModbusService service stoping...");
            client.stop();
            modbusService01.stop();
            modbusService02.stop();
            modbusService04.stop();
            cts.Cancel();
            log.Info("Protocol376ToModbusService service stoped.");
        }

        private void Start()
        {
            cts.Dispose();
            cts = new CancellationTokenSource();

            ReadConfig(AppDomain.CurrentDomain.BaseDirectory + "\\config.csv");

            //build config file begin---------------------------------------------------
            //ConfigRate(AppDomain.CurrentDomain.BaseDirectory + "\\meter_pt.csv");
            //WriteCSVConfig(AppDomain.CurrentDomain.BaseDirectory + "\\config_new.csv");

            //ReadConfig(@"D:\temp\Configuration\config.csv");
            //ReadXMLConfig(@"D:\temp\Configuration\channel3.xml");
            //OldXmlToCSVConfig(@"D:\temp\Configuration\config3.csv");
            //build config file end ---------------------------------------------------

            string server = ConfigurationManager.AppSettings["server"];
            int port = Int16.Parse(ConfigurationManager.AppSettings["port"]);
            client = new Protocol376ClientService(platform, server, port);
            client.start();

            modbusService01 = new ModbusService(1501, 1, platform);
            modbusService01.start();

            modbusService02 = new ModbusService(1502, 2, platform);
            modbusService02.start();

            modbusService03 = new ModbusService(1503, 3, platform);
            modbusService03.start();

            modbusService04 = new ModbusService(1504, 4, platform);
            modbusService04.start();

            //等待线程全部运行成功，否则可能服务会启动失败！
            Thread.Sleep(5000);
        }

        /// <summary>
        /// 读取表具的配置，包括表的集中器的地址，测点号等
        /// </summary>
        static void ReadConfig(string fileName)
        {
            try
            {
                DataTable dataTable = CSVFileHelper.OpenCSV(fileName);
                foreach (DataRow row in dataTable.Rows)
                {
                    MeterInfo meter = new MeterInfo();
                    meter.Channel = int.Parse(row[0].ToString());
                    meter.MeterType = int.Parse(row[1].ToString());
                    meter.ModbusAddress = int.Parse(row[2].ToString());
                    meter.MeterId = int.Parse(row[3].ToString());
                    meter.MeasurePoint = int.Parse(row[4].ToString());
                    meter.ConcentratorAddress = int.Parse(row[5].ToString());
                    meter.Rate = int.Parse(row[6].ToString());

                    meters.Add(meter.MeterId, meter);

                    int key = meter.Channel * 256 + meter.ModbusAddress;
                    Protocol376Device device = new Protocol376Device(meter, meter.MeterType);
                    platform.AddDevice(key, device);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

        }

        /// <summary>
        /// 读取老程序的XML配置文件
        /// </summary>
        /// <param name="fileName"></param>
        static void ReadXMLConfig(string fileName)
        {
            try
            {
                XElement channelConfig = XElement.Load(fileName);
                XElement server = channelConfig.Element("server");
                int modbusPort = Int16.Parse(server.Element("modbusPort").Value);
                string channelType = server.Element("channelType").Value;
                string sqlConnection = server.Element("sqlConnection").Value;
                int freshSQLTimeOfSecond = Int16.Parse(server.Element("freshSQLTimeOfSecond").Value);
                foreach (XElement eleDevice in channelConfig.Element("devices").Elements("device"))
                {
                    //MeterDevice device = new MeterDevice(Int16.Parse(eleDevice.Attribute("address").Value),
                    //    Int32.Parse(eleDevice.Attribute("meterId").Value));
                    //devices.Add(device);
                    MeterInfo meter = new MeterInfo();
                    meter.Channel = 3;
                    meter.MeterType = Protocol376Device.ColdHeatMeter;
                    meter.ModbusAddress = Int16.Parse(eleDevice.Attribute("address").Value);
                    meter.MeterId = Int32.Parse(eleDevice.Attribute("meterId").Value);
                    meter.MeasurePoint = 0;
                    meter.ConcentratorAddress = 0;
                    meter.Rate = 1;

                    channelsInfo.Add(meter);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        static void OldXmlToCSVConfig(string fileName)
        {
            using (StreamWriter file = new System.IO.StreamWriter(fileName, true))
            {
                string head = String.Format("channel,meterType,modbusAddress,meterId,measurePoint,concentratorAddress,rate\n");
                file.Write(head);

                foreach(MeterInfo info in channelsInfo)
                {
                    MeterInfo meter = meters[info.MeterId];
                    string row = String.Format("{0},{1},{2},{3},{4},{5},{6}\n", 2, info.MeterType, info.ModbusAddress, meter.MeterId, meter.MeasurePoint, meter.ConcentratorAddress, info.Rate);
                    file.Write(row);
                }
            }
        }

        /// <summary>
        /// 把配置文件写入CSV文件
        /// </summary>
        /// <param name="fileName"></param>

        static void WriteCSVConfig(string fileName)
        {
            using (StreamWriter file = new System.IO.StreamWriter(fileName, true))
            {
                string head = String.Format("channel,meterType,modbusAddress,meterId,measurePoint,concentratorAddress,rate\n");
                file.Write(head);

                foreach (KeyValuePair<int, MeterInfo> kv in meters)
                {
                    MeterInfo meter = kv.Value;
                    string row = String.Format("{0},{1},{2},{3},{4},{5},{6}\n", meter.Channel, meter.MeterType, meter.ModbusAddress, meter.MeterId, meter.MeasurePoint, meter.ConcentratorAddress, meter.Rate);
                    file.Write(row);                  
                }
            }
        }
    }
}
