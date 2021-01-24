using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace HandyUploadForm
{
    public class SignUtils
    {
        private static string Md5Hex(string data)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] dataHash = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in dataHash)
            {
                sb.Append(b.ToString("x2").ToLower());
            }
            return sb.ToString();
        }

        public static String sign(SortedDictionary<String, String> param, String secretKey)
        {
            if (param == null){
                return Md5Hex(secretKey + "|");
            }
                                                              // 拼接参数
            StringBuilder sb = new StringBuilder();
            foreach (String key in param.Keys)
            {
                String value = null;
                param.TryGetValue(key, out value);
                sb.Append(key).Append("=").Append(value).Append("&");
            }
            String content =sb.ToString().Substring(0, sb.ToString().Length-1); // 去掉最后一个 &
            String sign = Md5Hex(secretKey + "|" + content); // 使用 MD5 计算签名字符串
            return sign;
        }

        public static SignInfo sign()
        {
            SignInfo signInfo = new SignInfo();
            Random rd = new Random();
            signInfo.nonce = rd.Next().ToString();
            signInfo.timestamp = TimeUtil.getCurrentSeconds().ToString();

            SortedDictionary<String, String> param = new SortedDictionary<string, string>();
            param.Add("companyIdentity", GlobalData.companyIdentity);
            param.Add("nonce", signInfo.nonce);
            param.Add("timestamp", signInfo.timestamp);
            
            String value = SignUtils.sign(param, GlobalData.secretKey);
            signInfo.sign = value;
            return signInfo;
        }
    }
}
