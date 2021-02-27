using Newtonsoft.Json;
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

        public String repairOrderSeq { get; set; }
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
            itemSeq = "1";
            itemName = "";
            settlementTime = "";
            workHoursUnitPrice = "";
            workHoursFee = "";
        }

    }


    public class RepairItem
    {
        public String subtotal { get; set; }
        public List<RepairItemDetail> items { get; set; }

        public RepairItem()
        {
            subtotal = "0";
            items = new List<RepairItemDetail>();
        }
        public RepairItem(String defaults)
        {
            subtotal = "0";
            items = new List<RepairItemDetail>();
            items.Add(new RepairItemDetail());
        }
    }

    public class RepairPartDetail
    {
        public String partSeq { get; set; }
        public String partName { get; set; }
        public String partBrand { get; set; }
        public String partNo { get; set; }
        public String partUnitNumber { get; set; }
        public String partMoney { get; set; }
        public String oneselfPart { get; set; }

        public RepairPartDetail()
        {
            partSeq = "1";
            partName = "";
            partBrand = "";
            partNo = "";
            partUnitNumber = "";
            partMoney = "";
            oneselfPart = "0";
        }


    }


    public class RepairPart
    {
        public String subtotal { get; set; }
        public List<RepairPartDetail> parts { get; set; }
        
        public RepairPart()
        {
            subtotal = "0";
            parts = new List<RepairPartDetail>();
        }

        public RepairPart(String defaults)
        {
            subtotal = "0";
            parts = new List<RepairPartDetail>();
            parts.Add(new RepairPartDetail());
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
    public class RepairInfoInternal
    {
        public String sign { get; set; }
        public String nonce { get; set; }
        public String timestamp { get; set; }
        public String companyIdentity { get; set; }
        public String incomingInspectionId { get; set; }
        public String repairOrderSeq { get; set; }
        public String deliveryDate { get; set; }
        public String repairMileage { get; set; }
        public String settlementDate { get; set; }
        public String settlementSeq { get; set; }
        public RepairItem repairItems { get; set; }
        public RepairPart repairParts { get; set; }
    }

    public class RepairInfo
    {
        public String sign { get; set; }
        public String nonce { get; set; }
        public String timestamp { get; set; }
        public String companyIdentity { get; set; }
        public String incomingInspectionId { get; set; }
        public String repairOrderSeq { get; set; }
        public String deliveryDate { get; set; }
        public String repairMileage { get; set; }
        public String settlementDate { get; set; }
        public String settlementSeq { get; set; }
        public String repairItems { get; set; }
        public String repairParts { get; set; }
        public String otherFee { get; set; }
        public String total { get; set; }
        public String oldPartDisposal { get; set; }
        public RepairInfo()
        {
            otherFee = "[{\"otherFeeSeq\": \"\",  \"project\": \"\",  \"otherFeeMoney\": \"\"  }]";
            total = "";
            oldPartDisposal = "3";
        }

        public static RepairInfo fromRepairInfoInternal(RepairInfoInternal repairInfoInternal)
        {
            RepairInfo repairInfo = new RepairInfo();
            repairInfo.sign = repairInfoInternal.sign;
            repairInfo.nonce = repairInfoInternal.nonce;
            repairInfo.timestamp = repairInfoInternal.timestamp;
            repairInfo.companyIdentity = repairInfoInternal.companyIdentity;
            repairInfo.incomingInspectionId = repairInfoInternal.incomingInspectionId;
            repairInfo.repairOrderSeq = repairInfoInternal.repairOrderSeq;
            repairInfo.deliveryDate = repairInfoInternal.deliveryDate;
            repairInfo.repairMileage = repairInfoInternal.repairMileage;
            repairInfo.settlementDate = repairInfoInternal.settlementDate;
            repairInfo.settlementSeq = repairInfoInternal.settlementSeq;
            if (repairInfoInternal.repairItems==null || repairInfoInternal.repairItems.items.Count==0)
            {
                repairInfo.repairItems = JsonConvert.SerializeObject(new RepairItem());
            }else { 
                String tmpRepairItems = JsonConvert.SerializeObject(repairInfoInternal.repairItems) ;
                repairInfo.repairItems = tmpRepairItems;
            }
            if (repairInfoInternal.repairParts == null || repairInfoInternal.repairParts.parts.Count == 0)
            {
                repairInfo.repairParts = JsonConvert.SerializeObject(new RepairPart());
            } else { 
                String tmpRepairParts = JsonConvert.SerializeObject(repairInfoInternal.repairParts);
                repairInfo.repairParts = tmpRepairParts;
            }

            return repairInfo;
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
        public String gd_id { get; set; }
        public String gd_sn { get; set; }
        public String incomingInspectionId { get; set; }
        public String car_no { get; set; }
        public String customer_name { get; set; }
        public String vin_code { get; set; }
        public String error_desc { get; set; }
        public String oldPartDisposal { get; set; }
        public Image gp_pic { get; set; }
        public byte [] gp_pic_bytes { get; set; }

        public int upload_status { get; set; }

    }

    public class GDCardInfo
    {
        public String gd_id { get; set; }
        public SignInfo signInfo { get; set; }
        public RepairItem repairItem { get; set; }
        public RepairInfoInternal repairPair { get; set; }
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
