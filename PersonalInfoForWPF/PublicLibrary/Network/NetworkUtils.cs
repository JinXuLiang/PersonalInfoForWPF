using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PublicLibrary.Network
{
    public class NetworkUtils
    {
        //
        /// <summary>
        /// 获取本机的MAC地址(指当前激活的），测试发现：即使网线被拨出或禁用，也一样，但未测试有多个网络可达时，到底
        /// 返回哪个网卡的Mac地址,当前只是返回第一个Mac地址
        /// 返回字串格式：
        /// 02-1C-12-FF-0D-D6
        /// </summary>
        /// <returns></returns>
        public static  string GetLocalMacAddress()
        {
            string mac = null;
            ManagementObjectSearcher query = new ManagementObjectSearcher("Select * FROM Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection queryCollection = query.Get();
            foreach (ManagementObject mo in queryCollection)
            {
                if (mo["IPEnabled"].ToString() == "True"){
                   
                    mac = mo["MacAddress"].ToString();
                    mac = mac.Replace(':', '-');
                    break;
                }
            }
            return mac;
        }

        public static List<string> GetAllLocalMacAddress()
        {
            List<string> macs = new List<string>();
            String mac = null;
            ManagementObjectSearcher query = new ManagementObjectSearcher("Select * FROM Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection queryCollection = query.Get();
            foreach (ManagementObject mo in queryCollection)
            {
                if (mo["IPEnabled"].ToString() == "True")
                {

                    mac = mo["MacAddress"].ToString();
                    mac = mac.Replace(':', '-');
                    macs.Add(mac);
                  
                }
            }
            return macs;
        }

        /// <summary>
        /// 获取本机IPv4地址的集合
        /// </summary>
        /// <returns></returns>
        public static IPAddress[] GetLocalhostIPv4Addresses()
        {
            String LocalhostName = Dns.GetHostName();

            IPHostEntry host = Dns.GetHostEntry(LocalhostName);
            List<IPAddress> addresses = new List<IPAddress>();
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    addresses.Add(ip);
            }
            return addresses.ToArray();
        }

        /// <summary>
        /// 从流中读取全部数据，返回为字节数组
        /// 如果不可读，返回null
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] readFromStream(Stream stream)
        {
            if (stream == null || !stream.CanRead)
                return null;
            MemoryStream ms = new MemoryStream();

            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            do
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                ms.Write(buffer, 0, bytesRead);

            } while (bytesRead > 0);
            ms.Position = 0;

            return ms.ToArray();
        }

    }
}
