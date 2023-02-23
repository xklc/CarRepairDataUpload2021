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
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
<<<<<<< HEAD
using com.rongtong.car;
=======
using System.Net.Mail;
using System.Net.Mime;
>>>>>>> 0244969a396bca3271f7ddecd4b138135cbb43f9

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
<<<<<<< HEAD
            this.textBox3.Visible = configItem.debug.Equals("true");
            carOidMapping["0"] = "A";
            carOidMapping["1"] = "A";
            carOidMapping["2"] = "B";
            carOidMapping["4"] = "F";
            carOidMapping["5"] = "E";
            carOidMapping["20"] = "C"; //电

            setting.NullValueHandling = NullValueHandling.Ignore;
=======
            this.textBox3.Visible = configItem.debug.Equals("true", StringComparison.CurrentCultureIgnoreCase)
                        || configItem.debug.Equals("y", StringComparison.CurrentCultureIgnoreCase)
                        || configItem.debug.Equals("t", StringComparison.CurrentCultureIgnoreCase);
            try
            {
                getCompanyInfo();
             //   if (!this.abd)
              //  {
                    //sendMail();
                    //uploadStaticData();
                    //string sql = "insert into StringParameter(name,value) values('abd', '1')";
                    //dbExecNoReturn(sql);
             //   }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(Form1), ex);
            }
            finally
            {
            }
>>>>>>> 0244969a396bca3271f7ddecd4b138135cbb43f9
        }
        public ConfigItem configItem;
        private string connStr;
        private SqlConnection sqlConn;
        private string companyIdentity;
        private Dictionary<String, String> carOidMapping = new Dictionary<string, string>();

        public Dictionary<String, GDCardInfo> gdCardInfo = new Dictionary<String, GDCardInfo>();
<<<<<<< HEAD
        JsonSerializerSettings setting = new JsonSerializerSettings();
