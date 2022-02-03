using Protocol376Service.Modbus.IO;
using Protocol376Service.Modbus.IO.ByteArray;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service.Modbus.Protocols
{
    public class ModbusCodecBase
    {
        static ModbusCodecBase()
        {
            //fill the local array with the curretly supported commands
            CommandCodecs[ModbusCommand.FuncReadMultipleRegisters] = new ModbusCodecReadMultipleRegisters();
            CommandCodecs[ModbusCommand.FuncWriteMultipleRegisters] = new ModbusCodecWriteMultipleRegisters();
            CommandCodecs[ModbusCommand.FuncReadCoils] = new ModbusCodecReadMultipleDiscretes();
            CommandCodecs[ModbusCommand.FuncReadInputDiscretes] = new ModbusCodecReadMultipleDiscretes();
            CommandCodecs[ModbusCommand.FuncReadInputRegisters] = new ModbusCodecReadMultipleRegisters();
            CommandCodecs[ModbusCommand.FuncWriteCoil] = new ModbusCodecWriteSingleDiscrete();
            CommandCodecs[ModbusCommand.FuncWriteSingleRegister] = new ModbusCodecWriteSingleRegister();
            CommandCodecs[ModbusCommand.FuncForceMultipleCoils] = new ModbusCodecForceMultipleCoils();
        }


        public static readonly ModbusCommandCodec[] CommandCodecs = new ModbusCommandCodec[24];


        /// <summary>
        /// Append the typical header for a request command (master-side)
        /// </summary>
        /// <param name="command"></param>
        /// <param name="body"></param>
        internal static void PushRequestHeader( ModbusCommand command, ByteArrayWriter body)
        {
            body.WriteUInt16BE((ushort)command.Offset);
            body.WriteInt16BE((short)command.Count);
        }


        /// <summary>
        /// Extract the typical header for a request command (server-side)
        /// </summary>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <returns>True if the data could be read</returns>
        internal static bool PopRequestHeader( ModbusCommand command, ByteArrayReader body)
        {
            if (body.CanRead(4))
            {
                command.Offset = body.ReadUInt16BE();
                command.Count = body.ReadInt16BE();
                command.QueryTotalLength += 4;
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Helper for packing the discrete data outgoing as a bit-array
        /// </summary>
        /// <param name="command"></param>
        /// <param name="body"></param>
        internal static void PushDiscretes(ModbusCommand command, ByteArrayWriter body)
        {
            var count = command.Count;
            body.WriteByte((byte)((count + 7) / 8));

            int i = 0;
            int cell = 0;
            for (int k = 0; k < count; k++)
            {
                if (command.Data[k] != 0)
                    cell |= (1 << i);

                if (++i == 8)
                {
                    body.WriteByte((byte)cell);
                    i = 0;
                    cell = 0;
                }
            }

            if (i > 0)
                body.WriteByte((byte)cell);
        }


        /// <summary>
        /// Helper for unpacking discrete data incoming as a bit-array
        /// </summary>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <returns>True if the data could be read</returns>
        internal static bool PopDiscretes( ModbusCommand command, ByteArrayReader body)
        {
            var success = false;

            if (body.CanRead(1))
            {
                var byteCount = body.ReadByte();

                if (body.CanRead(byteCount))
                {
                    var count = command.Count;
                    command.Data = new ushort[count];
                    command.QueryTotalLength += (byteCount + 1);

                    int k = 0;
                    while (count > 0)
                    {
                        byteCount--;
                        int cell = body.ReadByte();

                        int n = count <= 8 ? count : 8;
                        count -= n;
                        for (int i = 0; i < n; i++)
                            command.Data[k++] = (ushort)(cell & (1 << i));
                    }

                    success = true;
                }
            }

            return success;
        }

    }
}
