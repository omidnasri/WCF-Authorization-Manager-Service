using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using GlobalCommonLib;

namespace ClientCommonLib
{

    public class CentralSessionEndpointBehavior : IEndpointBehavior
    {
        public CentralSessionEndpointBehavior()
        {

        }
        public CentralSessionEndpointBehavior(string user, string password)
        {
            this.User = user;
            this.Password = password;
        }
        public void AddBindingParameters(ServiceEndpoint serviceEndpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        { }
        public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, System.ServiceModel.Dispatcher.ClientRuntime behavior)
        {
            // Add the inspector
            behavior.MessageInspectors.Add(new CentralSessionClientMessageInspector(this.User, this.Password));
        }
        public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        { }
        public void Validate(ServiceEndpoint serviceEndpoint)
        { }
        public string User { get; set; }
        public string Password { get; set; }
    }

    public class CentralSessionClientMessageInspector : BehaviorExtensionElement, IClientMessageInspector
    {
        public CentralSessionClientMessageInspector()
        {

        }
        public CentralSessionClientMessageInspector(string user, string password)
        {
            this.User = user;
            this.Password = password;
        }
        public static event EventHandler<CentralSessionDataEventArgs<System.ServiceModel.Channels.Message>> PreRequestingService;
        private void InvokePreRequestingService(CentralSessionDataEventArgs<System.ServiceModel.Channels.Message> e)
        {
            EventHandler<CentralSessionDataEventArgs<System.ServiceModel.Channels.Message>> handler = PreRequestingService;
            if (handler != null) handler(this, e);
        }
        public static event EventHandler<CentralSessionDataEventArgs<System.ServiceModel.Channels.Message>> PostRequestingService;
        private void InvokePostRequestingService(CentralSessionDataEventArgs<System.ServiceModel.Channels.Message> e)
        {
            EventHandler<CentralSessionDataEventArgs<System.ServiceModel.Channels.Message>> handler = PostRequestingService;
            if (handler != null) handler(this, e);
        }
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private string _user;
        public string User
        {
            get { return _user; }
            set { _user = value; }
        }
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        public override Type BehaviorType
        {
            get { return typeof(CentralSessionEndpointBehavior); }
        }
        protected override object CreateBehavior()
        {
            return new CentralSessionEndpointBehavior();
        }

        #region IClientMessageInspector Members
        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            InvokePostRequestingService(new CentralSessionDataEventArgs<Message>(reply));
        }
        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel)
        {
            CredentialHelper.SetSessionData(_user, _password, ref request);
            InvokePreRequestingService(new CentralSessionDataEventArgs<Message>(request));
            return null;
        }

        #endregion
    }

    public class CentralSessionDataEventArgs<TData> : EventArgs where TData : class
    {
        readonly TData _data;
        public CentralSessionDataEventArgs(TData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            this._data = data;
        }
        public TData Data
        {
            get { return _data; }
        }
        public override string ToString()
        {
            return _data.ToString();
        }
    }
}