=======
>>>>>>> 0244969a396bca3271f7ddecd4b138135cbb43f9

        
        private SqlConnection getConnection()
        {
            if (sqlConn == null || sqlConn.State == System.Data.ConnectionState.Closed)
            {
                sqlConn = new SqlConnection(connStr);
            }
            return sqlConn;
        }

        private void intialTableSchema()
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
                string updateSql = "IF COL_LENGTH('DT_OM_GD', 'UPLOAD_STATUS') IS NULL ALTER TABLE DT_OM_GD ADD UPLOAD_STATUS INT DEFAULT 0 WITH VALUES";
                sqlcmd.CommandText = updateSql;
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
        public void getAppId()
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
                    cmd.CommandText = "select name, value from StringParameter where name in ('appid', 'secret')";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                           String name = reader["name"].ToString().Trim();
                           String value = reader["value"].ToString().Trim();

                            if (name.Equals("appid"))
                            {
                                GlobalData.appid = value;
                                this.companyIdentity = value;
                            }else if (name.Equals("secret"))
                            {
                                GlobalData.secret = value;
                            }
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

     
        //得到维修项目明细
        //public Dictionary<String, RepairItem> getRepairItemDetail(Dictionary<String, CarDisplayInfo> cardDisplayInfo)
        //{
        //    Dictionary<String, RepairItem> repairItems = new Dictionary<String, RepairItem>();

        //    List<String> gd_ids = new List<String>();
        //    foreach (var gd_id in cardDisplayInfo.Keys)
        //    {
        //        gd_ids.Add(gd_id);
        //    }

        //    string gd_ids_str = string.Join(",", gd_ids.ToArray());

        //    SqlConnection dbCon = getConnection();
        //    try
        //    {
        //        dbCon.Open();
        //        SqlCommand sqlcmd = new SqlCommand();
        //        string sql = string.Format("select gd_id,prj_nm as itemName,MAN_HOUR as settlementTime from DT_OM_BXXM where gd_id in ({0})", gd_ids_str);
        //        sqlcmd.CommandText = sql;
        //        sqlcmd.Connection = dbCon;


        //        SqlDataReader sqlDataReader = sqlcmd.ExecuteReader();
        //        Dictionary<String, Double> subtotals = new Dictionary<string, double>();
        //        while (sqlDataReader.Read())
        //        {
        //            string gd_id = getTrimString(sqlDataReader, "gd_id", "");

        //            RepairItemDetail repairItemDetail = new RepairItemDetail();
        //            repairItemDetail.itemName = getTrimString(sqlDataReader, "itemName", "");
        //            repairItemDetail.settlementTime = getTrimString(sqlDataReader, "settlementTime", "0");

        //            RepairItem repairItem = null;
        //            repairItems.TryGetValue(gd_id, out repairItem);
        //            if (repairItem == null)
        //            {
        //                repairItem = new RepairItem();
        //                repairItems.Add(gd_id, repairItem);
        //            }
        //            Double subtotal;
        //            subtotals.TryGetValue(gd_id, out subtotal);
        //            subtotal += Convert.ToDouble(repairItemDetail.settlementTime);
        //            repairItem.subtotal = subtotal.ToString();

        //            int cnt = repairItem.items.Count+ 1;
        //            repairItemDetail.itemSeq = cnt.ToString();
        //            repairItem.items.Add(repairItemDetail);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        dbCon.Close();
        //    }

        //    return repairItems;
        //}


        //计算维修配件列表
        //public Dictionary<String, RepairPart> getRepairPartsDetail(Dictionary<String, CarDisplayInfo> cardDisplayInfo)
        //{
        //    Dictionary<String, RepairPart> repairParts = new Dictionary<String, RepairPart>();

        //    List<String> gd_ids = new List<String>();
        //    foreach (var gd_id in cardDisplayInfo.Keys)
        //    {
        //        gd_ids.Add(gd_id);
        //    }

        //    string gd_ids_str = string.Join(",", gd_ids.ToArray());

        //    SqlConnection dbCon = getConnection();
        //    try
        //    {
        //        dbCon.Open();
        //        SqlCommand sqlcmd = new SqlCommand();
        //        string sql = string.Format("select d.gd_id,  C.PART_NM as partName, c.location as partBrand, C.ORIGINAL_FACTORY_ID as partNo, A.QTY as partUnitNumber from DT_EM_CKLJ A JOIN DT_EM_CKD  B ON A.OUTPUT_ID =B.OUTPUT_ID join DT_EM_LJML C on A.PART_ID = C.PART_ID join DT_OM_GD D ON d.GD_ID=B.RELATIVE_ID  where  D.gd_id in ({0})", gd_ids_str);
        //        sqlcmd.CommandText = sql;
        //        sqlcmd.Connection = dbCon;


        //        SqlDataReader sqlDataReader = sqlcmd.ExecuteReader();
        //        Dictionary<String, Double> subtotals = new Dictionary<string, double>();
        //        while (sqlDataReader.Read())
        //        {
        //            string gd_id = getTrimString(sqlDataReader, "gd_id", "");

        //            RepairPartDetail repairItemDetail = new RepairPartDetail();
        //            repairItemDetail.partName = getTrimString(sqlDataReader, "partName", "");
        //            repairItemDetail.partNo = getTrimString(sqlDataReader, "partNo", "");
        //            repairItemDetail.partBrand = getTrimString(sqlDataReader, "partBrand", "");
        //            repairItemDetail.partUnitNumber = getTrimString(sqlDataReader, "partUnitNumber", "");

        //            RepairPart repairPart = null;
        //            repairParts.TryGetValue(gd_id, out repairPart);
        //            if (repairPart == null)
        //            {
        //                repairPart = new RepairPart();
        //                repairParts.Add(gd_id, repairPart);
        //            }
        //            Double subtotal;
        //            subtotals.TryGetValue(gd_id, out subtotal);
        //            subtotal += Convert.ToDouble(repairItemDetail.partUnitNumber);
        //            repairPart.subtotal = subtotal.ToString();

        //            int cnt = repairPart.parts.Count + 1;
        //            repairItemDetail.partSeq = cnt.ToString();
        //            repairPart.parts.Add(repairItemDetail);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        dbCon.Close();
        //    }

        //    return repairParts;
        //}

        public String getOilType(String oirTypeInArmis)
        {
            String oilTypeInLanTu;
            carOidMapping.TryGetValue(oirTypeInArmis, out oilTypeInLanTu);

            if (oilTypeInLanTu == null)
            {
                oilTypeInLanTu = "Z"; //其他
            }

            return oilTypeInLanTu;
        }

        Dictionary<String, PickCarInfo> pickCarInfo;
        Dictionary<String, RepairItemInfo> repairItemInfos;
        Dictionary<String, SettleInfo> settleInfo;
        Dictionary<String, CarDisplayInfo> carDisplayInfo;

        public void getCarInfo(String date_str, out Dictionary<String, PickCarInfo> pickCarInfo,
            out Dictionary<String, RepairItemInfo> repairItemInfos,
            out Dictionary<String, SettleInfo> settleInfo,
            out Dictionary<String, CarDisplayInfo> carDisplayInfo
            )
        {
            pickCarInfo = new Dictionary<string, PickCarInfo>();
            repairItemInfos = new Dictionary<string, RepairItemInfo>();
            settleInfo = new Dictionary<string, SettleInfo>();
            carDisplayInfo = new Dictionary<string, CarDisplayInfo>();

            String gd_sn = textBox1.Text.Trim();
            String car_no = textBox2.Text.Trim();

            SqlConnection dbCon = getConnection();
            try
            {
                dbCon.Open();
                SqlCommand sqlcmd = new SqlCommand();
                string sql = string.Format("select t1.gd_sn,t1.upload_status,t1.settle_dt, t1.gd_id, t2.vin_code, t2.car_no, t1.in_dt, t3.miles, t4.gd_nm, t2.car_oil, t3.error_desp, t5.cust_nm from dt_om_gd t1 left join mt_cl t2 on t1.cl_id = t2.cl_id left join DT_OM_JJJC t3 on t1.gd_id = t3.gd_id left join mt_gdfl t4 on t1.gd_type_id = t4.id left join MT_KH t5 on t1.kh_id=t5.kh_id where t1.settle_dt >= '{0} 00:00:00' and t1.settle_dt<='{0} 23:59:59'", date_str);
                //                string sql = string.Format("select a.GD_ID, a.GD_SN, a.GD_SN as settlementSeq,convert(char(10), a.SETTLE_DT,120) as settlementDate, convert(char(10), a.IN_DT,120) as deliveryDate, b.Car_No as licensePlate,  b.VIN_CODE as vin, c.CUST_NM as vehicleOwner, c.CUST_NM as entrustRepair,  isnull(b.ENGINE_NO, '') engineNum, isnull(c.LINKMAN,'') contact, isnull(c.TEL1,'') contactDetails, isnull(b.gearbox_type, 1) obd,     case when (b.CAR_OIL=0 ) then   '轻型汽油车'  when (b.CAR_OIL=1 ) then  '重型汽油车'  when (b.CAR_OIL=2 ) then  '柴油车'  when (b.CAR_OIL=3 ) then  '其它车'  when (b.CAR_OIL=4 ) then  'LPG燃料车'  when (b.CAR_OIL=5 ) then  'CNG燃料车'  else  '其它车'  end as carType, g.CAR_TYPE as vehicleType,  b.CAR_SYMBOL as vehicleClassCode,  isnull(b.CAR_COLOR,'') color,  d.ERROR_DESP,   d.CAR_INFO as incomingInspectionId,  e.VENDOR as brand,  isnull(d.MILES,'0') repairMileage,  isnull(f.GD_PIC,'') GD_PIC   from DT_OM_GD a    join MT_CL b on a.cl_id=b.cl_id   join MT_KH c on a.KH_ID=c.KH_ID    join DT_OM_JJJC d on a.gd_id=d.gd_id   join MT_CC e on b.cc_id=e.cc_id  join MT_CX g on b.cc_id=g.cc_id  left join DT_OM_GDTP f on a.gd_id=f.gd_id where 1=1    and a.is_settle=1 and SUBSTRING(CONVERT(varchar(100), a.SETTLE_DT, 20), 1, 10)='2020-05-22'");
                if (gd_sn.Length > 0)
                {
                    sql += string.Format(" and t1.gd_sn like '%{0}%'", gd_sn);
                }

                if (car_no.Length > 0)
                {
                    sql += string.Format(" and t2.Car_No like '%{0}%'", car_no);
                }
                sqlcmd.CommandText = sql;
                sqlcmd.Connection = dbCon;

                SqlDataReader sqlDataReader = sqlcmd.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    gd_sn = getTrimString(sqlDataReader, "gd_sn", "");

                    CarDisplayInfo tmpCarDisplayInfo;
                    PickCarInfo tmpPickCarInfo;
                    SettleInfo tmpSettleInfo;

                    carDisplayInfo.TryGetValue(gd_sn, out tmpCarDisplayInfo);                    
                    pickCarInfo.TryGetValue(gd_sn, out tmpPickCarInfo);
                    settleInfo.TryGetValue(gd_sn, out tmpSettleInfo);
                    

                    if (tmpCarDisplayInfo == null)
                    {
                        tmpCarDisplayInfo = new CarDisplayInfo();
                        carDisplayInfo[gd_sn] = tmpCarDisplayInfo;
                    }

                    if (tmpPickCarInfo == null)
                    {
                        tmpPickCarInfo = new PickCarInfo();
                        pickCarInfo[gd_sn] = tmpPickCarInfo;
                    }


                    if (tmpSettleInfo == null)
                    {
                        tmpSettleInfo = new SettleInfo();                        
                        settleInfo[gd_sn] = tmpSettleInfo;
                    }

                    //展示信息
                    int upload_status = 0;
                    int.TryParse(getTrimString(sqlDataReader, "upload_status", "0"), out upload_status);
                    tmpCarDisplayInfo.upload_status = upload_status;
                    tmpCarDisplayInfo.car_no = getTrimString(sqlDataReader, "car_no", "");
                    tmpCarDisplayInfo.gd_sn = getTrimString(sqlDataReader, "gd_sn", "");
                    tmpCarDisplayInfo.customer_name = getTrimString(sqlDataReader, "cust_nm", "");
                    tmpCarDisplayInfo.vin_code = getTrimString(sqlDataReader, "vin_code", "");
                    tmpCarDisplayInfo.error_desc = getTrimString(sqlDataReader, "error_desp", "");


                    //接车信息
                    tmpPickCarInfo.vin = getTrimString(sqlDataReader, "vin_code", "");
                    tmpPickCarInfo.vpn = getTrimString(sqlDataReader, "car_no", "");
                    tmpPickCarInfo.orderCode = getTrimString(sqlDataReader, "gd_sn", "");
                    tmpPickCarInfo.repairTime = getTrimString(sqlDataReader, "in_dt", "").Replace("/","-");
                    tmpPickCarInfo.repairMileage = getTrimString(sqlDataReader, "miles", "");
                    tmpPickCarInfo.serviceType = getTrimString(sqlDataReader, "gd_nm", "");
                    tmpPickCarInfo.fuelType = getOilType(getTrimString(sqlDataReader, "car_oil", ""));
                    tmpPickCarInfo.faultDesc = getTrimString(sqlDataReader, "error_desp", ""); 
                    //结算信息
                    tmpSettleInfo.payTime = getTrimString(sqlDataReader, "settle_dt", "").Replace("/", "-");
                    tmpSettleInfo.orderCode = getTrimString(sqlDataReader, "gd_sn", "");
                }

                sqlDataReader.Close();

                gd_sn = "";
                sql = String.Format("select A.GD_ID, E.GD_SN, A.PRJ_NM as projectName,  A.man_hour as workingHours, c.PART_NM as partName, D.ORIGINAL_FACTORY_ID as partCode,  C.QTY as partQty, D.unit as unit from DT_OM_BXXM A join DT_EM_CKD  B ON A.GD_ID =B.RELATIVE_ID left JOIN DT_EM_CKLJ C ON B.OUTPUT_ID=C.OUTPUT_ID AND A.baoxiu_id=c.output_part_id LEFT JOIN DT_EM_LJML D ON C.PART_ID=D.PART_ID join dt_om_gd e on a.gd_id=e.gd_id JOIN MT_CL f on e.cl_id=f.cl_id where E.settle_dt>='{0} 00:00:00' and E.settle_dt<='{0} 23:59:59'  ", date_str);
                if (gd_sn.Length > 0)
                {
                    sql += string.Format(" and A.gd_sn like '%{0}%'", gd_sn);
                }

                if (car_no.Length > 0)
                {
                    sql += string.Format(" and F.Car_No like '%{0}%'", car_no);
                }
                sqlcmd.CommandText = sql;
                sqlDataReader = sqlcmd.ExecuteReader();
                var repairProjects = new Dictionary<String, Dictionary<RepairProject, List<RepairItem>>>();

                while (sqlDataReader.Read())
                {
                    gd_sn = getTrimString(sqlDataReader, "gd_sn", "");

                    Dictionary<RepairProject, List<RepairItem>> projects;
                    repairProjects.TryGetValue(gd_sn, out projects);
                    if (projects == null)
                    {
                        projects = new Dictionary<RepairProject, List<RepairItem>>();
                        repairProjects[gd_sn] = projects;
                    }

                    RepairProject repairProject = new RepairProject();

                    repairProject.projectName = getTrimString(sqlDataReader, "projectName", "");
                    repairProject.workingHours = getTrimString(sqlDataReader, "workingHours", "0");

                    List<RepairItem> repairItems;
                    projects.TryGetValue(repairProject, out repairItems);
                    if (repairItems == null)
                    {
                        repairItems = new List<RepairItem>();
                        projects[repairProject] = repairItems;
                    }


                    if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("partName")))
                    {
                        var repairItem = new RepairItem();
                        repairItem.partName = getTrimString(sqlDataReader, "partName", "");
                        repairItem.partCode = getTrimString(sqlDataReader, "partCode", "");
                        repairItem.partQty = getTrimString(sqlDataReader, "partQty", "");
                        repairItem.unit = getTrimString(sqlDataReader, "unit", "");
                        repairItems.Add(repairItem);
                    }
                }

                foreach (var item in repairProjects)
                {
                    RepairItemInfo repairItemInfo = new RepairItemInfo();
                    repairItemInfo.orderCode = item.Key;
                    
                    foreach (var item1 in item.Value)
                    {
                        repairItemInfo.repairProjectList.Add(item1.Key);
                        if (item1.Value!=null && item1.Value.Count>0 ) { 
                            repairItemInfo.repairProjectList[repairItemInfo.repairProjectList.Count - 1].repairPartList = item1.Value;
                        }
                    }
                    repairItemInfos[item.Key] = repairItemInfo;
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
        }
        


       

        private bool updateUploadStatus( string gd_id)
        {
            //TODO: 更新表格
            //            string sql = string.Format("update dataupload_gd set is_uploaded=1, request_str=\'{0}\', response_str=\'{1}\' where gd_id={2}",
            //                 req_json_data, resp_json_data, gd_id);
           // req_json_data = req_json_data.Replace("\\", "");
           // resp_json_data = resp_json_data.Replace("\\", "");
            string sql = string.Format("update dt_om_gd set upload_status=1 where gd_id={0}", gd_id);

            this.dbExecNoReturn(sql);
            return true;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            int count = 0, selectIndex = -1;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["Column1"];
                Boolean flag = Convert.ToBoolean(checkCell.Value);
                if (flag)
                {
                    selectIndex = i;
                    count = count + 1;
                }
            }

            if (count > 1)
            {
                MessageBox.Show("为避免出错, 一次只能上传一条");
                return;
            }

            if (count < 1)
            {
                MessageBox.Show("请选中要上传的记录");
                return;
            }



            LogHelper.WriteLog(typeof(Form1), "begin request access token from server");

            string gd_sn = dataGridView1.Rows[selectIndex].Cells[1].Value.ToString();

            PickCarInfo tmpPickCarInfo;
            pickCarInfo.TryGetValue(gd_sn, out tmpPickCarInfo);

            if (tmpPickCarInfo != null)
            {
                string json = JsonConvert.SerializeObject(tmpPickCarInfo, Formatting.None, setting);
                string sign = SignUtil.sign("pick", json, GlobalData.secret);
                tmpPickCarInfo.sign = sign;
                json = JsonConvert.SerializeObject(tmpPickCarInfo);

                String url = configItem.serverHost + URL.PICKUP_CAR_URL;
                //  MessageBox.Show(json);
                string debuginfo = this.textBox3.Text + "接车地址:" + url + "\r\n";
                Console.WriteLine(json);
                debuginfo += "请求数据:" + json + "\r\n";
                var restApiClient = new RestApiClient(url, HttpVerbNew.POST, ContentType.JSON, json);
                string response = restApiClient.MakeRequest();
                debuginfo += "响应:" + response + "\r\n\r\n";
                this.textBox3.Text = debuginfo;
                
            }

            RepairItemInfo tmpRepairItemInfo;
            repairItemInfos.TryGetValue(gd_sn, out tmpRepairItemInfo);
            if (tmpRepairItemInfo != null)
            {
                string json = JsonConvert.SerializeObject(tmpRepairItemInfo, Formatting.None, setting);

                string sign = SignUtil.sign("settleinfo", json, GlobalData.secret);
                tmpRepairItemInfo.sign = sign;
                json = JsonConvert.SerializeObject(tmpRepairItemInfo, Formatting.None, setting);
                String url = configItem.serverHost + URL.REPAIR_INFO_URL;
                //  MessageBox.Show(json);
                string debuginfo = this.textBox3.Text + "配件地址:" + url + "\r\n";
                Console.WriteLine(json);
                debuginfo += "请求数据:" + json + "\r\n";
                //    json = "{\"appId\":\"c737a6a7-8efa-46e6-9439-659dff1f3ef3\",\"sign\":\"04d4b81b916340b002a21ae961ed6886\",\"nonce\":\"753359534\",\"timestamp\":\"1627794303248\",\"orderCode\":\"R20210124001\",\"repairProjectList\":[{\"projectName\":\"二级维护*1\",\"projectType\":\"\",\"workingHours\":\"120.00\",\"repairPartList\":[{\"partName\":\"机油\",\"brandName\":\"\",\"partType\":\"\",\"partCode\":\"QPJY-AM-01C\",\"partQty\":\"1.00\",\"unit\":\"\"}]},{\"projectName\":\"更换起动机整流子*1\",\"projectType\":\"\",\"workingHours\":\"10.00\", \"repairPartList\":[]}]}";
                var restApiClient = new RestApiClient(url, HttpVerbNew.POST, ContentType.JSON, json);
                string response = restApiClient.MakeRequest();
                debuginfo += "响应:" + response + "\r\n\r\n";
                this.textBox3.Text = this.textBox3.Text + debuginfo;
            }

            SettleInfo tmpSettleInfo;
            settleInfo.TryGetValue(gd_sn, out tmpSettleInfo);
            if (tmpSettleInfo != null)
            {
                string json =  JsonConvert.SerializeObject(tmpSettleInfo, Formatting.None, setting);

                string sign = SignUtil.sign("settle", json, GlobalData.secret);
                tmpSettleInfo.sign = sign;
                json = JsonConvert.SerializeObject(tmpSettleInfo, Formatting.None, setting);
                String url = configItem.serverHost + URL.SETTLE_INFO_URL;
                //  MessageBox.Show(json);
                string debuginfo = this.textBox3.Text + "结算地址:" + url + "\r\n";
                Console.WriteLine(json);
                debuginfo += "请求数据:" + json + "\r\n";
                //    json = "{\"appId\":\"c737a6a7-8efa-46e6-9439-659dff1f3ef3\",\"sign\":\"04d4b81b916340b002a21ae961ed6886\",\"nonce\":\"753359534\",\"timestamp\":\"1627794303248\",\"orderCode\":\"R20210124001\",\"repairProjectList\":[{\"projectName\":\"二级维护*1\",\"projectType\":\"\",\"workingHours\":\"120.00\",\"repairPartList\":[{\"partName\":\"机油\",\"brandName\":\"\",\"partType\":\"\",\"partCode\":\"QPJY-AM-01C\",\"partQty\":\"1.00\",\"unit\":\"\"}]},{\"projectName\":\"更换起动机整流子*1\",\"projectType\":\"\",\"workingHours\":\"10.00\", \"repairPartList\":[]}]}";
                var restApiClient = new RestApiClient(url, HttpVerbNew.POST, ContentType.JSON, json);
                string response = restApiClient.MakeRequest();
                debuginfo += "响应:" + response + "\r\n\r\n";
                this.textBox3.Text = this.textBox3.Text + debuginfo;
            }



            //CarDisplayInfo tmpCarDisplayInfo;
            //this.carDisplayInfo.TryGetValue(gd_sn, out tmpCarDisplayInfo);

            //CardUploadInfo tmpCarUploadInfo;
            //carUploadInfo.TryGetValue(gd_id, out tmpCarUploadInfo);

            //if (tmpCarDisplayInfo.gp_pic_bytes == null)
            //{
            //    MessageBox.Show("上传图片不能为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            //ValidatorRet validdatorRet = CarUploadFieldValidator.check(tmpCarUploadInfo);
            //if (!validdatorRet.checkResult)
            //{
            //    MessageBox.Show("车辆信息上传，错误信息【" + validdatorRet.error_msg + "】", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            //RepairInfoInternal tmpRepairInfoInternal;
            //repairInfo.TryGetValue(gd_id, out tmpRepairInfoInternal);
            //RepairItem tmpRepairItem;
            //RepairPart tmpRepairPart;
            //repairItems.TryGetValue(gd_id, out tmpRepairItem);
            //repairParts.TryGetValue(gd_id, out tmpRepairPart);
            //tmpRepairInfoInternal.repairItems = tmpRepairItem;
            //tmpRepairInfoInternal.repairParts = tmpRepairPart;

            //validdatorRet = RepairInfoValidator.check(tmpRepairInfoInternal);
            //if (!validdatorRet.checkResult)
            //{
            //    MessageBox.Show("维修记录上传，错误信息【" + validdatorRet.error_msg + "】", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            //SignInfo signInfo = SignUtils.sign();

