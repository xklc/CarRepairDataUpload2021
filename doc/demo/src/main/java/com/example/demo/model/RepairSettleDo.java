package com.example.demo.model;


import lombok.Data;

import java.math.BigDecimal;

@Data
public class RepairSettleDo {
    private String appId;
    private String sign;
    private String timestamp;
    private String orderCode;
    private BigDecimal totalCost;
    private String payTime;
    private long nonce;
}
