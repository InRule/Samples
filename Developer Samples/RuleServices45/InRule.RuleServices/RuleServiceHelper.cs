using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.ServiceModel.Web;
using System.Xml;
using System.Xml.Xsl;
using InRule.Repository;
using InRule.Repository.EndPoints;
using InRule.RuleServices;
using InRule.RuleServices.Common.DataObjects;
using InRule.Runtime;
using Notification = InRule.RuleServices.Common.DataObjects.Notification;
using Validation = InRule.RuleServices.Common.DataObjects.Validation;

namespace InRule.RuleServices
{
    public static class RuleServiceHelper
    {
        public const string XmlNamespace = "http://inrule.com/RuleServices";
        public const string ApplicationName = "InRule Execution Service";
        public const string ResponseContentType = "text/xml"; // for REST use text/xml for SOAP use application/soap+xml
        public const string Delimeter = "|";

        public static string GetQueryParametersToString()
        {
            var queryParameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

            var sb = new StringBuilder();
            foreach (var key in queryParameters.AllKeys)
            {
                var value = queryParameters[key];
                sb.AppendFormat("{0}: {1}", key, value);
                sb.Append(" - ");
            }
            return sb.ToString();
        }

        public static string Serialize<T>(T objectToSerialize)
        {
            return Serialize(objectToSerialize, String.Empty);
        }

        public static string Serialize<T>(T objectToSerialize, string overrideRootName)
        {
            if (ReferenceEquals(objectToSerialize, null))
            {
                if (string.IsNullOrEmpty(overrideRootName))
                {
                    return String.Empty;
                }
                return string.Format("<{0}/>", overrideRootName);
            }

            DataContractSerializer serializer;
            if (string.IsNullOrEmpty(overrideRootName))
            {
                serializer = new DataContractSerializer(objectToSerialize.GetType());
            }
            else
            {
                serializer = new DataContractSerializer(objectToSerialize.GetType(), overrideRootName, XmlNamespace);
            }

            var output = new StringWriter();
            using (var writer = new XmlTextWriter(output) {Formatting = Formatting.Indented})
            {
                serializer.WriteObject(writer, objectToSerialize);
            }
            return StripEncoding(output.GetStringBuilder().ToString());
        }

        public static string StripEncoding(string xml)
        {
            return xml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
        }

        public static void LogEvent(string message)
        {
            EventLog.WriteEntry(ApplicationName, message, EventLogEntryType.Information);
        }

        public static void LogException(Exception ex)
        {
            var message = String.Format("{0}\r\n\r\n{1}", ex.Message, ex);
            EventLog.WriteEntry(ApplicationName, message, EventLogEntryType.Error);
        }

        public static void HandleWebException(Exception ex, RuleExecutionResponse response)
        {
            try
            {
                LogException(ex);
            }
            catch (Exception logEx)
            {
                // do not let logging failures affect the ability to return a response
                response.Error += String.Format("\r\n\r\nError logging failed on server: {0}", logEx.Message);
            }

            if (WebOperationContext.Current.IncomingRequest.Method == "POST")
            {
                var debug = OperationContext.Current.Host.Description.Behaviors.Find<ServiceDebugBehavior>();
                var includeStackTrace = (debug != null && debug.IncludeExceptionDetailInFaults);
                response.Error = includeStackTrace ? ex.ToString() : ex.Message;

                //WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
                //WebOperationContext.Current.OutgoingResponse.StatusDescription = WebUtility.HtmlEncode(ex.Message.Replace("\n", " ").Replace("\r", " "));
                WebOperationContext.Current.OutgoingResponse.ContentType = ResponseContentType;
            }
            else
            {
                // rethrow on GET verb
                throw ex;
            }
        }

        public static void OverrideEndPoints(RuleSession session, RuleExecutionRequest request)
        {
            if (request != null && request.EndPointOverrides != null && request.EndPointOverrides.Count > 0)
            {
                // apply each submitted override
                foreach (var overrideRequest in request.EndPointOverrides)
                {
                    // attempt to lookup the endpoint in the rule application
                    var targetEndPoint = session.GetRuleApplicationDef().EndPoints[overrideRequest.EndPointName];
                    if (targetEndPoint == null)
                    {
                        throw new Exception(String.Format("Cannot locate endpoint to override for '{0}'", overrideRequest.EndPointName));
                    }

                    // apply each overridden property
                    foreach (var prop in overrideRequest.Properties)
                    {
                        AssignEndPointOverrideProperty(session, targetEndPoint, prop);
                    }
                }
            }
        }

