using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace HandyUploadForm
{
    //上传的汽车信息
    public class CardUploadInfo
    {
        public String sign { get; set; }
        public String nonce { get; set; }
        public String timestamp { get; set; }
        public String companyIdentity { get; set; }
        public String incomingInspectionId { get; set; }
        public String licensePlate { get; set; }
        public String vin { get; set; }
        public String vehicleType { get; set; }
        public String engineNum { get; set; }
        public String vehicleOwner { get; set; }
        public String entrustRepair { get; set; }
        public String contact { get; set; }
        public String contactDetails { get; set; }
        public String obd { get; set; }
        public String carType { get; set; }
        public String vehicleClassCode { get; set; }
        public String drivingLicenseImg { get; set; }
        public String color { get; set; }
        public String brand { get; set; }
    }

    //上传维修信息
    public class RepairItemDetail
    {
        public String itemSeq { get; set; }
        public String itemName { get; set; }
        public String settlementTime  { get; set; }
        public String workHoursUnitPrice { get; set; }
        public String workHoursFee { get; set; }

        public RepairItemDetail()
        {
            workHoursUnitPrice = "1.0";
            workHoursFee = "1.0";
        }

    }


    public class RepairItem
    {
        public String subtotal { get; set; }
        public List<RepairItemDetail> items { get; set; }

        public RepairItem()
        {
            items = new List<RepairItemDetail>();
        }
    }

    public class RepairPartDetail
    {
        public String partSeq { get; set; }
        public String partName { get; set; }
        public String partNo { get; set; }
        public String partUnitNumber { get; set; }
        public String partMoney { get; set; }
        public String oneselfPart { get; set; }

        public RepairPartDetail()
        {
            partMoney = "1.0";
            oneselfPart = "1.0";
        }
    }


    public class RepairPart
    {
        public String subtotal { get; set; }
        public List<RepairPartDetail> parts { get; set; }
        
        public RepairPart()
        {
            parts = new List<RepairPartDetail>();
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

    //维修信息
    public class RepairInfo
    {
        public String sign { get; set; }
        public String nonce { get; set; }
        public String timestamp { get; set; }
        public String companyIdentity { get; set; }
        public String incomingInspectionId { get; set; }
        public String deliveryDate { get; set; }
        public String repairMileage { get; set; }
        public String settlementDate { get; set; }
        public String settlementSeq { get; set; }
        public RepairItem repairItems { get; set; }
        public RepairPart repairParts { get; set; }
    }

    public class Response
    {
        public int sign { get; set; }
        public String message { get; set; }
        public String data { get; set; }
    }

    //界面显示字段信息
    public class CarDisplayInfo
    {
        public String gd_id { get; set; }
        public String gd_sn { get; set; }
        public String incomingInspectionId { get; set; }
        public String car_no { get; set; }
        public String customer_name { get; set; }
        public String vin_code { get; set; }
        public String error_desc { get; set; }
        public Image gp_pic { get; set; }
        public byte [] gp_pic_bytes { get; set; }

    }

    public class GDCardInfo
    {
        public String gd_id { get; set; }
        public SignInfo signInfo { get; set; }
        public RepairItem repairItem { get; set; }
        public RepairInfo repairPair { get; set; }
    }

    public class SignInfo
    {
        public String nonce { get; set; }
        public String timestamp { get; set; }
        public String sign { get; set; }
    }

    public class ImageData
    {
        public String imageUrl;
    }
    public class ImageUploadResponse
    {
        public String code;
        public String message;
        public ImageData data;
    }
}
