  insert into [StringParameter](name, value, id, parentid) values('appid','b62c6b7e2d264ea19bb48a2c9d4894ea', 779, 50);
  insert into [StringParameter](name, value, id, parentid) values('secret','6d246682e3c5434e9b29a100d2cbcfba', 780, 50);


  公司名称：上海舒乐巴士汽车修理有限公司
appid:  924b4b137d9048a998dc352cb2cef031
secret:  798b203d34da4e5db138bb7003cd88e9
测试账号：15202174321
账号密码：888888
测试环境网址：https://test-www.yrdcarlife.com/enmanage/#/login


2023-02-03  修改natureOfUser逻辑， 由原来的默认为 1 修改为: 判断MT CL.REGIST NO 的值是否为空，不为空取 1，为空取 2) REGIST NO 为软件车辆中的营运证号码