        private static void AssignEndPointOverrideProperty(RuleSession session, EndPointDef endPointDef, RuleExecutionEndPointProperty property)
        {
            if (String.IsNullOrEmpty(property.Name))
            {
                throw new Exception(String.Format("No name was provided for a property override on endpoint '{0}'",
                    endPointDef.Name));
            }

            // check the type of the end point
            var endPointType = endPointDef.GetType();

            if (endPointType == typeof (DatabaseConnection))
            {
                switch (property.Name.ToLower())
                {
                    case "connectionstring":
                       session.Overrides.OverrideDatabaseConnection(endPointDef.Name, property.Value);
                       return;
                }
            }
            else if (endPointType == typeof (WebServiceDef))
            {
                switch (property.Name.ToLower())
                {
                    case "address":
                        session.Overrides.OverrideWebServiceAddress(endPointDef.Name, property.Value);
                        return;
                    case "certificate":
                        session.Overrides.OverrideWebServiceCertificate(endPointDef.Name, property.Value);
                        return;
                    case "wsdladdress":
                        session.Overrides.OverrideWebServiceWsdlAddress(endPointDef.Name, property.Value);
                        return;
                }
            }
            else if (endPointType == typeof (SendMailServerDef))
            {
                switch (property.Name.ToLower())
                {
                    case "address":
                        session.Overrides.OverrideMailServerConnection(endPointDef.Name, property.Value);
                        return;
                }
            }

            // throw down here, unsupported
            throw new Exception(String.Format("The property '{0}' on endpoint type '{1}' is currently not supported for override.", property.Name, endPointType.Name));
        }

        public static Entity GetEntityFromRuleSession(RuleSession session, string entityName)
        {
            // this returns the first matching entity by name
            return (from e in session.GetEntities()
                         where e.Name == entityName
                         select e).FirstOrDefault();
        }

        public static string GetReturnEntityXml(RuleExecutionRequest request, RuleSession session, Entity entity)
        {
            var xml = string.Empty;

            if (String.IsNullOrEmpty(request.ReturnEntity))
            {
                xml = GetEntityXml(entity);
            }
            else
            {
                var returnEntity = GetEntityFromRuleSession(session, request.ReturnEntity);
                if (returnEntity != null)
                {
                    xml = GetEntityXml(entity);
                }
            }
            return xml;
        }

        public static string GetEntityXml(Entity entity)
        {
            var xml = entity.GetXml();
            return StripEncoding(xml);
        }

        public static string GetRuleExecutionReport(RuleSession session)
        {
            return GetRuleExecutionReport(session, null, string.Empty, false, false, 100000);
        }

        public static string GetRuleExecutionReport(RuleSession session, Entity entity)
        {
            return GetRuleExecutionReport(session, entity, string.Empty, false, false, 100000);
        }

        public static string GetRuleExecutionReport(RuleSession session, Entity entity, string optionalMessages, bool showImages, bool disableOutputEscaping, int maxStateLength)
        {
            var sb = new StringBuilder();
            if (session.LastRuleExecutionLog.HasErrors)
            {
                sb.Append(GetRuntimeErrorsReport(session, optionalMessages, true, showImages, disableOutputEscaping));
                sb.Append(GetExecutionLogReport(session, optionalMessages, false, showImages, disableOutputEscaping));
            }
            else
            {
                sb.Append(GetExecutionLogReport(session, optionalMessages, true, showImages, disableOutputEscaping));
            }

            sb.Append(GetNotificationReport(session, optionalMessages, false, showImages, disableOutputEscaping, false));
            sb.Append(GetValidationReport(session, optionalMessages, false, showImages, disableOutputEscaping));

            if (entity != null)
                sb.Append(GetStateReport(session, entity, optionalMessages, false, showImages, disableOutputEscaping, maxStateLength));

            return sb.ToString();
        }

