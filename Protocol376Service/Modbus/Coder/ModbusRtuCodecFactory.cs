using Mina.Core.Session;
using Mina.Filter.Codec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service.Modbus.Coder
{
    class ModbusRtuCodecFactory : IProtocolCodecFactory
    {
        private readonly ModbusRtuEncoder _encoder = new ModbusRtuEncoder();
        private readonly ModbusRtuDecoder _decoder = new ModbusRtuDecoder();

        IProtocolDecoder IProtocolCodecFactory.GetDecoder(IoSession session)
        {
            return _decoder;
        }

        IProtocolEncoder IProtocolCodecFactory.GetEncoder(IoSession session)
        {
            return _encoder;
        }
    }
}
