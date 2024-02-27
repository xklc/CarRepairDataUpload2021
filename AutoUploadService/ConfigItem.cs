using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace AutoUploadService
{
    public class ConfigItem
    {
        public string DBHost;
        public string DBUser;
        public string DBPassword;
        public string DBName;
        public string serverHost;
        public int cmdTimeOut;
        public static string appId;
        public static string secret;


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
            serverHost = ConfigurationManager.AppSettings["server_host"];
            cmdTimeOut = Convert.ToInt32(ConfigurationManager.AppSettings["SqlCmdTimeOut"]);
        }


    }
}
