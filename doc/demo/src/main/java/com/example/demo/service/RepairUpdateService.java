package com.example.demo.service;

import cn.hutool.http.HttpUtil;
import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONObject;
import com.alibaba.fastjson.serializer.SerializerFeature;
import com.example.demo.consts.SecretConsts;
import com.example.demo.model.RepairUpdateDo;
import com.example.demo.utils.SignUtil;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.util.*;


@Service
public class RepairUpdateService {


    public final static String URL = "http://test-www.yrdcarlife.com/openapi/repairRecord/updateRepairInfoByOrderCode";


    public Object update() {
        Date date = new Date();
        String time = String.valueOf(date.getTime());
        long nonce = SignUtil.getLong();
        RepairUpdateDo repairUpdate = getRepairUpdate(time,nonce);
        repairUpdate.setSign(sign(time,nonce));
        return  HttpUtil.post(URL,JSON.toJSONString(repairUpdate));
    }



    private static RepairUpdateDo getRepairUpdate(String time,long nonce){
        List<RepairUpdateDo.Part> partList = new ArrayList<>();
        List<RepairUpdateDo.Project> projectList = new ArrayList<>();

        RepairUpdateDo.Part part = new RepairUpdateDo.Part();
        part.setPartCode("123");
        part.setPartName("机油机滤");
        part.setBrandName("天美牌");
        part.setPartType("铝制品");
        part.setPartQty(1.0);
        part.setUnit("个");
        partList.add(part);

        RepairUpdateDo.Project project = new RepairUpdateDo.Project();
//        project.setProjectName("更换机油机滤");
//        project.setProjectType("保养");
//        project.setWorkingHours(BigDecimal.valueOf(1.2));
//        project.setRepairPartList(partList);
        projectList.add(project);

        RepairUpdateDo repairUpdate = new RepairUpdateDo();
        repairUpdate.setAppId(SecretConsts.APPID);
        repairUpdate.setSign("");
        repairUpdate.setTimestamp(time);
        repairUpdate.setNonce(nonce);
        repairUpdate.setOrderCode("1");
        repairUpdate.setVin("WVWLJ57N4FV028811");
        repairUpdate.setVpn("GK12001");
        repairUpdate.setRepairTime("2021-06-10 12:12:12");
        repairUpdate.setTotalCost(BigDecimal.valueOf(10.0));
        repairUpdate.setPayTime("2021-06-12 12:12:12");
        repairUpdate.setRepairProjectList(projectList);
        return repairUpdate;
    }


    /**
     * 生成秘钥
     * @return
     */
    private static String sign(String time,long nonce){
        RepairUpdateDo repairUpdate = getRepairUpdate(time,nonce);
        JSONObject object = JSONObject.parseObject(JSON.toJSONString(repairUpdate, SerializerFeature.WriteMapNullValue));
        System.out.println(object.toJSONString());
        String sign = SignUtil.signature(object,SecretConsts.SECRET);
        System.out.println(sign);
        return sign;
    }



}
