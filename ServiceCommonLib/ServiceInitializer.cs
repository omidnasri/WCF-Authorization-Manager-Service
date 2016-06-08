using System;
using System.ServiceProcess;
using System.Threading;

namespace ServiceCommonLib
{
    public enum InitializerMode
    {
        Console,
        Gui,
        Service
    }
    public abstract class ServiceInitializer
    {
        public static event EventHandler<JerichoServiceEventArgs> OnServiceStart;
        public static event EventHandler<JerichoServiceEventArgs> OnGuiOrConsoleStart;
        public static void StartCurrentService(ServiceBase serviceToRun, InitializerMode initializerMode = InitializerMode.Service)
        {
            AppDomain.CurrentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler(CurrentDomainUnhandledException);
            LogHelper.OnLogEvent += new LogEventHandler(LogHelper_OnLogEvent);

            if (initializerMode == InitializerMode.Service)
            {
                if (OnServiceStart != null)
                    OnServiceStart(typeof(ServiceInitializer), new JerichoServiceEventArgs() { ServiceToRun = serviceToRun });
                var servicesToRun = new ServiceBase[]
                                                  {
                                                      serviceToRun
                                                  };
                ServiceBase.Run(servicesToRun);
            }
            else
            {
                if (OnGuiOrConsoleStart != null)
                    OnGuiOrConsoleStart(typeof(ServiceInitializer), new JerichoServiceEventArgs() { ServiceToRun = serviceToRun });
                new ManualResetEvent(false).WaitOne();
            }
        }
        public static void LogHelper_OnLogEvent(object sender, LogEventArgs e)
        {
            //Logger.LoggingUtility.WriteLog(e.Message);
        }
        public static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //Logger.LoggingUtility.WriteLog(e.ExceptionObject.ToString());
        }
    }
    public class JerichoServiceEventArgs : EventArgs
    {
        public ServiceBase ServiceToRun { get; set; }
    }
}
