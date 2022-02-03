using log4net;
using Mina.Core.Future;
using Mina.Core.Service;
using Mina.Core.Session;
using Mina.Filter.Codec;
using Mina.Filter.Logging;
using Mina.Transport.Socket;
using Protocol376Service.Protocol;
using Protocol376Service.Protocol.Coder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThreadState = System.Threading.ThreadState;
using Timer = System.Timers.Timer;

namespace Protocol376Service
{
    public class Protocol376ClientService
    {
        private ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private String serverIp = "192.168.166.1";
        private Int32 port = 8086;
        private IoConnector connector = new AsyncSocketConnector();
        private IoSession session;
        private Thread thread = null;
        private int ErrMeterCount = 0;

        private static volatile bool isReceived = true;

        private Protocol376Platform platform = null;

        private List<MeterInfo> meters = new List<MeterInfo>();
        public Protocol376ClientService(Protocol376Platform platform, string serverIp, int port)
        {
            this.platform = platform;
            this.serverIp = serverIp;
            this.port = port;
            foreach(KeyValuePair<int,Protocol376Device> kv in platform.devices)
            {
                Protocol376Device device = kv.Value;
                meters.Add(device.Meter);               
            }
        }

        public Protocol376ClientService()
        {
        }

        public void stop()
        {
            thread.Abort();//调用Thread.Abort方法试图强制终止thread线程

            //上面调用Thread.Abort方法后线程thread不一定马上就被终止了，所以我们在这里写了个循环来做检查，看线程thread是否已经真正停止。其实也可以在这里使用Thread.Join方法来等待线程thread终止，Thread.Join方法做的事情和我们在这里写的循环效果是一样的，都是阻塞主线程直到thread线程终止为止
            while (thread.ThreadState != ThreadState.Aborted)
            {
                //当调用Abort方法后，如果thread线程的状态不为Aborted，主线程就一直在这里做循环，直到thread线程的状态变为Aborted为止
                Thread.Sleep(100);
            }
        }

        public void start()
        {
            connector.FilterChain.AddLast("logger", new LoggingFilter());
            connector.FilterChain.AddLast("codec", new ProtocolCodecFilter(new Protocol376CodecFactory()));
            connector.MessageReceived += OnMessageReceived;
            connector.SessionOpened += OnSessionOpened;
            connector.SessionClosed += OnSessionClosed;
            connector.SessionDestroyed += OnSessionDestroyed;

            if (thread == null)
            {
                thread = new Thread(run);
            }                
            thread.Start();
        }       
        
        public void run()
        {
            int WaitingTimeOut = 0;
            int MetersCount = meters.Count;

            int index = 0;

            //连接服务器
            while (true)
            {
                try
                {
                    connector.DefaultRemoteEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), port);
                    IConnectFuture future = connector.Connect();
                    future.Await();
                    session = future.Session;
                    break;
                }
                catch 
                {
                    Thread.Sleep(3000);
                }
            }
            
            while (true)
            {
                try
                {
                    if (platform.idleTimeout < 1)
                    {
                        if (session.Connected && (isReceived || WaitingTimeOut > 5))
                        {
                            Protocol376FrameInfo frameInfo = new Protocol376FrameInfo();

                            if (index >= MetersCount)
                            {
                                index = 0;
                            }
                            MeterInfo meter = meters[index];

                            Protocol376Device device = platform.GetDevice(meter.ConcentratorAddress.ToString() + meter.MeasurePoint.ToString("D4"));
                            if (null != device)
                            {
                                device.CommunicationIsOK = false;
                                frameInfo.PN = meter.MeasurePoint.ToString();
                                frameInfo.SetCollectorAddress(meter.ConcentratorAddress.ToString());

                                frameInfo.CID = "11";    //功能码            
                                frameInfo.MSA = "10";
                                frameInfo.AFN = "0C";
                                frameInfo.FN = "33";
                                frameInfo.FIN = "1";

                                if (device.MeterType == Protocol376Device.ColdHeatMeter)
                                {
                                    frameInfo.FN = "188";
                                }

                                session.Write(frameInfo);
                                isReceived = false;

                                string strlog = String.Format("request->channel:{0},addr:{1},meterid:{2},concentrator:{3},point:{4}",
                                    meter.Channel, meter.ModbusAddress.ToString("D3"), meter.MeterId.ToString("D3"), meter.ConcentratorAddress.ToString("D8"), meter.MeasurePoint.ToString("D4"));

                                log.Debug(strlog);
                            }                          

                            index++;                            
                            WaitingTimeOut = 0;
                        }
                        else
                        {
                            WaitingTimeOut++;
                        }
                    }
                    else
                    {
                        platform.idleTimeout--;
                    }
                    
                    Thread.Sleep(1000);
                }
                catch(Exception ex)
                {
                    log.Error(ex.Message);
                }                
            }
        }
        private void OnSessionDestroyed(object sender, IoSessionEventArgs e)
        {
            //断绝重连
            while (true)
            {
                try
                {
                    if (!connector.Disposed)//如果不是因为客户端执行的关闭，则进行断线重连
                    {
                        session = connector.Connect().Await().Session;              
                    }
                    break;
                }
                catch (Exception ex) 
                {
                    log.Error(ex.Message); 
                    Thread.Sleep(1000); 
                }
            }
        }

        private void OnSessionOpened(object sender, IoSessionEventArgs e)
        {
            //Trace.WriteLine("TCP client {0} connected.", e.Session.RemoteEndPoint.ToString());
        }

        private void OnSessionClosed(object sender, IoSessionEventArgs e)
        {
            //Trace.WriteLine("TCP client {0} closed.", e.Session.RemoteEndPoint.ToString());
        }

        /// <summary>
        /// 接收数据后解析
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMessageReceived(object sender, IoSessionMessageEventArgs e)
        {
            try
            {        
                Protocol376FrameInfo frameInfo = (Protocol376FrameInfo)e.Message;

                if ((frameInfo.AFN == "0C" && frameInfo.Data.Length >= 22) && (frameInfo.FN == "0033" || frameInfo.FN == "0188"))
                {
                    this.log.Debug("response:" + frameInfo.Data.Substring(0, 52));
                    DateTime dateTime = DateTime.Parse(Utility.GetDateTime(frameInfo.Data, 0));
                    String log = String.Format("{0},{1},{2},{3}", dateTime.ToString("G"), frameInfo.GetCollectorAddress(), frameInfo.PN, Utility.GetPowerValue(frameInfo.Data, 12));
                    this.log.Debug(log); 
                    Protocol376Device device = platform.GetDevice(frameInfo.GetCollectorAddress() + frameInfo.PN);
                    TimeSpan span = DateTime.Now - dateTime;
                    if (null != device)
                    {
                        if (device.Meter.ModbusAddress == 1)
                        {
                            this.log.Debug("ErrMeterCount:" + ErrMeterCount.ToString());
                            ErrMeterCount = 0;
                        }
                        if (span.TotalMinutes < 2 * 60)
                        {
                            device.CommunicationIsOK = true;
                        }
                        else
                        {
                            device.CommunicationIsOK = false;
                            ErrMeterCount++;
                        }

                        device.SetMeterValue(frameInfo.Data);
                        device.LastUpdateTime = dateTime;
                    }
                } 
                else if (frameInfo.AFN == "05" && frameInfo.FN == "0031") //对时的反馈
                {

                }
            }
            catch(Exception ex)
            {
                this.log.Error(ex.Message);
            }
            finally
            {
                isReceived = true;
            }
            
        }     

    }
}
