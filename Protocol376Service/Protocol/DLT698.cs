using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service.Protocol
{
	#region 系统变量定义
	public partial class DLT698
	{
		public DLT698()
		{
			Delay = 0;
			PFC = 1;
			PW = "00000000000000000000000000000000";
			strA1 = "0";
			strA2 = "0";
			strMSA = "1"; //主站启动的发送帧的MSA应为非零值 主站地址
			bFlag = 0;    //组地址标记 （1=组地址 0=单地址）
			Tp = 0;
		}

		public struct FrameInfo
		{
			//是发送还是接收帧
			public bool IsSend;

			//帧头 68
			public const string FrameFirst = "68";

			//帧头 68
			public const string FrameSecond = "68";

			//帧长度
			public string L1;

			//控制域内容 (控制域C)
			public string DIR;//传输方向位 D7 -->DIR=0:表示此帧报文是由主站发出的下行报文
			public string PRM;//启动标志位 D6 -->PRM=1:表示此帧报文来自启动站；PRM=0:表示此帧报文来自从动站。
			public string FCB;//帧计数位 D5(上行为ACD) -->FCV=1时，FCB表示每个站连续的发送/确认或者请求/响应服务的变化位。复位命令中的FCB=0
			public string FCV;//帧计数有效位 D4
			public string CID;//控制域 D3～D0 功能码PRM=1:功能码11【请求∕响应帧，请求2级数据】，功能码10【请求∕响应帧，请求1级数据】

			//地址域内容
			public string A1;//行政区划码
			public string A2;//终端地址
			public string MSA;//主站地址
			public string ArrFlag;//主站地址和终端组地址标志 //标记时候为组地址 （1=组地址 0=单地址）

			public string AFN;//应用层功能码

			//帧序列域
			//D7D6 D5 D4D3~D0
			//TpV FIRFINCON PSEQ∕RSEQ
			public string TpV;//时间标签有效位 0:附加信息域中无时间标签Tp , 1:附加信息域中带有时间标签Tp

			public string FIR;//首帧标志:报文的第一帧
			public string FIN;//末帧标志:报文的最后一帧
			/*
			 *FIR FIN    应用说明
			 *0   0      多帧:中间帧
			 *0   1      多帧:结束帧
			 *1   0      多帧:第1帧，有后续帧。
			 *1   1      单帧
			 */

			public string CON;//请求确认标志位CON=1:表示需要对该帧报文进行确认；CON=0:表示不需要对该帧报文进行确认
			public string PSEQ;//启动帧序号 PSEQ取自1字节的启动帧计数器PFC的低4位计数值0~15

			//数据单元标识
			public string FN;//信息类标识码 Fn:【F130=0130】
			public string PN;//信息点标识码 pn:【p0=0000】

			//数据内容(数据单元为按数据单元标识所组织的数据，包括参数、命令等)
			public string Data;

			//附加信息域AUX定义(附加信息域可由消息认证码字段PW、事件计数器EC和时间标签Tp组成)
			public string PW;// 密码
			public string EC1;// 事件计数器
			public string EC2;// 事件计数器

			//时间标签Tp内容 (启动帧帧序号计数器PFC+启动帧发送时标+允许发送传输延时时间)
			//启动帧帧序号计数器
			public string PFC;// 启动帧计数器 1字节
			public string Time;// 启动帧发送时标4字节
			public string Delay;// 传输延时 1字节

			//校验
			public string CS;

			//结束标志
			public const string FrameEnd = "16";

			//发送接收byte
			public string FrameByte;

			public void Clear()
			{
				IsSend = true;
				L1 = string.Empty;
				DIR = string.Empty;
				PRM = string.Empty;
				FCB = string.Empty;
				CID = string.Empty;
				A1 = string.Empty;
				A2 = string.Empty;
				MSA = string.Empty;
				ArrFlag = string.Empty;//标记时候为组地址
				AFN = string.Empty;
				TpV = string.Empty;
				FIR = string.Empty;
				FIN = string.Empty;
				CON = string.Empty;
				PSEQ = string.Empty;
				FN = string.Empty;
				PN = string.Empty;
				Data = string.Empty;
				PW = string.Empty;
				EC1 = string.Empty;
				EC2 = string.Empty;
				PFC = string.Empty;
				Time = string.Empty;
				Delay = string.Empty;
				CS = string.Empty;
				FrameByte = string.Empty;
			}
		}

		/// <summary>
		/// BCD码
		/// </summary>
		private string _strA1;
		public string strA1
		{
			get
			{
				return _strA1.PadLeft(4, '0');
			}
			set
			{
				_strA1 = value;
			}
		}

		/// <summary>
		/// 传入10进制数
		/// </summary>
		private string _strA2;
		public string strA2
		{
			get
			{
				return _strA2;
			}
			set
			{
				_strA2 = value;
			}
		}

		private string _strMSA;
		public string strMSA
		{
			get
			{
				return _strMSA;
			}
			set
			{
				_strMSA = value;
			}
		}

		public byte _bFlag;
		public byte bFlag
		{
			get
			{
				return _bFlag;
			}
			set
			{
				_bFlag = value;
			}
		}

		private string _pw;
		public string PW
		{
			/// 密码
			get
			{
				return _pw;
			}
			set
			{
				_pw = value;
			}
		}

		private byte _Tp;
		public byte Tp
		{
			/// 时标
			get
			{
				return _Tp;
			}
			set
			{
				_Tp = value;
			}
		}

		private byte _PFC;
		public byte PFC
		{
			/// 帧计数器
			get
			{

				return _PFC;
			}
			set
			{
				_PFC = value;
			}
		}

		private byte _Delay;
		public byte Delay
		{
			/// 允许传输延时时间
			get
			{
				return _Delay;
			}
			set
			{
				_Delay = value;
			}
		}

		private string _Data;
		public string Data
		{
			get
			{
				return _Data;
			}
			set
			{
				_Data = value;
			}
		}
	}
	#endregion

	#region 数据帧处理
	public partial class DLT698
	{
		/// <summary>
		/// 生成发送数据帧
		/// </summary>
		public byte[] GetSendDate(byte bDIR, byte bPRM, byte bFCB, byte bFCV,
								  byte bCID, string strAFN, byte bTpv, byte bFIR, byte bFIN, byte bCON,
								  string strFN, string strPN, string strUserData) //参数=strUserData
		{
			string strTemp = string.Empty;
			string strCS = string.Empty;
			string pStrSend = string.Empty;

			//控制域
			string ControlField = GetControlField(bDIR, bPRM, bFCB, bFCV, bCID);
			pStrSend = ControlField;

			//地址域
			string AddressField = GetAddressField(strA1, strA2, strMSA, bFlag);
			pStrSend += AddressField;

			//应用层功能码
			pStrSend += strAFN.PadLeft(2, '0');

			//时标
			bTpv = _Tp;
			byte bPSEQ = (byte)(PFC & 15); //PSEQ取自1字节的启动帧计数器PFC的低4位计数值0～15。

			//序列域
			string SEQ = GetSEQ(bTpv, bFIR, bFIN, bCON, bPSEQ);
			pStrSend += SEQ;

			//PNFN
			string DataCellMark = GetDataCellMark(ushort.Parse(strFN), ushort.Parse(strPN));
			pStrSend += DataCellMark;

			//用户数据
			pStrSend += strUserData;

			//密码
			if ((strAFN == "01") || (strAFN == "04") || (strAFN == "05") || (strAFN == "06") || (strAFN == "0F") || (strAFN == "10"))
			{
				string reverseStr = ReverseStr(_pw);
				pStrSend += reverseStr;
			}

			// Tp，启动帧帧号计数器，当前时间（MMHHmmss日时分秒），超时时间
			if (bTpv == 1)
			{
				//启动帧帧序号计数器PFC
				pStrSend += PFC.ToString("X2");

				//启动帧发送时标,BCD码
				pStrSend += DateTime.Now.Second.ToString("D2") + DateTime.Now.Minute.ToString("D2") + DateTime.Now.Hour.ToString("D2") + DateTime.Now.Day.ToString("D2");

				//允许发送传输延时时间
				pStrSend += _Delay.ToString("X2");
			}

			//校验计算
			string cs = CalculateCS(pStrSend); ;
			strCS = cs;

			//计算数据长度
			strTemp = GetDataLen((ushort)(pStrSend.Length / 2));

			//完整数据帧
			string frame = "68" + strTemp + strTemp + "68" + pStrSend + strCS + "16";
			pStrSend = frame;

			return HexStrToByte(pStrSend);
		}

		public byte[] GetSendDate(byte bCID, string strAFN, byte bTpv, byte bFIR, byte bFIN, byte bCON,
								  string strFN, string strPN, string strUserData)
		{
			/* bDIR=0:表示此帧报文是由主站发出的下行报文
			 * bPRM=1:表示此帧报文来自启动站
			 * bFCB=0 :
			 * bFCV=0:FCV=0表示FCB位Invalid */
			return GetSendDate(0, 1, 0, 0, bCID, strAFN, bTpv, bFIR, bFIN, bCON, strFN, strPN, strUserData);
		}

		public byte[] GetSendDate(string strCID, string strAFN, string strFN, string strPN, string strUserData)
		{
			return GetSendDate(0, 1, 0, 0, byte.Parse(strCID), strAFN, Tp, 1, 1, 0, strFN, strPN, strUserData);
		}

	}
	#endregion

	#region 系统函数定义
	public partial class DLT698
	{
		private int HexToInt(string HexChar)
		{
			int tmp = -1;
			bool convert = Int32.TryParse(HexChar, NumberStyles.HexNumber, null, out tmp);
			return tmp;
		}

		/// <summary>
		/// 字符串倒序
		/// </summary>
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

		//=============================================================================

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
			strA2 = strA2.PadLeft(4, '0');

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
			strA2 = ushort.Parse(strValue.Substring(6, 2) + strValue.Substring(4, 2), NumberStyles.HexNumber).ToString();
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
				////计算高位
				//DA = (ushort)(DA | (((iPN - 1) / 8 + 1) << 8));
				////计算低位
				//DA = (ushort)(DA | (1 << (iPN % 8 - 1)));
				////计算高位
				DA = (ushort)(DA | (((iPN - 1) / 8 + 1) << 8));
				////计算低位
				DA = (ushort)(DA | (1 << ((iPN -1) % 8)));
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

		//===================================================================================================================

		/// <summary>
		/// 解析数据帧
		/// </summary>
		public void ParseData(byte[] Arr, int ArrLength, ref FrameInfo gFrame)
		{
			bool IsFirst = false;

			string strTemp = string.Empty;
			string strTemp1 = string.Empty;

			//过滤帧头无用数据
			for (int i = 0; i < ArrLength; i++)
			{
				if (!IsFirst)
				{
					if (Arr[i] == 0x68)
					{
						IsFirst = true;
						strTemp = "68";
					}
					else
						continue;
				}
				else
					strTemp += Arr[i].ToString("X2");
			}

			FillStruct(strTemp, ref gFrame);
		}

		/// <summary>
		/// 填充帧结构体 (TO FrameData)
		/// </summary>
		/// <param name="strValue"></param>
		private void FillStruct(string strValue, ref FrameInfo gFrame)
		{
			try
			{
				gFrame.Clear();

				//L1的长度
				gFrame.L1 = GetDataLen(strValue.Substring(2, 4)).ToString();

				//控制域
				GetControlField(strValue.Substring(12, 2), ref gFrame.DIR, ref gFrame.PRM, ref gFrame.FCB, ref gFrame.FCV, ref gFrame.CID);

				//地址域
				GetAddressField(strValue.Substring(14, 10), ref gFrame.A1, ref gFrame.A2, ref gFrame.MSA, ref gFrame.ArrFlag);

				//应用层功能码
				gFrame.AFN = strValue.Substring(24, 2);

				//帧序列域
				GetSEQ(strValue.Substring(26, 2), ref gFrame.TpV, ref gFrame.FIR, ref gFrame.FIN, ref gFrame.CON, ref gFrame.PSEQ);

				//数据单元标志
				GetDataCellMark(strValue.Substring(28, 8), ref gFrame.FN, ref gFrame.PN);

				//数据内容
				string strTemp = strValue.Substring(36, strValue.Length - 40);

				if (gFrame.DIR == "0") //发出的数据
				{
					if (gFrame.TpV == "1") //带时标
					{
						GetTimeFlag(strTemp.Substring(strTemp.Length - 12, 12), ref gFrame.PFC, ref gFrame.Time, ref gFrame.Delay);
						strTemp = strTemp.Substring(0, strTemp.Length - 12);
					}
					//带密码
					if ((gFrame.AFN == "01") || (gFrame.AFN == "04") || (gFrame.AFN == "05") || (gFrame.AFN == "06") || (gFrame.AFN == "0F") || (gFrame.AFN == "10"))
					{
						gFrame.PW = strTemp.Substring(strTemp.Length - 4, 4);
						strTemp = strTemp.Substring(0, strTemp.Length - 4);
					}

					gFrame.Data = strTemp;
				}
				else //返回数据帧
				{
					if (gFrame.TpV == "1") //带时标
					{
						GetTimeFlag(strTemp.Substring(strTemp.Length - 12, 12), ref gFrame.PFC, ref gFrame.Time, ref gFrame.Delay);
						strTemp = strTemp.Substring(0, strTemp.Length - 12);
					}

					//判断EC是否存在,此时FCB=ACD
					if (gFrame.FCB == "1")
					{
						gFrame.EC1 = strTemp.Substring(strTemp.Length - 2, 2);
						gFrame.EC2 = strTemp.Substring(strTemp.Length - 4, 2);
						strTemp = strTemp.Substring(0, strTemp.Length - 4);
					}

					gFrame.Data = strTemp;
				}

				//校验
				gFrame.CS = strValue.Substring(strValue.Length - 4, 2);

				gFrame.FrameByte = strValue;
			}
			catch { }
		}

	}
	#endregion

	#region 接收帧有效性判断
	public partial class DLT698
	{
		public struct NeedAssert
		{
			//SEQ判断
			public bool SEQ;
			//Tpv判断
			public bool Tpv;
		}
		public NeedAssert Assert;

		/// <summary>
		/// 基本有效性：长度判断,CS判断
		/// </summary>
		public bool Assert_Basic(byte[] frame)
		{
			bool retLen = false;
			bool retCS = false;

			if (frame[frame.Length - 1] == 0x16)
			{
				int j = 0;
				for (j = 0; j < frame.Length; j++) if (frame[j] == 0x68) break;

				//长度判断
				string strdatalen = frame[j + 1].ToString("X2") + frame[j + 2].ToString("X2");
				int DataLen = GetDataLen(strdatalen);
				if ((frame.Length >= DataLen + 8) && (frame[j + 5] == 0x68)) retLen = true;

				//CS判断 (在长度正确的情况下)
				if (retLen)
				{
					int SumMod = 0;
					for (int i = j + 6; i < frame.Length - 2; i++) SumMod += frame[i];
					if ((byte)(SumMod % 256) == frame[frame.Length - 2]) retCS = true;
				}
			}

			return retLen & retCS;
		}

		/// <summary>
		/// SEQ判断
		/// </summary>
		public bool Assert_SEQ(string PSEQ, string SendSEQ)
		{
			if (!Assert.SEQ) return true;

			bool recTrue = false;

			if (PSEQ == SendSEQ)
			{
				recTrue = true;
			}

			return recTrue;
		}

		/// <summary>
		/// Tpv判断
		/// </summary>
		public bool Assert_Tpv(string Tpv, string SendTpv)
		{
			if (!Assert.Tpv) return true;

			bool recTrue = false;

			if (Tpv == SendTpv)
			{
				recTrue = true;
			}

			return recTrue;
		}


	}
	#endregion
}
