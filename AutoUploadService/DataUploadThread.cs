using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using com.rongtong.car;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Configuration;

namespace AutoUploadService
{
    public class SignInfo
    {
        public String appId;
        public String sign;
        public long nonce;
        public String timestamp;
        public static Random rd = new Random();
        public SignInfo()
        {

            this.appId = ConfigItem.appId;
            this.nonce = rd.Next();
            this.timestamp = TimeUtil.getCurrentMillSeconds().ToString();
        }
    }
    //接车信息
    public class PickCarInfo : SignInfo
    {
        public String vin { get; set; }
        public String vpn { get; set; }
        public String orderCode { get; set; }
        public String repairTime { get; set; }
        public String repairMileage { get; set; }
        public String serviceType { get; set; }
        public String fuelType { get; set; }
        public String requirement { get; set; }
        public String faultDesc { get; set; }
        public int natureOfUse { get; set; }
        public String carPhoto { get; set; }
        public String drivingPermitPhoto { get; set; }

        public PickCarInfo()
        {
            natureOfUse = 1;
            requirement = "";
            carPhoto = "";
            drivingPermitPhoto = "";
        }
    }

    //配件信息上传
    public class RepairItem
    {
        public String partName { get; set; }
        public String brandName { get; set; }
        public String partType { get; set; }
        public String partCode { get; set; }
        public Double partQty { get; set; }
        public String unit { get; set; }

        public RepairItem()
        {
            brandName = "";
            partType = "";
        }
    }
    public class RepairProject
    {
        public String projectName { get; set; }
        public String projectType { get; set; }
        public Double workingHours { get; set; }


        public override bool Equals(object obj)
        {
            var stu = obj as RepairProject;
            if (stu == null) return false;
            return projectName == stu.projectName;
        }
        public override int GetHashCode()
        {
            return projectName.GetHashCode();
        }

        public List<RepairItem> repairPartList { get; set; }
        public RepairProject()
        {
            projectType = "";
            workingHours = 0.0;
            repairPartList = new List<RepairItem>();
        }
    }

    public class RepairItemInfo : SignInfo
    {
        public String orderCode { get; set; }
        public List<RepairProject> repairProjectList { get; set; }

        public RepairItemInfo()
        {
            repairProjectList = new List<RepairProject>();
        }
    }

    //结算信息
    public class SettleInfo : SignInfo
    {
        public String orderCode { get; set; }
        public Decimal totalCost { get; set; }
        public String payTime { get; set; }

        public SettleInfo()
        {
            totalCost = 0;
        }
    }


    public class UpdateRepairInfoByOrderCode:SignInfo
    {
        public String orderCode { get; set; }
        public String vin { get; set; }
        public String vpn { get; set; }
        public String repairTime { get; set; }
        public String payTime { get; set; }
        public List<RepairProject> repairProjectList { get; set; }


        public UpdateRepairInfoByOrderCode(PickCarInfo pickCarInfo, RepairItemInfo repairItemInfo, SettleInfo settleInfo)
        {
            if (pickCarInfo != null)
            {
                orderCode = pickCarInfo.orderCode;
                vin = pickCarInfo.vin;
                vpn = pickCarInfo.vpn;
                repairTime = pickCarInfo.repairTime;                
            }
            if (settleInfo != null)
            {
                payTime = settleInfo.payTime;
            }
            if (repairItemInfo != null)
            {
                repairProjectList = repairItemInfo.repairProjectList;
            }
        }
    }


    //上传维修信息
    public class RepairItemDetail : SignInfo
    {
        public String itemSeq { get; set; }
        public String itemName { get; set; }
        public String settlementTime { get; set; }
        public String workHoursUnitPrice { get; set; }
        public String workHoursFee { get; set; }

        public RepairItemDetail()
        {
            itemSeq = "1";
            itemName = "";
            settlementTime = "";
            workHoursUnitPrice = "";
            workHoursFee = "";
        }

    }

    public class CompanyErrRepairRecord : SignInfo
    {
        public String vpn { get; set; }
        public String repairTime { get; set; }
        public String projectType { get; set; }
        public String unit { get; set; }
        public String partCode { get; set; }
        public String partName { get; set; }
        public String partType { get; set; }
        public Double partQty { get; set; }
        public String projectName { get; set; }
        public String workingHours { get; set; }
        public String vin { get; set; }
        public String orderCode { get; set; }

        public CompanyErrRepairRecord()
        {
            projectType = "";
            partType = "";
        }
    }


    public class ValidatorRet
    {
        public Boolean checkResult;
        public String error_msg;

        public ValidatorRet()
        {
            this.checkResult = true;
            this.error_msg = "";
        }

        public ValidatorRet(Boolean retValue, String error_msg)
        {
            this.checkResult = retValue;
            this.error_msg = error_msg;
        }
    }



    public class Response
    {
        public int sign { get; set; }
        public String message { get; set; }
        public String data { get; set; }
    }

    public class CommonResponse
    {
        public int code { get; set; }
        public String message { get; set; }
        public String data { get; set; }
    }

    //界面显示字段信息


    public class GDCardInfo
    {
        public String gd_id { get; set; }
        public SignInfo signInfo { get; set; }
        public RepairItem repairItem { get; set; }
        // public RepairInfoInternal repairPair { get; set; }
    }


    public class DataUpload
    {
        public ConfigItem configItem;
        private string connStr;
        private SqlConnection sqlLantuConn, sqlRongtoneConn;

        private Dictionary<String, String> carOidMapping = new Dictionary<string, string>();

        private SqlConnection getConnection()
        {
            if (sqlLantuConn == null || sqlLantuConn.State == System.Data.ConnectionState.Closed)
            {
                sqlLantuConn = new SqlConnection(connStr);
            }

            return sqlLantuConn;
        }

        private SqlConnection getRongtoneConnection()
        {
            if (sqlRongtoneConn == null || sqlRongtoneConn.State == System.Data.ConnectionState.Closed)
            {
                sqlRongtoneConn = new SqlConnection(connStr);
            }

            return sqlRongtoneConn;
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
                LogHelper.WriteLog(typeof(DataUpload), ex);
            }
            finally
            {
                dbCon.Close();
            }
        }

