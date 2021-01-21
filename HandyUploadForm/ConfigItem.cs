using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CarRepairDataUpload
{
    public class ConfigItem
    {
        public string DBHost;
        public string DBUser;
        public string DBPassword;
        public string DBName;
        public string CompanyCode;
        public string CompanyPassword;


        public ConfigItem()
        {
            this.loadConf();
        }

        public void loadConf()
        {
            DBHost = ConfigurationManager.AppSettings["DBHost"];
            DBUser = ConfigurationManager.AppSettings["DBUser"];
            DBPassword = ConfigurationManager.AppSettings["DBPassword"];
            DBName = ConfigurationManager.AppSettings["DBName"];
            CompanyCode = ConfigurationManager.AppSettings["CompanyCode"];
            CompanyPassword = ConfigurationManager.AppSettings["CompanyPassword"];

        }


    }
}
