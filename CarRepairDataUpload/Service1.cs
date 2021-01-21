using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace CarRepairDataUpload
{
    public partial class CarRepairDataUpload : ServiceBase
    {
        public CarRepairDataUpload()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.xml"));
        }

        protected override void OnStart(string[] args)
        {
            ConfigItem configItem = new ConfigItem();
            DataUpload dataUpload = new DataUpload(configItem);
            Thread parameterThread = new Thread(new ParameterizedThreadStart(dataUpload.ParameterRun));
            parameterThread.Name = "DataUploadThread";
            parameterThread.Start(null);
        }

        protected override void OnStop()
        {
            System.Environment.Exit(0);
        }
    }
}
