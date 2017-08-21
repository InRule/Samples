using System.Collections.Generic;
using System.Runtime.Serialization;

namespace InRule.RuleServices.Common.DataObjects
{
    [DataContract(Namespace = Constants.XmlNamespace)]
    public class RuleExecutionEndPointOverride
    {
        public RuleExecutionEndPointOverride() { Properties = new List<RuleExecutionEndPointProperty>();}

        [DataMember(IsRequired = true, Order = 0)]
        public string EndPointName { get; set; }

        [DataMember(IsRequired = true, Order = 1)]
        public List<RuleExecutionEndPointProperty> Properties { get; set; }
    }
}