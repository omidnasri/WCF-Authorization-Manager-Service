using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using ServiceCommonLib;
using static System.Console;

namespace ServiceA
{
    public class ServiceAHost
    {
        internal static ServiceHost HostServiceA = null;
        /// <summary>
        /// Start Service A 
        /// </summary>
        /// <returns></returns>
        internal static bool StartService()
        {
            WriteLine("Starting Service A...");
            HostServiceA = new ServiceHost(typeof(ServiceA));
            bool result = false;
            try
            {
                if (null != HostServiceA.Description.Endpoints)
                {
                    foreach (var endpoint in HostServiceA.Description.Endpoints)
                    {
                        if (endpoint.Contract.ContractType.Equals(typeof(ServiceA)))
                        {
                            if (!endpoint.Behaviors.Any(b =>
                                b is CentralSessionDataContextProcessInspector))
                                endpoint.Behaviors.Add(new CentralSessionDataContextProcessInspector());
                        }
                    }
                }
                HostServiceA.Open();
                result = true;
                WriteLine("Service A has been started successfully.");
            }
            catch (Exception exc)
            {
                WriteLine("Failed to start Service A.");
                LogHelper.CreateNewLogMessage(typeof(ServiceAHost), string.Format("Could not start Service A. \nExcpetion Type: {0}\nMessage: {1}", exc.GetType().ToString(), exc.Message));
            }
            return result;
        }
        /// <summary>
        /// Stops the service
        /// </summary>
        internal static bool StopService()
        {
            var result = false;
            try
            {
                if (HostServiceA?.State == CommunicationState.Opened)
                {
                    WriteLine("Stopping/Closing Service A ...");
                    HostServiceA.Abort();
                    HostServiceA.Close  ();
                }
                result = true;
                WriteLine("Service A has been stopped/closed successfully.");
            }
            catch(Exception ex)
            {
                WriteLine("Failed to stop/close Service A.");
                LogHelper.CreateNewLogMessage(typeof(ServiceAHost), string.Format("Error occured while stopping Service A." + ex));
            }
            return result;
        }
    }
}