        public static string GetRuntimeErrorsReport(RuleSession session, string optionalMessages, bool showReportHead, bool showImages, bool disableOutputEscaping)
        {
            var xml = GetRuntimeErrorsXml(session.LastRuleExecutionLog.ErrorMessages);
            return GetReportHtml(xml, "Runtime Execution Errors", session.GetRuleApplicationDef().Name, optionalMessages, showReportHead, showImages, disableOutputEscaping);
        }

        public static string GetExecutionLogReport(RuleSession session, string optionalMessages, bool showReportHead, bool showImages, bool disableOutputEscaping)
        {
            var xml = GetExecutionLogXml(session.LastRuleExecutionLog);
            return GetReportHtml(xml, "Execution Log", session.GetRuleApplicationDef().Name, optionalMessages, showReportHead, showImages, disableOutputEscaping);
        }

        public static string GetExecutionLogText(RuleSession session)
        {
            var s = new StringBuilder();
            foreach (var msg in session.LastRuleExecutionLog.AllMessages)
            {
                s.AppendLine(msg.Description);
            }
            return s.ToString();
        }

        public static string GetNotificationReport(RuleSession session, string optionalMessages, bool showReportHead, bool showImages, bool disableOutputEscaping, bool parseLabelFromMessage)
        {
            var xml = GetNotificationLogXml(session, false, parseLabelFromMessage);
            return GetReportHtml(xml, "Notification Log", session.GetRuleApplicationDef().Name, optionalMessages, showReportHead, showImages,
                                 disableOutputEscaping);
        }

        public static string GetNotificationText(RuleSession session)
        {
            var s = new StringBuilder();
            foreach (var note in session.GetNotifications())
            {
                s.AppendLine(note.Message);
            }
            return s.ToString();
        }

        public static string GetValidationReport(RuleSession session, string optionalMessages, bool showReportHead, bool showImages, bool disableOutputEscaping)
        {
            string xml = GetValidationLogXml(session);
            return GetReportHtml(xml, "Validation Log", session.GetRuleApplicationDef().Name, optionalMessages, showReportHead, showImages, disableOutputEscaping);
        }

        public static string GetValidationText(RuleSession session)
        {
            var s = new StringBuilder();
            foreach (var validation in session.GetValidations())
            {
                s.AppendLine(validation.Target + " - " + validation.Message);
            }
            return s.ToString();
        }
        
        public static string GetStateReport(RuleSession session, Entity entity, string optionalMessages, bool showReportHead, bool showImages, bool disableOutputEscaping, int maxStateLength)
        {
            string xml;
            var sectionName = "State Report: " + entity.Name;

            try
            {
                xml = entity.GetXml();
                if (xml.Length > maxStateLength)
                {
                    xml = "<EmptyReport></EmptyReport>";
                    sectionName += " - The state report exceeded the max length: " + maxStateLength;
                }
            }
            catch (Exception ex)
            {
                xml = "<EmptyReport></EmptyReport>";
                sectionName += ex.Message;
            }

            if (xml.Length > maxStateLength)
            {
                xml = "<EmptyReport>The state report exceeded the max length -" + maxStateLength + "</EmptyReport>";
            }

            xml = xml.Replace(@"encoding=""utf-16""", @"encoding=""utf-8""");

            return GetReportHtml(xml, sectionName, session.GetRuleApplicationDef().Name, optionalMessages, showReportHead, showImages, disableOutputEscaping);
        }

        public static string GetPerformanceStatisticsReport(RuleSession session)
        {
            return session.LastRuleExecutionLog.GetHtml();
        }

        public static string GetRuntimeErrorsXml(Collection<ErrorLogMessage> errors)
        {
            var sw = new StringWriter();
            var xw = new XmlTextWriter(sw);

            using (xw)
            {
                xw.WriteStartElement("RuntimeErrorLog");
                foreach (var message in errors)
                {
                    xw.WriteElementString(message.SourceElementId, message.Description);
                }
                xw.WriteEndElement();
                return sw.ToString();
            }
        }

