using System.Runtime.Serialization;

namespace InRule.RuleServices.Common.DataObjects
{
    [DataContract(Namespace = Constants.XmlNamespace)]
    public class RuleExecutionParameter
    {
        [DataMember(IsRequired = true, Order = 0)]
        public string Name { get; set; }

        [DataMember(IsRequired = true, Order = 1)]
        public string Value { get; set; }
    }
}