using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace GlobalCommonLib
{
    public class CredentialHelper
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const string HnForUserName = "UserName";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const string HNamespaceForUserName = @"http://UserName.url";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const string HnForPassword = "Password";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const string HNamespaceForPassword = @"http://Password.url";

        public static CentralRequestSession GetSessionData(Message request)
        {
            var user = string.Empty;
            var pass = string.Empty;
            if (request.Headers.Any(h => h.Name.Equals(HnForUserName)))
                user = request.Headers.GetHeader<string>(HnForUserName, HNamespaceForUserName, HnForUserName);
            if (request.Headers.Any(h => h.Name.Equals(HnForPassword)))
                pass = request.Headers.GetHeader<string>(HnForPassword, HNamespaceForPassword, HnForPassword);
            return new CentralRequestSession(user, pass);
        }

        public static void SetSessionData(string userName, string password, ref  Message request)
        {
            var userHeader = new MessageHeader<string> { Actor = HnForUserName, Content = userName };
            //Creating an untyped header to add to the WCF context
            System.ServiceModel.Channels.MessageHeader unTypedHeaderForUser = userHeader.GetUntypedHeader(HnForUserName, HNamespaceForUserName);
            //Add the header to the current request
            request.Headers.Add(unTypedHeaderForUser);

            var passwordHeader = new MessageHeader<string> { Actor = HnForPassword, Content = password };
            //Creating an untyped header to add to the WCF context
            System.ServiceModel.Channels.MessageHeader unTypedHeaderForPassword = passwordHeader.GetUntypedHeader(HnForPassword, HNamespaceForPassword);
            //Add the header to the current request
            request.Headers.Add(unTypedHeaderForPassword);
        }
    }
}
