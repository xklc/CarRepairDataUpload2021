using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading;
using DataContractJsonSerializer;
using Newtonsoft.Json;

namespace CarRepairDataUpload
{
    public class DataUpload
    {
        public ConfigItem configItem;
        private string connStr;
        private SqlConnection sqlConn;
        private string commpanyname;

        private static string accessTokenUrl = "https://api.qcda.shanghaiqixiu.org/restservices/lcipprodatarest/lcipprogetaccesstoken/query";
        private static string carRepairItemUrl = "https://api.qcda.shanghaiqixiu.org/restservices/lcipprodatarest/lcipprocarfixrecordadd/query";

        private SqlConnection getConnection()
        {
            if (sqlConn == null || sqlConn.State == System.Data.ConnectionState.Closed)
            {
                sqlConn = new SqlConnection(connStr);
            }
            return sqlConn;
        }

        private void dbExecNoReturn(string sql)
        {
            SqlConnection dbCon = getConnection();
            try
            {
                dbCon.Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = dbCon;
                sqlcmd.CommandText = sql;
                sqlcmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dbCon.Close();
            }
        }

        public DataUpload(ConfigItem configItem)
        {
            this.configItem = configItem;
            this.connStr = String.Format("server={0};database={1};uid={2};pwd={3}", configItem.DBHost,
                                configItem.DBName, configItem.DBUser, configItem.DBPassword);
        }

