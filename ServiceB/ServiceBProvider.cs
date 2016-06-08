using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace ServiceB
{
    partial class ServiceBProvider : ServiceBase
    {
        public ServiceBProvider()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ServiceBHost.StartService();
        }

        protected override void OnStop()
        {
            ServiceBHost.StopService();
        }
    }
}
