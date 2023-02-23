using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace HandyUploadForm
{
    public class SignUtils
    {
        //private static string Md5Hex(string data)
        //{
        //    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        //    byte[] dataHash = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
        //    StringBuilder sb = new StringBuilder();
        //    foreach (byte b in dataHash)
        //    {
        //        sb.Append(b.ToString("x2").ToLower());
        //    }
        //    return sb.ToString();
        //}


        //public static SignInfo sign(PickCarInfo pickCarInfo)
        //{
        //    SignInfo signInfo = new SignInfo();

        //    //string json = JsonConvert.SerializeObject(pickCarInfo);

        //    IDictionary<String, Object> list = ToMap(pickCarInfo);

        //    String value = SignUtils.sign(list, GlobalData.secret);
        //    signInfo.sign = value;
        //    return signInfo;
        //}
    }
}
