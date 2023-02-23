using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Text;

namespace HandyUploadForm
{
    public class ConfigItem
    {
        public string DBHost;
        public string DBUser;
        public string DBPassword;
        public string DBName;
        public string serverHost;
        public int cmdTimeOut;
        public static string appId;
        public static string secret;


        public ConfigItem()
        {
            this.loadConf();
        }

        public void loadConf()
        {
            DBHost = ConfigurationManager.AppSettings["DBHost"];
            DBUser = ConfigurationManager.AppSettings["DBUser"];
            DBPassword = ConfigurationManager.AppSettings["DBPassword"];
            DBName = ConfigurationManager.AppSettings["DBName"];
            serverHost = ConfigurationManager.AppSettings["server_host"];
            cmdTimeOut = Convert.ToInt32(ConfigurationManager.AppSettings["SqlCmdTimeOut"]);
        }


    }
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


    public class UpdateRepairInfoByOrderCode : SignInfo
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
}
