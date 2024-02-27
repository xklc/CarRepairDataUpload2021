1. 数据库管理器执行如下sql
日志表
CREATE TABLE [donghu_upload] (
	[id] [int] IDENTITY (1, 1) NOT NULL ,
	[gd_id] [int] NOT NULL ,
	[request_str] [text] COLLATE Chinese_PRC_CI_AS NULL ,
	[reponse_str] [varchar] (512) COLLATE Chinese_PRC_CI_AS NULL ,
	[is_uploaded] [int] NULL CONSTRAINT [DF_donghu_upload_is_uploaded] DEFAULT (0),
	[create_time] [datetime] NULL CONSTRAINT [DF_donghu_upload_create_time] DEFAULT (getdate())
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

-- 触发器
create TRIGGER [update_settle_flag_trigger_for_donghu]
   ON  [dbo].[DT_OM_GD]
   for insert, update
   AS
	declare @gd_id int,
	@cl_id int,
	@settle_dt datetime,
	@upflag int
BEGIN
	select @upflag = case when i.settle_dt=d.settle_dt then 0 else 1 end, @gd_id=i.gd_id, @settle_dt=i.settle_dt, @cl_id=i.cl_id from deleted d left join inserted i on i.gd_id = d.gd_id and i.is_settle=1
   if (@upflag>0)
	 insert donghu_upload(gd_id, is_uploaded) values(@gd_id,0)
END;


2. 修改配置文件DongHuUpload.exe.config

把这一行替换为客户相关的信息
<add name="netmis_db" connectionString="server=192.168.60.217,1433;uid=sa;pwd=123456;database=netmis_en;pooling=false"/>

3. 安装服务
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe"  "L:\DongHuUpload\DongHuUpload.exe"
(上面的路径替换为客户服务器上实际路径)

4. 测试.
