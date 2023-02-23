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
        public List<string> vinCodeList = new List<string>();
        public List<string> errorDespList = new List<string>();

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
            string error_random = ConfigurationManager.AppSettings["error_random"];
            string vin_random = ConfigurationManager.AppSettings["vin_random"];

            vinCodeList.Clear();
            errorDespList.Clear();

            foreach (var error in error_random.Split(','))
            {
                if (error.Trim().Length == 0) continue;
                errorDespList.Add(error.Trim());
            }
            foreach (var vin in vin_random.Split(','))
            {
                if (vin.Trim().Length == 0) continue;
                vinCodeList.Add(vin.Trim());
            }

        }


    }
}
