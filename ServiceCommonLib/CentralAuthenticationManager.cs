using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Policy;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using GlobalCommonLib;
using static System.Console;

namespace ServiceCommonLib
{
    public class CentralUserIdentity : IIdentity
    {
        private readonly CentralRequestSession _session;
        public CentralUserIdentity(CentralRequestSession session)
        {
            this._session = session;
        }
        public string Name => _session.UserName;
        public string AuthenticationType => "Central";
        public bool IsAuthenticated => true;
        public CentralRequestSession Session => _session;
    }
    public class CentralPrincipal : IPrincipal
    {
        readonly IIdentity _identity;
        string[] _roles = null;
        public CentralPrincipal(IIdentity identity)
        {
            this._identity = identity;
        }
        public static CentralPrincipal Current => Thread.CurrentPrincipal as CentralPrincipal;
        public IIdentity Identity { get { return _identity; } }
        public string[] Roles
        {
            get
            {
                //Findout Role and set here 
                return _roles;
            }
        }
        public bool IsInRole(string role)
        {
            //Findout Role and set here 
            return true;
        }
    }
    
    public class ServiceAuthorizationManager : System.ServiceModel.ServiceAuthorizationManager
    {
        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            return true;
        }
    }
}
namespace ServiceCommonLib.Test
{
    public class CentralAuthenticationManager : ServiceAuthenticationManager
    {
        public override ReadOnlyCollection<IAuthorizationPolicy> Authenticate(ReadOnlyCollection<IAuthorizationPolicy> authPolicy, Uri listenUri, ref Message message)
        {
            var session = CredentialHelper.GetSessionData(message);
            CheckCredentials(session);
            var identity = new CentralUserIdentity(session);
            IPrincipal user = new CentralPrincipal(identity);
            message.Properties["Principal"] = user;
            return authPolicy;
        }
        public void CheckCredentials(CentralRequestSession credentials)
        {
            WriteLine("Checking Credentils for {0}..........", credentials.UserName);
            // check the user and password against a database; 
            // if not match 
            // throw new AuthenticationException("Incorrect credentials!");        
            WriteLine("{0} is Valid!!", credentials.UserName);
        }
    }
}