        private AccessTokenResponse getToken()
        {
            AccessTokenRequest acessTokenRequest = new AccessTokenRequest();
            acessTokenRequest.companycode = configItem.CompanyCode;
            acessTokenRequest.companypassword = configItem.CompanyPassword;
            //Json.NET序列化
            string jsonData = JsonConvert.SerializeObject(acessTokenRequest);

            try
            {
                RestApiClient restApiClient = new RestApiClient(accessTokenUrl, HttpVerbNew.POST, ContentType.JSON, jsonData);
                String response = restApiClient.MakeRequest();
                AccessTokenResponse accessTokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(response);
                return accessTokenResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static string formatCostListCode(string srcCode)
        {
            string day = srcCode.Substring(1, 9);
            string number = srcCode.Substring(9);
            string resultCode = "QXT_" + day + "_0" + number;
            return resultCode;
        }

        public string getTrimString(SqlDataReader sdr, string key, string defaultValue)
        {
            string value = sdr[key].ToString();
            if (value == null || value.Trim().Length == 0)
            {
                value = defaultValue;
            }
            return value.Trim();
        }

        private string getDateStr(SqlDataReader sdr, string key, string defaultValue)
        {
            return Convert.ToDateTime(sdr[key]).ToString("yyyyMMdd");
        }

        //公司名字从数据库中查询得到
        public void getCompanyName()
        {
            //string filename = "companyname";
           // string companyname = null;
            //if (File.Exists(filename))
            //{
            //    companyname = File.ReadAllText("companyname", Encoding.UTF8);
            //}
            //else
            //{
            SqlConnection dbCon = getConnection();
            try
            {
                dbCon.Open();
                using (SqlCommand cmd = dbCon.CreateCommand())
                {
                    cmd.CommandText = "select value from StringParameter where Name='公司名称 '";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            this.commpanyname = reader["value"].ToString();
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dbCon.Close();
            }
            //    File.WriteAllText(filename, companyname);
            //}

           // return commpanyname;

        }

        private List<CarRepaireRequest> getCarItemList(string access_token)
        {
            SqlConnection dbCon = getConnection();

            if (commpanyname == null)
            {
                this.getCompanyName();
            }
            
            if (this.commpanyname == null)
            {
                return null;
            }
            List<CarRepaireRequest> requestList = new List<CarRepaireRequest>();
            Dictionary<string, CarRepaireRequest> gdId2CarRepaireRequest = new Dictionary<string, CarRepaireRequest>();

            try
            {
                dbCon.Open();
                SqlCommand sqlcmd = new SqlCommand();

                string sql = "select a.gd_id, a.GD_SN, b.VIN_CODE, b.Car_no, A.IN_DT, C.MILES,  A.SETTLE_DT,C.ERROR_DESP,A.GD_SN from DT_OM_GD a, MT_CL b, DT_OM_JJJC C where A.CL_ID=B.CL_ID and A.GD_ID=C.GD_ID and a.is_settle =1 and a.gd_id in (select top 100 gd_id from dataupload_gd where is_uploaded=0 order by create_time desc) order by a.settle_dt asc";
                sqlcmd.CommandText = sql;
                sqlcmd.Connection = dbCon;


                SqlDataReader sqlDataReader = sqlcmd.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    CarRepaireRequest carRepaireRequest = new CarRepaireRequest();
                    carRepaireRequest.access_token = access_token;
                    BasicInfo basicInfo = new BasicInfo();
                    basicInfo.vehicleplatenumber = getTrimString(sqlDataReader, "Car_no", "N/A");
                    basicInfo.gd_id = getTrimString(sqlDataReader, "gd_id", "N/A");
                    basicInfo.companyname = this.commpanyname;
                    basicInfo.vin = getTrimString(sqlDataReader, "VIN_CODE", this.configItem.vinCodeList[new Random().Next(this.configItem.vinCodeList.Count)]);
                    if (basicInfo.vin.Length != 17)
                    {
                        basicInfo.vin = this.configItem.vinCodeList[new Random().Next(this.configItem.vinCodeList.Count)];
                    }
                    basicInfo.repairdate = getDateStr(sqlDataReader, "IN_DT", "N/A");
                    basicInfo.settledate = getDateStr(sqlDataReader, "SETTLE_DT", "N/A");
                    string miles = new Random().Next(100000).ToString();
                    basicInfo.repairmileage = getTrimString(sqlDataReader, "MILES", miles);

                    basicInfo.faultdescription = getTrimString(sqlDataReader, "ERROR_DESP", "");
                    if (basicInfo.faultdescription == null || basicInfo.faultdescription.Length == 0)
                    {
                        basicInfo.faultdescription = this.configItem.errorDespList[new Random().Next(configItem.errorDespList.Count)];
                    }
                    //basicInfo.costlistcode = formatCostListCode(getTrimString(sqlDataReader, "GD_SN", ""));
                    basicInfo.costlistcode = getTrimString(sqlDataReader, "GD_SN", "");
                    carRepaireRequest.basicInfo = basicInfo;
                    gdId2CarRepaireRequest[basicInfo.gd_id] = carRepaireRequest;
                    requestList.Add(carRepaireRequest);
                }
                sqlDataReader.Close();

                if (requestList.Count == 0)
                {
                    return requestList; 
                }

                List<String> keys = new List<String>();
                foreach (var key in gdId2CarRepaireRequest.Keys)
                {
                    keys.Add(key.ToString());
                }
                ;
                String gdIds = "(" + string.Join(",", keys.ToArray()) + ")";


                Dictionary<String, List<VehicleParts>> gdId2VehiclePartsList = new Dictionary<string, List<VehicleParts>>();
                sql = "select d.gd_id, C.PART_NM,C.ORIGINAL_FACTORY_ID, A.QTY from DT_EM_CKLJ A, DT_EM_CKD B, DT_EM_LJML C, DT_OM_GD D where A.OUTPUT_ID =B.OUTPUT_ID and A.PART_ID = C.PART_ID and d.GD_ID=B.RELATIVE_ID " +
                " AND d.gd_id in " + gdIds;
                sqlcmd.CommandText = sql;
                sqlDataReader = sqlcmd.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    VehicleParts vehicleParts = new VehicleParts();
                    String gd_id = getTrimString(sqlDataReader, "gd_id", "N/A");
                    vehicleParts.partsname = getTrimString(sqlDataReader, "PART_NM", "N/A");
                    vehicleParts.partscode = getTrimString(sqlDataReader, "ORIGINAL_FACTORY_ID", "N/A");
                    //if (vehicleParts.partscode != null)
                    //{
                    //    vehicleParts.partscode = vehicleParts.partscode.Replace("ht_", "qxt_");
                    //}
                    vehicleParts.partsquantity = getTrimString(sqlDataReader, "QTY", "N/A");

                    List<VehicleParts> vehiclePartsList = null;
                    if (!gdId2VehiclePartsList.TryGetValue(gd_id, out vehiclePartsList))
                    {
                        vehiclePartsList = new List<VehicleParts>();
                        gdId2VehiclePartsList[gd_id] = vehiclePartsList;
                    }
                    vehiclePartsList.Add(vehicleParts);
                    //if (gdId2VehiclePartsList.ContainsKey(gd_id))
                    //{
                    //    List<VehicleParts> vehiclePartsList = gdId2VehiclePartsList[gd_id];
                    //    if (vehiclePartsList == null)
                    //    {
                    //        vehiclePartsList = new List<VehicleParts>();
                    //        gdId2VehiclePartsList[gd_id] = vehiclePartsList;
                    //    }
                    //    vehiclePartsList.Add(vehicleParts);
                    //}
                }
                sqlDataReader.Close();

                sql = "select A.GD_ID, B.PRJ_NM, B.MAN_HOUR from DT_OM_GD A, DT_OM_BXXM B where A.GD_ID=B.GD_ID AND A.gd_id in " + gdIds;
                Dictionary<String, List<RepairProject>> gdId2RepairProjectList = new Dictionary<String, List<RepairProject>>();
                sqlcmd.CommandText = sql;
                sqlDataReader = sqlcmd.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    RepairProject repairProject = new RepairProject();
                    String gd_id = getTrimString(sqlDataReader, "gd_id", "N/A");
                    repairProject.repairproject = getTrimString(sqlDataReader, "PRJ_NM", "N/A");
                    repairProject.workinghours = getTrimString(sqlDataReader, "MAN_HOUR", "N/A");


                    List<RepairProject> repairProjectList = null;
                    if (!gdId2RepairProjectList.TryGetValue(gd_id, out repairProjectList))
                    {
                        repairProjectList = new List<RepairProject>();
                        gdId2RepairProjectList[gd_id] = repairProjectList;
                    }
                    repairProjectList.Add(repairProject);

                }
                sqlDataReader.Close();

                foreach (var key in gdId2CarRepaireRequest.Keys)
                {
                    List<VehicleParts> vehiclePartsList = null;
                    if (!gdId2VehiclePartsList.TryGetValue(key, out vehiclePartsList))
                    {
                        vehiclePartsList = new List<VehicleParts>();
                    }
                    CarRepaireRequest carRepaireRequest = (CarRepaireRequest)gdId2CarRepaireRequest[key];
                    carRepaireRequest.vehiclepartslist = vehiclePartsList;
                }

                foreach (var key in gdId2CarRepaireRequest.Keys)
                {
                    List<RepairProject> repairProjectsList = null;
                    if (!gdId2RepairProjectList.TryGetValue(key, out repairProjectsList))
                    {
                        repairProjectsList = new List<RepairProject>();
                    }
                    CarRepaireRequest carRepaireRequest = (CarRepaireRequest)gdId2CarRepaireRequest[key];
                    carRepaireRequest.repairprojectlist = repairProjectsList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dbCon.Close();
            }

            return requestList;
        }

        private CarRepairResponse uploadCarItem(CarRepaireRequest carRepaireRequest)
        {
            //string jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii });

            //Json.NET序列化
          //  string jsonData = JsonConvert.SerializeObject(carRepaireRequest, Formatting.Indented, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii });
            string jsonData = JsonConvert.SerializeObject(carRepaireRequest);
            try
            {
                RestApiClient restApiClient = new RestApiClient(carRepairItemUrl, HttpVerbNew.POST, ContentType.JSON, jsonData);
                String response = restApiClient.MakeRequest();
                CarRepairResponse carRepaireResponse = JsonConvert.DeserializeObject<CarRepairResponse>(response);
                return carRepaireResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool updateUploadStatus(string req_json_data, string resp_json_data, string gd_id)
        {
            //TODO: 更新表格
            string sql = string.Format("update dataupload_gd set is_uploaded=1, request_str=\'{0}\', response_str=\'{1}\' where gd_id={2}",
                 req_json_data, resp_json_data, gd_id);
            this.dbExecNoReturn(sql);
            return true;
        }

        public void ParameterRun(object param)
        {
            while (true)
            {
                try
                {
                    //get access_token
                    LogHelper.WriteLog(typeof(DataUpload), "begin request access token from server");
                    AccessTokenResponse accessTokenResponse = getToken();

                    if (accessTokenResponse != null)
                    {
                        LogHelper.WriteLog(typeof(DataUpload), "access_token:" + accessTokenResponse.access_token);
                        LogHelper.WriteLog(typeof(DataUpload), "begin get car repair items from db");
                        List<CarRepaireRequest> carRepaireRequestList = getCarItemList(accessTokenResponse.access_token);
                        LogHelper.WriteLog(typeof(DataUpload), "get car repair items from db finish, size:" + carRepaireRequestList.Count);
                        //上传
                        //先输出到屏幕
                        foreach (var carRepaireRequest in carRepaireRequestList)
                        {
                            string request_json_data = JsonConvert.SerializeObject(carRepaireRequest);
                            LogHelper.WriteLog(typeof(DataUpload), "Begin upload repair item to server, RequestStr:" + request_json_data);
                            CarRepairResponse carRepairResponse = uploadCarItem(carRepaireRequest);
                            string response_json_data = JsonConvert.SerializeObject(carRepairResponse);
                            LogHelper.WriteLog(typeof(DataUpload), "IN:" + request_json_data + "|||" + "RESPONSE:" + response_json_data);
                            //   last_settle_time = carRepaireRequest.basicInfo.dtSettleDate.getTime();
                            updateUploadStatus(request_json_data, response_json_data, carRepaireRequest.basicInfo.gd_id);
                            //writeLastSettleTime();
                            //System.out.println(JSON.toJSONString(carRepaireRequest));


                        }
                    }
                    else
                    {
                        LogHelper.WriteLog(typeof(DataUpload), "access token is null, error happend!");
                    }
                    Thread.Sleep(1000);
                }
                catch (System.Exception ex)
                {
                    LogHelper.WriteLog(typeof(DataUpload), ex.Message);
                }
                finally
                {
                }
                Thread.Sleep(60000);
            }
        }
    }
}
