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
using System.Net.Mail;
using System.Net.Mime;

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
        }
        public ConfigItem configItem;
        private string connStr;
        private SqlConnection sqlConn;
        private string companyIdentity;

        public Dictionary<String, GDCardInfo> gdCardInfo = new Dictionary<String, GDCardInfo>();

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
        public void getCompanyIdentity()
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
                    cmd.CommandText = "select name, value from StringParameter where name in ('companyIdentity', 'secretKey')";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                           String name = reader["name"].ToString().Trim();
                           String value = reader["value"].ToString().Trim();

                            if (name.Equals("companyIdentity"))
                            {
                                GlobalData.companyIdentity = value;
                                this.companyIdentity = value;
                            }else if (name.Equals("secretKey"))
                            {
                                GlobalData.secretKey = value;
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
        public Dictionary<String, RepairItem> getRepairItemDetail(Dictionary<String, CarDisplayInfo> cardDisplayInfo)
        {
            Dictionary<String, RepairItem> repairItems = new Dictionary<String, RepairItem>();

            List<String> gd_ids = new List<String>();
            foreach (var gd_id in cardDisplayInfo.Keys)
            {
                gd_ids.Add(gd_id);
            }

            string gd_ids_str = string.Join(",", gd_ids.ToArray());

            SqlConnection dbCon = getConnection();
            try
            {
                dbCon.Open();
                SqlCommand sqlcmd = new SqlCommand();
                string sql = string.Format("select gd_id,prj_nm as itemName,MAN_HOUR as settlementTime from DT_OM_BXXM where gd_id in ({0})", gd_ids_str);
                sqlcmd.CommandText = sql;
                sqlcmd.Connection = dbCon;


                SqlDataReader sqlDataReader = sqlcmd.ExecuteReader();
                Dictionary<String, Double> subtotals = new Dictionary<string, double>();
                while (sqlDataReader.Read())
                {
                    string gd_id = getTrimString(sqlDataReader, "gd_id", "");

                    RepairItemDetail repairItemDetail = new RepairItemDetail();
                    repairItemDetail.itemName = getTrimString(sqlDataReader, "itemName", "");
                    repairItemDetail.settlementTime = getTrimString(sqlDataReader, "settlementTime", "0");

                    RepairItem repairItem = null;
                    repairItems.TryGetValue(gd_id, out repairItem);
                    if (repairItem == null)
                    {
                        repairItem = new RepairItem();
                        repairItems.Add(gd_id, repairItem);
                    }
                    Double subtotal;
                    subtotals.TryGetValue(gd_id, out subtotal);
                    subtotal += Convert.ToDouble(repairItemDetail.settlementTime);
                    repairItem.subtotal = subtotal.ToString();

                    int cnt = repairItem.items.Count+ 1;
                    repairItemDetail.itemSeq = cnt.ToString();
                    repairItem.items.Add(repairItemDetail);
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

            return repairItems;
        }


        //计算维修配件列表
        public Dictionary<String, RepairPart> getRepairPartsDetail(Dictionary<String, CarDisplayInfo> cardDisplayInfo)
        {
            Dictionary<String, RepairPart> repairParts = new Dictionary<String, RepairPart>();

            List<String> gd_ids = new List<String>();
            foreach (var gd_id in cardDisplayInfo.Keys)
            {
                gd_ids.Add(gd_id);
            }

            string gd_ids_str = string.Join(",", gd_ids.ToArray());

            SqlConnection dbCon = getConnection();
            try
            {
                dbCon.Open();
                SqlCommand sqlcmd = new SqlCommand();
                string sql = string.Format("select d.gd_id,  C.PART_NM as partName, c.location as partBrand, C.ORIGINAL_FACTORY_ID as partNo, A.QTY as partUnitNumber from DT_EM_CKLJ A JOIN DT_EM_CKD  B ON A.OUTPUT_ID =B.OUTPUT_ID join DT_EM_LJML C on A.PART_ID = C.PART_ID join DT_OM_GD D ON d.GD_ID=B.RELATIVE_ID  where  D.gd_id in ({0})", gd_ids_str);
                sqlcmd.CommandText = sql;
                sqlcmd.Connection = dbCon;


                SqlDataReader sqlDataReader = sqlcmd.ExecuteReader();
                Dictionary<String, Double> subtotals = new Dictionary<string, double>();
                while (sqlDataReader.Read())
                {
                    string gd_id = getTrimString(sqlDataReader, "gd_id", "");

                    RepairPartDetail repairItemDetail = new RepairPartDetail();
                    repairItemDetail.partName = getTrimString(sqlDataReader, "partName", "");
                    repairItemDetail.partNo = getTrimString(sqlDataReader, "partNo", "");
                    repairItemDetail.partBrand = getTrimString(sqlDataReader, "partBrand", "");
                    repairItemDetail.partUnitNumber = getTrimString(sqlDataReader, "partUnitNumber", "");

                    RepairPart repairPart = null;
                    repairParts.TryGetValue(gd_id, out repairPart);
                    if (repairPart == null)
                    {
                        repairPart = new RepairPart();
                        repairParts.Add(gd_id, repairPart);
                    }
                    Double subtotal;
                    subtotals.TryGetValue(gd_id, out subtotal);
                    subtotal += Convert.ToDouble(repairItemDetail.partUnitNumber);
                    repairPart.subtotal = subtotal.ToString();

                    int cnt = repairPart.parts.Count + 1;
                    repairItemDetail.partSeq = cnt.ToString();
                    repairPart.parts.Add(repairItemDetail);
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

            return repairParts;
        }

        public void getCarInfo(String date_str, out Dictionary<String, CardUploadInfo> carUploadInfo,
            out  Dictionary<String, CarDisplayInfo> carDisplayInfo,
            out Dictionary<String, RepairInfoInternal> repairInfo)
        {
            carUploadInfo = new Dictionary<String, CardUploadInfo>();
            carDisplayInfo = new Dictionary<String, CarDisplayInfo>();
            repairInfo = new Dictionary<String, RepairInfoInternal>();

            String gd_sn = textBox1.Text.Trim();
            String car_no = textBox2.Text.Trim();

            SqlConnection dbCon = getConnection();
            try
            {
                dbCon.Open();
                SqlCommand sqlcmd = new SqlCommand();
                string sql = string.Format("select a.GD_ID, a.GD_SN, a.upload_status, case when (a.BADPART='旧配件已确认，并由托修方收回') then 1 when (a.BADPART='旧配件已确认，托修方申明放弃') then 2 when (a.BADPART='无旧件') then 3 else 2 end as BADPART, a.GD_SN as settlementSeq,convert(char(10), a.SETTLE_DT,120) as settlementDate, convert(char(10), a.IN_DT,120) as deliveryDate, b.Car_No as licensePlate,  b.VIN_CODE as vin, c.CUST_NM as vehicleOwner, c.CUST_NM as entrustRepair,  isnull(b.ENGINE_NO, '') engineNum, isnull(c.LINKMAN,'') contact, isnull(c.TEL1,'') contactDetails, isnull(b.gearbox_type, 1) obd,     case when (b.CAR_OIL=0 ) then   '轻型汽油车'  when (b.CAR_OIL=1 ) then  '重型汽油车'  when (b.CAR_OIL=2 ) then  '柴油车'  when (b.CAR_OIL=3 ) then  '其它车'  when (b.CAR_OIL=4 ) then  'LPG燃料车'  when (b.CAR_OIL=5 ) then  'CNG燃料车'  else  '其它车'  end as carType, g.CAR_TYPE as vehicleType,  b.CAR_SYMBOL as vehicleClassCode,  isnull(b.CAR_COLOR,'') color,  d.ERROR_DESP,   d.CAR_INFO as incomingInspectionId,  e.VENDOR as brand,  isnull(d.MILES,'0') repairMileage,  isnull(f.GD_PIC,'') GD_PIC   from DT_OM_GD a    join MT_CL b on a.cl_id=b.cl_id   join MT_KH c on a.KH_ID=c.KH_ID    join DT_OM_JJJC d on a.gd_id=d.gd_id   join MT_CC e on b.cc_id=e.cc_id  join MT_CX g on b.cx_id=g.cx_id  left join DT_OM_GDTP f on a.gd_id=f.gd_id where 1=1    and a.is_settle=1 and SUBSTRING(CONVERT(varchar(100), a.SETTLE_DT, 20), 1, 10)='{0}'", date_str);
//                string sql = string.Format("select a.GD_ID, a.GD_SN, a.GD_SN as settlementSeq,convert(char(10), a.SETTLE_DT,120) as settlementDate, convert(char(10), a.IN_DT,120) as deliveryDate, b.Car_No as licensePlate,  b.VIN_CODE as vin, c.CUST_NM as vehicleOwner, c.CUST_NM as entrustRepair,  isnull(b.ENGINE_NO, '') engineNum, isnull(c.LINKMAN,'') contact, isnull(c.TEL1,'') contactDetails, isnull(b.gearbox_type, 1) obd,     case when (b.CAR_OIL=0 ) then   '轻型汽油车'  when (b.CAR_OIL=1 ) then  '重型汽油车'  when (b.CAR_OIL=2 ) then  '柴油车'  when (b.CAR_OIL=3 ) then  '其它车'  when (b.CAR_OIL=4 ) then  'LPG燃料车'  when (b.CAR_OIL=5 ) then  'CNG燃料车'  else  '其它车'  end as carType, g.CAR_TYPE as vehicleType,  b.CAR_SYMBOL as vehicleClassCode,  isnull(b.CAR_COLOR,'') color,  d.ERROR_DESP,   d.CAR_INFO as incomingInspectionId,  e.VENDOR as brand,  isnull(d.MILES,'0') repairMileage,  isnull(f.GD_PIC,'') GD_PIC   from DT_OM_GD a    join MT_CL b on a.cl_id=b.cl_id   join MT_KH c on a.KH_ID=c.KH_ID    join DT_OM_JJJC d on a.gd_id=d.gd_id   join MT_CC e on b.cc_id=e.cc_id  join MT_CX g on b.cc_id=g.cc_id  left join DT_OM_GDTP f on a.gd_id=f.gd_id where 1=1    and a.is_settle=1 and SUBSTRING(CONVERT(varchar(100), a.SETTLE_DT, 20), 1, 10)='2020-05-22'");
                if (gd_sn.Length > 0)
                {
                    sql += string.Format(" and a.GD_SN like '%{0}%'", gd_sn);
                }

                if (car_no.Length > 0)
                {
                    sql += string.Format(" and b.Car_No like '%{0}%'", car_no);
                }
                sqlcmd.CommandText = sql;
                sqlcmd.Connection = dbCon;

                SqlDataReader sqlDataReader = sqlcmd.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    string gd_id = getTrimString(sqlDataReader, "gd_id", "");

                    CardUploadInfo tmpCardUploadInfo;
                    CarDisplayInfo tmpCarDisplayInfo;
                    RepairInfoInternal tmpRepairInfo;
                    carUploadInfo.TryGetValue(gd_id, out tmpCardUploadInfo);
                    carDisplayInfo.TryGetValue(gd_id, out tmpCarDisplayInfo);
                    repairInfo.TryGetValue(gd_id, out tmpRepairInfo);

                    if (tmpCardUploadInfo == null)
                    {
                        tmpCardUploadInfo = new CardUploadInfo();
                        carUploadInfo[gd_id] = tmpCardUploadInfo;
                    }

                    if (tmpCarDisplayInfo == null)
                    {
                        tmpCarDisplayInfo = new CarDisplayInfo();
                        carDisplayInfo[gd_id] = tmpCarDisplayInfo;
                    }

                    if (tmpRepairInfo == null)
                    {
                        tmpRepairInfo = new RepairInfoInternal();
                        repairInfo[gd_id] = tmpRepairInfo;
                    }

                    tmpCardUploadInfo.companyIdentity = GlobalData.companyIdentity;
                    tmpCardUploadInfo.incomingInspectionId = getTrimString(sqlDataReader, "incomingInspectionId", "");
                    tmpCardUploadInfo.repairOrderSeq = getTrimString(sqlDataReader, "GD_SN", "");
                    tmpCardUploadInfo.entrustRepair = getTrimString(sqlDataReader, "entrustRepair", "");
                    tmpCardUploadInfo.licensePlate = getTrimString(sqlDataReader, "licensePlate", "");
                    tmpCardUploadInfo.vin = getTrimString(sqlDataReader, "vin", "");
                    tmpCardUploadInfo.vehicleType = getTrimString(sqlDataReader, "vehicleType", "");
                    tmpCardUploadInfo.engineNum = getTrimString(sqlDataReader, "engineNum", "");
                    tmpCardUploadInfo.vehicleOwner = getTrimString(sqlDataReader, "vehicleOwner", "");
                    tmpCardUploadInfo.entrustRepair = getTrimString(sqlDataReader, "entrustRepair", "");
                    tmpCardUploadInfo.contact = getTrimString(sqlDataReader, "contact", "");
                    tmpCardUploadInfo.contactDetails = getTrimString(sqlDataReader, "contactDetails", "");
                    tmpCardUploadInfo.carType = getTrimString(sqlDataReader, "carType", "");
                    tmpCardUploadInfo.vehicleClassCode = getTrimString(sqlDataReader, "vehicleClassCode", "");
                    tmpCardUploadInfo.obd = getTrimString(sqlDataReader, "obd", "");
                    tmpCardUploadInfo.color = getTrimString(sqlDataReader, "color", "");
                    tmpCardUploadInfo.brand = getTrimString(sqlDataReader, "brand", "");


                    tmpCarDisplayInfo.gd_id = getTrimString(sqlDataReader, "GD_ID", "");
                    int upload_status = 0;
                    int.TryParse(getTrimString(sqlDataReader, "upload_status", "0"), out upload_status);
                    tmpCarDisplayInfo.upload_status = upload_status;
                    tmpCarDisplayInfo.car_no = getTrimString(sqlDataReader, "licensePlate", "");
                    tmpCarDisplayInfo.gd_sn = getTrimString(sqlDataReader, "GD_SN", "");
                    tmpCarDisplayInfo.customer_name = getTrimString(sqlDataReader, "vehicleOwner", "");
                    tmpCarDisplayInfo.incomingInspectionId = getTrimString(sqlDataReader, "incomingInspectionId", "");
                    tmpCarDisplayInfo.vin_code = getTrimString(sqlDataReader, "vin", "");
                    tmpCarDisplayInfo.error_desc = getTrimString(sqlDataReader, "ERROR_DESP", "");

                    byte[] gp_pic_byte = (byte[])sqlDataReader["GD_PIC"];
                    
               

                        if (gp_pic_byte.Length >10)
                        {
                        Image image;
                        if (gp_pic_byte != null)
                        {
                            MemoryStream mymemorystream = new MemoryStream(gp_pic_byte);
                            image = Image.FromStream(mymemorystream, true);
                            mymemorystream.Close();
                            tmpCarDisplayInfo.gp_pic = image;
                            tmpCarDisplayInfo.gp_pic_bytes = gp_pic_byte;
                        }

                    }

                    tmpRepairInfo.companyIdentity = GlobalData.companyIdentity;
                    tmpRepairInfo.incomingInspectionId = getTrimString(sqlDataReader, "incomingInspectionId", "");
                    tmpRepairInfo.repairOrderSeq = getTrimString(sqlDataReader, "GD_SN", "");
                    tmpRepairInfo.deliveryDate = getTrimString(sqlDataReader, "deliveryDate", "");
                    tmpRepairInfo.repairMileage = getTrimString(sqlDataReader, "repairMileage", "0");
                    tmpRepairInfo.settlementDate = getTrimString(sqlDataReader, "settlementDate", "");
                    tmpRepairInfo.settlementSeq = getTrimString(sqlDataReader, "settlementSeq", "");

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
            int count = 0, selectIndex = -1 ;
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

            string gd_id = dataGridView1.Rows[selectIndex].Cells[7].Value.ToString();

            CarDisplayInfo tmpCarDisplayInfo;
            this.carDisplayInfo.TryGetValue(gd_id, out tmpCarDisplayInfo);

            CardUploadInfo tmpCarUploadInfo;
            carUploadInfo.TryGetValue(gd_id, out tmpCarUploadInfo);

            if (tmpCarDisplayInfo.gp_pic_bytes == null)
            {
                MessageBox.Show("上传图片不能为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ValidatorRet validdatorRet = CarUploadFieldValidator.check(tmpCarUploadInfo);
            if (!validdatorRet.checkResult)
            {
                MessageBox.Show("车辆信息上传，错误信息【" + validdatorRet.error_msg + "】", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            RepairInfoInternal tmpRepairInfoInternal;
            repairInfo.TryGetValue(gd_id, out tmpRepairInfoInternal);
            RepairItem tmpRepairItem;
            RepairPart tmpRepairPart;
            repairItems.TryGetValue(gd_id, out tmpRepairItem);
            repairParts.TryGetValue(gd_id, out tmpRepairPart);
            tmpRepairInfoInternal.repairItems = tmpRepairItem;
            tmpRepairInfoInternal.repairParts = tmpRepairPart;

            validdatorRet = RepairInfoValidator.check(tmpRepairInfoInternal);
            if (!validdatorRet.checkResult)
            {
                MessageBox.Show("维修记录上传，错误信息【" + validdatorRet.error_msg + "】", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SignInfo signInfo = SignUtils.sign();

            tmpCarUploadInfo.sign = signInfo.sign;
            tmpCarUploadInfo.timestamp = signInfo.timestamp;
            tmpCarUploadInfo.nonce = signInfo.nonce;
            tmpCarUploadInfo.companyIdentity = GlobalData.companyIdentity;



            this.Cursor = Cursors.WaitCursor;
            String imageUrl = getImageUrl(signInfo, tmpCarDisplayInfo.gp_pic_bytes);

            if (!imageUrl.StartsWith("http")) 
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("无法获取上传图片url, 消息【" + imageUrl + "】");
                return;
            }


            tmpCarUploadInfo.companyIdentity = GlobalData.companyIdentity;
            tmpCarUploadInfo.drivingLicenseImg = imageUrl;

            
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

            tmpRepairInfoInternal.companyIdentity = GlobalData.companyIdentity;
            tmpRepairInfoInternal.nonce = signInfo.nonce;
            tmpRepairInfoInternal.sign = signInfo.sign;
            tmpRepairInfoInternal.timestamp = signInfo.timestamp;

            RepairInfo repairInfo1 = RepairInfo.fromRepairInfoInternal(tmpRepairInfoInternal);


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

            this.Cursor = Cursors.Default;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        Dictionary<String, CardUploadInfo> carUploadInfo;
        Dictionary<String, CarDisplayInfo> carDisplayInfo;
        Dictionary<String, RepairInfoInternal> repairInfo;

        Dictionary<String, RepairItem> repairItems;
        Dictionary<String, RepairPart> repairParts;

        public void reloadUploadStatus()
        {
            //  this.listView1.Items.Clear();
            //得到日期

            dataGridView1.Rows.Clear();
            String gdSn = textBox1.Text.Trim();
            String carNo = textBox2.Text.Trim();

            string settle_dt = this.dateTimePicker1.Text;

            this.getCarInfo(settle_dt, out carUploadInfo, out carDisplayInfo, out repairInfo);


            if (carDisplayInfo.Count == 0)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("没有查到任何记录", "提示", MessageBoxButtons.OK);
                return;
            }

            repairItems = getRepairItemDetail(carDisplayInfo);
            repairParts = getRepairPartsDetail(carDisplayInfo);

            this.Cursor = Cursors.WaitCursor;

            foreach (var item in carDisplayInfo.Values)
            {
                int index = this.dataGridView1.Rows.Add();

                if (item.upload_status == 1)
                {
                    this.dataGridView1.Rows[index].DefaultCellStyle.BackColor = Color.FromArgb(186, 201, 236);
                }
                this.dataGridView1.Rows[index].Cells[1].Value = item.gd_sn;
                this.dataGridView1.Rows[index].Cells[2].Value = item.incomingInspectionId;
                this.dataGridView1.Rows[index].Cells[3].Value = item.car_no;
                this.dataGridView1.Rows[index].Cells[4].Value = item.customer_name;
                this.dataGridView1.Rows[index].Cells[5].Value = item.vin_code;
                this.dataGridView1.Rows[index].Cells[6].Value = item.error_desc;
                this.dataGridView1.Rows[index].Cells[7].Value = item.gd_id;
            }
            dataGridView1.ClearSelection();

            this.Cursor = Cursors.Default;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length==0 && textBox2.Text.Trim().Length == 0)
            {
                MessageBox.Show("工单号或者车牌号不能同时为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return;
            }

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
            this.getCompanyIdentity();
            if (StringUtil.isEmpty(GlobalData.companyIdentity))
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
