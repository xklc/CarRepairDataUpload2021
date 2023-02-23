using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;

namespace AutoUploadService
{
    static class Program
    {



        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string [] args)
        {
            Console.WriteLine("start11");
            log4net.Config.XmlConfigurator.Configure();
            string sn = ConfigurationManager.AppSettings["sn"];
            //string realSn=EncryUtil.getEcryptedHardDiskId();
            //if (!sn.Equals(realSn))
            //{
            //    Console.WriteLine("start33 :" + realSn);
            //    System.Threading.Thread.Sleep(6000);
            //    LogHelper.WriteLog(typeof(Program), "failed to validate sn");
            //    return;
            //}
            Console.WriteLine("start22");

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