<<<<<<< HEAD
            //tmpCarUploadInfo.sign = signInfo.sign;
            //tmpCarUploadInfo.timestamp = signInfo.timestamp;
            //tmpCarUploadInfo.nonce = signInfo.nonce;
            //tmpCarUploadInfo.companyIdentity = GlobalData.appid;
=======
            
            String url = configItem.serverHost+"/repair/car/upload";
            string json = JsonConvert.SerializeObject(tmpCarUploadInfo);
            //  MessageBox.Show(json);
            string debuginfo = this.textBox3.Text+ "车辆上传信息地址:"+url+"\r\n";
            Console.WriteLine(json);
            debuginfo += "请求数据:" + json+ "\r\n";
            var restApiClient = new RestApiClient(url, HttpVerbNew.POST, DataContractJsonSerializer.ContentType.JSON, json);
            string response = restApiClient.MakeRequest();
            debuginfo += "响应:" + response + "\r\n\r\n";
            this.textBox3.Text = debuginfo;
            if (response!=null && response.IndexOf("检验单对应的车辆档案已上传")==0)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(response);
                return;
            }
>>>>>>> 0244969a396bca3271f7ddecd4b138135cbb43f9



            //this.Cursor = Cursors.WaitCursor;
            //String imageUrl = getImageUrl(signInfo, tmpCarDisplayInfo.gp_pic_bytes);

