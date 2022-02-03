using log4net;
using Mina.Core.Service;
using Mina.Core.Session;
using Mina.Filter.Codec;
using Mina.Filter.Logging;
using Mina.Transport.Socket;
using Protocol376Service.Device;
using Protocol376Service.Modbus;
using Protocol376Service.Modbus.Coder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ThreadState = System.Threading.ThreadState;

namespace Protocol376Service
{
    public class ModbusService
    {
        private ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IoAcceptor acceptor = new AsyncSocketAcceptor();      
        private int modbusPort = 502;
        private Thread thread = null;
        private int channel = 1;
        private Protocol376Platform platform;

        public ModbusService(int modbusPort, int channel, Protocol376Platform platform)
        {
            this.channel = channel;
            this.modbusPort = modbusPort;
            this.platform = platform;
        }        

        public int ModbusPort { get => modbusPort; set => modbusPort = value; }
       
        public void run()
        {
            try
            {
                acceptor.FilterChain.AddLast("logger", new LoggingFilter());
                acceptor.FilterChain.AddLast("codec", new ProtocolCodecFilter(new ModbusRtuCodecFactory()));
                acceptor.MessageReceived += OnMessageReceived;
                acceptor.SessionOpened += OnSessionOpened;
                acceptor.SessionClosed += OnSessionClosed;

                acceptor.Bind(new IPEndPoint(IPAddress.Any, modbusPort));
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }            
        }

        public void start()
        {
            if (thread == null)
            {
                thread = new Thread(run);
            }
            thread.Start();
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

        private void OnSessionOpened(object sender, IoSessionEventArgs e)
        {
            IDictionary<long, IoSession> sessions = acceptor.ManagedSessions;
            if (sessions.Count > 2)
            {
                foreach (var item in sessions)
                {
                    if (item.Value != e.Session)
                    {
                        item.Value.CloseNow();
                        break;
                    }
                }
            }        
        }

        private void OnSessionClosed(object sender, IoSessionEventArgs e)
        {

        }

        private void OnMessageReceived(object sender, IoSessionMessageEventArgs e)
        {
            var traceLines = new StringBuilder();

            ModbusCommand command = (ModbusCommand)e.Message;
            int address = command.Address;

            int key = channel * 256 + address;

            Protocol376Device device = platform.GetDevice(key);


            this.platform.idleTimeout = 10;

            if (null == device)
            {
                log.Info("Modbus channel:" + channel.ToString() + " id:" + address.ToString("D3") + " is null.");
                return;
            }

            if (!device.CommunicationIsOK)
            {
                log.Info("Modbus channel:" + channel.ToString() + " id:" + address.ToString("D3") + " communication is not ok.");
                return;
            }          

            //take the proper function command handler
            switch (command.FunctionCode)
            {
                /*
                case ModbusCommand.FuncReadCoils:
                    var boolArray = DataAccess[address].ReadCoils((ushort)command.Offset, (ushort)command.Count);
                    for (var i = 0; i < command.Count; i++)
                    {
                        command.Data[i] = (ushort)(boolArray[i] ? 1 : 0);
                        traceLines.Append($"[{command.Offset + i}]={command.Data[i]} ");
                    }
                    traceLines.AppendLine(string.Empty);
                    break;


                case ModbusCommand.FuncReadInputDiscretes:
                    boolArray = DataAccess[address].ReadInputDiscretes((ushort)command.Offset, (ushort)command.Count);
                    for (var i = 0; i < command.Count; i++)
                    {
                        command.Data[i] = (ushort)(boolArray[i] ? 1 : 0);
                        traceLines.Append($"[{command.Offset + i}]={command.Data[i]} ");
                    }
                    traceLines.AppendLine(string.Empty);
                    break;


                case ModbusCommand.FuncWriteCoil:
                    DataAccess[address].WriteCoils((ushort)command.Offset, new[] { command.Data[0] != 0 });
                    traceLines.AppendLine($"[{command.Offset}]={command.Data[0]} ");
                    break;


                case ModbusCommand.FuncForceMultipleCoils:
                    var boolList = new List<bool>();
                    for (var i = 0; i < command.Count; i++)
                    {
                        var index = command.Offset + (i / 16);
                        var mask = 1 << (i % 16);
                        var value = (command.Data[index] & mask) != 0;
                        boolList.Add(value);
                        traceLines.Append($"[{index}]={value} ");
                    }
                    DataAccess[address].WriteCoils((ushort)command.Offset, boolList.ToArray());
                    traceLines.AppendLine(string.Empty);
                    break;

                case ModbusCommand.FuncReadInputRegisters:
                    command.Data = DataAccess[address].ReadInputRegisters((ushort)command.Offset, (ushort)command.Count);
                    for (var i = 0; i < command.Count; i++)
                    {
                        traceLines.Append($"[{command.Offset + i}]={command.Data[i]} ");
                    }
                    traceLines.AppendLine(string.Empty);
                    break;
                */
                case ModbusCommand.FuncReadMultipleRegisters:
                    command.Data = device.DataAccess.ReadHoldingRegisters((ushort)command.Offset, (ushort)command.Count);
                    for (var i = 0; i < command.Count; i++)
                    {
                        traceLines.Append($"[{command.Offset + i}]={command.Data[i]} ");
                    }
                    break;

                /*
                case ModbusCommand.FuncWriteMultipleRegisters:
                    DataAccess[address].WriteRegisters((ushort)command.Offset, command.Data);
                    for (var i = 0; i < command.Count; i++)
                    {
                        traceLines.Append($"[{command.Offset + i}]={command.Data[i]} ");
                    }
                    traceLines.AppendLine(string.Empty);
                    break;

                case ModbusCommand.FuncWriteSingleRegister:
                    DataAccess[address].WriteRegisters((ushort)command.Offset, command.Data);
                    for (var i = 0; i < command.Count; i++)
                    {
                        traceLines.Append($"[{command.Offset + i}]={command.Data[i]} ");
                    }
                    traceLines.AppendLine(string.Empty);
                    break;
                */
                case ModbusCommand.FuncReadExceptionStatus:
                    traceLines.AppendLine("ModbusSlave: Unhandled command FuncReadExceptionStatus");
                    break;


                default:
                    //return an exception
                    Trace.TraceError("ModbusSlave: Illegal Modbus FunctionCode");
                    command.ExceptionCode = ModbusCommand.ErrorIllegalFunction;
                    break;
            }
            log.Info("Modbus channel:" + channel.ToString() + " id:" + address.ToString("D3") + " requested, and response is: " + traceLines.ToString());
            e.Session.Write(command);
        }        
    }
}
