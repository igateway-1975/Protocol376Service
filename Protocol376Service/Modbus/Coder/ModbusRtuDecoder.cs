using Mina.Core.Buffer;
using Mina.Core.Session;
using Mina.Filter.Codec;
using System;
using Protocol376Service.Modbus.Protocols;
using Protocol376Service.Modbus.IO.ByteArray;
using System.Diagnostics;

namespace Protocol376Service.Modbus.Coder
{
    public class ModbusRtuDecoder : IProtocolDecoder
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

        void IProtocolDecoder.Decode(IoSession session, IoBuffer input, IProtocolDecoderOutput output)
        {
            try
            {
                ByteArrayWriter writer = new ByteArrayWriter();
                writer.WriteBytes(input.GetRemaining().Array, 0, input.Limit - input.Position);
                ByteArrayReader incoming = writer.ToReader();

                int length = incoming.Length;
                if (length < 4)
                {
                    input.Position = input.Limit;
                    return;
                }

                byte address = incoming.ReadByte();
                byte fncode = incoming.ReadByte();


                if (fncode < ModbusCodecBase.CommandCodecs.Length)
                {
                    ModbusCommand command = new ModbusCommand(fncode);

                    command.Address = address;
                    command.QueryTotalLength = 2;

                    var codec = ModbusCodecBase.CommandCodecs[fncode];

                    var body = new ByteArrayReader(incoming.ReadBytes(length - 4));

                    if (codec != null && codec.ServerDecode(command, body))
                    {
                        //calculate the CRC-16 over the received stream
                        ushort crcCalc;
                        unchecked
                        {
                            crcCalc = (ushort)ModbusRtuDecoder.CRC16.Compute(((IByteArray)incoming).Data, 0, command.QueryTotalLength);
                        }

                        //validate the CRC-16
                        ushort crcRead = ByteArrayHelpers.ReadUInt16LE(((IByteArray)incoming).Data, command.QueryTotalLength);

                        if (crcRead == crcCalc)
                        {
                            output.Write(command);
                            Trace.WriteLine("接收:" + ByteArrayHelpers.ToHex(((IByteArray)incoming).Data));
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {

            }
            finally
            {
                input.Position = input.Limit;
            }            
        }

        void IProtocolDecoder.Dispose(IoSession session)
        {
            throw new NotImplementedException();
        }

        void IProtocolDecoder.FinishDecode(IoSession session, IProtocolDecoderOutput output)
        {
            //throw new NotImplementedException();
        }        
    }
}