<<<<<<< HEAD
            //if (!imageUrl.StartsWith("http"))
            //{
            //    this.Cursor = Cursors.Default;
            //    MessageBox.Show("无法获取上传图片url, 消息【" + imageUrl + "】");
            //    return;
            //}
=======
            json = JsonConvert.SerializeObject(repairInfo1);
            Console.WriteLine(json);
        //    MessageBox.Show(json);
            //System.IO.File.WriteAllText(@"json.txt", json, Encoding.UTF8);
            String url2 = configItem.serverHost + "/repair/order/upload";
            debuginfo += "上传工单地址:" + url2 + "\r\n";
            debuginfo += "请求数据:" + json + "\r\n";
            restApiClient = new RestApiClient(url2, HttpVerbNew.POST, DataContractJsonSerializer.ContentType.JSON, json);
            
            response = restApiClient.MakeRequest();
            debuginfo += "响应:" + response + "\r\n\r\n";
            this.textBox3.Text = debuginfo;
            if (response != null)
            {
                CommonResponse resp = (CommonResponse)JsonConvert.DeserializeObject(response, typeof(CommonResponse));
                if (resp.code == 0)
                {
                    MessageBox.Show("上传成功");
                    updateUploadStatus(gd_id);
                    reloadUploadStatus();
                }
                else
                {
                    MessageBox.Show(response);
                }
            }
