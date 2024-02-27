using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace DongHuUpload
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string [] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            GlobalData.load();
            if (args.Length > 0)
            {
                Service1 service1 = new Service1();
                service1.Start(null);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new Service1()
                };
                ServiceBase.Run(ServicesToRun);
            }

        }
    }
}
