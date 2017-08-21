using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using System.Xml.Schema;
namespace AzureDecisionServiceWebRole
{
    [DataContract (Namespace="")]
    public class Request 
    {
        [DataMember]
        public string MortgageApplication { get; set; }
    }
    [DataContract(Namespace="")]
    public class Response
    {
        [DataMember]
        public string MortgageApplication { get; set; }
    } 
    [ServiceContract]
    [XmlSerializerFormat]
    public interface IMortgageDecision
    {
        //REST
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml,  BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/presentXML")]
        Response process(Request req);  
    }
}
