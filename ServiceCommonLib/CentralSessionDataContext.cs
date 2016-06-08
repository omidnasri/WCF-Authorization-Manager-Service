using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using GlobalCommonLib;

namespace ServiceCommonLib
{
    public class CentralSessionDataContext : IExtension<OperationContext>
    {
        // The "current" custom context
        public static CentralSessionDataContext Current
        {
            get { return OperationContext.Current.Extensions.Find<CentralSessionDataContext>(); }
        }

        #region IExtension<OperationContext> Members


        public void Attach(OperationContext owner)
        {
            //no-op
        }

        public void Detach(OperationContext owner)
        {
            //no-op
        }

        #endregion

        //You can have lots more of these -- this is the stuff that you
        //want to store on your custom context
        public CentralRequestSession RequestSession { get; set; }
    }

    public class CentralSessionDataContextBehaviorAttribute : Attribute, IServiceBehavior, IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request,
                                          IClientChannel channel,
                                          InstanceContext instanceContext)
        {
            var ctx = new CentralSessionDataContext();
            OperationContext.Current.Extensions.Add(ctx);
            return request.Headers.MessageId;
        }
        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            OperationContext.Current.Extensions.Remove(CentralSessionDataContext.Current);
        }

        #region IServiceBehavior Members
        public void AddBindingParameters(ServiceDescription serviceDescription,
                                         ServiceHostBase serviceHostBase,
                                         System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints,
                                         BindingParameterCollection bindingParameters)
        {
            //no-op
        }
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher cd in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher ed in cd.Endpoints)
                {
                    ed.DispatchRuntime.MessageInspectors.Add(this);
                }
            }
        }
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            //no-op           
        }
        #endregion
    }

    public class CentralSessionDataContextProcessInspector : BehaviorExtensionElement, IEndpointBehavior, IDispatchMessageInspector
    {
        #region Overrides of BehaviorExtensionElement

        /// <summary>
        /// Creates a behavior extension based on the current configuration settings.
        /// </summary>
        /// <returns>
        /// The behavior extension.
        /// </returns>
        protected override object CreateBehavior()
        {
            return new CentralSessionDataContextProcessInspector();
        }

        /// <summary>
        /// Gets the type of behavior.
        /// </summary>
        /// <returns>
        /// B <see cref="T:System.Type"/>.
        /// </returns>
        public override Type BehaviorType
        {
            get { return typeof(CentralSessionDataContextProcessInspector); }
        }

        #endregion

        #region Implementation of IEndpointBehavior

        /// <summary>
        /// Implement to confirm that the endpoint meets some intended criteria.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }

        /// <summary>
        /// Implement to pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify.</param><param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {

        }

        /// <summary>
        /// Implements a modification or extension of the service across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param><param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
        }

        /// <summary>
        /// Implements a modification or extension of the client across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param><param name="clientRuntime">The client runtime to be customized.</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {

        }

        #endregion

        #region Implementation of IDispatchMessageInspector

        /// <summary>
        /// Called after an inbound message has been received but before the message is dispatched to the intended operation.
        /// </summary>
        /// <returns>
        /// The object used to correlate state. This object is passed back in the <see cref="M:System.ServiceModel.Dispatcher.IDispatchMessageInspector.BeforeSendReply(System.ServiceModel.Channels.Message@,System.Object)"/> method.
        /// </returns>
        /// <param name="request">The request message.</param><param name="channel">The incoming channel.</param><param name="instanceContext">The current service instance.</param>
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            CentralSessionDataContext.Current.RequestSession = CredentialHelper.GetSessionData(request);
            return true;
        }

        /// <summary>
        /// Called after the operation has returned but before the reply message is sent.
        /// </summary>
        /// <param name="reply">The reply message. This value is null if the operation is one way.</param><param name="correlationState">The correlation object returned from the <see cref="M:System.ServiceModel.Dispatcher.IDispatchMessageInspector.AfterReceiveRequest(System.ServiceModel.Channels.Message@,System.ServiceModel.IClientChannel,System.ServiceModel.InstanceContext)"/> method.</param>
        public void BeforeSendReply(ref Message reply, object correlationState)
        {

        }

        #endregion
    }
}