        public static string GetExecutionLogXml(RuleExecutionLog executionLog)
        {
            var sw = new StringWriter();
            var xw = new XmlTextWriter(sw);

            using (xw)
            {
                xw.WriteStartElement("ExecutionLog");
                foreach (var message in executionLog.AllMessages)
                {
                    xw.WriteElementString("ExecutionMessage", message.Description);
                }
                xw.WriteEndElement();
                return sw.ToString();
            }
        }

        public static string GetNotificationLogXml(RuleSession session, bool includeMetadata, bool parseLabelFromMessage)
        {
            var sw = new StringWriter();
            var xw = new XmlTextWriter(sw);
            var notifications = session.GetNotifications(true);

            using (xw)
            {
                xw.WriteStartElement("NotificationLog");

                if (notifications != null)
                {
                    foreach (var notification in notifications)
                    {
                        var sb = new StringBuilder();

                        var label = string.Empty;
                        var message = notification.Message;

                        if (parseLabelFromMessage)
                        {
                            var parts = message.Split(":".ToCharArray(), 2);
                            if (parts.Length == 2)
                            {
                                label = parts[0].Replace(" ", "_");
                                message = parts[1];
                            }
                        }

                        sb.Append(message);

                        if (includeMetadata)
                        {
                            sb.Append("  ----> TYPE: " + notification.Type);

                            if (notification.AssociatedField != null)
                                sb.Append(" ASSOCIATED FIELD: " + notification.AssociatedField);

                            sb.Append(" KEY: " + notification.ElementId);
                        }

                        if (String.IsNullOrEmpty(label) && !String.IsNullOrWhiteSpace(notification.AssociatedField))
                        {
                            label = session.GetElement(notification.AssociatedField).Name;
                        }
                        
                        xw.WriteElementString(label, sb.ToString());
                    }
                }
                xw.WriteEndElement();

                return sw.ToString();
            }
        }

        public static string GetValidationLogXml(RuleSession session)
        {
            var sw = new StringWriter();
            var xw = new XmlTextWriter(sw);

            var validations = session.GetValidations(true);

            using (xw)
            {
                xw.WriteStartElement("ValidationLog");
                foreach (var validation in validations)
                {
                    var label = session.GetElement(validation.Target).Name;
                    xw.WriteElementString(label, validation.Message);
                }
                xw.WriteEndElement();
                return sw.ToString();
            }
        }

        public static string GetReportHtml(string reportXml, string reportName, string ruleAppName, string optionalMessages, bool showReportHead, bool showImages, bool disableOutputEscaping)
        {
            var args = new XsltArgumentList();
            args.AddParam("ruleAppName", "", ruleAppName);
            args.AddParam("sectionName", "", reportName);
            args.AddParam("reportDateTime", "", DateTime.Now.ToString());
            args.AddParam("messages", "", optionalMessages);
            args.AddParam("showReportHead", "", showReportHead);
            args.AddParam("showSectionHead", "", true);
            args.AddParam("showImages", "", showImages);
            args.AddParam("disableOutputEscaping", "", disableOutputEscaping);

            return GetReportHtml(reportXml, args);
        }

        public static string GetReportHtml(string contentXml, XsltArgumentList args)
        {
            var reportTemplate = GetReportTemplateStream();
            using (var sr = new StreamReader(reportTemplate))
            {
                return TransformToString(contentXml, sr.ReadToEnd(), args);
            }
        }

        public static Stream GetReportTemplateStream()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("InRule.RuleServices.Resources.RuleExecutionReport.xsl");
        }

