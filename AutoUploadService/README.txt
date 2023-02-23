  insert into [StringParameter](name, value) values('appid','924b4b137d9048a998dc352cb2cef031');
  insert into [StringParameter](name, value) values('secret','798b203d34da4e5db138bb7003cd88e9');


  公司名称：上海舒乐巴士汽车修理有限公司
appid:  924b4b137d9048a998dc352cb2cef031
secret:  798b203d34da4e5db138bb7003cd88e9
测试账号：15202174321
账号密码：888888
测试环境网址：https://test-www.yrdcarlife.com/enmanage/#/login


2023-02-03  修改natureOfUser逻辑， 由原来的默认为 1 修改为: 判断MT CL.REGIST NO 的值是否为空，不为空取 1，为空取 2) REGIST NO 为软件车辆中的营运证号码