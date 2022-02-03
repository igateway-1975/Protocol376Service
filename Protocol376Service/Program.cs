using log4net;
using Protocol376Service.Device;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Protocol376Service
{
    class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Protocol376ToModbusService()
            };
            ServiceBase.Run(ServicesToRun);
        }


        /*
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static private Dictionary<int, MeterInfo> meters = new Dictionary<int, MeterInfo>();
        static private List<MeterDevice> devices = new List<MeterDevice>();
        static private volatile Protocol376Platform platform = new Protocol376Platform();
        static void Main(string[] args)
        {

            ReadXMLConfig(@"D:\temp\Configuration\channel3.xml");
            ReadConfig(@"D:\temp\Configuration\config.csv");
            WriteCSVConfig(@"D:\temp\Configuration\channel3.csv", 3, Protocol376Device.ColdHeatMeter);

            //         log.Info("Protocol376Service start ...");

            //         ReadConfig(@"config.csv");

            //         string server = ConfigurationManager.AppSettings["server"];
            //         int port = Int16.Parse(ConfigurationManager.AppSettings["port"]);
            //         Protocol376ClientService client = new Protocol376ClientService(platform, server, port, 1);
            //         client.start();

            //         ModbusService modbusService01 = new ModbusService(1501, 1, platform);
            //         modbusService01.start();

            //         ModbusService modbusService02 = new ModbusService(1502, 2, platform);
            //         modbusService02.start();

            //         ModbusService modbusService04 = new ModbusService(1504, 4, platform);
            //         modbusService04.start();

            //         while (true)
            //         {
            //             string key = System.Console.ReadLine();
            //	switch (key.ToUpper())
            //	{					
            //		case "EXIT":
            //			Environment.Exit(0);
            //			break;
            //		default:
            //                     Console.Clear();
            //                     break;
            //	}
            //}
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

                    meters.Add(meter.MeterId, meter);

                    int key = meter.Channel*256 + meter.ModbusAddress;
                    Protocol376Device device = new Protocol376Device(meter, meter.MeterType);
                    platform.AddDevice(key, device);
                }
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
            
        }

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
                    MeterDevice device = new MeterDevice(Int16.Parse(eleDevice.Attribute("address").Value),
                        Int32.Parse(eleDevice.Attribute("meterId").Value));
                    devices.Add(device);
                }               
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        static void WriteCSVConfig(string fileName, int channel, int meterType)
        {
            using (StreamWriter file = new System.IO.StreamWriter(fileName, true))
            {
                string head = String.Format("channel,meterType,modbusAddress,meterId,measurePoint,concentratorAddress\n");
                file.Write(head);

                foreach (MeterDevice device in devices)
                {
                    if (meters.ContainsKey(device.MeterIdInDB)){
                        MeterInfo meter = meters[device.MeterIdInDB];
                        string row = String.Format("{0},{1},{2},{3},{4},{5}\n", channel, meterType, device.ModbusId, meter.MeterId, meter.MeasurePoint, meter.ConcentratorAddress);
                        file.Write(row);
                    }
                }
            }
        }
        */
    }
}
