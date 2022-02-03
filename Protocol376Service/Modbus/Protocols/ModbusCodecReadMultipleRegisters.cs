using Protocol376Service.Modbus.IO;
using Protocol376Service.Modbus.IO.ByteArray;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service.Modbus.Protocols
{
    /// <summary>
    /// Modbus codec for commands: writing multiple register data
    /// </summary>
    public class ModbusCodecReadMultipleRegisters
        : ModbusCommandCodec
    {
        #region Client codec

        public override bool ClientEncode( ModbusCommand command,  ByteArrayWriter body)
        {
            ModbusCodecBase.PushRequestHeader( command, body);
            return true;
        }


        public override bool ClientDecode( ModbusCommand command, ByteArrayReader body)
        {
            var success = false;

            if (body.CanRead(1))
            {
                var count = body.ReadByte();

                if (body.CanRead(count))
                {
                    count /= 2;
                    command.Data = new ushort[count];
                    for (int i = 0; i < count; i++)
                        command.Data[i] = body.ReadUInt16BE();

                    success = true;
                }
            }

            return success;
        }

        #endregion


        #region Server codec

        public override bool ServerEncode( ModbusCommand command, ByteArrayWriter body)
        {
            var count = command.Count;
            body.WriteByte((byte)(count * 2));
            for (int i = 0; i < count; i++)
            {
                body.WriteUInt16BE(command.Data[i]);
            }               

            return true;
        }


        public override bool ServerDecode( ModbusCommand command, ByteArrayReader body)
        {
            if (ModbusCodecBase.PopRequestHeader(command, body))
            {
                command.Data = new ushort[command.Count];
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

    }
}
