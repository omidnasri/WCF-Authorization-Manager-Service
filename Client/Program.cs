using System.ServiceModel;
using ClientCommonLib;

namespace Client
{
    class Program
    {
        static void Main()
        {
            var channelFactory = new ChannelFactory<ServiceA.Contract.IServiceA>("WSHttpBinding_IServiceA");
            channelFactory.Endpoint.Behaviors.Add(new CentralSessionEndpointBehavior("Uname", "Pass"));
            var proxy = channelFactory.CreateChannel();

            System.Console.WriteLine("Client: Calling Service A.......");
            var result = proxy.DoWork();
            System.Console.WriteLine("Service A Replied:" + result);


            System.Console.ReadLine();
        }
    }
}
