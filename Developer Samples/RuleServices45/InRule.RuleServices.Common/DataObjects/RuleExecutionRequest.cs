using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace InRule.RuleServices.Common.DataObjects
{
    [XmlRoot(Namespace = Constants.XmlNamespace)]
    [DataContract(Namespace = Constants.XmlNamespace)]
    public class RuleExecutionRequest
    {
        public RuleExecutionRequest()
        {
            Parameters = new List<RuleExecutionParameter>();
            EndPointOverrides = new List<RuleExecutionEndPointOverride>();
        }

        [DataMember(IsRequired = true, Order = 0)]
        public string RuleApp { get; set; }

        [DataMember(IsRequired = false, Order = 1)]
        public string RuleSet { get; set; }

        [DataMember(IsRequired = false, Order = 2)]
        public List<RuleExecutionParameter> Parameters { get; set; }

        [DataMember(IsRequired = false, Order = 3)]
        public string Entity { get; set; }

        [DataMember(IsRequired=false, Order=4)]
        public string EntityXml { get; set; }

        [DataMember(IsRequired = false, Order = 5)]
        public string ReturnEntity { get; set; }

        [DataMember(IsRequired = false, Order = 6)]
        public string ResponseType { get; set; }

        [DataMember(IsRequired = false, Order = 7)]
        public List<RuleExecutionEndPointOverride> EndPointOverrides { get; set; }
    }
}