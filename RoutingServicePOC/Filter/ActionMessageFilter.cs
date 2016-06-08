using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace RoutingServicePOC.Filter
{
    public class ActionMessageFilter : MessageFilter
    {
        public string WildcardExpression { get; set; }

        public ActionMessageFilter(string wildcardExpression)
        {
            this.WildcardExpression = wildcardExpression;
        }

        public override bool Match(System.ServiceModel.Channels.Message message)
        {
            var url = message.Headers.Action.Substring(0, message.Headers.Action.LastIndexOf(@"/"));
            var urlNodes = url.Split(new string[] { @"/", "//" }, StringSplitOptions.RemoveEmptyEntries);
            return Array.Exists(urlNodes, s => s.TrimStart().TrimEnd().Equals(WildcardExpression));
        }

        public override bool Match(System.ServiceModel.Channels.MessageBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}