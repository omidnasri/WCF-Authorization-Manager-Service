using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceA.Contract;
using ServiceCommonLib;

namespace ServiceA
{
    [CentralSessionDataContextBehavior]
    public class ServiceA : IServiceA
    {
        public string DoWork()
        {
            System.Console.WriteLine("Consuming Service A");
            return "I am Service A";
        }
    }
}
