using HandyUploadForm;
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
        public string serverHost;
        public string debug;
        public string img_server_host;
        //public string defaultImgUrl;


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
            img_server_host = ConfigurationManager.AppSettings["img_server_host"];
            debug = ConfigurationManager.AppSettings["debug"];
            //   defaultImgUrl = ConfigurationManager.AppSettings["default_img_url"];
        }
    }
}
