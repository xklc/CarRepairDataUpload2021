using System;
using System.Collections.Generic;
using System.Text;

namespace HandyUploadForm
{
    public class CarUploadFieldValidator
    {
        //"companyIdentity":"企业身份",取表stringparameter.value where id=100 的值
        //"incomingInspectionId":"检查单编号", DT_OM_JJJC.CAR_INFO 
        //"licensePlate":"车牌号", MT_CL.CAR_NO
        //"vin":"车辆识别号", MT_CL.VIN_CODE
        //"vehicleType":"车型", MT_CX.CAR_TYPE  
        //"engineNum":"发动机号", MT_CL.ENGINE_NO
        //"vehicleOwner":"机动车所有人", MT_KH.CUST_NM
        //"entrustRepair":"托修方", MT_KH.CUST_NM
        //"contact":"联系人",MT_KH.LINKMAN
        //"contactDetails":"联系方式",MT_KH.TEL1
        //"obd":"有无obd通讯接口： 0 无  1 有",   MT_CL.gearbox_type  
        //"carType":"车辆类型", MT_CL.CAR_OIL  0,1,2,3,4,5的值分别代表 轻型汽油车，重型汽油车，柴油车，其它车，LPG燃料车，CNG燃料车  
        //"vehicleClassCode":"车辆分类代号", MT_CL.CAR_SYMBOL
        //"drivingLicenseImg":"行驶证照片",  显示在上传程序上，上传时让客户自己选择照片
        //"color":"黑色",MT_CL.CAR_COLOR
        //"brand":"丰田" MT_CC.VENDOR MT_CL.CC_ID=MT_CC.CC_ID

        public static ValidatorRet check(CardUploadInfo carUploadInfo)
        {
            if (StringUtil.isEmpty(carUploadInfo.incomingInspectionId))
            {
                return new ValidatorRet(false, "检查单编号不能为空");
            }
            else if (StringUtil.isEmpty(carUploadInfo.licensePlate))
            {
                return new ValidatorRet(false, "车牌号不能为空");
            }
            else if (StringUtil.isEmpty(carUploadInfo.vin))
            {
                return new ValidatorRet(false, "vin_code[车辆识别号]不能为空");
            }
            else if (StringUtil.isEmpty(carUploadInfo.vehicleType))
            {
                return new ValidatorRet(false, "车型不能为空");
            }
            else if (StringUtil.isEmpty(carUploadInfo.engineNum))
            {
                return new ValidatorRet(false, "发动机号不能为空");
            }
            else if (StringUtil.isEmpty(carUploadInfo.vehicleOwner) || StringUtil.isEmpty(carUploadInfo.entrustRepair))
            {
                return new ValidatorRet(false, "客户名称【机动车所有人】不能为空");
            }
            else if (StringUtil.isEmpty(carUploadInfo.contact))
            {
                return new ValidatorRet(false, "联系人不能为空");
            }
            else if (StringUtil.isEmpty(carUploadInfo.contactDetails))
            {
                return new ValidatorRet(false, "联系方式不能为空");
            }
            else if (StringUtil.isEmpty(carUploadInfo.carType))
            {
                return new ValidatorRet(false, "车辆类型不能为空");
            }
            else if (StringUtil.isEmpty(carUploadInfo.obd))
            {
                return new ValidatorRet(false, "obd通讯接口不能为空");
            }
            else if (StringUtil.isEmpty(carUploadInfo.carType))
            {
                return new ValidatorRet(false, "车辆分类代号【MT_CL.CAR_SYMBO】不能为空");
            }
            else if (StringUtil.isEmpty(carUploadInfo.color))
            {
                return new ValidatorRet(false, "车辆颜色不能为空");
            }
            else if (StringUtil.isEmpty(carUploadInfo.brand))
            {
                return new ValidatorRet(false, "车辆品牌不能为空");
            }

            return new ValidatorRet();
        }
    }


    public class RepairInfoValidator
    {
        public static ValidatorRet check(RepairInfoInternal repairnfo)
        {
            if (StringUtil.isEmpty(repairnfo.deliveryDate))
            {
                new ValidatorRet(false, "送修日期不能为空");
            }
            else if (StringUtil.isEmpty(repairnfo.repairMileage))
            {
                return new ValidatorRet(false, "送修里程不能为空");
            }
            else if (StringUtil.isEmpty(repairnfo.settlementDate))
            {
                return new ValidatorRet(false, "结算日期不能为空");
            }
            else if (StringUtil.isEmpty(repairnfo.settlementSeq))
            {
                return new ValidatorRet(false, "结算编号不能为空");
            }
            //else if (repairnfo.repairItems == null || (StringUtil.isEmpty(repairnfo.repairItems.subtotal)) || repairnfo.repairItems.items.Count == 0)
            //{
            //    return new ValidatorRet(false, "维修项目不能为空");
            //}
            //else if (repairnfo.repairParts == null || (StringUtil.isEmpty(repairnfo.repairParts.subtotal)) || repairnfo.repairParts.parts.Count == 0)
            //{
            //    return new ValidatorRet(false, "维修配件不能为空");
            //}
            return new ValidatorRet();
        }
      }
}
