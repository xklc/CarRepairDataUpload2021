﻿using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;

namespace CarRepairDataUpload
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new CarRepairDataUpload()
            //};
            //ServiceBase.Run(ServicesToRun);

            log4net.Config.XmlConfigurator.Configure();
            CarRepairDataUpload service1 = new CarRepairDataUpload();
            service1.Start(null);
        }
    }
}
