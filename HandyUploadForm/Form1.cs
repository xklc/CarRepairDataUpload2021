using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DataContractJsonSerializer;
using CarRepairDataUpload;
using Newtonsoft.Json;

namespace HandyUploadForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.configItem = new ConfigItem();
            this.connStr = String.Format("server={0};database={1};uid={2};pwd={3}", configItem.DBHost,
                                configItem.DBName, configItem.DBUser, configItem.DBPassword);
        }
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
                string createsql = "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='dataupload_gd' AND xtype='U') CREATE TABLE [dbo].[dataupload_gd] ([id] [int] IDENTITY (1, 1) NOT NULL , [gd_id] [int] NOT NULL ,[cl_id] [int] NOT NULL ,[settle_dt] [datetime] NULL ,	[is_uploaded] [bit] NULL ,[request_str] [text] COLLATE Chinese_PRC_CI_AS NULL ,	[response_str] [nchar] (200) COLLATE Chinese_PRC_CI_AS NULL ,	[create_time] [datetime] default getdate() )";
                sqlcmd.CommandText = createsql;
                sqlcmd.ExecuteNonQuery();
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


        private AccessTokenResponse getToken()
        {
            this.getUploadUserNameAndCode();
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

        //公司名字从数据库中查询得到
        public void getUploadUserNameAndCode()
        {
            //string filename = "companyname";
            // string companyname = null;
            //if (File.Exists(filename))
            //{
            //    companyname = File.ReadAllText("companyname", Encoding.UTF8);
            //}
            //else
            //{

 //       [uname]
 //       [varchar] (50) COLLATE Chinese_PRC_CI_AS NULL ,
	//[upass]
 //       [varchar] (50) COLLATE Chinese_PRC_CI_AS NULL ,
	//[url]
 //       [varchar] (500) COLLATE Chinese_PRC_CI_AS NULL ,
	//[upurl]
 //       [varchar] (500) COLLATE Chinese_PRC_CI_AS NULL
          //如果配置文件中为空, 则读取数据库表中的配置
          if (configItem.CompanyCode.Trim().Length > 0)
            {
                return;
            }
         SqlConnection dbCon = getConnection();
            try
            {
                dbCon.Open();
                using (SqlCommand cmd = dbCon.CreateCommand())
                {
                    cmd.CommandText = "select * from up_name";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            this.configItem.CompanyCode = reader["uname"].ToString();
                            this.configItem.CompanyPassword = reader["upass"].ToString();
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

        private List<CarRepaireRequest> getCarItemList(string access_token,List<String> gd_ids)
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

                String gd_id_str = String.Join(",", gd_ids.ToArray());
                //string sql = "select a.gd_id, a.GD_SN, b.VIN_CODE, b.Car_no, A.IN_DT, C.MILES,  A.SETTLE_DT,C.ERROR_DESP,A.GD_SN from DT_OM_GD a, MT_CL b, DT_OM_JJJC C where A.CL_ID=B.CL_ID and A.GD_ID=C.GD_ID and a.is_settle =1 and a.gd_id in (select top 100 gd_id from dataupload_gd where is_uploaded=0 order by create_time desc) order by a.settle_dt asc";
                string sql = String.Format("select a.gd_id, a.GD_SN, b.VIN_CODE, b.Car_no, A.IN_DT, C.MILES,  A.SETTLE_DT,C.ERROR_DESP,A.GD_SN from DT_OM_GD a, MT_CL b, DT_OM_JJJC C where A.CL_ID=B.CL_ID and A.GD_ID=C.GD_ID and a.is_settle =1 and a.gd_id in ({0}) order by a.settle_dt asc", gd_id_str);
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
                    basicInfo.vin = getTrimString(sqlDataReader, "VIN_CODE", "N/A");
                    if (basicInfo.vin.Length != 17)
                    {
                        basicInfo.vin = "";
                    }
                    basicInfo.repairdate = getDateStr(sqlDataReader, "IN_DT", "N/A");
                    basicInfo.settledate = getDateStr(sqlDataReader, "SETTLE_DT", "N/A");
                    string miles = new Random().Next(100000).ToString();
                    basicInfo.repairmileage = getTrimString(sqlDataReader, "MILES", miles);

                    basicInfo.faultdescription = getTrimString(sqlDataReader, "ERROR_DESP", "小修");
          
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
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.ToString(), "错误, 官方服务可能出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw ex;
            }
        }

        private bool updateUploadStatus(string req_json_data, string resp_json_data, string gd_id)
        {
            //TODO: 更新表格
            //            string sql = string.Format("update dataupload_gd set is_uploaded=1, request_str=\'{0}\', response_str=\'{1}\' where gd_id={2}",
            //                 req_json_data, resp_json_data, gd_id);
           // req_json_data = req_json_data.Replace("\\", "");
           // resp_json_data = resp_json_data.Replace("\\", "");
            string sql = string.Format("insert into dataupload_gd(gd_id,cl_id, is_uploaded,request_str,response_str) values({0},1,0, \'{1}\',\'{2}\')", gd_id, req_json_data, resp_json_data);

            this.dbExecNoReturn(sql);
            return true;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            List<String> gd_ids = new List<String>();
            for (int index=0; index<this.listView1.Items.Count; index++)
            {
                if (listView1.Items[index].Checked)
                {
                    if (listView1.Items[index].BackColor == Color.Red)
                    {
                        MessageBox.Show(listView1.Items[index].ToolTipText, "错误!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    String gd_id = String.Format("{0}", listView1.Items[index].Text);
                    gd_ids.Add(gd_id);
                }
            }
            //get access_token
            LogHelper.WriteLog(typeof(Form1), "begin request access token from server");
            AccessTokenResponse accessTokenResponse = getToken();

            if (accessTokenResponse != null )
            {
                if (accessTokenResponse.code.Equals("1")) { 
                LogHelper.WriteLog(typeof(Form1), "access_token:" + accessTokenResponse.access_token);
                LogHelper.WriteLog(typeof(Form1), "begin get car repair items from db");
                List<CarRepaireRequest> carRepaireRequestList = getCarItemList(accessTokenResponse.access_token, gd_ids);
                LogHelper.WriteLog(typeof(Form1), "get car repair items from db finish, size:" + carRepaireRequestList.Count);
                //上传
                //先输出到屏幕
                foreach (var carRepaireRequest in carRepaireRequestList)
                {
                    string request_json_data = JsonConvert.SerializeObject(carRepaireRequest);
                    LogHelper.WriteLog(typeof(Form1), "Begin upload repair item to server, RequestStr:" + request_json_data);
                    CarRepairResponse carRepairResponse = uploadCarItem(carRepaireRequest);
                    string response_json_data = JsonConvert.SerializeObject(carRepairResponse);
                    LogHelper.WriteLog(typeof(Form1), "IN:" + request_json_data + "|||" + "RESPONSE:" + response_json_data);
                    //   last_settle_time = carRepaireRequest.basicInfo.dtSettleDate.getTime();
                    //2019年1月23日 应永川要求, 注销这段功能
                  //  updateUploadStatus(request_json_data, response_json_data, carRepaireRequest.basicInfo.gd_id);
                    //writeLastSettleTime();
                    //System.out.println(JSON.toJSONString(carRepaireRequest));


                }
                    MessageBox.Show("上传成功");
                }
                else
                {
                    MessageBox.Show(accessTokenResponse.status, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            else
            {
                LogHelper.WriteLog(typeof(Form1), "access token is null, error happend!");
                MessageBox.Show("无法得到AccessToken, 请检查网络. 否则半个小时后重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Cursor = Cursors.Default;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.listView1.Items.Clear();
            //得到日期
            String datestr = this.dateTimePicker1.Text;
            SqlConnection dbCon = getConnection();

            int recordcount
                = 0;
            try
            {
                dbCon.Open();
                using (SqlCommand cmd = dbCon.CreateCommand())
                {
                    cmd.CommandText = "select a.GD_ID, a.GD_SN,b.Car_No, b.VIN_CODE, c.CUST_NM, d.ERROR_DESP from DT_OM_GD a, MT_CL b, MT_KH c, DT_OM_JJJC d where 1=1 " +
                        "and a.CL_ID=b.CL_ID "+
                        "and a.KH_ID=c.KH_ID "+
                        "and a.GD_ID=d.GD_ID "+
                        "and a.is_settle=1 and SUBSTRING(CONVERT(varchar(100), a.SETTLE_DT, 20), 1, 10)='" + datestr+"'";
                    
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {                      
                        while (reader.Read())
                        {
                            recordcount++;
                            String GD_ID = reader["GD_ID"].ToString();
                            String GD_SN = reader["GD_SN"].ToString();
                            String Car_No = reader["Car_No"].ToString();
                            String VIN_CODE = reader["VIN_CODE"].ToString();
                            String CUST_NM = reader["CUST_NM"].ToString();
                            String ERROR_DESP = reader["ERROR_DESP"].ToString();

                            ListViewItem lvi =  this.listView1.Items.Add(GD_ID);

                            String hint = (Car_No.Length<8) ? "车牌号长度不对" : "";
                            if (VIN_CODE.Trim().Length < 17)
                            {
                                hint = hint.Length == 0 ? "VIN码为空或者长度不对" : "\r\nVIN码为空或者长度不对";
                            }
                            if (ERROR_DESP.Trim().Length == 0)
                            {
                                ERROR_DESP = "小修";
                            }
                            lvi.SubItems.Add(GD_SN.Trim());
                            lvi.SubItems.Add(Car_No.Trim());
                            lvi.SubItems.Add(CUST_NM.Trim());
                            lvi.SubItems.Add(VIN_CODE.Trim());
                            lvi.SubItems.Add(ERROR_DESP.Trim());
                            if (hint.Length > 0)
                            {
                                lvi.ToolTipText = hint;
                                lvi.BackColor = Color.Red;
                            }
                            lvi.EnsureVisible();
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

            if (recordcount == 0)
            {
                MessageBox.Show("没有查到任何记录", "提示", MessageBoxButtons.OK);
            }
        }
        private Point pointView = new Point(0, 0);//鼠标位置 外部存储变量


        ToolTip toolTip = new ToolTip();
        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewItem lv = this.listView1.GetItemAt(e.X, e.Y);
            if (lv != null)
            {
                

                if (pointView.X != e.X || pointView.Y != e.Y)//比较当前位置和上一次鼠标的位置是否相同，防止tooltip因MouseMove事件不停刷新造成的闪烁问题，
                {
                    toolTip.SetToolTip(listView1, lv.ToolTipText);
                }
            }
            else
            {
                toolTip.Hide(listView1);//当鼠标位置无listviewitem时，自动隐藏tooltip
            }
            pointView = new Point(e.X, e.Y);//存储本次的鼠标位置，为下次得位置比较准备
        }


    }
}
