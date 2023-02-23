package com.example.demo.model;

import lombok.Data;

import java.math.BigDecimal;
import java.util.List;

@Data
public class RepairMaintainDo {
    private String appId;
    private String sign;
    private String timestamp;
    private String orderCode;
    private List<Project> repairProjectList;
    private long nonce;

    @Data
    public static class Project {
        private String projectName;
        private String projectType;
        private BigDecimal workingHours;
        private List<Part> repairPartList;

    }
    @Data
    public static class Part {
        private String partName;
        private String brandName;
        private String partType;
        private String partCode;
        private Double partQty;
        private String unit;

    }

}

