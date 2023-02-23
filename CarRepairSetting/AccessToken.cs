using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
//using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DataContractJsonSerializer
{
   // [DataContract]
    public class AccessTokenRequest
    {
     //   [DataMember]
        public string companycode { get; set; }
     //   [DataMember]
        public string companypassword { get; set; }

    }

   // [DataContract]
    public class AccessTokenResponse
    {
    //    [DataMember]
        public string code { get; set; }
    //    [DataMember]
        public string status { get; set; }
    //    [DataMember]
        public string access_token { get; set; }
    }

 //   [DataContract]
    public class BasicInfo
    {
   //     [DataMember]
        //    "vehicleplatenumber":"车牌号码",
        public string vehicleplatenumber;

   //     [DataMember]
        //            "companyname":"维修企业名称",
        public string companyname;

    //    [DataMember]
        //            "vin":"车辆识别代码",
        public string vin;

   //     [DataMember]
        //            "repairdate":"送修日期",
        public string repairdate;

     //   [DataMember]
        //            "repairmileage":"送修里程",
        public string repairmileage;

    //    [DataMember]
        //            "settledate":"结算日期",
        public string settledate;

    //    [DataMember]
        //            "faultdescription":"故障描述",
        public string faultdescription;

    //    [DataMember]
        //            "costlistcode":"结算清单编号"
        public string costlistcode;

    public string gd_id;

    }

  //  [DataContract]
    //配件信息
    public class VehicleParts
    {
        //配件名称
        public string partsname;
        //配件数量
        public string partsquantity;
        //配件编码
        public string partscode;
    }

 //   [DataContract]
    //维修项目
    public class RepairProject
    {
  //      [DataMember]
        //维修项目
        public String repairproject;
  //      [DataMember]
        //"workinghours":"维修工时"
        public String workinghours;
    }

 //   [DataContract]
    public class CarRepaireRequest
    {
  //      [DataMember]
        public string access_token;
   //     [DataMember]
        public BasicInfo basicInfo;
   //     [DataMember]
        public List<VehicleParts> vehiclepartslist;
   //     [DataMember]
        public List<RepairProject> repairprojectlist;
    }

  //  [DataContract]
    public class CarRepairResponse
    {
 //       [DataMember]
        public String code;
  //      [DataMember]
        public String status;
    }


    public enum HttpVerbNew
    {
        GET,            //method  常用的就这几样，可以添加其他的   get：获取    post：修改    put：写入    delete：删除
        POST,
        PUT,
        DELETE
    }
    public class ContentType//根据Postman整理，可以添加
    {
        public static string Text = "text/plain";
        public static string JSON = "application/json;charset=UTF-8";
        public static string Javascript = "application/javascript";
        public static string XML = "application/xml";
        public static string TextXML = "text/xml";
        public static string HTML = "text/html";
    }
    public class RestApiClient
    {
        public string EndPoint { get; set; }    //请求的url地址  
        public HttpVerbNew Method { get; set; }    //请求的方法
        public string ContentType { get; set; } //格式类型
        public string PostData { get; set; }    //传送的数据
        public RestApiClient()
        {
            EndPoint = "";
            Method = HttpVerbNew.GET;
            ContentType = "text/xml";
            PostData = "";
        }
        public RestApiClient(string endpoint, string contentType)
        {
            EndPoint = endpoint;
            Method = HttpVerbNew.GET;
            ContentType = contentType;
            PostData = "";
        }
        public RestApiClient(string endpoint, HttpVerbNew method, string contentType)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = contentType;
            PostData = "";
        }
        public RestApiClient(string endpoint, HttpVerbNew method, string contentType, string postData)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = contentType;
            PostData = postData;
        }
        // 添加https
       // private static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受     
        }
        // end添加https
        public string MakeRequest()
        {
            return MakeRequest("");
        }
        public string MakeRequest(string parameters)
        {
            var request = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);
            // 添加https
            if (EndPoint.Substring(0, 8) == "https://")
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            }
            // end添加https
            request.Method = Method.ToString();
            request.ContentLength = 0;
            request.ContentType = ContentType;
            if (!string.IsNullOrEmpty(PostData) && Method == HttpVerbNew.POST)//如果传送的数据不为空，并且方法是post
            {
                //   var encoding = new UTF8Encoding();
                //   var bytes = Encoding.GetEncoding("utf-8").GetBytes(PostData);//编码方式按自己需求进行更改，我在项目中使用的是UTF-8
                var bytes = Encoding.UTF8.GetBytes(PostData);//编码方式按自己需求进行更改，我在项目中使用的是UTF-8
                request.ContentLength = bytes.Length;
                using (var writeStream = request.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }
            if (!string.IsNullOrEmpty(PostData) && Method == HttpVerbNew.PUT)//如果传送的数据不为空，并且方法是put
            {
                //  var encoding = new UTF8Encoding();
                var bytes = Encoding.UTF8.GetBytes(PostData);//编码方式按自己需求进行更改，我在项目中使用的是UTF-8
               // var bytes = encoding.GetBytes(PostData);
                request.ContentLength = bytes.Length;
                using (var writeStream = request.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = string.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }
                // grab the response
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                        using (var reader = new StreamReader(responseStream))
                        {
                            responseValue = reader.ReadToEnd();
                        }
                }
                return responseValue;
            }
        }
        public bool CheckUrl(string parameters)
        {
            bool bResult = true;
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);
            myRequest.Method = Method.ToString();             //设置提交方式可以为＂ｇｅｔ＂，＂ｈｅａｄ＂等
            myRequest.Timeout = 10000;　             //设置网页响应时间长度
            myRequest.AllowAutoRedirect = false;//是否允许自动重定向
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            bResult = (myResponse.StatusCode == HttpStatusCode.OK);//返回响应的状态
            return bResult;
        }
    }

}
