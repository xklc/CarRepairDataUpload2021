using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using DataContractJsonSerializer;
using Newtonsoft.Json;
using System.Diagnostics;
using System.ServiceProcess;

namespace CarRepairSetting
{
    public partial class Form1 : Form
    {
        public string DBHost;
        public string DBUser;
        public string DBPassword;
        public string DBName;
        public string CompanyCode;
        public string CompanyPassword;
        public List<string> vinCodeList = new List<string>();
        public List<string> errorDespList = new List<string>();

        private string accessTokenUrl = "https://api.qcda.shanghaiqixiu.org/restservices/lcipprodatarest/lcipprogetaccesstoken/query";
       private string carRepairItemUrl = "https://api.qcda.shanghaiqixiu.org/restservices/lcipprodatarest/lcipprocarfixrecordadd/query";
       // private string carRepairItemUrl = "http://192.168.2.202/DataUploadService/CarItemUploadService";

        public void loadConfig()
        {
            // string file = System.Windows.Forms.Application.ExecutablePath;
            // System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(file);           
            DBHost = ConfigurationManager.AppSettings["DBHost"];
            DBUser = ConfigurationManager.AppSettings["DBUser"];
            DBPassword = ConfigurationManager.AppSettings["DBPassword"];
            DBName = ConfigurationManager.AppSettings["DBName"];
            CompanyCode = ConfigurationManager.AppSettings["CompanyCode"];
            CompanyPassword = ConfigurationManager.AppSettings["CompanyPassword"];
            string error_random = ConfigurationManager.AppSettings["error_random"];
            string vin_random = ConfigurationManager.AppSettings["vin_random"];

            vinCodeList.Clear();
            errorDespList.Clear();

            foreach (var error in error_random.Split(','))
            {
                if (error.Trim().Length == 0) continue;
                errorDespList.Add(error.Trim());
            }
            foreach (var vin in vin_random.Split(','))
            {
                if (vin.Trim().Length == 0) continue;
                vinCodeList.Add(vin.Trim());
            }



        }
        public Form1()
        {
            InitializeComponent();
            loadConfig();
        }

        private static string formatCostListCode(string srcCode)
        {
            string day = srcCode.Substring(1, 9);
            string number = srcCode.Substring(9);
            string resultCode = "QXT_" + day + "_0" + number;
            return resultCode;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.textBox1.Text = DBHost;
            this.textBox2.Text = DBUser;
            this.textBox3.Text = DBPassword;
            this.textBox4.Text = DBName;
            this.textBox5.Text = CompanyCode;
            this.textBox6.Text = CompanyPassword;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["DBHost"].Value = this.textBox1.Text.Trim();
            config.AppSettings.Settings["DBUser"].Value = this.textBox2.Text.Trim();
            config.AppSettings.Settings["DBPassword"].Value = this.textBox3.Text.Trim();
            config.AppSettings.Settings["DBName"].Value = this.textBox4.Text.Trim();

            config.AppSettings.SectionInformation.ForceSave = true;
            config.Save();
            ConfigurationManager.RefreshSection("appSettings");
            this.DBHost = this.textBox1.Text.Trim();
            this.DBName = this.textBox4.Text.Trim();
            this.DBUser = this.textBox2.Text.Trim();
            this.DBPassword = this.textBox3.Text.Trim();

            MessageBox.Show("设置成功");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (ConnectionTest())
            {
                MessageBox.Show("数据库连接测试成功", "提示", 0);
            }
        }

        private bool ConnectionTest()
        {

            string connString = String.Format("server={0};database={1};uid={2};pwd={3}", this.DBHost,
                                this.DBName, this.DBUser, this.DBPassword);
            SqlConnection dbCon = new SqlConnection(connString);

            try
            {
                //  dbCon.Open();
                SqlExtensions.QuickOpen(dbCon, 3000);
            }
            catch (Exception es)
            {
                MessageBox.Show("clsSqlite clsDbSqlite", "err:" + es.Message, 0);
                return false;
            }
            return true;
        }

