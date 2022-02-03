using Mina.Core.Buffer;
using Mina.Core.Session;
using Mina.Filter.Codec;
using Protocol376Service.Modbus.IO.ByteArray;
using Protocol376Service.Modbus.Protocols;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service.Modbus.Coder
{
    class ModbusRtuEncoder : IProtocolEncoder
    {
        private static HashCRC16 _crc16;
        /// <summary>
        /// Give the function for calculating the CRC16 (0xA001)
        /// over a byte array
        /// </summary>
        public static HashCRC16 CRC16
        {
            get
            {
                if (_crc16 == null)
                    _crc16 = new HashCRC16();

                return _crc16;
            }
        }

        void IProtocolEncoder.Dispose(IoSession session)
        {
            throw new NotImplementedException();
        }

        void IProtocolEncoder.Encode(IoSession session, object message, IProtocolEncoderOutput output)
        {
            ModbusCommand command = (ModbusCommand)message;
            byte fncode = command.FunctionCode;

            ByteArrayWriter body = new ByteArrayWriter();
            var codec = ModbusCodecBase.CommandCodecs[fncode];
            if (codec != null)
            {
                codec.ServerEncode(command, body);
            }

            int length = (command.ExceptionCode == 0) ? 2 + body.Length : 3;

            ByteArrayWriter writer = new ByteArrayWriter();
            writer.WriteByte(command.Address);

            if (command.ExceptionCode == 0)
            {
                //function code
                writer.WriteByte(fncode);
                //body data
                writer.WriteBytes(body);
            }
            else
            {
                //function code
                writer.WriteByte((byte)(command.FunctionCode | 0x80));
                //exception code
                writer.WriteByte(command.ExceptionCode);
            }
            //CRC-16
            ushort crc;
            unchecked
            {
                crc = (ushort)ModbusRtuEncoder.CRC16.Compute( ((IByteArray)writer).Data, 0, writer.Length);
            }

            writer.WriteUInt16LE(crc);

            IoBuffer buffer = IoBuffer.Allocate(writer.Length);
            buffer.Put(writer.ToByteArray());
           
            buffer.Flip();
            output.Write(buffer);

            Trace.WriteLine("发送:" + ByteArrayHelpers.ToHex(writer.ToByteArray()));
            Trace.WriteLine("---------------------------------------------------------------");
        }
    }
}