        public static string TransformToString(XmlDocument xmlDoc, XslCompiledTransform xslDoc, XsltArgumentList args)
        {
            // create navigator
            var nav = xmlDoc.CreateNavigator();

            // create a stream
            var stream = new MemoryStream();

            xslDoc.Transform(nav, args, stream);

            // required flushing
            stream.Flush();
            stream.Position = 0;

            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd().Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "").Replace("\r\n", "").Replace("'", "\"");
            }
        }

        public static string TransformToString(string xml, string xsl, XsltArgumentList args)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            var xslTransform = new XslCompiledTransform();
            xslTransform.Load(new XmlTextReader(new StringReader(xsl)));

            return TransformToString(xmlDoc, xslTransform, args);
        }

        public static void SetResponseText(RuleSession session, Entity entity, RuleExecutionResponse response)
        {
            response.ResponseText = "";
            var isPost = WebOperationContext.Current.IncomingRequest.Method == "POST";

            // force local cache expiration for future requests
            WebOperationContext.Current.OutgoingResponse.Headers.Add(HttpResponseHeader.Expires, DateTime.Now.ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"));
            WebOperationContext.Current.OutgoingResponse.Headers.Add(HttpResponseHeader.CacheControl, "no-cache");

            var responseTypeEnum = ParseResponseType(response);

            // validate response types and set content type
            bool useResponseContainer;
            string contentType;
            ProcessResponseTypes(isPost, response, responseTypeEnum, out useResponseContainer, out contentType);

            if (responseTypeEnum.HasFlag(RuleExecutionResponseType.EntityXml))
            {
                if (entity != null)
                {
                    var xml = GetEntityXml(entity);
                    if (useResponseContainer || isPost)
                    {
                        response.EntityXml = xml;
                    }
                    else
                    {
                        response.ResponseText = xml;
                    }
                }
                else
                {
                    response.ResponseText = "Warning: Entity is null or not found";
                }
            }

            if (responseTypeEnum.HasFlag(RuleExecutionResponseType.NotificationsXml))
            {
                SetResponseNotifications(session, response);
                if (!useResponseContainer)
                {
                    response.ResponseText = Serialize(response.Notifications, "Notifications");
                }
            }

            if (responseTypeEnum.HasFlag(RuleExecutionResponseType.ValidationsXml))
            {
                SetResponseValidations(session, response);
                if (!useResponseContainer)
                {
                    response.ResponseText = Serialize(response.Validations, "Validations");
                }
            }

            if (responseTypeEnum.HasFlag(RuleExecutionResponseType.ExecutionLogXml))
            {
                var xml = GetExecutionLogXml(session.LastRuleExecutionLog);
                if (useResponseContainer || isPost)
                {
                    response.ExecutionLogXml = xml;
                }
                else
                {
                    response.ResponseText = xml;
                }
            }

            if (responseTypeEnum.HasFlag(RuleExecutionResponseType.NotificationsText))
            {
                response.ResponseText += GetNotificationText(session);
            }

            if (responseTypeEnum.HasFlag(RuleExecutionResponseType.ValidationsText))
            {
                response.ResponseText += GetValidationText(session);
            }

            if (responseTypeEnum.HasFlag(RuleExecutionResponseType.ExecutionLogText))
            {
                response.ResponseText = GetExecutionLogText(session);
            }

            if (responseTypeEnum.HasFlag(RuleExecutionResponseType.RuleExecutionResponseXml) || responseTypeEnum.HasFlag(RuleExecutionResponseType.NotSpecified))
            {
                // always uses container
                response.EntityXml = GetEntityXml(entity);
                response.ExecutionLogXml = GetExecutionLogXml(session.LastRuleExecutionLog);
                SetResponseNotifications(session, response);
                SetResponseValidations(session, response);
            }

            if (responseTypeEnum.HasFlag(RuleExecutionResponseType.RuleExecutionReport))
            {
                response.ResponseText = GetRuleExecutionReport(session, entity);
            }

            if (responseTypeEnum.HasFlag(RuleExecutionResponseType.PerformanceStatisticsReport))
            {
                response.ResponseText = GetPerformanceStatisticsReport(session);
            }

            if (responseTypeEnum.HasFlag(RuleExecutionResponseType.Invalid))
            {
                response.ResponseText = "Warning: Invalid ResponseType Specified";
                contentType = ResponseContentType;
            }

            if (WebOperationContext.Current != null)
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = contentType;
            }

            if (useResponseContainer && !isPost)
            {
                response.ResponseText = Serialize(response);
            }
        }

        private static RuleExecutionResponseType ParseResponseType(RuleExecutionResponse response)
        {
            // take response type string and convert it to enum

            var responseTypeEnum = (RuleExecutionResponseType)0;

            if (String.IsNullOrEmpty(response.ResponseType))
            {
                responseTypeEnum = RuleExecutionResponseType.NotSpecified;
                response.ResponseType = RuleExecutionResponseType.NotSpecified.ToString();
            }
            else
            {
                var responseTypesList = response.ResponseType.Split(Delimeter.ToCharArray()).ToList();
                foreach (var responseType in responseTypesList)
                {
                    if (!Enum.IsDefined(typeof(RuleExecutionResponseType), responseType))
                    {
                        throw new Exception("The requested response type is not a valid option.  Invalid response type: " + responseType);
                    }
                    responseTypeEnum |= (RuleExecutionResponseType)Enum.Parse(typeof(RuleExecutionResponseType), responseType);
                }
            }

            return responseTypeEnum;
        }


        private static void ProcessResponseTypes(bool isPost, RuleExecutionResponse response, RuleExecutionResponseType responseTypeEnum, out bool useResponseContainer, out string contentType)
        {
            var responseTypeCount = (response.ResponseType == null) ? 0 : response.ResponseType.Split(Delimeter.ToCharArray()).Length;
            var usingXml = false;
            var usingReport = false;
            useResponseContainer = false;

            // if it is a post, we always return the RuleExecutionResponse
            if (isPost)
            {
                contentType = ResponseContentType;
                useResponseContainer = true;
                return;
            }

            if (
                responseTypeEnum.HasFlag(RuleExecutionResponseType.NotificationsXml) ||
                responseTypeEnum.HasFlag(RuleExecutionResponseType.EntityXml) ||
                responseTypeEnum.HasFlag(RuleExecutionResponseType.ValidationsXml) ||
                responseTypeEnum.HasFlag(RuleExecutionResponseType.RuleExecutionResponseXml) ||
                responseTypeEnum.HasFlag(RuleExecutionResponseType.NotSpecified)
                )
            {
                usingXml = true;
            }

            if (
                responseTypeEnum.HasFlag(RuleExecutionResponseType.NotificationsText) ||
                responseTypeEnum.HasFlag(RuleExecutionResponseType.PerformanceStatisticsReport) ||
                responseTypeEnum.HasFlag(RuleExecutionResponseType.RuleExecutionReport) ||
                responseTypeEnum.HasFlag(RuleExecutionResponseType.ValidationsText)
                )
            {
                usingReport = true;
            }

            // special case for allowing notification and validation text
            if (responseTypeEnum.HasFlag(RuleExecutionResponseType.NotificationsText) &&
                responseTypeEnum.HasFlag(RuleExecutionResponseType.ValidationsText) && responseTypeCount == 2)
            {
                contentType = ResponseContentType;
                return;
            }

            if ((usingXml && usingReport) || (usingReport && responseTypeCount > 1))
            {
                throw new Exception("The requested response types are not a valid combination.  Invalid response types: " + response.ResponseType.Replace(Delimeter, ", "));
            }

            contentType = usingReport ? "text/html" : ResponseContentType;
            if (responseTypeCount > 1 || responseTypeEnum.HasFlag(RuleExecutionResponseType.RuleExecutionResponseXml) || responseTypeEnum.HasFlag(RuleExecutionResponseType.NotSpecified))
            {
                useResponseContainer = true;
            }
        }

        public static void SetResponseNotifications(RuleSession session, RuleExecutionResponse response)
        {
            if (session.GetNotifications().Count > 0)
            {
                response.Notifications = new List<Notification>();
                foreach (var note in session.GetNotifications())
                {
                    var notification = new Notification { Message = note.Message, Type = note.Type.ToString() };
                    response.Notifications.Add(notification);
                }
            }
        }

        public static void SetResponseValidations(RuleSession session, RuleExecutionResponse response)
        {
            if (session.GetValidations().Count > 0)
            {
                response.Validations = new List<Validation>();
                foreach (var val in session.GetValidations())
                {
                    var validation = new Validation { Message = val.Target + " - " + val.Message };
                    response.Validations.Add(validation);
                }
            }
        }
    }

    public static class NameValueCollectionExtensions
    {
        public static bool ContainsKey(this NameValueCollection collection, string key)
        {
            if (collection.Get(key) == null)
            {
                return collection.AllKeys.Contains(key);
            }

            return true;
        } 
    }
}