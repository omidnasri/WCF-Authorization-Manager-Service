using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceCommonLib;

namespace ServiceB
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceInitializer.OnGuiOrConsoleStart += new EventHandler<JerichoServiceEventArgs>(JerichoServiceInitializer_OnGuiOrConsoleStart);
            ServiceInitializer.StartCurrentService(new ServiceBProvider(), InitializerMode.Console);
        }

        static void JerichoServiceInitializer_OnGuiOrConsoleStart(object sender, JerichoServiceEventArgs e)
        {
            ServiceBHost.StartService();
        }
    }
}