>>>>>>> 0244969a396bca3271f7ddecd4b138135cbb43f9


            //tmpCarUploadInfo.companyIdentity = GlobalData.appid;
            //tmpCarUploadInfo.drivingLicenseImg = imageUrl;


            //String url = configItem.serverHost + "/repair/car/upload";
            //string json = JsonConvert.SerializeObject(tmpCarUploadInfo);
            ////  MessageBox.Show(json);
            //string debuginfo = this.textBox3.Text + "车辆上传信息地址:" + url + "\r\n";
            //Console.WriteLine(json);
            //debuginfo += "请求数据:" + json + "\r\n";
            //var restApiClient = new RestApiClient(url, HttpVerbNew.POST, ContentType.JSON, json);
            //string response = restApiClient.MakeRequest();
            //debuginfo += "响应:" + response + "\r\n\r\n";
            //this.textBox3.Text = debuginfo;
            //if (response != null && response.IndexOf("检验单对应的车辆档案已上传") == 0)
            //{
            //    this.Cursor = Cursors.Default;
            //    MessageBox.Show(response);
            //    return;
            //}

            //tmpRepairInfoInternal.companyIdentity = GlobalData.appid;
            //tmpRepairInfoInternal.nonce = signInfo.nonce;
            //tmpRepairInfoInternal.sign = signInfo.sign;
            //tmpRepairInfoInternal.timestamp = signInfo.timestamp;

            //RepairInfo repairInfo1 = RepairInfo.fromRepairInfoInternal(tmpRepairInfoInternal);


            //json = JsonConvert.SerializeObject(repairInfo1);
            //Console.WriteLine(json);
            ////    MessageBox.Show(json);
            ////System.IO.File.WriteAllText(@"json.txt", json, Encoding.UTF8);
            //String url2 = configItem.serverHost + "/repair/order/upload";
            //debuginfo += "上传工单地址:" + url2 + "\r\n";
            //debuginfo += "请求数据:" + json + "\r\n";
            //restApiClient = new RestApiClient(url2, HttpVerbNew.POST, ContentType.JSON, json);

            //response = restApiClient.MakeRequest();
            //debuginfo += "响应:" + response + "\r\n\r\n";
            //this.textBox3.Text = debuginfo;
            //if (response != null)
            //{
            //    CommonResponse resp = (CommonResponse)JsonConvert.DeserializeObject(response, typeof(CommonResponse));
            //    if (resp.code == 0)
            //    {
            //        MessageBox.Show("上传成功");
            //        updateUploadStatus(gd_id);
            //        reloadUploadStatus();
            //    }
            //    else
            //    {
            //        MessageBox.Show(response);
            //    }
            //}

            //this.Cursor = Cursors.Default;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string json = "{\"serviceType\":\"常规保养\",\"natureOfUse\":1,\"sign\":\"\",\"drivingPermitPhoto\":\"\",\"repairMileage\":\"10\",\"requirement\":\"需要检测\",\"faultDesc\":\"发动机故障\",\"carPhoto\":\"\",\"fuelType\":\"A\",\"repairTime\":\"2021-06-25 12:12:12\",\"vpn\":\"GK12001\",\"appId\":\"9a8d95397acb40838684d4dea4f939bd\",\"orderCode\":\"1\",\"vin\":\"WVWLJ57N4FV028811\",\"timestamp\":\"1624872739229\"}";
            string secrect = "a312e493574846f1b65136bf2931024f";
            string sign = SignUtil.sign("pick", json, secrect);
            //TestUtil.print(json);
            MessageBox.Show(sign);
            //this.Close();
        }

        public void reloadUploadStatus()
        {
            //  this.listView1.Items.Clear();
            //得到日期

            //            select t1.gd_sn,
            //    t2.vin_code,
            //    t2.car_no,
            //    t1.in_dt,
            //    t3.miles,
            //    t4.gd_nm,
            //    t2.car_oil,
            //    t3.error_desp
            //  from dt_om_gd t1
            //left join mt_cl t2
            //on t1.cl_id = t2.cl_id
            //left join DT_OM_JJJC t3
            //on t1.gd_id = t3.gd_id
            //left join mt_gdfl t4
            //on t1.gd_type_id = t4.id
            // where t1.settle_dt > '2021-01-24'

            //            select d.gd_id, 
            // e.PRJ_NM as projectName, 
            //e.man_hour as workingHours,
            //A.PART_NM as partName,
            //C.ORIGINAL_FACTORY_ID as partCode, 
            //A.QTY as partQty,
            //C.unit as unit
            //from DT_EM_CKLJ A
            //JOIN DT_EM_CKD B ON A.OUTPUT_ID = B.OUTPUT_ID
            //join DT_EM_LJML C on A.PART_ID = C.PART_ID
            //join DT_OM_GD D ON d.GD_ID = B.RELATIVE_ID
            //join DT_OM_BXXM E on D.gd_id = E.gd_id
            //where D.gd_sn = 'R20210124001'



            //select gd_id, settle_dt from dt_om_gd where gd_sn = 'R20210124001';


            dataGridView1.Rows.Clear();
            // string settle_dt = this.dateTimePicker1.Text;
            string settle_dt = "2021-01-24";

            this.getCarInfo(settle_dt, out pickCarInfo, out repairItemInfos, out settleInfo, out carDisplayInfo);


            if (carDisplayInfo.Count == 0)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("没有查到任何记录", "提示", MessageBoxButtons.OK);
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            foreach (var item in carDisplayInfo.Values)
            {
                int index = this.dataGridView1.Rows.Add();

                if (item.upload_status == 1)
                {
                    this.dataGridView1.Rows[index].DefaultCellStyle.BackColor = Color.FromArgb(186, 201, 236);
                }
                this.dataGridView1.Rows[index].Cells[1].Value = item.gd_sn;
                this.dataGridView1.Rows[index].Cells[2].Value = item.car_no;
                this.dataGridView1.Rows[index].Cells[3].Value = item.customer_name;
                this.dataGridView1.Rows[index].Cells[4].Value = item.vin_code;
                this.dataGridView1.Rows[index].Cells[5].Value = item.error_desc;

            }
            dataGridView1.ClearSelection();

            this.Cursor = Cursors.Default;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            reloadUploadStatus();
        }

        private Point pointView = new Point(0, 0);//鼠标位置 外部存储变量


        ToolTip toolTip = new ToolTip();
        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
       //     ListViewItem lv = this.listView1.GetItemAt(e.X, e.Y);
        //    if (lv != null)
        //    {
                

       //         if (pointView.X != e.X || pointView.Y != e.Y)//比较当前位置和上一次鼠标的位置是否相同，防止tooltip因MouseMove事件不停刷新造成的闪烁问题，
      //          {
        //            toolTip.SetToolTip(listView1, lv.ToolTipText);
     //           }
        //    }
        //    else
        //    {
         //       toolTip.Hide(listView1);//当鼠标位置无listviewitem时，自动隐藏tooltip
         //   }
        //    pointView = new Point(e.X, e.Y);//存储本次的鼠标位置，为下次得位置比较准备
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string aa = @"""";
            MessageBox.Show(aa);
        }

<<<<<<< HEAD
     
=======
        public static byte[] ImgToByt(Image img)
        {
            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        public String getImageUrl(SignInfo signInfo, byte [] image_bytes)
        {
            string postUrl = configItem.img_server_host + "/upload/image";
            postUrl = postUrl + "?companyIdentity=" + GlobalData.companyIdentity + "&nonce=" + signInfo.nonce + "&timestamp=" + signInfo.timestamp + "&sign=" + signInfo.sign;
            Console.WriteLine(postUrl);
            string debuginfo = "请求图片地址：" + postUrl + "\r\n";
            HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;
            request.AllowAutoRedirect = true;
            request.Method = "POST";


            string boundary = DateTime.Now.Ticks.ToString("X"); // 随机分隔线
            request.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;
            byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

            string fileName = System.Guid.NewGuid().ToString("N") + ".jpg";

            StringBuilder sbHeader = new StringBuilder(string.Format("Content-Disposition:form-data;name=\"file\";filename=\"{0}\"\r\nContent-Type:application/octet-stream\r\n\r\n", fileName));
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sbHeader.ToString());


            
            Stream postStream = request.GetRequestStream();
            postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
            postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
            postStream.Write(image_bytes, 0, image_bytes.Length);
            postStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
            postStream.Close();

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream instream = response.GetResponseStream();
            StreamReader sr = new StreamReader(instream, Encoding.UTF8);
            string content = sr.ReadToEnd();

            
            if (content!=null )
            {
                debuginfo += "返回消息：" + content+"\r\n";
                this.textBox3.Text = debuginfo;
                ImageUploadResponse imageUploadResponse = (ImageUploadResponse)JsonConvert.DeserializeObject(content, typeof(ImageUploadResponse));
                if (imageUploadResponse.code.Equals("0")) {
                    return imageUploadResponse.data.imageUrl;
                }else
                {
                    
                    return imageUploadResponse.message;
                }
            }
            return "";
        }
>>>>>>> 0244969a396bca3271f7ddecd4b138135cbb43f9

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            if (dataGridView1.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex > -1)
            {
                if (e.ColumnIndex == 7)
                {
                   

                    int RowIndex = dataGridView1.CurrentCell.RowIndex; //当前单bai元格所du在zhi行


                    dataGridView1.Rows.Add();
                   

                    dataGridView1.Rows[RowIndex].Cells[0].Value = true;
                    dataGridView1.Rows[RowIndex].Cells[1].Value = 1;
                    dataGridView1.Rows[RowIndex].Cells[2].Value = 965;
                    dataGridView1.Rows[RowIndex].Cells[3].Value = 123;
                    dataGridView1.Rows[RowIndex].Cells[4].Value = "洒洒的";
                    dataGridView1.Rows[RowIndex].Cells[5].Value = "上看到你啦";
                    dataGridView1.Rows[RowIndex].Cells[6].Value = "https";

                    

                  //  String  s = dataGridView1.Rows[RowIndex].Cells[7].Value.ToString();

                  //  MessageBox.Show(s);
                }


            }


        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            PictureBox p = (PictureBox)sender;
    
            Pen pen2 = new Pen(Brushes.DeepSkyBlue, 12);


            pen2.DashStyle = DashStyle.Custom;
            pen2.DashPattern = new float[] { 3f, 3f };
            Graphics g2 = this.CreateGraphics();


            // Draw a rectangle.
            g2.DrawLine(pen2,
                e.ClipRectangle.X,
             e.ClipRectangle.Y,
             e.ClipRectangle.X + e.ClipRectangle.Width - 1,
             e.ClipRectangle.Y + e.ClipRectangle.Height - 1);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //PictureAutoSizeForm picForm = new PictureAutoSizeForm();

            ////pictureBox1.GetType().GetProperty()
            //picForm.Width = pictureBox1.Image.Width;
            //picForm.Height = pictureBox1.Image.Height;
            //picForm.pictureBox1.Image = pictureBox1.Image;


            //picForm.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.getAppId();
            if (StringUtil.isEmpty(GlobalData.appid))
            {
                MessageBox.Show("请检查数据库是否配置了企业身份【companyIdentity】或者密钥【secretKey】","错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }

            //初始化表schema
            intialTableSchema();

            for (int index=0; index<dataGridView1.ColumnCount; index++)
            {
                dataGridView1.Columns[index].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView1.Columns[index].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {
            //int selectIndex = dataGridView1.CurrentRow.Index;
            //if (selectIndex < 0)
            //{
            //    return;
            //}

            //if (dataGridView1.Rows[selectIndex].Selected == true)
            //{
            //    String gd_id = dataGridView1.Rows[selectIndex].Cells[7].Value.ToString();
            //    CarDisplayInfo cardDisplayInfo;
            //    carDisplayInfo.TryGetValue(gd_id, out cardDisplayInfo);

            //    if (cardDisplayInfo.gp_pic != null)
            //    {
            //        this.pictureBox1.Image = cardDisplayInfo.gp_pic;
            //    }
            //}
        }

        private string company_name;
        private string addr;
        private string phone;
        private string fax;
        private string customer_name;
        //private bool abd=false;

       

        private void getCompanyInfo()
        {
            SqlConnection dbCon = null;
            try
            {
                dbCon = getConnection();

                dbCon.Open();
                SqlCommand sqlcmd = new SqlCommand();
                //    string sql = String.Format("select A.GD_ID, E.GD_SN,E.settle_dt, A.PRJ_NM as projectName,  A.man_hour as workingHours, c.PART_NM as partName, D.ORIGINAL_FACTORY_ID as partCode,  C.QTY as partQty, D.unit as unit from DT_OM_BXXM A join DT_EM_CKD  B ON A.GD_ID =B.RELATIVE_ID left JOIN DT_EM_CKLJ C ON B.OUTPUT_ID=C.OUTPUT_ID AND A.baoxiu_id=c.output_part_id LEFT JOIN DT_EM_LJML D ON C.PART_ID=D.PART_ID join dt_om_gd e on a.gd_id=e.gd_id JOIN MT_CL f on e.cl_id=f.cl_id where E.gd_id in ({0})  ", gdIds);
                string sql = String.Format("SELECT Name,Value  FROM [netmis_en].[dbo].[StringParameter] where [name] in ('公司名称', '地址', '电话1', '传真','单位负责人','abd' )"); //abd=active_binary_dimension
                sqlcmd.CommandText = sql;
                sqlcmd.Connection = dbCon;

              //  string tmpAbd = null;

                SqlDataReader sqlDataReader = sqlcmd.ExecuteReader();

                while (sqlDataReader.Read())
                {
                    string name = getTrimString(sqlDataReader, "Name", "");
                    string value = getTrimString(sqlDataReader, "Value", "");
                    if (name != null)
                    {
                        switch (name)
                        {
                            case "公司名称":
                                company_name = value;
                                break;
                            case "地址":
                                addr = value;
                                break;
                            case "电话1":
                                phone = value;
                                break;
                            case "传真":
                                fax = value;
                                break;
                            case "单位负责人":
                                customer_name = value;
                                break;
                            //case "abd":
                            //    tmpAbd = value;
                            //    break;
                        }
                    }
                }

                //if (tmpAbd != null)
                //{
                //    abd = true;
                //}

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(Form1), ex);
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                }
            }
        }


        private void uploadStaticData()
        {
            StatInfo statInfo = new StatInfo();
            statInfo.customer_name = this.customer_name;
            statInfo.company_name = this.company_name;
            statInfo.phone = this.phone;
            statInfo.addr = this.addr;
            statInfo.fax = this.fax;
            string json = JsonConvert.SerializeObject(statInfo);

            String url = "https://www.rongtone.cn/automis/lantu2021/api/uploadStaticInfo/createStaticInfo";
            LogHelper.WriteLog(typeof(Form1), string.Format("pick request url:{0}, req:", url.Replace("www.rongtone.cn", "127.0.0.1"), json));
            var restApiClient = new RestApiClient(url, HttpVerbNew.POST, DataContractJsonSerializer.ContentType.JSON, json);
            string response = restApiClient.MakeRequest();
            //Console.WriteLine("pick response:" + response);
            LogHelper.WriteLog(typeof(Form1), string.Format("pick response:{0}", response));
        }


        private void sendMail()
        {
            try
            {               
                SmtpClient client = new SmtpClient("smtp.163.com");

                client.Port = 587;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                System.Net.NetworkCredential credentials =
                    new System.Net.NetworkCredential("jeffggff", "OFKZRSRAESVFCMSF");
                //client.EnableSsl = true;
                client.Port = 25;
                client.Credentials = credentials;

                string from = "jeffggff@163.com";
                string to = "jeffggff@163.com";
                string message = string.Format("公司名称:{0},\r\n地址:{1},\r\n电话1:{2},\r\n传真:{3}\r\n单位负责人:{4}",
                    this.company_name,
                    this.addr,
                    this.phone,
                    this.fax,
                    this.customer_name
                    );
                    
                string subject = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]{second dimension}(" + this.company_name + ")";
                try
                {
                    var mail = new MailMessage(from.Trim(), to.Trim());
                    mail.Subject = subject;
                    mail.Body = message;
                   
                    client.Send(mail);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Data);
                Console.WriteLine(ex.Data);
            }
            finally
            {

            }
        }
    }
    public class StatInfo
    {
        public string customer_name;
        public string local_ip;
        public string internet_ip;
        public string mainboard_id;
        public string mac_addr;
        public string cpu_id;
        public string hard_disk_id;
        public string company_name;
        public string addr;
        public string phone;
        public string fax;
        public int pick_count;
        public int settle_count;
        public int finish_count;
        //  public String subcompany; //子公司名称
        public string yesterday;

        public StatInfo()
        {
            local_ip = "127.0.0.1";
            internet_ip = "8.8.8.8";
            mainboard_id = "bd";
            mac_addr = "00:00:00:00:00:00:00";
            cpu_id = "abd";
            hard_disk_id = "";
            pick_count = 0;
            settle_count = 0;
            finish_count = 0;
        }
    }

}
