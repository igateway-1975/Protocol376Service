using Mina.Core.Buffer;
using Mina.Core.Session;
using Mina.Filter.Codec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service.Protocol.Coder
{
    class Protocol376Encoder : IProtocolEncoder
    {
        DLT698 dlt = new DLT698();
        public void Dispose(IoSession session)
        {
            throw new NotImplementedException();
        }

        public void Encode(IoSession session, object message, IProtocolEncoderOutput output)
        {
            Protocol376FrameInfo frameInfo = (Protocol376FrameInfo)message;
            dlt.PFC += 1;
            dlt.strA1 = frameInfo.A1;
            dlt.strA2 = frameInfo.A2;
            dlt.strMSA = "10";
            byte[] data = dlt.GetSendDate(frameInfo.CID, frameInfo.AFN, frameInfo.FN, frameInfo.PN, "");

            IoBuffer buffer = IoBuffer.Allocate(data.Length);
            buffer.Put(data);

            buffer.Flip();
            output.Write(buffer);
        }
	}
}
