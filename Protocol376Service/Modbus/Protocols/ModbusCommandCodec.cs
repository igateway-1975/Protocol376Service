using Protocol376Service.Modbus.IO;
using Protocol376Service.Modbus.IO.ByteArray;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service.Modbus.Protocols
{
    public class ModbusCommandCodec
    {
        #region Client codec

        /// <summary>
        /// Encode the client-side command toward the remote slave device
        /// </summary>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <returns>True when the function operated successfully</returns>
        public virtual bool ClientEncode( ModbusCommand command,  ByteArrayWriter body)
        {
            return false;
        }

        /// <summary>
        /// Decode the incoming data from the remote slave device 
        /// to a client-side command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <returns>True when the function operated successfully</returns>
        public virtual bool ClientDecode( ModbusCommand command, ByteArrayReader body)
        {
            return false;
        }

        #endregion


        #region Server codec

        /// <summary>
        /// Encode the server-side command toward the master remote device
        /// </summary>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <returns>True when the function operated successfully</returns>
        public virtual bool ServerEncode( ModbusCommand command, ByteArrayWriter body)
        {
            return false;
        }

        /// <summary>
        /// Decode the incoming data from the remote master device 
        /// to a server-side command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <returns>True when the function operated successfully</returns>
        public virtual bool ServerDecode( ModbusCommand command, ByteArrayReader body)
        {
            return false;
        }

        #endregion
    }
}
