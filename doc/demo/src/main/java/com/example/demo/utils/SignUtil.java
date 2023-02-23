package com.example.demo.utils;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONArray;
import com.alibaba.fastjson.JSONObject;
import com.alibaba.fastjson.serializer.SerializerFeature;
import lombok.SneakyThrows;
import org.apache.commons.lang3.StringUtils;

import java.net.URLDecoder;
import java.security.MessageDigest;
import java.util.*;

public class SignUtil {
    private final static String MD5 = "MD5";
    private final static String ENCODE_MODE = "UTF-8";

    /**
     * 签名
     * @param obj 业务参数
     * @param secret secret
     * @return java.lang.String
     * @date 2021/6/15 17:30
     */
    public static String signature(Map<String, Object> obj, String secret) {
        Iterator<Map.Entry<String, Object>> iterator = obj.entrySet().iterator();
        SortedMap<String, Object> sortedMap = new TreeMap<>();
        while(iterator.hasNext()) {
            Map.Entry<String, Object> entry = iterator.next();
            String key = entry.getKey();
            Object value = entry.getValue();
            if (!key.equalsIgnoreCase("sign")) {
                sortedMap.put(key, value);
            }
        }
        String source = concatMap(sortedMap) + secret;
        System.out.println("签名原数据: "+source);
        return md5(source);
    }

    /**
     * 参数拼接
     * @param map
     * @return java.lang.String
     * @author jks
     * @date 2021/6/15 17:31
     */
    private static String concatMap(SortedMap<String, Object> map) {
        return concatMap2String(map,new StringBuilder());
    }

    @SneakyThrows
    private static String concatMap2String(SortedMap<String, Object> map, StringBuilder buffer) {
        Iterator<Map.Entry<String, Object>> iterator = map.entrySet().iterator();
        while (iterator.hasNext()) {
            Map.Entry<String, Object> entry = iterator.next();
            String key = entry.getKey();
            Object value = entry.getValue();
            String valueStr = "";
            if (value == null) {
                valueStr = "";
            }else{
                if(value instanceof JSONArray || value instanceof List){
                    JSONArray valArr = JSONArray.parseArray(JSON.toJSONString(value));
                    valueStr = transArray2String(valArr);
                }else if(value instanceof JSONObject || value instanceof Map){
                    Map valMap = JSONObject.parseObject(JSONObject.toJSONString(value,
                            SerializerFeature.WriteMapNullValue), Map.class);
                    valueStr = transMap2String(valMap);
                }else{
                    valueStr = value.toString();
                    if (StringUtils.isBlank(valueStr)) {
                        valueStr = "";
                    }
                }
            }
            buffer.append(String.format("%s=%s&", key, URLDecoder.decode(valueStr, "UTF-8")));
        }
        if (buffer.length() > 0) {
            buffer.deleteCharAt(buffer.length() - 1);
        }
        return buffer.toString();
    }

    /**
     * 数组字符串拼接
     * @param valArr
     * @return
     */
    @SneakyThrows
    private static String transArray2String(JSONArray valArr) {
        StringBuilder buffer = new StringBuilder();
        for(int i = 0;i<valArr.size();i++) {
            try{
                JSONObject valObj = valArr.getJSONObject(i);
                buffer.append("&");
                concatMap2String(transObj(valObj),buffer);
            }catch (ClassCastException e){
                Object val = valArr.get(i);
                String valStr = "";
                if(null == val){
                    valStr = "";
                }else{
                    valStr = String.valueOf(val);
                }
                buffer.append(String.format("%s&",URLDecoder.decode(valStr, "UTF-8")));
            }
        }
        if (buffer.length() > 0) {
            if(buffer.lastIndexOf("&") == buffer.length() - 1){
                buffer.deleteCharAt(buffer.length() - 1);
            }
            if(buffer.indexOf("&") == 0){
                buffer.deleteCharAt(0);
            }
        }
        return buffer.toString();
    }

    /**
     * 对象字符串拼接
     * @param map
     * @return
     */
    private static String transMap2String(Map map) {
        return concatMap2String(transObj(map),new StringBuilder());
    }

    /**
     * 将map集合key有序排列
     * @param obj
     * @return
     */
    private static SortedMap<String, Object> transObj(Map<String,Object> obj) {
        SortedMap<String, Object> sortedMap = new TreeMap<>();
        Iterator<Map.Entry<String, Object>> iterator = obj.entrySet().iterator();
        while (iterator.hasNext()) {
            Map.Entry<String, Object> entry = iterator.next();
            String key = entry.getKey();
            Object value = entry.getValue();
            sortedMap.put(key, value);
        }
        return sortedMap;
    }

    /**
     * md5加密
     *
     * @param source
     * @return
     */
    private static String md5(String source) {
        try {
            MessageDigest md5 = MessageDigest.getInstance(MD5);
            byte[] result = md5.digest(source.getBytes(ENCODE_MODE));
            StringBuffer buffer = new StringBuffer();
            for (byte b : result) {
                int num = b & 0xff;
                String str = Integer.toHexString(num);
                if (str.length() == 1) {
                    buffer.append("0");
                }
                buffer.append(str);
            }
            return buffer.toString();
        } catch (Exception e) {
        }
        return null;
    }

    public static Long getLong(){
        return (long)(Math.random()*90+10);
    }


}
