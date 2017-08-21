using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;
using InRule.RuleServices.Common.DataObjects;

namespace InRule.RuleServices
{
    [ServiceContract]
    public interface IRuleExecutionService
    {
        /* To use SOAP instead of REST, use an OperationContract here instead of WebInvoke, then change binding in the web.config file.
         * Change ResponseContentType in RuleServiceHelper.cs to application/soap+xml. Use DataContract formatter instead if XML serializer for SOAP. 
        [OperationContract] */
        [XmlSerializerFormat]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        RuleExecutionResponse ExecuteRuleRequest(RuleExecutionRequest request);
       
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        Stream ApplyRules(string ruleApp, string entity, string entityXml, string returnEntity, string responseType);

        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        Stream ExecuteRuleSet(string ruleApp, string ruleset, string entity, string entityXml, string returnEntity, string responseType);
    }
}
