package com.example.demo.service;

import cn.hutool.http.HttpUtil;
import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONObject;
import com.alibaba.fastjson.serializer.SerializerFeature;
import com.example.demo.consts.SecretConsts;
import com.example.demo.model.RepairMaintainDo;
import com.example.demo.utils.SignUtil;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.util.*;


@Service
public class RepairMaintainService {

    private final static String URL = "http://test-www.yrdcarlife.com/openapi/repairRecord/updateRepairItemByOrderCode";


    public Object update() {
        Date date = new Date();
        String time = String.valueOf(date.getTime());
        long nonce = SignUtil.getLong();
//        String time ="1632122033662";
//        long nonce = 585116154;
        RepairMaintainDo repairMaintainDo = getRepairMaintain(time,nonce);
        repairMaintainDo.setSign(sign(time,nonce));
        System.out.println(JSON.toJSONString(repairMaintainDo));
        return HttpUtil.post(URL,JSON.toJSONString(repairMaintainDo));
    }


    private static RepairMaintainDo getRepairMaintain(String time,long nonce){
        List<RepairMaintainDo.Part> partList = new ArrayList<>();
        List<RepairMaintainDo.Project> projectList = new ArrayList<>();
        RepairMaintainDo.Part part = new RepairMaintainDo.Part();
        part.setPartCode("KGQLMSAE810");
        part.setPartName("防冻液");
        part.setBrandName("");
        part.setPartType("");
        part.setPartQty(1.0);
        part.setUnit("桶");
        partList.add(part);

        RepairMaintainDo.Project project = new RepairMaintainDo.Project();
        project.setProjectName("一级维护");
        project.setProjectType("");
        project.setWorkingHours(BigDecimal.valueOf(10.0));
        project.setRepairPartList(partList);
        projectList.add(project);

        RepairMaintainDo repairMaintainDo = new RepairMaintainDo();
        repairMaintainDo.setOrderCode("R20210920003");
        repairMaintainDo.setAppId(SecretConsts.APPID);
        repairMaintainDo.setSign("");
        repairMaintainDo.setTimestamp(time);
        repairMaintainDo.setNonce(nonce);
        repairMaintainDo.setRepairProjectList(projectList);
        return repairMaintainDo;
    }


    /**
     * 生成秘钥
     * @return
     */
    private static String sign(String time,long nonce){
        RepairMaintainDo repairMaintainDo = getRepairMaintain(time,nonce);
        JSONObject object = JSONObject.parseObject(JSON.toJSONString(repairMaintainDo, SerializerFeature.WriteMapNullValue));
        System.out.println(object.toJSONString());
        String sign = SignUtil.signature(object,SecretConsts.SECRET);
        System.out.println(sign);
        return sign;
    }


}
