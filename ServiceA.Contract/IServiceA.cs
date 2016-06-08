using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace ServiceA.Contract
{
    [ServiceContract]
    public interface IServiceA
    {
        [OperationContract]
        string DoWork();
    }
}
