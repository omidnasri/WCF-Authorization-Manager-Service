using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace ServiceA
{
    partial class ServiceAProvider : ServiceBase
    {
        public ServiceAProvider()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ServiceAHost.StartService();
        }

        protected override void OnStop()
        {
            ServiceAHost.StopService();
        }
    }
}