        public ArrayList getFileList(string dir)
        {
            ArrayList sqlFileList = new ArrayList();

            DirectoryInfo d = new DirectoryInfo(dir);

            foreach (FileInfo nextFile in d.GetFiles())
            {
                if (nextFile.Name.EndsWith(".sql"))
                {
                    sqlFileList.Add(nextFile.FullName);
                }
            }
            return sqlFileList;
        }

        private SqlConnection getConnection()
        {
            string connString = String.Format("server={0};database={1};uid={2};pwd={3}", this.DBHost,
        this.DBName, this.DBUser, this.DBPassword);
            return new SqlConnection(connString);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SqlConnection dbCon = getConnection();
            dbCon.Open();
            SqlCommand sqlcmd = new SqlCommand();
            sqlcmd.Connection = dbCon;
            ArrayList fileList = getFileList(System.AppDomain.CurrentDomain.BaseDirectory + @"\sql");
            try
            {
                foreach (var file in fileList)
                {
                    string sql = File.ReadAllText(file.ToString(), Encoding.UTF8);
                    sqlcmd.CommandText = sql;
                    sqlcmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("sql语句执行失败:" + ex.Message);
            }
            finally
            {
                dbCon.Close();
            }

            MessageBox.Show("初始化脚本执行成功", "信息", MessageBoxButtons.OK);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["CompanyCode"].Value = this.CompanyCode = this.textBox5.Text.Trim();
            config.AppSettings.Settings["CompanyPassword"].Value = this.CompanyPassword = this.textBox6.Text.Trim();
            config.AppSettings.SectionInformation.ForceSave = true;
            config.Save();
            ConfigurationManager.RefreshSection("appSettings");
            MessageBox.Show("设置成功");

        }

        private AccessTokenResponse getToken()
        {
            AccessTokenRequest acessTokenRequest = new AccessTokenRequest();
            acessTokenRequest.companycode = this.CompanyCode = this.textBox5.Text;
            acessTokenRequest.companypassword = this.CompanyPassword = this.textBox6.Text;
            //Json.NET序列化
            string jsonData = JsonConvert.SerializeObject(acessTokenRequest);

            try
            {
                RestApiClient restApiClient = new RestApiClient(this.accessTokenUrl, HttpVerbNew.POST, ContentType.JSON, jsonData);
                String response = restApiClient.MakeRequest();
                AccessTokenResponse accessTokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(response);
                return accessTokenResponse;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw ex;
            }
        }

        private void dbExecNoReturn(string sql)
        {
            SqlConnection dbCon = getConnection();
            List<CarRepaireRequest> requestList = new List<CarRepaireRequest>();
            Dictionary<string, CarRepaireRequest> gdId2CarRepaireRequest = new Dictionary<string, CarRepaireRequest>();

            try
            {
                dbCon.Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = dbCon;
                sqlcmd.CommandText = sql;
                sqlcmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                dbCon.Close();
            }
        }

        private void updateRequestResponseInfo(CarRepaireRequest carRepaireRequest, CarRepairResponse carRepaireResponse, string request_str, string response_str)
        {
            string result = carRepaireResponse.code == "1" ? "新增成功" : "新增失败";
            string sql = string.Format("update dataupload_gd set is_uploaded=1, request_str='{0}', response_str='{1}' where gd_id=",
                                request_str, response_str, carRepaireRequest.basicInfo.gd_id);
            dbExecNoReturn(sql);
        }

        private CarRepairResponse getCarRepaireResult(CarRepaireRequest carRepaireRequest)
        {
            //Json.NET序列化
            string jsonData = JsonConvert.SerializeObject(carRepaireRequest);
            this.textBox7.Text = jsonData;
            // string jsonData = JsonConvert.SerializeObject(carRepaireRequest, Formatting.Indented, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii});
            try
            {
                RestApiClient restApiClient = new RestApiClient(this.carRepairItemUrl, HttpVerbNew.POST, ContentType.JSON, jsonData);
                String response = restApiClient.MakeRequest();
                CarRepairResponse carRepaireResponse = JsonConvert.DeserializeObject<CarRepairResponse>(response);
                return carRepaireResponse;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw ex;
            }

        }
        private void button3_Click(object sender, EventArgs e)
        {
            AccessTokenResponse accessTokenResponse = getToken();
            if (accessTokenResponse != null)
            {
                MessageBox.Show(accessTokenResponse.status, "信息!", MessageBoxButtons.OK);
            }

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
        public string getCompanyName()
        {
            string filename = "companyname";
            string companyname = null;
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
                                companyname = reader["value"].ToString();
                            }
                            reader.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    dbCon.Close();
                }
            //    File.WriteAllText(filename, companyname);
          //  }

            return companyname;

        }
        private void button5_Click(object sender, EventArgs e)
        {
            this.listView1.Items.Clear();
            AccessTokenResponse accessTokenResponse = getToken();
            string companyname = this.getCompanyName();
            if (accessTokenResponse == null)
            {
                return;
            }

            //1. 更新数据库里面最后一条结算记录时间戳
              string sql = "update dt_om_gd set settle_dt=dateadd(s, -10, settle_dt) where gd_id =(select top 1 gd_id from dt_om_gd where is_settle=1 order by settle_dt desc)";
            // string sql = "update dt_om_gd set settle_dt=dateadd(s, -10, settle_dt) where gd_id =(46936)";
            SqlConnection dbCon = getConnection();
            List<CarRepaireRequest> requestList = new List<CarRepaireRequest>();
            Dictionary<string, CarRepaireRequest> gdId2CarRepaireRequest = new Dictionary<string, CarRepaireRequest>();

            try
            {
                dbCon.Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = dbCon;
                sqlcmd.CommandText = sql;
                sqlcmd.ExecuteNonQuery();

                sql = "select a.gd_id, a.GD_SN, b.VIN_CODE, b.Car_no, A.IN_DT, C.MILES,  A.SETTLE_DT,C.ERROR_DESP,A.GD_SN from DT_OM_GD a, MT_CL b, DT_OM_JJJC C where A.CL_ID=B.CL_ID and A.GD_ID=C.GD_ID and a.is_settle =1 and a.gd_id in (select top 100 gd_id from dataupload_gd where is_uploaded=0 order by create_time desc) order by a.settle_dt asc";
                sqlcmd.CommandText = sql;


                SqlDataReader sqlDataReader = sqlcmd.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    CarRepaireRequest carRepaireRequest = new CarRepaireRequest();
                    carRepaireRequest.access_token = accessTokenResponse.access_token;
                    BasicInfo basicInfo = new BasicInfo();
                    basicInfo.vehicleplatenumber = getTrimString(sqlDataReader, "Car_no", "N/A");
                    basicInfo.gd_id = getTrimString(sqlDataReader, "gd_id", "N/A");
                    basicInfo.companyname = companyname;
                    basicInfo.vin = getTrimString(sqlDataReader, "VIN_CODE", vinCodeList[new Random().Next(vinCodeList.Count)]);
                    if (basicInfo.vin.Length != 17)
                    {
                        basicInfo.vin = vinCodeList[new Random().Next(vinCodeList.Count)];
                    }
                    basicInfo.repairdate = getDateStr(sqlDataReader, "IN_DT", "N/A");
                    basicInfo.settledate = getDateStr(sqlDataReader, "SETTLE_DT", "N/A");
                    string miles = new Random().Next(100000).ToString();
                    basicInfo.repairmileage = getTrimString(sqlDataReader, "MILES", miles);

                    basicInfo.faultdescription = getTrimString(sqlDataReader, "ERROR_DESP", "常规维修");
                    //basicInfo.costlistcode = formatCostListCode(getTrimString(sqlDataReader, "GD_SN", ""));
                    basicInfo.costlistcode = getTrimString(sqlDataReader, "GD_SN", "");
                    carRepaireRequest.basicInfo = basicInfo;
                    gdId2CarRepaireRequest[basicInfo.gd_id] = carRepaireRequest;
                    requestList.Add(carRepaireRequest);
                }
                sqlDataReader.Close();

                if (gdId2CarRepaireRequest.Keys.Count == 0)
                {
                    MessageBox.Show("未查到任何有效的结算记录");
                    return;
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


                if (requestList.Count > 0)
                {
                    CarRepairResponse carRepaireResponse = getCarRepaireResult(requestList[0]);

                    string req_json_data = JsonConvert.SerializeObject(requestList[0]);
                    string resp_json_data = JsonConvert.SerializeObject(carRepaireResponse);
                    this.textBox8.Text = resp_json_data;

                    //
                    if (carRepaireResponse != null)
                    {
                        //TODO: 更新表格
                        sql = string.Format("update dataupload_gd set is_uploaded=1, request_str=\'{0}\', response_str=\'{1}\' where gd_id={2}",
                             req_json_data, resp_json_data, requestList[0].basicInfo.gd_id);
                        sqlcmd.CommandText = sql;
                        sqlcmd.ExecuteNonQuery();
                    }

                    //this.listView1.add
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = requestList[0].basicInfo.vehicleplatenumber;
                    lvi.SubItems.Add(requestList[0].basicInfo.costlistcode);
                    lvi.SubItems.Add(req_json_data);
                    lvi.SubItems.Add(resp_json_data);
                    lvi.SubItems.Add(carRepaireResponse.code == "1"?"上传成功":"上传错误");
                    lvi.SubItems.Add(carRepaireResponse.status);
                    listView1.Items.Add(lvi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("更新失败,错误:" + ex.Message, "错误", MessageBoxButtons.OK);
                return;
            }
            finally
            {
                dbCon.Close();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            MessageBox.Show("安装服务");            
            string CurrentDirectory = System.Environment.CurrentDirectory;
            string source_file = CurrentDirectory + "\\CarRepairSetting.exe.config";
            string desting_file = CurrentDirectory + @"\Service\CarRepairDataUpload.exe.config";
            bool isrewrite = true; // true=覆盖已存在的同名文件,false则反之
            System.IO.File.Copy(source_file, desting_file, isrewrite);
            source_file = CurrentDirectory + "\\CarRepairSetting.vshost.exe.config";
            desting_file = CurrentDirectory + @"\Service\CarRepairDataUpload.vshost.exe.config";
            System.IO.File.Copy(source_file, desting_file, isrewrite);
            System.Environment.CurrentDirectory = CurrentDirectory + "\\Service";
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = "Install.bat";
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            System.Environment.CurrentDirectory = CurrentDirectory;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            MessageBox.Show("卸载服务");
            string CurrentDirectory = System.Environment.CurrentDirectory;
            System.Environment.CurrentDirectory = CurrentDirectory + "\\Service";
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = "Uninstall.bat";
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            System.Environment.CurrentDirectory = CurrentDirectory;            
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ServiceController serviceController = new ServiceController("CarRepairDataUpload");
            serviceController.Start();
            MessageBox.Show("启动服务");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            ServiceController serviceController = new ServiceController("CarRepairDataUpload");
            if (serviceController.CanStop) {
                MessageBox.Show("可以停止");
                serviceController.Stop();
            }
            else
            {
                MessageBox.Show("不能停止停止");
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            ServiceController serviceController = new ServiceController("CarRepairDataUpload");
            string Status = serviceController.Status.ToString();
            MessageBox.Show("状态:" + Status);
        }

        //private void button14_Click(object sender, EventArgs e)
        //{
        //    AccessTokenRequest accessTokenRequest = new AccessTokenRequest();
        //    accessTokenRequest.companycode = "12345";
        //    accessTokenRequest.companypassword = "23456";
        //    string json_data = JsonConvert.SerializeObject(accessTokenRequest);
        //    MessageBox.Show(json_data);
        //}
    }
}
