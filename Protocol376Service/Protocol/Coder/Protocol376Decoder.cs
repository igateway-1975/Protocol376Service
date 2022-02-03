using log4net;
using Mina.Core.Buffer;
using Mina.Core.Session;
using Mina.Filter.Codec;
using SQL2Modbus.Modbus.IO.ByteArray;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service.Protocol.Coder
{
    class Protocol376Decoder : IProtocolDecoder
    {
		private ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		public void Decode(IoSession session, IoBuffer input, IProtocolDecoderOutput output)
        {
            try
            {
                ByteArrayWriter writer = new ByteArrayWriter();
                writer.WriteBytes(input.GetRemaining().Array, 0, input.Limit - input.Position);
                ByteArrayReader incoming = writer.ToReader();       

                byte[] data = incoming.ReadBytes(incoming.Length);    
                
                if (FrameValid(data))
                {
					Protocol376FrameInfo protocol376FrameInfo = new Protocol376FrameInfo();
					ParseData(data, data.Length, ref protocol376FrameInfo);
					output.Write(protocol376FrameInfo);
				}
            }
            catch (ObjectDisposedException ex)
            {
				log.Error(ex.Message);
            }
            finally
            {
                input.Position = input.Limit;
            }
        }

        public void Dispose(IoSession session)
        {
            throw new NotImplementedException();
        }

        public void FinishDecode(IoSession session, IProtocolDecoderOutput output)
        {

        }

		private int HexToInt(string HexChar)
		{
			int tmp = -1;
			bool convert = Int32.TryParse(HexChar, NumberStyles.HexNumber, null, out tmp);
			return tmp;
		}

		/// <summary>
		/// 字符串倒序
		/// </summary>
		/// <param name="strValue"></param>
		/// <returns></returns>
		private string ReverseStr(string strValue)
		{
			if (strValue == string.Empty)
				return string.Empty;

			string strTemp = string.Empty;
			for (int i = 0; i < strValue.Length / 2; i++)
				strTemp = strValue.Substring(i * 2, 2) + strTemp;

			return strTemp;
		}

		private byte[] HexStrToByte(string s)
		{
			s = s.Replace(@"\r\n", "");
			string repc = @"~!@#$%^&*()_+-={}|[ ]\;':,.<>?/，。；“”、‘";
			for (int i = 0; i < repc.Length; i++) s = s.Replace(repc.Substring(i, 1), "");

			byte[] buffer = new byte[s.Length / 2];
			for (int i = 0; i < s.Length; i += 2)
				buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);

			return buffer;
		}

	
		/// <summary>
		/// 根据发送帧的长度计算L的值 ---> (在生成发送数据帧的时候用 GetSendDate)
		/// </summary>
		/// <param name="iDataLen">帧长度</param>
		/// <returns>返回字符串</returns>
		private string GetDataLen(ushort iDataLen)
		{
			//2009规约使用
			iDataLen = (ushort)((iDataLen << 2) | 2);
			string strTemp = iDataLen.ToString("X4");
			strTemp = strTemp.Substring(2, 2) + strTemp.Substring(0, 2);

			return strTemp;
		}

		/// <summary>
		/// 根据数据长度L计算接收的数据长度 ---> (只在解析数据帧的时候用到 FillStruct )
		/// 长度固定为2字节，D0,D1固定为01，D2-D15为数据长度
		/// </summary>
		/// <param name="strData">数据L，长度为2字节</param>
		/// <returns></returns>
		private ushort GetDataLen(string strData)
		{
			//先将数据颠倒
			strData = strData.Substring(2, 2) + strData.Substring(0, 2);
			//将数据转为数字
			ushort i = ushort.Parse(strData, NumberStyles.HexNumber);

			//再将数据右移2位得到结果
			return (ushort)(i >> 2);
		}

		private string GetDataLenSpace(string strLen)
		{
			string strTemp = GetDataLen(ushort.Parse(strLen));
			return strTemp.Substring(0, 2) + " " + strTemp.Substring(2, 2);
		}

		/// <summary>
		/// 计算控制域(C) --->(在生成发送数据帧的时候用 GetSendDate)
		/// </summary>
		/// <param name="bDIR">传输方向</param>
		/// <param name="bPRM">启动标志,当请求数据时为1</param>
		/// <param name="bFCB">帧计数</param>
		/// <param name="bFCV">帧计数有效</param>
		/// <param name="bCID">功能码</param>
		/// <returns>返回控制域</returns>
		private string GetControlField(byte bDIR, byte bPRM, byte bFCB, byte bFCV, byte bCID)
		{
			bCID = (byte)(bCID | (bDIR << 7) | (bPRM << 6) | (bFCB << 5) | (bFCV << 4));
			return bCID.ToString("X2");
		}

		/// <summary>
		/// 控制域分解 ---> (只在解析数据帧的时候用到 FillStruct )
		/// </summary>
		private void GetControlField(string strValue, ref string strDIR, ref string strPRM, ref string strFCB, ref string strFCV, ref string strCID)
		{
			byte bTemp = (byte)HexToInt(strValue);
			strDIR = ((bTemp >> 7) & 1).ToString("0");
			strPRM = ((bTemp >> 6) & 1).ToString("0");
			strFCB = ((bTemp >> 5) & 1).ToString("0");
			strFCV = ((bTemp >> 4) & 1).ToString("0");
			strCID = (bTemp & 15).ToString();
		}

		/// <summary>
		/// 计算地址域(A)---> (在生成发送数据帧的时候用 GetSendDate)
		/// </summary>
		/// <param name="strA1">行政区划码 BCD 2字节</param>
		/// <param name="strA2">终端地址BIN 2字节, 使用广播地址（FFFF）的时候必须将组地址标识设置为1</param>
		/// <param name="strMSA">主站地址</param>
		/// <param name="bFlag">组地址标志 BIN 1字节</param>
		/// <returns></returns>
		private string GetAddressField(string strA1, string strA2, string strMSA, byte bFlag)
		{//D0位为终端组地址标志:D0=0表示终端地址A2为单地址；D0=1表示终端地址A2为组地址
		 //D1～D7:组成0～127个主站地址MSA
			strA1 = strA1.PadLeft(4, '0');
			strA2 = Convert.ToInt32(strA2, 10).ToString("X4");//strA2.PadLeft(4, '0');

			ushort i = ushort.Parse(strMSA);
			if (strA2 == "FFFF")
				strMSA = ((ushort)((i << 1) | 1)).ToString("X2");
			else
				strMSA = ((ushort)(i << 1) | bFlag).ToString("X2");

			return strA1.Substring(2, 2) + strA1.Substring(0, 2) + strA2.Substring(2, 2) + strA2.Substring(0, 2) + strMSA;
		}

		/// <summary>
		/// 地址域分解---> (只在解析数据帧的时候用到 FillStruct )
		/// </summary>
		private void GetAddressField(string strValue, ref string strA1, ref string strA2, ref string strMSA, ref string strArrFlag)
		{
			byte bTemp = (byte)HexToInt(strValue.Substring(8, 2));
			strA1 = strValue.Substring(2, 2) + strValue.Substring(0, 2);
			strA2 = ushort.Parse(strValue.Substring(6, 2) + strValue.Substring(4, 2), NumberStyles.HexNumber).ToString("D4");
			strMSA = ((bTemp >> 1) & 127).ToString();
			strArrFlag = (bTemp & 1).ToString();
		}

		/// <summary>
		/// 计算帧序列域---> (在生成发送数据帧的时候用 GetSendDate)
		/// </summary>
		/// <param name="bTpv">帧时间标签有效位</param>
		/// <param name="bFIR">首帧标志FIR</param>
		/// <param name="bFIN">末帧标志FIN</param>
		/// <param name="bCON">请求确认标志位CON</param>
		/// <param name="bPSEQRSEQ">启动帧序号PSEQ，响应帧序号RSEQ</param>
		/// <returns></returns>
		private string GetSEQ(byte bTpv, byte bFIR, byte bFIN, byte bCON, byte bPSEQ)
		{
			bPSEQ = (byte)(bPSEQ | (bTpv << 7) | (bFIR << 6) | (bFIN << 5) | (bCON << 4));
			return bPSEQ.ToString("X2");
		}

		/// <summary>
		/// SEQ 帧序列域分解---> (只在解析数据帧的时候用到 FillStruct )
		/// </summary>
		private void GetSEQ(string strValue, ref string strTpV, ref string strFIR, ref string strFIN, ref string strCON, ref string strPSEQ)
		{
			byte bTemp = (byte)HexToInt(strValue);

			strTpV = ((bTemp >> 7) & 1).ToString();
			strFIR = ((bTemp >> 6) & 1).ToString();
			strFIN = ((bTemp >> 5) & 1).ToString();
			strCON = ((bTemp >> 4) & 1).ToString();
			strPSEQ = (bTemp & 15).ToString();
		}

		/// <summary>
		/// 计算数据单元标识---> (在生成发送数据帧的时候用 GetSendDate)
		/// </summary>
		/// <param name="iFN"></param>
		/// <param name="iPN"></param>
		/// <returns></returns>
		private string GetDataCellMark(ushort iFN, ushort iPN)
		{//F（8*（n-1）+1）～F（8n）
			ushort DT = 0;
			if (iFN == 0)
			{
				DT = iFN; //???? Fn（n=1～248）
			}
			else
			{
				//计算高位
				DT = (ushort)(DT | (((iFN - 1) / 8) << 8));
				//计算低位
				DT = (ushort)(DT | (1 << ((iFN - 1) % 8)));
			}

			ushort DA = 0;
			if ((iPN == 0) || (iPN == 0xFFFF))
			{
				DA = iPN;
			}
			else
			{
				//计算高位
				DA = (ushort)(DA | (((iPN - 1) / 8 + 1) << 8));
				//计算低位
				DA = (ushort)(DA | (1 << (iPN % 8 - 1)));
			}
			string strTemp = DT.ToString("X4") + DA.ToString("X4");

			return strTemp.Substring(6, 2) + strTemp.Substring(4, 2) + strTemp.Substring(2, 2) + strTemp.Substring(0, 2);
		}

		/// <summary>
		/// 数据单元标识 FN,PN 分解 ---> (只在解析数据帧的时候用到 FillStruct )
		/// </summary>
		private void GetDataCellMark(string strValue, ref string strFN, ref string strPN)
		{
			strFN = FNToDec(strValue.Substring(4, 4));
			strPN = PNToDec(strValue.Substring(0, 4));
		}

		/// <summary>
		/// 计算帧校验和
		/// </summary>
		/// <param name="strValue">要计算的字符串</param>
		/// <returns>计算结果</returns>
		private string CalculateCS(string strValue)
		{
			int SumMod = 0;
			for (int i = 0; i < strValue.Length / 2; i++)
				SumMod += Convert.ToByte(strValue.Substring(2 * i, 2), 16);

			return ((byte)(SumMod % 256)).ToString("X2");
		}

		/// <summary>
		/// 加密解密算法
		/// </summary>
		/// <param name="PW">密码</param>
		/// <param name="strMask">密钥</param>
		/// <returns></returns>
		public ushort CalculatePW(ushort PW, string strMask)
		{
			ushort crc = 0;

			if (strMask.Length == 0)
				return 0;

			for (int i = 0; i < strMask.Length / 2; i++)
			{
				crc ^= (ushort)(HexToInt(strMask.Substring(i * 2, 2)) & 0xFF);
				for (int j = 0; j < 8; j++)
				{
					if ((crc & 0x0001) == 1)
						crc = (ushort)((crc >> 1) ^ PW);
					else
						crc >>= 1;
				}
			}

			return crc;
		}

		/// <summary>
		/// 分解时标---> (只在解析数据帧的时候用到 FillStruct )
		/// </summary>
		private void GetTimeFlag(string strValue, ref string strPFC, ref string strTime, ref string strDelay)
		{
			strPFC = HexToInt(strValue.Substring(0, 2)).ToString();
			strTime = strValue.Substring(8, 2) + strValue.Substring(6, 2) + strValue.Substring(4, 2) + strValue.Substring(2, 2);
			strDelay = HexToInt(strValue.Substring(10, 2)).ToString();
		}

		/// <summary>
		/// PN转为数字
		/// </summary>
		/// <param name="strValue"></param>
		/// <returns></returns>
		private string PNToDec(string strValue)
		{
			if ((strValue == "0000") || (strValue == "FFFF"))
				return strValue;

			int iTemp1 = HexToInt(strValue.Substring(0, 2));
			int iTemp2 = HexToInt(strValue.Substring(2, 2));
			iTemp2 = iTemp2 * 8;
			for (int i = 0; i < 8; i++)
			{
				if ((iTemp1 >> i) == 1)
				{
					iTemp2 += (i + 1);
					break;
				}
			}
			iTemp2 -= 8;

			if (iTemp2 <= 0) return "无效";
			else return iTemp2.ToString("0000");
		}

		/// <summary>
		/// FN转为数字
		/// 80 01
		/// </summary>
		/// <param name="strValue"></param>
		/// <returns></returns>
		private string FNToDec(string strValue)
		{
			int iTemp1 = HexToInt(strValue.Substring(0, 2));
			int iTemp2 = HexToInt(strValue.Substring(2, 2));
			if (iTemp2 > 30) return "无效";
			iTemp2 = iTemp2 * 8;
			for (int i = 0; i < 8; i++)
			{
				if ((iTemp1 >> i) == 1)
				{
					iTemp2 += (i + 1);
					break;
				}
			}

			return iTemp2.ToString("0000");
		}

		/// <summary>
		/// 数据帧解析
		/// </summary>
		/// <param name="data"></param>
		/// <param name="length"></param>
		/// <param name="frame"></param>
		public void ParseData(byte[] data, int length, ref Protocol376FrameInfo frameInfo)
		{
			bool IsFirst = false;

			string strTemp = string.Empty;
			string strTemp1 = string.Empty;

			//过滤帧头无用数据
			for (int i = 0; i < length; i++)
			{
				if (!IsFirst)
				{
					if (data[i] == 0x68)
					{
						IsFirst = true;
						strTemp = "68";
					}
					else
						continue;
				}
				else
					strTemp += data[i].ToString("X2");
			}

			FillStruct(strTemp, ref frameInfo);
		}

		/// <summary>
		/// 填充帧结构体 (TO FrameData)
		/// </summary>
		/// <param name="strValue"></param>
		private void FillStruct(string strValue, ref Protocol376FrameInfo frameInfo)
		{
			try
			{
				frameInfo.Clear();

				//L1的长度
				frameInfo.L1 = GetDataLen(strValue.Substring(2, 4)).ToString();

				//控制域
				GetControlField(strValue.Substring(12, 2), ref frameInfo.DIR, ref frameInfo.PRM, ref frameInfo.FCB, ref frameInfo.FCV, ref frameInfo.CID);

				//地址域
				GetAddressField(strValue.Substring(14, 10), ref frameInfo.A1, ref frameInfo.A2, ref frameInfo.MSA, ref frameInfo.ArrFlag);

				//应用层功能码
				frameInfo.AFN = strValue.Substring(24, 2);

				//帧序列域
				GetSEQ(strValue.Substring(26, 2), ref frameInfo.TpV, ref frameInfo.FIR, ref frameInfo.FIN, ref frameInfo.CON, ref frameInfo.PSEQ);

				//数据单元标志
				GetDataCellMark(strValue.Substring(28, 8), ref frameInfo.FN, ref frameInfo.PN);

				//数据内容
				string strTemp = strValue.Substring(36, strValue.Length - 40);

				if (frameInfo.DIR == "0") //发出的数据
				{
					if (frameInfo.TpV == "1") //带时标
					{
						GetTimeFlag(strTemp.Substring(strTemp.Length - 12, 12), ref frameInfo.PFC, ref frameInfo.Time, ref frameInfo.Delay);
						strTemp = strTemp.Substring(0, strTemp.Length - 12);
					}
					//带密码
					if ((frameInfo.AFN == "01") || (frameInfo.AFN == "04") || (frameInfo.AFN == "05") || (frameInfo.AFN == "06") || (frameInfo.AFN == "0F") || (frameInfo.AFN == "10"))
					{
						frameInfo.PW = strTemp.Substring(strTemp.Length - 4, 4);
						strTemp = strTemp.Substring(0, strTemp.Length - 4);
					}

					frameInfo.Data = strTemp;
				}
				else //返回数据帧
				{
					if (frameInfo.TpV == "1") //带时标
					{
						GetTimeFlag(strTemp.Substring(strTemp.Length - 12, 12), ref frameInfo.PFC, ref frameInfo.Time, ref frameInfo.Delay);
						strTemp = strTemp.Substring(0, strTemp.Length - 12);
					}

					//判断EC是否存在,此时FCB=ACD
					if (frameInfo.FCB == "1")
					{
						frameInfo.EC1 = strTemp.Substring(strTemp.Length - 2, 2);
						frameInfo.EC2 = strTemp.Substring(strTemp.Length - 4, 2);
						strTemp = strTemp.Substring(0, strTemp.Length - 4);
					}

					frameInfo.Data = strTemp;
				}

				//校验
				frameInfo.CS = strValue.Substring(strValue.Length - 4, 2);

				frameInfo.FrameByte = strValue;
			}
			catch(Exception ex) 
			{
				log.Error(ex.Message);
			}
		}		

        /// <summary>
        /// 初步判断一下得到的数据帧是否有效
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public bool FrameValid(byte[] frame)
        {
            bool retLen = false;
            bool retCS = false;
            int pos = 0;

            if (frame[frame.Length - 1] == 0x16)
            {                
                for (; pos < frame.Length; pos++)
                {
                    if (frame[pos] == 0x68) break;
                }

                //长度判断
                string strDataLenth = frame[pos + 1].ToString("X2") + frame[pos + 2].ToString("X2");

                int DataLen = GetDataLen(strDataLenth);
                if ((frame.Length >= DataLen + 8) && (frame[pos + 5] == 0x68))
                {
                    retLen = true;
                }

                //CS判断 (在长度正确的情况下)
                if (retLen)
                {
                    int SumMod = 0;
                    for (int i = pos + 6; i < frame.Length - 2; i++)
                    {
                        SumMod += frame[i];
                    }
                    if ((byte)(SumMod % 256) == frame[frame.Length - 2])
                    {
                        retCS = true;
                    }
                }
            }

            return retLen & retCS;
        }
    }
}
