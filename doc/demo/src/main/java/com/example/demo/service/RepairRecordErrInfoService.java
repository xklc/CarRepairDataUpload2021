package com.example.demo.service;

import cn.hutool.http.HttpUtil;
import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONObject;
import com.alibaba.fastjson.serializer.SerializerFeature;
import com.example.demo.consts.SecretConsts;
import com.example.demo.model.RepairRecodErrInfoDo;
import com.example.demo.utils.SignUtil;
import org.springframework.stereotype.Service;

import java.util.*;


@Service
public class RepairRecordErrInfoService {

    private final static String URL = "http://test-www.yrdcarlife.com/openapi/repairRecord/queryCompanyErrRepairRecord";


    public Object query() {
        Date date = new Date();
        String time = String.valueOf(date.getTime());
        long nonce = SignUtil.getLong();
        RepairRecodErrInfoDo repairRecord = getRepairRecord(time,nonce);
        repairRecord.setSign(sign(time,nonce));
        return HttpUtil.post(URL,JSON.toJSONString(repairRecord));
    }


    /**
     * 对象参数
     * @param time
     * @return
     */
    private static RepairRecodErrInfoDo getRepairRecord(String time,long nonce){
        RepairRecodErrInfoDo repairRecord = new RepairRecodErrInfoDo();
        repairRecord.setAppId(SecretConsts.APPID);
        repairRecord.setSign("");
        repairRecord.setTimestamp(time);
        repairRecord.setNonce(nonce);
        return repairRecord;
    }


    /**
     * 生成秘钥
     * @return
     */
    private static String sign(String time,long nonce){
        RepairRecodErrInfoDo repairRecord = getRepairRecord(time,nonce);
        JSONObject object = JSONObject.parseObject(JSON.toJSONString(repairRecord, SerializerFeature.WriteMapNullValue));
        System.out.println(object.toJSONString());
        String sign = SignUtil.signature(object,SecretConsts.SECRET);
        return sign;
    }




}
