using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace AutoUploadService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.xml"));
        }

        private void syncDateTime()
        {
            try
            {
                ServiceController serviceController = new ServiceController("w32time");
                if (serviceController.Status != ServiceControllerStatus.Running)
                {
                    serviceController.Start();
                }

                LogHelper.WriteLog(typeof(Service1), "w32time service is running");

                Process processTime = new Process();
                processTime.StartInfo.FileName = "w32tm";
                processTime.StartInfo.Arguments = "/resync";
                processTime.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processTime.Start();
                processTime.WaitForExit();

                LogHelper.WriteLog(typeof(Service1), "w32time service has sync local dateTime from NTP server");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(Service1), ex);

            }
        }
        
        protected override void OnStart(string[] args)
        {
            ConfigItem configItem = new ConfigItem();
            DataUpload dataUpload = new DataUpload(configItem);

            //判断数据库是否可以连通， 如果不能连通则等待
            Thread carAutoUploadThread = new Thread(new ParameterizedThreadStart(dataUpload.ParameterRun));
            carAutoUploadThread.Name = "AutoMisUploadThread";
            carAutoUploadThread.Start(null);

            Thread statInfoThread = new Thread(new ParameterizedThreadStart(dataUpload.uploadStatInfo));
            statInfoThread.Name = "StatInfoThread";
            statInfoThread.IsBackground = true;
            statInfoThread.Start(null);

            syncDateTime();
        }

        protected override void OnStop()
        {
            System.Environment.Exit(0);
        }

        public void Start(string[] args)
        {
            this.OnStart(args);
        }
    }
}
