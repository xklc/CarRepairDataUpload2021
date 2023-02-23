package com.example.demo.model;

import lombok.Data;

@Data
public class RepairRecordDo {

    private String appId;
    private String sign;
    private String timestamp;
    private String vin;
    private String vpn;
    private String orderCode;
    private String repairTime;
    private String repairMileage;
    private String serviceType;
    private String fuelType;
    private String requirement;
    private String faultDesc;
    private int natureOfUse;
    private String carPhoto;
    private String drivingPermitPhoto;
    private long nonce;
}
