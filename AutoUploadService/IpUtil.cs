using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoUploadService
{
    public class IpUtil
    {
        public static string getLocalIp()
        {
            ///获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }
        /// <summary>
        /// C#根据第三方提供的IP查询服务获取公网外网IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetInterNetIPFromAPI()
        {
            //测试OK, 此接口查询速度最快
            var html2 = HttpGetPageHtml("http://www.net.cn/static/customercare/yourip.asp", "gbk");
            var ip2 = GetIPFromHtml(html2);
            if (!String.IsNullOrEmpty(ip2)) return ip2;

            //测试OK
            var html1 = HttpGetPageHtml("https://www.ip.cn", "utf-8");
            var ip1 = GetIPFromHtml(html1);
            if (!String.IsNullOrEmpty(ip1)) return ip1;

            //测试失败，不提供查询服务，需要购买api服务
            var html3 = HttpGetPageHtml("http://www.ip138.com/ips138.asp", "gbk");
            var ip3 = GetIPFromHtml(html3);
            if (!String.IsNullOrEmpty(ip3)) return ip3;

            return "";
        }


        /// <summary>
        /// 获取页面html
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="encoding">编码方式</param>
        /// <returns></returns>
        private static string HttpGetPageHtml(string url, string encoding)
        {
            string pageHtml = string.Empty;
            try
            {
                using (WebClient MyWebClient = new WebClient())
                {
                    Encoding encode = Encoding.GetEncoding(encoding);
                    MyWebClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.84 Safari/537.36");
                    MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
                    Byte[] pageData = MyWebClient.DownloadData(url); //从指定网站下载数据
                    pageHtml = encode.GetString(pageData);
                }
            }
            catch (Exception e)
            {

            }
            return pageHtml;
        }

        /// <summary>
        /// 从html中通过正则找到ip信息(只支持ipv4地址)
        /// </summary>
        /// <param name="pageHtml"></param>
        /// <returns></returns>
        private static string GetIPFromHtml(String pageHtml)
        {
            //验证ipv4地址
            string reg = @"(?:(?:(25[0-5])|(2[0-4]\d)|((1\d{2})|([1-9]?\d)))\.){3}(?:(25[0-5])|(2[0-4]\d)|((1\d{2})|([1-9]?\d)))";
            string ip = "";
            Match m = Regex.Match(pageHtml, reg);
            if (m.Success)
            {
                ip = m.Value;
            }
            return ip;

        }

        public static String getCpuSn()
        {
            string cpuInfo = "";//cpu序列号
            ManagementClass cimobject = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = cimobject.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                return cpuInfo;
            }
            return "";
        }

        public static String getHardDiskId()
        {
            //获取硬盘ID
            String HDid;
            ManagementClass cimobject1 = new ManagementClass("Win32_DiskDrive");
            ManagementObjectCollection moc1 = cimobject1.GetInstances();
            foreach (ManagementObject mo in moc1)
            {
                HDid = (string)mo.Properties["Model"].Value;
                return HDid;
            }
            return "";
        }

        public static String getMacAddr()
        {
            ////获取网卡硬件地址
            string macAddr="";
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc2 = mc.GetInstances();
            foreach (ManagementObject mo in moc2)
            {
                if ((bool)mo["IPEnabled"] == true)
                {
                    macAddr = mo["MacAddress"].ToString();
                    return macAddr;
                }                    
                mo.Dispose();                
            }
            return macAddr;
        }

        public static string getMainBoardId()
        {
            string strbNumber = string.Empty;
            ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_baseboard");
            foreach (ManagementObject mo in mos.Get())
            {
                strbNumber = mo["SerialNumber"].ToString();
                break;
            }
            return strbNumber;
        }
    }
    }
