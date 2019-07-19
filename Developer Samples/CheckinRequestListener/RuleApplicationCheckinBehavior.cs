using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace CheckinRequestListener
{
    public class RuleApplicationCheckinBehavior : BehaviorExtensionElement, IEndpointBehavior
    {
        protected override object CreateBehavior()
        {
            return new RuleApplicationCheckinBehavior();
        }
        public override Type BehaviorType => typeof(RuleApplicationCheckinBehavior);
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            foreach (var operation in endpointDispatcher.DispatchRuntime.Operations)
            {
                operation.ParameterInspectors.Add(new RuleApplicationParameterInspector());
            }
        }
        #region Unused Interface Methods
        public void Validate(ServiceEndpoint endpoint)
        {
        }
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }
        #endregion
    }
}