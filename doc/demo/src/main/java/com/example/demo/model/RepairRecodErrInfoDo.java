package com.example.demo.model;

import lombok.Data;

@Data
public class RepairRecodErrInfoDo {

    private String appId;
    private String sign;
    private String timestamp;
    private long nonce;
}
