IF EXISTS ( SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '[dataupload_gd]')DROP TABLE [dataupload_gd]
go  
CREATE TABLE [dbo].[dataupload_gd](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[gd_id] [int] NOT NULL,
	[cl_id] [int] NOT NULL,
	[settle_dt] [datetime] NULL,
	[is_uploaded] [bit] NULL CONSTRAINT [DF_dataupload_gd_is_uploaded]  DEFAULT ((0)),
	[request_str] [nchar](3300) COLLATE Chinese_PRC_CI_AS NULL,
	[response_str] [nchar](200) COLLATE Chinese_PRC_CI_AS NULL,
	[create_time] [datetime] NULL CONSTRAINT [DF_dataupload_gd_create_to,e]  DEFAULT (getdate())
)
