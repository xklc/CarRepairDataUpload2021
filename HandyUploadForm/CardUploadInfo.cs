﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace HandyUploadForm
{
    public class SignInfo
    {
        public String appId;
        public String sign;
        public String nonce;
        public String timestamp;
        public static Random rd = new Random();
        public SignInfo()
        {
            
            this.appId = GlobalData.appid;
            this.nonce = rd.Next().ToString();       
            this.timestamp = TimeUtil.getCurrentMillSeconds().ToString();
        }
    }
    //接车信息
    public class PickCarInfo:SignInfo
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
        public int  natureOfUse { get; set; }
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
        public String partQty { get; set; }
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
        public String workingHours { get; set; }


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
            repairPartList = new List<RepairItem>();
        }
    }
    
    public class RepairItemInfo:SignInfo
    {
        public String orderCode { get; set; }
        public List<RepairProject> repairProjectList { get; set; }

        public RepairItemInfo()
        {
            repairProjectList = new List<RepairProject>();
        }
    }
    
    //结算信息
    public class SettleInfo:SignInfo
    {
        public String orderCode { get; set; }
        public String totalCost { get; set; }
        public String payTime { get; set; }
        
        public SettleInfo()
        {
            totalCost="0";
        }
    }
    
  
    //上传维修信息
    public class RepairItemDetail:SignInfo
    {
        public String itemSeq { get; set; }
        public String itemName { get; set; }
        public String settlementTime  { get; set; }
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

    public class CompanyErrRepairRecord:SignInfo
    {
        public String vpn { get; set; }
        public String repairTime { get; set; }
        public String projectType { get; set; }
        public String unit { get; set; }
        public String partCode { get; set; }
        public String partName { get; set; }
        public String partType { get; set; }
        public String partQty { get; set; }
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
    public class CarDisplayInfo
    {
        public String gd_sn { get; set; }
       // public String incomingInspectionId { get; set; }
        public String car_no { get; set; }
        public String customer_name { get; set; }
        public String vin_code { get; set; }
        public String error_desc { get; set; }
        public String oldPartDisposal { get; set; }
        public Image gp_pic { get; set; }
       // public byte [] gp_pic_bytes { get; set; }

        public int upload_status { get; set; }

    }

    public class GDCardInfo
    {
        public String gd_id { get; set; }
        public SignInfo signInfo { get; set; }
        public RepairItem repairItem { get; set; }
       // public RepairInfoInternal repairPair { get; set; }
    }
}
