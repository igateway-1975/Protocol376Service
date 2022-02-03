using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service.Protocol
{
    public class Protocol376FrameInfo
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

		/// <summary>
		/// 21020037 (0037是十六进制数)
		/// </summary>
		/// <param name="address"></param>
		public void SetCollectorAddress(String address)
        {
			A1 = address.Substring(0, 4);
			A2 = GetControlAddress(address.Substring(4, 4));
		}

		public string GetCollectorAddress()
		{
			return A1 + A2;
		}

		private string GetControlAddress(string address)
        {
			return int.Parse(address).ToString("X4");
		}

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
}