        private void updateUploadGdStatus(string gd_sn, string upload_type, string req, string resp)
        {
            JObject jobject = (JObject)JsonConvert.DeserializeObject(resp);
            String is_uploaded = jobject["code"].ToString();
            if (is_uploaded.Equals("0"))
            {
                is_uploaded = "1";
            }

            int len = req.Length;
            if (len >= 3000)
            {
                len = 3000;
            }
            LogHelper.WriteLog(typeof(DataUpload), String.Format("req length:{0}, len:{1}", req.Length, len));
            req = req.Substring(0, len);
            string sql = string.Format("update dataupload_gd set is_uploaded={4}, request_str='{0}' , response_str='{1}', upload_time=getdate() where gd_sn='{2}' and upload_type='{3}'", req, resp, gd_sn, upload_type, is_uploaded);
           // LogHelper.WriteLog(typeof(GlobalData), sql);
            dbExecNoReturn(sql);
        }

        public void getOpenApi()
        {
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
                                ConfigItem.appId = value;
                            }
                            else if (name.Equals("secret"))
                            {
                                ConfigItem.secret = value;
                            }
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(DataUpload), ex);
            }
            finally
            {
                dbCon.Close();
            }

            LogHelper.WriteLog(typeof(DataUpload), string.Format("appid:{0}, secret:{1}", ConfigItem.appId, ConfigItem.secret));
        }

        private bool isTriggerExists(string trigger_name)
        {
            string sql = string.Format("select count(*) cnt from sysobjects where id=object_id(N'{0}') and objectproperty(object_id('{0}'),N'IsTrigger')=1", trigger_name);
            SqlConnection dbCon = getConnection();
            int cnt = 0;
            using (SqlCommand cmd = dbCon.CreateCommand())
            {
                // string date_str = DateTime.Now.ToString("yyyy-MM-dd");
                //string sql = String.Format("select * from dataupload_gd where is_uploaded=0 and create_time>='{0} 00:00:00'", date_str);
                //Console.WriteLine("sql:" + sql);
                cmd.CommandText = sql;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string cnt1 = getTrimString(reader, "cnt", "0");
                        cnt = Convert.ToInt32(cnt1);
                        break;
                    }
                }
            }
            return cnt > 0;
        }

        private void intialTableSchema()
        {
            SqlConnection dbCon = getConnection();
            try
            {
                dbCon.Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = dbCon;
                sqlcmd.CommandTimeout = configItem.cmdTimeOut;
                string createsql = "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='dataupload_gd' AND xtype='U') CREATE TABLE [dbo].[dataupload_gd] ([id] [int] IDENTITY (1, 1) NOT NULL , [gd_id] [int] NOT NULL , [gd_sn] [nchar](32) COLLATE Chinese_PRC_CI_AS NOT NULL,[upload_type] [nchar](32) COLLATE Chinese_PRC_CI_AS NOT NULL,[cl_id] [int] NOT NULL ,[settle_dt] [datetime] NULL ,	[is_uploaded] [int] NULL ,[request_str] [nvarchar](3000) COLLATE Chinese_PRC_CI_AS NULL ,	[response_str] [nchar] (200) COLLATE Chinese_PRC_CI_AS NULL ,	[create_time] [datetime] default getdate(), [upload_time] [datetime] default getdate() )";
                sqlcmd.CommandText = createsql;
                sqlcmd.ExecuteNonQuery();
                createsql = "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='dataupload_gd_history' AND xtype='U') CREATE TABLE [dbo].[dataupload_gd_history] ([id] [int] IDENTITY (1, 1) NOT NULL , [gd_id] [int] NOT NULL , [gd_sn] [nchar](32) COLLATE Chinese_PRC_CI_AS NOT NULL,[upload_type] [nchar](32) COLLATE Chinese_PRC_CI_AS NOT NULL,[cl_id] [int] NOT NULL ,[settle_dt] [datetime] NULL ,	[is_uploaded] [int] NULL ,[request_str] [nvarchar](3000) COLLATE Chinese_PRC_CI_AS NULL ,	[response_str] [nchar] (200) COLLATE Chinese_PRC_CI_AS NULL ,	[create_time] [datetime] default getdate(), [upload_time] [datetime] default getdate() )";
                sqlcmd.CommandText = createsql;
                sqlcmd.ExecuteNonQuery();
                string updateSql = "IF COL_LENGTH('DT_OM_GD', 'UPLOAD_STATUS') IS NULL ALTER TABLE DT_OM_GD ADD UPLOAD_STATUS INT DEFAULT 0 WITH VALUES";
                sqlcmd.CommandText = updateSql;
                sqlcmd.ExecuteNonQuery();
                //generate trigger
                //updateSql = "if(OBJECT_ID('dataupload_gd_insert') is not null) drop trigger dataupload_gd_insert";
                //sqlcmd.CommandText = updateSql;
                //sqlcmd.ExecuteNonQuery();
                if (!isTriggerExists("dataupload_gd_insert"))
                {
                    updateSql = "CREATE TRIGGER dataupload_gd_insert ON dataupload_gd AFTER INSERT, UPDATE AS declare @is_uploaded int; declare @gd_id int; declare @gd_sn nchar(32); declare @upload_type nchar(32); declare @cl_id int; declare @request_str nvarchar(3000); declare @response_str nchar(200); declare @settle_dt datetime;  BEGIN select @is_uploaded=is_uploaded, @settle_dt=settle_dt, @gd_id=gd_id, @gd_sn=gd_sn, @cl_id=cl_id,@request_str=request_str, @response_str=response_str, @upload_type=upload_type from inserted; if update(is_uploaded) and @is_uploaded>0 begin 	insert into dataupload_gd_history(gd_id, gd_sn, is_uploaded, upload_type, cl_id, settle_dt, request_str, response_str, create_time, upload_time) values(@is_uploaded, @gd_sn, @is_uploaded, @upload_type, @cl_id, @settle_dt, @request_str, @response_str, getdate(),getdate()); end END";
                    sqlcmd.CommandText = updateSql;
                    sqlcmd.ExecuteNonQuery();
                }
                //updateSql = "if(OBJECT_ID('trigger_dt_om_gd_Insert') is not null) drop trigger trigger_dt_om_gd_Insert";
                //sqlcmd.CommandText = updateSql;
                //sqlcmd.ExecuteNonQuery();
                //updateSql = "create trigger trigger_dt_om_gd_Insert  on dt_om_gd after insert as declare @gd_id int; declare @cl_id int; declare @is_uploaded int; declare @cnt int; declare @gd_sn nchar(32); DECLARE @vin_code nchar(17); begin select @cl_id=cl_id, @gd_id=gd_id, @gd_sn=gd_sn from inserted; select @vin_code=ltrim(rtrim(vin_code)) from mt_cl where cl_id=@cl_id; if len(@vin_code)=17 begin set @cnt=0; select @cnt=count(id) from dataupload_gd where gd_id=@gd_id and ltrim(rtrim(upload_type))='pick'; select  @is_uploaded=is_uploaded from dataupload_gd where gd_id=@gd_id and ltrim(rtrim(upload_type))='pick';if (@cnt=0) begin insert into dataupload_gd(gd_id, gd_sn, cl_id, upload_type, is_uploaded, create_time) values(@gd_id, @gd_sn, @cl_id, 'pick', 0, getdate()); end else if (@is_uploaded>1) begin update dataupload_gd set is_uploaded=0 where gd_id=@gd_id and ltrim(rtrim(upload_type))='pick'; end end end";

                if (!isTriggerExists("trigger_dt_om_gd_Insert"))
                {
                    updateSql = "create trigger trigger_dt_om_gd_Insert  on dt_om_gd after insert as declare @gd_id int; declare @cl_id int; declare @is_uploaded int; declare @cnt int; declare @gd_sn nchar(32); DECLARE @vin_code nchar(17); begin select @cl_id=cl_id, @gd_id=gd_id, @gd_sn=gd_sn from inserted; select @vin_code=ltrim(rtrim(vin_code)) from mt_cl where cl_id=@cl_id; if len(@vin_code)=17 begin set @cnt=0; select @cnt=count(id) from dataupload_gd where gd_id=@gd_id and ltrim(rtrim(upload_type))='pick'; select  @is_uploaded=is_uploaded from dataupload_gd where gd_id=@gd_id and ltrim(rtrim(upload_type))='pick';if (@cnt=0) begin insert into dataupload_gd(gd_id, gd_sn, cl_id, upload_type, is_uploaded, create_time) values(@gd_id, @gd_sn, @cl_id, 'pick', 0, getdate()); end else if (@is_uploaded>1) begin update dataupload_gd set is_uploaded=0 where gd_id=@gd_id and ltrim(rtrim(upload_type))='pick'; end end end";
                    sqlcmd.CommandText = updateSql;
                    sqlcmd.ExecuteNonQuery();
                }
                //updateSql = "if(OBJECT_ID('trigger_dt_om_gd_Update') is not null) drop  trigger trigger_dt_om_gd_Update";
                //sqlcmd.CommandText = updateSql;
                //sqlcmd.ExecuteNonQuery();
                if (!isTriggerExists("trigger_dt_om_gd_Update"))
                {
                    updateSql = @"create trigger trigger_dt_om_gd_Update  on dt_om_gd 
                                    after update as 
                                    declare @gd_id int; 
                                    declare @cl_id int; 
                                    declare @gd_sn nchar(32); 
                                    declare @vin_code nchar(17); 
                                    declare @is_settle_deleted int; 
                                    declare @is_settle_inserted int; 
                                    declare @is_finish_deleted int; 
                                    declare @is_finish_inserted int; 
                                    declare @cnt int; 
                                    declare @finish_dt datetime; 
                                    declare @settle_dt datetime; 
                                    begin 
                                        select @cl_id=cl_id, @gd_id=gd_id, @gd_sn=gd_sn, @is_settle_deleted=is_settle, @is_finish_deleted=finish, @finish_dt=finish_dt, @settle_dt=settle_dt from deleted; 
                                        select @cl_id=cl_id, @gd_id=gd_id, @gd_sn=gd_sn, @is_settle_inserted=is_settle, @is_finish_inserted=finish, @finish_dt=finish_dt, @settle_dt=settle_dt from inserted; 
                                        select @vin_code=ltrim(rtrim(vin_code)) from mt_cl where cl_id=@cl_id; if (len(@vin_code)=17) 
                                        begin 
                                            if (@is_finish_deleted=0 and @is_finish_inserted=1) 
                                                begin 
                                                    select @cnt=count(gd_id) from dataupload_gd where gd_id=@gd_id and upload_type in ('repairinfo'); 
                                                    if (@cnt=0) 
                                                        begin 
                                                            insert into dataupload_gd(gd_id, gd_sn, cl_id, upload_type, is_uploaded, create_time) values(@gd_id, @gd_sn, @cl_id, 'repairinfo', 0, getdate()); 
                                                        end 
                                                end 
                                                    if (@is_settle_deleted=0 and @is_settle_inserted=1) 
                                                        begin 
                                                            select @cnt=count(gd_id) from dataupload_gd where gd_id=@gd_id and upload_type in ('settle'); 
                                                            if (@cnt=0) 
                                                                begin 
                                                                    insert into dataupload_gd(gd_id, gd_sn, cl_id, upload_type, is_uploaded, create_time) values(@gd_id, @gd_sn, @cl_id, 'settle', 0, getdate()); 
                                                                end 
                                                            else
                                                                begin
                                                                    insert into dataupload_gd(gd_id, gd_sn, cl_id, upload_type, is_uploaded, create_time) values(@gd_id, @gd_sn, @cl_id, 'updateRepairInfoByOrderCode', 0, getdate()); 
                                                                end
                                                        end 
                                       end 
                                    end";
                    sqlcmd.CommandText = updateSql;
                    sqlcmd.ExecuteNonQuery();
                }
                //updateSql = "if(OBJECT_ID('tr_mt_cl_update_insert') is not null) drop  trigger tr_mt_cl_update_insert";
                //sqlcmd.CommandText = updateSql;
                //sqlcmd.ExecuteNonQuery();
                //updateSql = "CREATE TRIGGER tr_mt_cl_update_insert ON mt_cl AFTER INSERT ,UPDATE AS BEGIN declare @old_vin_code nvarchar(50); DECLARE @vin_code nvarchar(50); DECLARE @cl_id int begin if update(vin_code) begin select @old_vin_code=rtrim(ltrim(vin_code)), @cl_id=cl_id FROM deleted; SELECT @vin_code=rtrim(ltrim(vin_code)), @cl_id=cl_id FROM inserted; if len(@vin_code)=17 and @old_vin_code!=@vin_code begin declare @cnt int; select @cnt=count(id) from dataupload_gd where datediff(day, create_time, getdate())<=3 and cl_id=@cl_id and is_uploaded>1; if (@cnt>0) begin update dataupload_gd set is_uploaded=0 where datediff(day, getdate(), create_time)<=3 and cl_id=@cl_id; end else begin insert into dataupload_gd(gd_id, gd_sn, cl_id,upload_type) select gd_id, gd_sn, cl_id, 'pick' from dt_om_gd where datediff(day, in_dt, getdate())<=3 and cl_id=@cl_id; insert into dataupload_gd(gd_id, gd_sn, cl_id,upload_type) select gd_id, gd_sn, cl_id, 'repairinfo' from dt_om_gd where datediff(day, in_dt, getdate())<=3 and cl_id=@cl_id and is_settle=1; insert into dataupload_gd(gd_id, gd_sn, cl_id,upload_type) select gd_id, gd_sn, cl_id, 'settle' from dt_om_gd where datediff(day, in_dt, getdate())<=3 and cl_id=@cl_id and is_settle=1; end end end end end";
                if (!isTriggerExists("tr_mt_cl_update_insert"))
                {
                    updateSql = @"CREATE TRIGGER tr_mt_cl_update_insert 
                                ON mt_cl AFTER INSERT ,UPDATE 
                                AS 
                                BEGIN 
	                                declare @old_vin_code nvarchar(50); 
	                                DECLARE @vin_code nvarchar(50); 
	                                DECLARE @cl_id int;
	                                begin 
		                                if update(vin_code) 
		                                begin 
			                                select @old_vin_code=rtrim(ltrim(vin_code)), @cl_id=cl_id FROM deleted; 
			                                SELECT @vin_code=rtrim(ltrim(vin_code)), @cl_id=cl_id FROM inserted; 
			                                if len(@vin_code)=17 and @old_vin_code!=@vin_code 
				                                begin 
					                                declare @cnt int; 
					                                select @cnt=count(id) from dataupload_gd where datediff(day, create_time, getdate())<=3 and cl_id=@cl_id and is_uploaded=1; 
					                                if (@cnt>0) 
						                                begin 
							                                update dataupload_gd set is_uploaded=0 where datediff(day, getdate(), create_time)<=3 and cl_id=@cl_id; 
						                                end 
					                                else 
						                                begin 
							                                insert into dataupload_gd(gd_id, gd_sn, cl_id,upload_type) select gd_id, gd_sn, cl_id, 'pick' from dt_om_gd where datediff(day, in_dt, getdate())<=3 and cl_id=@cl_id; 
							                                insert into dataupload_gd(gd_id, gd_sn, cl_id,upload_type) select gd_id, gd_sn, cl_id, 'repairinfo' from dt_om_gd where datediff(day, finish_dt, getdate())<=3 and cl_id=@cl_id and is_settle=0; 
							                                insert into dataupload_gd(gd_id, gd_sn, cl_id,upload_type) select gd_id, gd_sn, cl_id, 'settle' from dt_om_gd where datediff(day, settle_dt, getdate())<=3 and cl_id=@cl_id and is_settle=1; 
						                                end 
				                                end 
		                                end 
	                                end 
                                end";
                    sqlcmd.CommandText = updateSql;
                    sqlcmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(DataUpload), ex);
            }
            finally
            {
                dbCon.Close();
            }
        }

        private void initGlobalData()
        {
            getCompanyInfo();
            GlobalData.cpu_id = IpUtil.getCpuSn();
            GlobalData.harddisk_id = IpUtil.getHardDiskId();
            GlobalData.mac_addr = IpUtil.getMacAddr();
            GlobalData.mainboard_id = IpUtil.getMainBoardId();
            GlobalData.local_ip = IpUtil.getLocalIp();
            GlobalData.wan_ip = IpUtil.GetInterNetIPFromAPI();
        }

        public DataUpload(ConfigItem configItem)
        {
            this.configItem = configItem;
            this.connStr = String.Format("server={0};database={1};uid={2};pwd={3}", configItem.DBHost,
                                configItem.DBName, configItem.DBUser, configItem.DBPassword);

            intialTableSchema();

            getOpenApi();

            initGlobalData();

            carOidMapping["0"] = "A";
            carOidMapping["1"] = "A";
            carOidMapping["2"] = "B";
            carOidMapping["4"] = "F";
            carOidMapping["5"] = "E";
            carOidMapping["20"] = "C"; //电
        }

        public static string PICK = "pick";
        public static string REPAIRINFO = "repairinfo";
        public static string SETTLE = "settle";
        public static string UPDATE_REPAIRINFO_BY_ORDERCODE = "updateRepairInfoByOrderCode";

        public class UploadItem
        {
            public string id;
            public string gd_id;
            public string gd_sn;
            public string cl_id;
            public string upload_type; // pick 接车信息  settle：结算
        }

        public string getTimeString(SqlDataReader sdr, string key, string defaultValue)
        {
            int columnId = sdr.GetOrdinal(key);
            return sdr.GetDateTime(columnId).ToString("yyyy-MM-dd HH:mm:ss");
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

        public List<UploadItem> getUploadItemList()
        {
            List<UploadItem> uploadItemList = new List<UploadItem>();
            SqlConnection dbCon = getConnection();
            try
            {
                dbCon.Open();
                using (SqlCommand cmd = dbCon.CreateCommand())
                {
                    // string date_str = DateTime.Now.ToString("yyyy-MM-dd");
                    //string sql = String.Format("select * from dataupload_gd where is_uploaded=0 and create_time>='{0} 00:00:00'", date_str);
                    string sql = String.Format("select * from dataupload_gd where (is_uploaded=0 and upload_type='pick') or (upload_type in ('settle','repairinfo','updateRepairInfoByOrderCode') and is_uploaded=0 and gd_id in (select gd_id from dataupload_gd where is_uploaded=1 and upload_type='pick')) order by id asc");
                    //Console.WriteLine("sql:" + sql);
                    cmd.CommandText = sql;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UploadItem uploadItem = new UploadItem();
                            uploadItem.id = getTrimString(reader, "id", "");
                            uploadItem.gd_id = getTrimString(reader, "gd_id", "");
                            uploadItem.gd_sn = getTrimString(reader, "gd_sn", "");
                            uploadItem.cl_id = getTrimString(reader, "cl_id", "");
                            uploadItem.upload_type = getTrimString(reader, "upload_type", "");
                            uploadItemList.Add(uploadItem);
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(DataUpload), ex);
            }
            finally
            {
                dbCon.Close();
            }


            return uploadItemList;
        }

        public List<string> getUploadedItems(List<UploadItem> uploadItemList, string type)
        {
            List<string> gdIds = new List<string>();
            foreach (var item in uploadItemList)
            {
                if (item.upload_type.Equals(type))
                {
                    gdIds.Add(item.gd_id);
                }

            }
            return gdIds;
        }

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

        private void getPickCarInfos(List<string> pickGdIds,
            out Dictionary<string, PickCarInfo> pickCarInfo
            )
        {
            pickCarInfo = new Dictionary<string, PickCarInfo>();
            if (pickGdIds.Count > 0)
            {

                string gdIds = string.Join(",", pickGdIds.ToArray());
                SqlConnection dbCon = getConnection();
                try
                {
                    dbCon.Open();
                    SqlCommand sqlcmd = new SqlCommand();
                    string sql = string.Format("select t1.gd_sn,t1.upload_status,t1.settle_dt, t1.gd_id, t2.vin_code, t2.car_no,t2.regist_no, t1.in_dt, t3.miles, t4.gd_nm, t2.car_oil, t3.error_desp, t5.cust_nm from dt_om_gd t1 left join mt_cl t2 on t1.cl_id = t2.cl_id left join DT_OM_JJJC t3 on t1.gd_id = t3.gd_id left join mt_gdfl t4 on t1.gd_type_id = t4.id left join MT_KH t5 on t1.kh_id=t5.kh_id where t1.gd_id in ({0}) and len(ltrim(rtrim(t2.vin_code)))=17", gdIds);
                    // string sql = string.Format("select t1.gd_sn,t1.upload_status,t1.settle_dt, t1.gd_id, t2.vin_code, t2.car_no, t1.in_dt, t3.miles, t4.gd_nm, t2.car_oil, t3.error_desp, t5.cust_nm from dt_om_gd t1 left join mt_cl t2 on t1.cl_id = t2.cl_id left join DT_OM_JJJC t3 on t1.gd_id = t3.gd_id left join mt_gdfl t4 on t1.gd_type_id = t4.id left join MT_KH t5 on t1.kh_id=t5.kh_id where t1.gd_id in ({0}) ", gdIds);
                    sqlcmd.CommandText = sql;
                    sqlcmd.Connection = dbCon;

                    SqlDataReader sqlDataReader = sqlcmd.ExecuteReader();
                    while (sqlDataReader.Read())
                    {
                        string gd_sn = getTrimString(sqlDataReader, "gd_sn", "");

                        PickCarInfo tmpPickCarInfo;


                        pickCarInfo.TryGetValue(gd_sn, out tmpPickCarInfo);

                        if (tmpPickCarInfo == null)
                        {
                            tmpPickCarInfo = new PickCarInfo();
                            pickCarInfo[gd_sn] = tmpPickCarInfo;
                        }

                        //接车信息
                        tmpPickCarInfo.vin = getTrimString(sqlDataReader, "vin_code", "");
                        tmpPickCarInfo.vpn = getTrimString(sqlDataReader, "car_no", "").Replace("-", "").Replace("－", "");
                        tmpPickCarInfo.orderCode = getTrimString(sqlDataReader, "gd_sn", "");
                        tmpPickCarInfo.repairTime = getTimeString(sqlDataReader, "in_dt", "");
                        tmpPickCarInfo.repairMileage = getTrimString(sqlDataReader, "miles", "");
                        tmpPickCarInfo.serviceType = getTrimString(sqlDataReader, "gd_nm", "");
                        tmpPickCarInfo.fuelType = getOilType(getTrimString(sqlDataReader, "car_oil", ""));
                        tmpPickCarInfo.faultDesc = getTrimString(sqlDataReader, "error_desp", "小修");

                        //2023-02-23 由原来的默认为 1 修改为: 判断
                       // MT CL.REGIST NO 的值是否为空，不为空取 1，为空取 2) REGIST NO 为软件车辆中的营运证号码 +
                        string regist_no = getTrimString(sqlDataReader, "regist_no", "");
                        tmpPickCarInfo.natureOfUse = regist_no.Length > 0 ? 1 : 2;

                    }

                    sqlDataReader.Close();

                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(typeof(DataUpload), ex);
                }
                finally
                {
                    dbCon.Close();
                }
            }

        }
        private void uploadPickCarInfos(List<string> pickGdIds)
        {
            if (pickGdIds.Count > 0)
            {
                try
                {
                    Dictionary<string, PickCarInfo> pickCarInfo;
                    getPickCarInfos(pickGdIds, out pickCarInfo);
                    //upload pickup info
                    foreach (var item in pickCarInfo)
                    {
                        string json = JsonConvert.SerializeObject(item.Value);
                        json = json.Replace('%', '％');
                        LogHelper.WriteLog(typeof(DataUpload), string.Format("before encrypt, pick request:{0}", json));
                        json = SignUtil.sign(PICK, json, ConfigItem.secret);

                        String url = configItem.serverHost + URL.PICKUP_CAR_URL;
                        LogHelper.WriteLog(typeof(DataUpload), string.Format("pick request url:{0}, req:{1}", url, json));
                        var restApiClient = new RestApiClient(url, HttpVerbNew.POST, ContentType.JSON, json);
                        string response = restApiClient.MakeRequest();
                        Console.WriteLine("pick response:" + response);
                        LogHelper.WriteLog(typeof(DataUpload), string.Format("pick response:{0}", response));
                        updateUploadGdStatus(item.Value.orderCode, PICK, json, response);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(typeof(DataUpload), ex);
                }
            }
        }

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


        private void getRepairItemAndSettleInfo(List<string> settleGdIds,
            out Dictionary<String, RepairItemInfo> repairItemInfos,
            out Dictionary<string, SettleInfo> settleInfos)
        {
            repairItemInfos = new Dictionary<String, RepairItemInfo>();
            settleInfos = new Dictionary<string, SettleInfo>();

            if (settleGdIds.Count > 0)
            {
                string gdIds = string.Join(",", settleGdIds.ToArray());
                SqlConnection dbCon = null;
                try
                {
                    dbCon = getConnection();

                    dbCon.Open();
                    SqlCommand sqlcmd = new SqlCommand();
                    //    string sql = String.Format("select A.GD_ID, E.GD_SN,E.settle_dt, A.PRJ_NM as projectName,  A.man_hour as workingHours, c.PART_NM as partName, D.ORIGINAL_FACTORY_ID as partCode,  C.QTY as partQty, D.unit as unit from DT_OM_BXXM A join DT_EM_CKD  B ON A.GD_ID =B.RELATIVE_ID left JOIN DT_EM_CKLJ C ON B.OUTPUT_ID=C.OUTPUT_ID AND A.baoxiu_id=c.output_part_id LEFT JOIN DT_EM_LJML D ON C.PART_ID=D.PART_ID join dt_om_gd e on a.gd_id=e.gd_id JOIN MT_CL f on e.cl_id=f.cl_id where E.gd_id in ({0})  ", gdIds);
                    string sql = String.Format("select A.GD_ID, E.GD_SN,E.settle_dt, A.PRJ_NM as projectName,  A.man_hour as workingHours, c.PART_NM as partName, D.ORIGINAL_FACTORY_ID as partCode,  C.QTY as partQty, D.unit as unit from DT_OM_BXXM A join DT_EM_CKD  B ON A.GD_ID =B.RELATIVE_ID left JOIN DT_EM_CKLJ C ON B.OUTPUT_ID=C.OUTPUT_ID  LEFT JOIN DT_EM_LJML D ON C.PART_ID=D.PART_ID join dt_om_gd e on a.gd_id=e.gd_id JOIN MT_CL f on e.cl_id=f.cl_id where E.gd_id in ({0})  ", gdIds);
                    sqlcmd.CommandText = sql;
                    sqlcmd.Connection = dbCon;

                    SqlDataReader sqlDataReader = sqlcmd.ExecuteReader();

                    var repairProjects = new Dictionary<String, Dictionary<RepairProject, List<RepairItem>>>();
                    while (sqlDataReader.Read())
                    {
                        string gd_sn = getTrimString(sqlDataReader, "gd_sn", "");

                        SettleInfo tmpSettleInfo;
                        settleInfos.TryGetValue(gd_sn, out tmpSettleInfo);
                        if (tmpSettleInfo == null)
                        {
                            tmpSettleInfo = new SettleInfo();
                            settleInfos[gd_sn] = tmpSettleInfo;
                        }

                        string settle_dt = getTimeString(sqlDataReader, "settle_dt", "").Replace("/", "-");
                        tmpSettleInfo.orderCode = gd_sn;
                        tmpSettleInfo.payTime = settle_dt;

                        Dictionary<RepairProject, List<RepairItem>> projects;
                        repairProjects.TryGetValue(gd_sn, out projects);
                        if (projects == null)
                        {
                            projects = new Dictionary<RepairProject, List<RepairItem>>();
                            repairProjects[gd_sn] = projects;
                        }

                        RepairProject repairProject = new RepairProject();

                        repairProject.projectName = getTrimString(sqlDataReader, "projectName", "");
                        double value = 0;
                        Double.TryParse(getTrimString(sqlDataReader, "workingHours", "0"), out value);
                        repairProject.workingHours = value;

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
                            //repairItem.partQty = getTrimString(sqlDataReader, "partQty", "");
                            Double partQty = Convert.ToDouble(getTrimString(sqlDataReader, "partQty", "1"));

                            repairItem.partQty = partQty;
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
                            //if (item1.Value != null && item1.Value.Count > 0)
                            if (item1.Value != null && item1.Value.Count > 0 && repairItemInfo.repairProjectList.Count == 1)
                            {
                                repairItemInfo.repairProjectList[repairItemInfo.repairProjectList.Count - 1].repairPartList = item1.Value;
                            }
                        }
                        repairItemInfos[item.Key] = repairItemInfo;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(typeof(DataUpload), ex);
                }
                finally
                {
                    if (dbCon != null)
                    {
                        dbCon.Close();
                    }
                }
            }
        }

        private void uploadRepairItemInfos(Dictionary<String, RepairItemInfo> repairItemInfos)
        {
            foreach (var item in repairItemInfos)
            {

                string json = JsonConvert.SerializeObject(item.Value);
                json = json.Replace('%', '％');
                LogHelper.WriteLog(typeof(DataUpload), string.Format("before encrption, repairinfo request req:{0}", json));
                json = SignUtil.sign(REPAIRINFO, json, ConfigItem.secret);

                String url = configItem.serverHost + URL.REPAIR_ITEM_URL;
                LogHelper.WriteLog(typeof(DataUpload), string.Format("repairinfo request url:{0}, req:{1}", url, json));
                var restApiClient = new RestApiClient(url, HttpVerbNew.POST, ContentType.JSON, json);
                string response = restApiClient.MakeRequest();

                Console.WriteLine("repairinfo response:" + response);
                LogHelper.WriteLog(typeof(DataUpload), "repairinfo response:" + response);
                updateUploadGdStatus(item.Value.orderCode, REPAIRINFO, json, response);
            }
        }
        private void uploadSettleInfos(Dictionary<string, SettleInfo> settleInfos)
        {
            foreach (var item in settleInfos)
            {
                JsonSerializerSettings setting = new JsonSerializerSettings();
                setting.NullValueHandling = NullValueHandling.Ignore;
                string json = JsonConvert.SerializeObject(item.Value, Formatting.None, setting);
                json = json.Replace('%', '％');
                LogHelper.WriteLog(typeof(DataUpload), string.Format("before encrption, settle request req:{0}", json));
                json = SignUtil.sign(SETTLE, json, ConfigItem.secret);
                String url = configItem.serverHost + URL.SETTLE_INFO_URL;
                LogHelper.WriteLog(typeof(DataUpload), string.Format("settle request url:{0}, req:", url, json));
                var restApiClient = new RestApiClient(url, HttpVerbNew.POST, ContentType.JSON, json);
                string response = restApiClient.MakeRequest();

                LogHelper.WriteLog(typeof(DataUpload), "settle response:" + response);
                updateUploadGdStatus(item.Value.orderCode, SETTLE, json, response);
            }
        }

        //更新错误信息上传接口
        private void uploadUpdateRepairInfoByOrderCodes(Dictionary<string, UpdateRepairInfoByOrderCode> updateRepairInfoByOrderCode)
        {
            foreach (var item in updateRepairInfoByOrderCode)
            {
                JsonSerializerSettings setting = new JsonSerializerSettings();
                setting.NullValueHandling = NullValueHandling.Ignore;
                string json = JsonConvert.SerializeObject(item.Value, Formatting.None, setting);
                json = json.Replace('%', '％');
                LogHelper.WriteLog(typeof(DataUpload), string.Format("before encrption, updateRepairInfoByOrderCode request req:{0}", json));
                json = SignUtil.sign(UPDATE_REPAIRINFO_BY_ORDERCODE, json, ConfigItem.secret);
                String url = configItem.serverHost + URL.REPAIR_INFO_URL;
                LogHelper.WriteLog(typeof(DataUpload), string.Format("updateRepairInfoByOrderCode request url:{0}, req:", url, json));
                var restApiClient = new RestApiClient(url, HttpVerbNew.POST, ContentType.JSON, json);
                string response = restApiClient.MakeRequest();

                LogHelper.WriteLog(typeof(DataUpload), "settle response:" + response);
                updateUploadGdStatus(item.Value.orderCode, SETTLE, json, response);
            }
        }

        private void uploadSettleInfo(List<String> gdIds)
        {
            Dictionary<String, RepairItemInfo> repairItemInfos;
            Dictionary<string, SettleInfo> settleInfos;
            getRepairItemAndSettleInfo(gdIds, out repairItemInfos, out settleInfos);
            uploadSettleInfos(settleInfos);
        }

        private void uploadRepairItem(List<String> gdIds)
        {
            Dictionary<String, RepairItemInfo> repairItemInfos;
            Dictionary<string, SettleInfo> settleInfos;
            getRepairItemAndSettleInfo(gdIds, out repairItemInfos, out settleInfos);
            uploadRepairItemInfos(repairItemInfos);
        }

        private void uploadUpdateRepairInfoByOrderCode(List<String> gdIds)
        {
            Dictionary<string, PickCarInfo> pickCarInfo;
            getPickCarInfos(gdIds, out pickCarInfo);

            Dictionary<String, RepairItemInfo> repairItemInfos;
            Dictionary<string, SettleInfo> settleInfos;
            getRepairItemAndSettleInfo(gdIds, out repairItemInfos, out settleInfos);
            Dictionary<String, UpdateRepairInfoByOrderCode> updateRepairINfoByOrderCodes = new Dictionary<string, UpdateRepairInfoByOrderCode>();

            foreach (var gdId in gdIds)
            {
                UpdateRepairInfoByOrderCode updateRepairINfoByOrderCode = new UpdateRepairInfoByOrderCode(pickCarInfo[gdId], repairItemInfos[gdId], settleInfos[gdId]);
                updateRepairINfoByOrderCodes[gdId] = updateRepairINfoByOrderCode;
            }
            uploadUpdateRepairInfoByOrderCodes(updateRepairINfoByOrderCodes);
        }

        private void getCompanyInfo()
        {
            SqlConnection dbCon = null;
            try
            {
                dbCon = getConnection();

                dbCon.Open();
                SqlCommand sqlcmd = new SqlCommand();
                //    string sql = String.Format("select A.GD_ID, E.GD_SN,E.settle_dt, A.PRJ_NM as projectName,  A.man_hour as workingHours, c.PART_NM as partName, D.ORIGINAL_FACTORY_ID as partCode,  C.QTY as partQty, D.unit as unit from DT_OM_BXXM A join DT_EM_CKD  B ON A.GD_ID =B.RELATIVE_ID left JOIN DT_EM_CKLJ C ON B.OUTPUT_ID=C.OUTPUT_ID AND A.baoxiu_id=c.output_part_id LEFT JOIN DT_EM_LJML D ON C.PART_ID=D.PART_ID join dt_om_gd e on a.gd_id=e.gd_id JOIN MT_CL f on e.cl_id=f.cl_id where E.gd_id in ({0})  ", gdIds);
                string sql = String.Format("SELECT Name,Value  FROM [netmis_en].[dbo].[StringParameter] where [name] in ('公司名称', '地址', '电话1', '传真','单位负责人','companyIdentity' )");
                sqlcmd.CommandText = sql;
                sqlcmd.Connection = dbCon;

                SqlDataReader sqlDataReader = sqlcmd.ExecuteReader();

                Boolean is_bd = false;

                while (sqlDataReader.Read())
                {
                    string name = getTrimString(sqlDataReader, "Name", "");
                    string value = getTrimString(sqlDataReader, "Value", "");
                    if (name != null)
                    {
                        switch (name)
                        {
                            case "公司名称":
                                GlobalData.company_name = value;
                                break;
                            case "地址":
                                GlobalData.addr = value;
                                break;
                            case "电话1":
                                GlobalData.phone = value;
                                break;
                            case "传真":
                                GlobalData.fax = value;
                                break;
                            case "单位负责人":
                                GlobalData.customer_name = value;
                                break;
                            case "companyIdentity":
                                is_bd = true; //is binary dimension
                                break;
                        }
                    }
                }

                GlobalData.is_abd = is_bd;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(DataUpload), ex);
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                }
            }
        }

        private void uploadStaticData(StatInfo statInfo)
        {
            if (statInfo.pick_count == null)
            {
                return;
            }
            string json = JsonConvert.SerializeObject(statInfo);

            String url = "https://www.rongtone.cn/automis/lantu2021/api/uploadStaticInfo/createStaticInfo";
            // LogHelper.WriteLog(typeof(DataUpload), string.Format("pick request url:{0}, req:", url.Replace("www.rongtone.cn", "127.0.0.1"), json));
            var restApiClient = new RestApiClient(url, HttpVerbNew.POST, ContentType.JSON, json);
            string response = restApiClient.MakeRequest();
            //Console.WriteLine("pick response:" + response);
            // LogHelper.WriteLog(typeof(DataUpload), string.Format("pick response:{0}", response));
        }
        private void getYesterdayStaticData(StatInfo statInfo)
        {
            SqlConnection dbCon = null;
            try
            {
                dbCon = getRongtoneConnection();

                dbCon.Open();
                SqlCommand sqlcmd = new SqlCommand();
                //    string sql = String.Format("select A.GD_ID, E.GD_SN,E.settle_dt, A.PRJ_NM as projectName,  A.man_hour as workingHours, c.PART_NM as partName, D.ORIGINAL_FACTORY_ID as partCode,  C.QTY as partQty, D.unit as unit from DT_OM_BXXM A join DT_EM_CKD  B ON A.GD_ID =B.RELATIVE_ID left JOIN DT_EM_CKLJ C ON B.OUTPUT_ID=C.OUTPUT_ID AND A.baoxiu_id=c.output_part_id LEFT JOIN DT_EM_LJML D ON C.PART_ID=D.PART_ID join dt_om_gd e on a.gd_id=e.gd_id JOIN MT_CL f on e.cl_id=f.cl_id where E.gd_id in ({0})  ", gdIds);
                string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                string today = DateTime.Now.AddDays(0).ToString("yyyy-MM-dd");
                statInfo.yesterday = yesterday;
                string sql = String.Format("SELECT upload_type, count(id) cnt from dataupload_gd where is_uploaded>0 and create_time between {0} and {1} group by upload_type", yesterday, today);
                sqlcmd.CommandText = sql;
                sqlcmd.Connection = dbCon;

                SqlDataReader sqlDataReader = sqlcmd.ExecuteReader();

                while (sqlDataReader.Read())
                {
                    string upload_type = getTrimString(sqlDataReader, "upload_type", "");
                    string cnt = getTrimString(sqlDataReader, "cnt", "0");
                    int count = Convert.ToInt32(cnt);

                    if (upload_type != null)
                    {
                        switch (upload_type)
                        {
                            case "pick":
                                statInfo.pick_count = count;
                                break;
                            case "repairinfo":
                                statInfo.finish_count = count;
                                break;
                            case "settle":
                                statInfo.settle_count = count;
                                break;
                        }
                    }
                }

                uploadStaticData(statInfo);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(DataUpload), ex);
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                }
            }

        }
        private Hashtable updateData = new Hashtable();

        //上传统计信息
        public void uploadStatInfo(object param)
        {
            while (true)
            {
                string dtStr = DateTime.Now.ToString("yyyy-MM-dd");
                if (!updateData.ContainsKey(dtStr))
                {
                    updateData.Clear();
                    //获取统计信息
                    StatInfo statInfo = new StatInfo();
                    getYesterdayStaticData(statInfo);
                    updateData.Add(dtStr, "");
                }
                Thread.Sleep(8 * 3600 * 1000);
            }

        }

        public void ParameterRun(object param)
        {
            while (true)
            {
                try
                {
                    //获取记录
                    // LogHelper.WriteLog(typeof(DataUpload), "begin get dataupload record");
                    List<UploadItem> uploadItemList = getUploadItemList();
                    List<string> pickGdIds = getUploadedItems(uploadItemList, PICK);
                    List<string> repairGdIds = getUploadedItems(uploadItemList, REPAIRINFO);
                    List<string> settleGdIds = getUploadedItems(uploadItemList, SETTLE);
                    List<string> updateRepairInfoGdIds = getUploadedItems(uploadItemList, UPDATE_REPAIRINFO_BY_ORDERCODE); 

                    uploadPickCarInfos(pickGdIds);
                    Thread.Sleep(10 * 1000);
                    uploadRepairItem(repairGdIds);
                    Thread.Sleep(10 * 1000);
                    uploadSettleInfo(settleGdIds);
                    Thread.Sleep(10 * 1000);
                    uploadUpdateRepairInfoByOrderCode(updateRepairInfoGdIds);
                }
                catch (System.Exception ex)
                {
                    LogHelper.WriteLog(typeof(DataUpload), ex);
                }
                finally
                {
                }
                Thread.Sleep(10000);
                //判断注册码是否正确， 否则退出
                //string sn = ConfigurationManager.AppSettings["sn"];
                //string realSn = EncryUtil.getEcryptedHardDiskId();
                //if (!sn.Equals(realSn))
                //{
                //    System.Diagnostics.Process.GetCurrentProcess().Kill();
                //}
            }
        }
    }

    //统计信息
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
            this.customer_name = GlobalData.customer_name;
            this.local_ip = GlobalData.local_ip;
            this.internet_ip = GlobalData.wan_ip;
            this.mainboard_id = GlobalData.mainboard_id;
            this.mac_addr = GlobalData.mac_addr;
            this.cpu_id = GlobalData.cpu_id;
            this.hard_disk_id = GlobalData.harddisk_id;
            this.company_name = GlobalData.company_name;
            this.addr = GlobalData.addr;
            this.phone = GlobalData.phone;
            this.fax = GlobalData.fax;
            if (GlobalData.is_abd)
            {
                this.cpu_id = this.cpu_id + "_ABD";
            }
            //   this.subcompany = "溶通";      
        }
    }


}


