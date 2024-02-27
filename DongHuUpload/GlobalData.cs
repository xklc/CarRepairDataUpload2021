using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace DongHuUpload
{
    public class GlobalData
    {
        public static string server_url;
        public static int upload_internal;
        public static string factory_name;

        public static void load()
        {
            server_url = ConfigurationManager.AppSettings["server_url"];
            factory_name = ConfigurationManager.AppSettings["factory_name"];
            if (!Int32.TryParse(ConfigurationManager.AppSettings["upload_internal"], out upload_internal))
            {
                upload_internal = 10000;
            }
        }
    }
}
