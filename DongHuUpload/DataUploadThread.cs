using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace DongHuUpload
{
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
        public static string JSON = "application/json; charset=UTF-8";
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
        //private static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
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
            var request = (HttpWebRequest)HttpWebRequest.Create(EndPoint + parameters);
            // 添加https
            if (EndPoint.Substring(0, 8) == "https://")
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            }
            // end添加https
            request.Method = Method.ToString();
            request.ContentLength = 0;
            request.ContentType = ContentType;
            request.Accept = "text/html,application/json,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36 Hutool";

            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            //request.Headers.Add("Cache-Control", "no-cache");
            request.ServicePoint.Expect100Continue = false;
            if (!string.IsNullOrEmpty(PostData) && Method == HttpVerbNew.POST)//如果传送的数据不为空，并且方法是post
            {
                var bytes = Encoding.UTF8.GetBytes(PostData);//编码方式按自己需求进行更改，我在项目中使用的是UTF-8
                request.ContentLength = bytes.Length;
                using (var writeStream = request.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }
            if (!string.IsNullOrEmpty(PostData) && Method == HttpVerbNew.PUT)//如果传送的数据不为空，并且方法是put
            {
                var encoding = new UTF8Encoding();
                var bytes = Encoding.GetEncoding("UTF-8").GetBytes(PostData);//编码方式按自己需求进行更改，我在项目中使用的是UTF-8
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

    public class RepairProject
    {
        public String project_name; //维修项目
        public String working_hour; //工时
        public String unit_price;//工时
        public double amount; //工时费
    }


    public class RepairItem
    {
        public String item_name; //配件名称
        public String in_price; //成本价
        public String amount; //数量
        public double in_price_d; //成本价
        public double amount_d; //数量
        public double total_price; //总价格
    }

    public class UploadInfo
    {
        public String gd_sn;
        public String car_no;
        public String in_date;
        public String settle_date;
        public String custmer_name;
        public List<RepairProject> repair_projects;
        public List<RepairItem> repair_items;

        public UploadInfo()
        {
            repair_projects = new List<RepairProject>();
            repair_items = new List<RepairItem>();
        }
    }


    public class Tuple
    {
        public string Item1;
        public string Item2;

        public Tuple(string t1, string t2)
        {
            this.Item1 = t1;
            this.Item2 = t2;
        }
    }


    public class UploadItem
    {
        public String project;
        public String partName;
        public Double price;
        public Double count;
        public Double amount;
        public Double laborCost;
    }

    public class UploadInfoDonghu
    {
        public String date;
        public String orderNo;
        public String plateNo;
        public String factory;
        public List<UploadItem> details;


        public UploadInfoDonghu()
        {
            details = new List<UploadItem>();
        }
    }

    public class DataUploadThread
    {

        public String connStr;
        public List<Tuple> gd_ids = new List<Tuple>();
        Dictionary<String, UploadInfo> upload_info_list = new Dictionary<string, UploadInfo>();


        public string GetConnectionStringsConfig(string connectionName)
        {
            //指定config文件读取
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string connectionString =
                config.ConnectionStrings.ConnectionStrings[connectionName].ConnectionString.ToString();
            return connectionString;
        }

        public void getGdIds()
        {
            gd_ids.Clear();
      
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                using (SqlCommand cmd = con.CreateCommand())
                {
                    

                    String sql = "select top 1 id, gd_id from donghu_upload where is_uploaded=0 order by id asc";
                    cmd.CommandText = sql;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            String id = reader.GetInt32(0).ToString();
                            String gd_id = reader.GetInt32(1).ToString();
                            gd_ids.Add(new Tuple(id, gd_id));
                        }
                    }
                }
            }
            if (gd_ids.Count > 0)
            {
                LogHelper.WriteLog(typeof(DataUploadThread), "upload gd_ids:" + formatGdIds());
            }
        }

        private String  formatGdIds()
        {
            string[] gd_ids_tmp = new string[gd_ids.Count];
            for (int index=0;index< gd_ids.Count; index++)
            {
                gd_ids_tmp[index] = gd_ids[index].Item2;
            }
            return String.Join(",", gd_ids_tmp);
        }

        private string getTrimString(SqlDataReader sdr, string key, string defaultValue)
        {
            string value = sdr[key].ToString();
            if (value == null || value.Trim().Length == 0)
            {
                value = defaultValue;
            }
            return value.Trim();
        }                            

        public string getTimeString(SqlDataReader sdr, string key, string defaultValue)
        {
            int columnId = sdr.GetOrdinal(key);
            return sdr.GetDateTime(columnId).ToString("yyyy-MM-dd HH:mm:ss");
        }

        public double getDoubleValue(SqlDataReader sdr, string key, Double defaultValue)
        {
            Double value = defaultValue;
            int columnId = sdr.GetOrdinal(key);
            if (! sdr.IsDBNull(columnId))
            {
                String v = sdr.GetString(columnId);
                value = sdr.GetDouble(columnId);
            }
            return value;
        }

        public string getDateString(SqlDataReader sdr, string key, string defaultValue)
        {
            int columnId = sdr.GetOrdinal(key);
            return sdr.GetDateTime(columnId).ToString("yyyy/MM/dd");
        }

        public void getUploadInfos()
        {
            upload_info_list.Clear();

            if (gd_ids.Count > 0) {
                String ids = formatGdIds();
                String sql = String.Format("select t1.gd_sn, t1.gd_id, t1.settle_dt, t2.car_no, t1.in_dt, t3.error_desp, t5.cust_nm from dt_om_gd t1 left join mt_cl t2 on t1.cl_id = t2.cl_id left join DT_OM_JJJC t3 on t1.gd_id = t3.gd_id left join MT_KH t5 on t1.kh_id=t5.kh_id where t1.gd_id in ({0})", ids);
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        con.Open();

                        cmd.CommandText = sql;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                String gd_id = getTrimString(reader, "gd_id", "");
                                String gd_sn = getTrimString(reader, "gd_sn", "");
                                String settle_dt = getTimeString(reader, "settle_dt", "");
                                String settle_date = getDateString(reader, "settle_dt", "");
                                String in_dt = getTimeString(reader, "in_dt", "");
                                String cust_nm = getTrimString(reader, "cust_nm", "");
                                String car_no = getTrimString(reader, "car_no", "");

                                UploadInfo uploadInfo = new UploadInfo();
                                uploadInfo.gd_sn = gd_sn;
                                uploadInfo.in_date = in_dt;
                                uploadInfo.settle_date = settle_date;
                                uploadInfo.car_no = car_no;
                                uploadInfo.custmer_name = cust_nm;
                                upload_info_list[gd_id] = uploadInfo;
                            }
                        }
                    }
                }

                sql = String.Format("select A.GD_ID, A.unit_price, A.PRJ_NM as project_name,  A.man_hour as working_hour from DT_OM_BXXM A where gd_id in ({0})", ids);
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        con.Open();

                        cmd.CommandText = sql;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                String gd_id = getTrimString(reader, "gd_id", "");
                                String unit_price = getTrimString(reader, "unit_price", "0");
                                Double unit_price_d = 0;
                                Double.TryParse(unit_price, out unit_price_d);
                                Double working_hour_d = 0;
                                String project_name = getTrimString(reader, "project_name", "");
                                String working_hour = getTrimString(reader, "working_hour", "0");
                                Double.TryParse(working_hour, out working_hour_d);
                                RepairProject repair_project = new RepairProject();
                                repair_project.project_name = project_name;
                                repair_project.unit_price = unit_price;
                                repair_project.working_hour = working_hour;
                                repair_project.amount = Math.Round(Math.Round(unit_price_d, 2) *Math.Round(working_hour_d, 2), 2);
                                upload_info_list[gd_id].repair_projects.Add(repair_project);
                            }
                        }
                    }
                }

                sql = String.Format("select B.relative_id as gd_id, c.PART_NM as item_name,  C.QTY as amount, c.in_price as in_price from DT_EM_CKD  B left JOIN DT_EM_CKLJ C ON B.OUTPUT_ID=C.OUTPUT_ID where  B.relative_id in ({0})", ids);
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        con.Open();

                        cmd.CommandText = sql;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                String gd_id = getTrimString(reader, "gd_id", "");
                                String in_price = getTrimString(reader, "in_price", "0");
                                Double in_price_d = 0;
                                String item_name = getTrimString(reader, "item_name", "");
                                String amount = getTrimString(reader, "amount", "0");
                                Double amount_d = 0;
                                Double.TryParse(amount, out amount_d);
                                Double.TryParse(in_price, out in_price_d);
                                RepairItem repair_item = new RepairItem();
                                repair_item.item_name = item_name;
                                repair_item.amount = amount;
                                repair_item.in_price = in_price;
                                repair_item.amount_d = Math.Round(amount_d,2);
                                repair_item.in_price_d = Math.Round(in_price_d, 2);
                                repair_item.total_price = Math.Round(amount_d * in_price_d, 2);
                                upload_info_list[gd_id].repair_items.Add(repair_item);
                            //    LogHelper.WriteLog(typeof(DataUploadThread), String.Format("gd_id:{0}, item_name:{1}, amount:{2}, in_price:{3}", gd_id, item_name, repair_item.amount, in_price));
                            }
                        }
                    }
                }
            }
        }

        private void dbExecNoReturn(string sql)
        {
            
            using (SqlConnection dbCon = new SqlConnection(connStr))
            {
                dbCon.Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = dbCon;
                sqlcmd.CommandText = sql;
                sqlcmd.ExecuteNonQuery();
            }
        }

        private void updateUploadGdStatus(string id, string resp, string req)
        {
            //JObject jobject = (JObject)JsonConvert.DeserializeObject(resp);
            //String is_uploaded = jobject["code"].ToString();
            //if (is_uploaded.Equals("0"))
            //{
            //    is_uploaded = "1";
            //}
             String is_uploaded = "1";
             string sql = string.Format("update donghu_upload set is_uploaded=1, request_str='{3}', reponse_str='{1}' where id={2}", is_uploaded, req, id, resp);
            // LogHelper.WriteLog(typeof(GlobalData), sql);
            dbExecNoReturn(sql);
        }

        private UploadInfoDonghu convertToDongHuFormat(UploadInfo uploadInfo)
        {
            UploadInfoDonghu uploadInfoDonghu = new UploadInfoDonghu();
            uploadInfoDonghu.date = uploadInfo.settle_date;
            uploadInfoDonghu.factory = GlobalData.factory_name;
            uploadInfoDonghu.orderNo = uploadInfo.gd_sn;
            uploadInfoDonghu.plateNo = uploadInfo.car_no;

            for (int index=0; index<uploadInfo.repair_projects.Count; index++)
            {
                UploadItem uploadItem = new UploadItem();
                uploadItem.project = uploadInfo.repair_projects[index].project_name;
                uploadItem.laborCost = uploadInfo.repair_projects[index].amount;
                if (uploadInfo.repair_projects.Count<= uploadInfo.repair_items.Count)
                {
                    uploadItem.partName = uploadInfo.repair_items[index].item_name;
                    uploadItem.price = uploadInfo.repair_items[index].in_price_d;
                    uploadItem.count = uploadInfo.repair_items[index].amount_d;
                    uploadItem.amount = uploadInfo.repair_items[index].total_price;
                }

                if (uploadInfo.repair_projects.Count > uploadInfo.repair_items.Count && index < uploadInfo.repair_items.Count)
                {
                    uploadItem.partName = uploadInfo.repair_items[index].item_name;
                    uploadItem.price = uploadInfo.repair_items[index].in_price_d;
                    uploadItem.count = uploadInfo.repair_items[index].amount_d;
                    uploadItem.amount = uploadInfo.repair_items[index].total_price;
                }
                uploadInfoDonghu.details.Add(uploadItem);
            }

            if (uploadInfo.repair_projects.Count < uploadInfo.repair_items.Count)
            {
                int last_project_id = uploadInfo.repair_projects.Count - 1;
                for (int index= uploadInfo.repair_projects.Count; index< uploadInfo.repair_items.Count; index++)
                {
                    UploadItem uploadItem = new UploadItem();
                    uploadItem.project = uploadInfo.repair_projects[last_project_id].project_name;
                    uploadItem.laborCost = uploadInfo.repair_projects[last_project_id].amount;
                    uploadItem.partName = uploadInfo.repair_items[index].item_name;
                    uploadItem.price = uploadInfo.repair_items[index].in_price_d;
                    uploadItem.count = uploadInfo.repair_items[index].amount_d;
                    uploadItem.amount = uploadInfo.repair_items[index].total_price;
                    uploadInfoDonghu.details.Add(uploadItem);
                }
            }

                return uploadInfoDonghu;

        }
        public void uploadSettleInfo(UploadInfo upload_info)
        {
            UploadInfoDonghu uploadInfoDongHu = convertToDongHuFormat(upload_info);
            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;
            string json = JsonConvert.SerializeObject(uploadInfoDongHu, Formatting.None, setting);
            Console.WriteLine(json);

            try
            {
                LogHelper.WriteLog(typeof(DataUploadThread), "upload request:" + json);
                var restApiClient = new RestApiClient(GlobalData.server_url, HttpVerbNew.POST, ContentType.JSON, json);
                string response = restApiClient.MakeRequest();

                LogHelper.WriteLog(typeof(DataUploadThread), "upload response:" + response);
                updateUploadGdStatus(gd_ids[0].Item1, json, response);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(DataUploadThread), "error during upload");
                LogHelper.WriteLog(typeof(DataUploadThread), ex);
            }
        }

        public void getAndUpload()
        {
            getGdIds();
            if (gd_ids.Count > 0)
            {
                getUploadInfos();
                foreach (var item in upload_info_list)
                {
                    uploadSettleInfo(item.Value);
                }
            }
        }
        public void ParameterRun(object param)
        {
            //
            connStr = GetConnectionStringsConfig("netmis_db");
            while (true)
            {
                getAndUpload();
                Thread.Sleep(GlobalData.upload_internal);
            }

        }
    }
}
