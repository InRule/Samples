using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace InRule.RuleServices.Common.DataObjects
{
    [XmlRoot(Namespace = Constants.XmlNamespace)]
    [DataContract(Namespace = Constants.XmlNamespace)]
    public class RuleExecutionResponse
    {
        [DataMember(IsRequired = true, Order = 0)]
        public string ResponseText { get; set; }

        [DataMember(IsRequired = false, Order = 1)]
        public string EntityXml { get; set; }

        [DataMember(IsRequired = false, Order = 2)]
        public string Error { get; set; }

        [DataMember(IsRequired = false, Order = 3)]
        public List<Notification> Notifications { get; set; }

        [DataMember(IsRequired = false, Order=4)]
        public List<Validation> Validations { get; set; }

        [DataMember(IsRequired = false, Order = 5)]
        public string ExecutionLogXml { get; set; }

        [DataMember(IsRequired = false, Order = 6)]
        public string ResponseType { get; set; }
    }

    [Flags]
    public enum RuleExecutionResponseType
    {
        None = 0,
        EntityXml = 1,
        NotificationsXml = 2,
        NotificationsText = 4,
        ValidationsXml = 8,
        ValidationsText = 16,
        ExecutionLogXml = 32,
        ExecutionLogText = 64,
        PerformanceStatisticsReport = 128,
        RuleExecutionReport = 256,
        RuleExecutionResponseXml = 512,
        Invalid = 1024,
        NotSpecified = 2048
    }

    [DataContract(Namespace = Constants.XmlNamespace)]
    public class Notification
    {
        [DataMember(IsRequired = true, Order = 0)]
        public string Type { get; set; }

        [DataMember(IsRequired = true, Order = 1)]
        public string Message { get; set; }
    }

    [DataContract(Namespace = Constants.XmlNamespace)]
    public class Validation
    {
        [DataMember(IsRequired = true, Order = 0)]
        public string Message { get; set; }
    }
}