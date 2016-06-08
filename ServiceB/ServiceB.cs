using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceB.Contract;
using ServiceCommonLib;

namespace ServiceB
{
    public class ServiceB : IServiceB
    {
        [CentralSessionDataContextBehavior]
        public string DoWork()
        {
            System.Console.WriteLine("Consuming Service B");
            return "I am Service B";

        }
    }
}
