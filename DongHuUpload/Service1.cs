using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace DongHuUpload
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            DataUploadThread dataUpload = new DataUploadThread();
            Thread donghuUploadThread = new Thread(new ParameterizedThreadStart(dataUpload.ParameterRun));
            donghuUploadThread.Name = "DonghuUploadThread";
            donghuUploadThread.Start(null);
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
