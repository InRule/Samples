using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using InRule.RuleServices.Common.DataObjects;

namespace InRule.RuleServices.Common.Connectivity
{
    public class RuleClient : WebClient
    {
        public static string GetRuleServiceUri()
        {
            var serviceUri = ConfigurationManager.AppSettings[Constants.InRuleRuleServiceUri];
            if (String.IsNullOrWhiteSpace(serviceUri))
            {
                throw new Exception(
                    "The service URI could not be located in the web.config file. Please add an AppSetting for 'RuleServiceUri' and set its value.");
            }
            return serviceUri;
        }

        public string GetApplyRulesResponse(string ruleApp, string entity, string entityXml, string returnEntity, string responseType)
        {
            var requestUri = String.Format(Constants.AppyRulesUriTemplate,
                                               GetRuleServiceUri(),
                                               ruleApp, entity, entityXml, returnEntity, responseType);

            return DownloadString(requestUri);
        }

        public RuleExecutionResponse ExecuteRuleRequest(RuleExecutionRequest request)
        {
            var requestUri = String.Format(Constants.ExecuteRuleRequestTemplate, GetRuleServiceUri());
            Headers.Add("Content-Type", "text/xml");

            return GetObjectFromXml<RuleExecutionResponse>(UploadData(requestUri, GetXmlBytesFromObject(request)));
        }

        public static string GetXmlStringFromObject(object o)
        {
            return Encoding.UTF8.GetString(GetXmlBytesFromObject(o));
        }

        public static byte[] GetXmlBytesFromObject(object o)
        {
            var serializer = new XmlSerializer(o.GetType(), Constants.XmlNamespace);
            using (var ms = new MemoryStream())
            {
                using (var sw = new XmlTextWriter(ms, Encoding.UTF8))
                {
                    serializer.Serialize(sw, o);
                    return ms.GetBuffer();
                }
            }
        }

        public T GetObjectFromXml<T>(byte[] xml) where T : class
        {
            var serializer = new XmlSerializer(typeof(T), Constants.XmlNamespace);
            using (var ms = new MemoryStream(xml))
            {
                using (var r = new XmlTextReader(ms))
                {
                    return serializer.Deserialize(r) as T;
                }
            }
        }
        
        public string GetExecuteRuleSetResponse(string ruleApp, string ruleSet, string entity, string entityXml,
            string returnEntity, string responseType, string parameterString)
        {
            var requestUri = String.Format(Constants.ExecuteRuleSetUriTemplate,
                GetRuleServiceUri(),
                ruleApp,
                ruleSet,
                entity,
                entityXml,
                returnEntity,
                responseType
                );

            requestUri += parameterString;

            return DownloadString(requestUri);
        }


        public int Timeout { get; set; }

        public RuleClient()
        {
            Timeout = 60000;
        }

        public RuleClient(int timeout)
        {
            Timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            request.Timeout = Timeout;
            return request;
        }
    }
}


