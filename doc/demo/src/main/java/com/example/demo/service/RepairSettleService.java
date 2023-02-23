package com.example.demo.service;

import cn.hutool.http.HttpUtil;
import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONObject;
import com.alibaba.fastjson.serializer.SerializerFeature;
import com.example.demo.consts.SecretConsts;
import com.example.demo.model.RepairSettleDo;
import com.example.demo.utils.SignUtil;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.util.*;


@Service
public class RepairSettleService {


    private final static String URL = "http://test-www.yrdcarlife.com/openapi/repairRecord/updateSettleInfoByOrderCode";


    public Object update() {
        Date date = new Date();
        String time = String.valueOf(date.getTime());
        long nonce = SignUtil.getLong();
        RepairSettleDo repairSettle = getRepairSettle(time,nonce);
        repairSettle.setSign(sign(time,nonce));
        return HttpUtil.post(URL,JSON.toJSONString(repairSettle));
    }


    private static RepairSettleDo getRepairSettle(String time,long nonce){
        RepairSettleDo repairSettle = new RepairSettleDo();
        repairSettle.setAppId(SecretConsts.APPID);
        repairSettle.setSign("");
        repairSettle.setTimestamp(time);
        repairSettle.setNonce(nonce);
        repairSettle.setOrderCode("1");
        repairSettle.setTotalCost(BigDecimal.valueOf(10.0));
        repairSettle.setPayTime("2021-06-12 12:12:12");
        return repairSettle;
    }


    /**
     * 生成秘钥
     * @return
     */
    private static String sign(String time,long nonce){
        RepairSettleDo repairSettle = getRepairSettle(time,nonce);
        JSONObject object = JSONObject.parseObject(JSON.toJSONString(repairSettle, SerializerFeature.WriteMapNullValue));
        System.out.println(object.toJSONString());
        String sign = SignUtil.signature(object,SecretConsts.SECRET);
        System.out.println(sign);
        return sign;
    }





}
