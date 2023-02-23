package com.example.demo.controller;


import com.example.demo.service.*;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class DemoController {

    @Autowired
    private RepairRecordService repairRecordService;

    @Autowired
    private RepairMaintainService repairMaintainService;

    @Autowired
    private RepairSettleService repairSettleService;

    @Autowired
    private RepairUpdateService repairUpdateService;

    @Autowired
    private RepairRecordErrInfoService repairRecordErrInfoService;


    /**
     * 上传接车信息
     * @return
     */
    @GetMapping("/insertRepairRecord")
    public Object insertRepairRecord(){
      return repairRecordService.insert();
    }


    /**
     * 上传维修项目及配件信息
     * @return
     */
    @GetMapping("/updateMaintain")
    public Object updateMaintain(){
        return repairMaintainService.update();
    }


    /**
     * 上传结算信息
     * @return
     */
    @GetMapping("/updateSettle")
    public Object updateSettle(){
        return repairSettleService.update();
    }


    /**
     * 更新已上传的错误信息
     * @return
     */
    @GetMapping("/updateRepairRecode")
    public Object updateRepairRecode(){
        return repairUpdateService.update();
    }


    /**
     * 查询已上传的错误信息
     * @return
     */
    @GetMapping("/queryRepairRecord")
    public Object queryRepairRecord(){
        return repairRecordErrInfoService.query();
    }


}
