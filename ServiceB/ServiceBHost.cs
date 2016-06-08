using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using ServiceCommonLib;

namespace ServiceB
{
    public class ServiceBHost
    {
        internal static ServiceHost HostServiceB = null;
        /// <summary>
        /// Start Service B 
        /// </summary>
        /// <returns></returns>
        internal static bool StartService()
        {
            System.Console.WriteLine("Starting Service B...");
            HostServiceB = new ServiceHost(typeof(ServiceB));
            bool result = false;
            try
            {
                if (null != HostServiceB.Description.Endpoints)
                {
                    foreach (var endpoint in HostServiceB.Description.Endpoints)
                    {
                        if (endpoint.Contract.ContractType.Equals(typeof(ServiceB)))
                        {
                            if (!endpoint.Behaviors.Any(b =>
                                b is CentralSessionDataContextProcessInspector))
                                endpoint.Behaviors.Add(new CentralSessionDataContextProcessInspector());
                        }
                    }
                }
                HostServiceB.Open();
                result = true;
                System.Console.WriteLine("Service B has been started successfully.");
            }
            catch (Exception exc)
            {
                System.Console.WriteLine("Failed to start Service B.");
                LogHelper.CreateNewLogMessage(typeof(ServiceBHost), string.Format("Could not start Service B. \nExcpetion Type: {0}\nMessage: {1}", exc.GetType().ToString(), exc.Message));
            }
            return result;
        }
        /// <summary>
        /// Stops the service
        /// </summary>
        internal static bool StopService()
        {
            bool result = false;
            try
            {
                if (HostServiceB != null)
                    if (HostServiceB.State == CommunicationState.Opened)
                    {
                        System.Console.WriteLine("Stopping/Closing Service B ...");
                        HostServiceB.Abort();
                        HostServiceB.Close();
                    }
                result = true;
                System.Console.WriteLine("Service B has been stopped/closed successfully.");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Failed to stop/close Service B.");
                LogHelper.CreateNewLogMessage(typeof(ServiceBHost), string.Format("Error occured while stopping Service B." + ex));
            }

            return result;
        }
    }
}
