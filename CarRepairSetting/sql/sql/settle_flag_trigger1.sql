IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[update_settle_flag_trigger]'))
DROP TRIGGER [dbo].[update_settle_flag_trigger]
go
create TRIGGER [update_settle_flag_trigger]
   ON  [dbo].[DT_OM_GD]
   AFTER update
   AS
	declare @gd_id int,
	@cl_id int,
	@settle_dt datetime,
	@upflag int
BEGIN
	select @upflag = case when i.settle_dt=d.settle_dt then 0 else 1 end, @gd_id=i.gd_id, @settle_dt=i.settle_dt, @cl_id=i.cl_id from deleted d inner join inserted i on i.gd_id = d.gd_id and i.is_settle=1
   if (@upflag>0)
	 insert dataupload_gd(gd_id, cl_id, settle_dt, is_uploaded) values(@gd_id, @cl_id, @settle_dt,0)
END;