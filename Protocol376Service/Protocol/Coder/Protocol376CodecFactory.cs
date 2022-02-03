using Mina.Core.Session;
using Mina.Filter.Codec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service.Protocol.Coder
{
    class Protocol376CodecFactory : IProtocolCodecFactory
    {
        private readonly Protocol376Encoder _encoder = new Protocol376Encoder();
        private readonly Protocol376Decoder _decoder = new Protocol376Decoder();
        public IProtocolDecoder GetDecoder(IoSession session)
        {
            return _decoder;
        }

        public IProtocolEncoder GetEncoder(IoSession session)
        {
            return _encoder;
        }
    }
}
