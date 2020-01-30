using InRule.Repository.Service.Data.Requests;
using InRule.Repository.Service.Requests;
using System;
using System.Linq;
using System.ServiceModel.Dispatcher;
using System.Configuration;
using System.Xml;
using System.Collections.Generic;

namespace CheckinRequestListener
{
    public class RuleApplicationParameterInspector : IParameterInspector
    {
        public object BeforeCall(string operationName, object[] inputs)
        {
            if (string.Equals(operationName, "CheckinRuleApp", StringComparison.OrdinalIgnoreCase))
            {
                //RepositoryWebRequest request = (RepositoryWebRequest)inputs.FirstOrDefault();
                //var checkinRequest = request as CheckinRuleAppRequest;
                //Add your logic here to work with checkinRequest if desired
            }
            else if (string.Equals(operationName, "ApplyLabel", StringComparison.OrdinalIgnoreCase))
            {
                RepositoryWebRequest request = (RepositoryWebRequest)inputs.FirstOrDefault();
                var applyLabelRequest = request as ApplyLabelRequest;
                // Set the correlationState to the label being applied and the GUID of the Rule App
                return applyLabelRequest.Label + "|" + applyLabelRequest.AppDef.Guid; 
            }

            return null;
        }
        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            try
            {
                if (string.Equals(operationName, "CheckinRuleApp", StringComparison.OrdinalIgnoreCase))
                {
                    var response = (InRule.Repository.Service.Data.Responses.CheckinRuleAppResponse)returnValue;
                    var ruleApp = RuleAppDataCache.Add(response.RuleAppXml.Xml);

                    // If you always want to trigger reguardless of label, then trigger upon checkin complete and not on ApplyLabel
                    if(!IsLabelTriggerConfigured() && IsRuleAppConfiguredToTriggerPipeline(ruleApp.Name))
                        TriggerPipeline(ruleApp);
                }
                else if (string.Equals(operationName, "ApplyLabel", StringComparison.OrdinalIgnoreCase) && IsLabelTriggerConfigured())
                {
                    string label = correlationState.ToString().Split('|')[0];
                    if (IsLabelConfiguredToTriggerPipeline(label))
                    {
                        string ruleAppGuid = correlationState.ToString().Split('|')[1];
                        var ruleApp = RuleAppDataCache.Get(ruleAppGuid);

                        if (IsRuleAppConfiguredToTriggerPipeline(ruleApp.Name))
                            TriggerPipeline(ruleApp);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in RuleApplicationParameterInspector following checkin: " + ex.Message);
            }
        }

        #region Helpers
        private bool IsRuleAppConfiguredToTriggerPipeline(string ruleAppName)
        {
            string ruleAppsToInclude = ConfigurationManager.AppSettings["AutoPromoteRuleAppsNamed"];

            if (string.IsNullOrEmpty(ruleAppsToInclude))
                return true;

            if (!string.IsNullOrEmpty(ruleAppName) && ruleAppsToInclude.Split('|').Contains(ruleAppName))
                return true;

            return false;
        }
        private bool IsLabelTriggerConfigured()
        {
            string labelsToInclude = ConfigurationManager.AppSettings["AutoPromoteRuleAppsLabeled"];
            return !string.IsNullOrEmpty(labelsToInclude);
        }
        private bool IsLabelConfiguredToTriggerPipeline(string label)
        {
            string labelsToInclude = ConfigurationManager.AppSettings["AutoPromoteRuleAppsLabeled"];

            if (string.IsNullOrEmpty(labelsToInclude))
                return true;

            if (!string.IsNullOrEmpty(label) && labelsToInclude.Split('|').Contains(label))
                return true;

            return false;
        }
        private void TriggerPipeline(RuleAppData ruleApp)
        {
            // If desired, include properties from ruleApp in the pipeline trigger request.
            string org = ConfigurationManager.AppSettings["AzureDevOpsPipelineOrg"];
            string project = ConfigurationManager.AppSettings["AzureDevOpsPipelineProject"];
            string pipelineId = ConfigurationManager.AppSettings["AzureDevOpsPipelineID"];
            string authToken = ConfigurationManager.AppSettings["AzureDevOpsAuthToken"];
            AzureDevOpsApiHelper.QueuePipelineBuild(org, project, pipelineId, authToken);
        }
        #endregion
    }
    public class RuleAppDataCache
    {
        private static object _ruleAppsLock = new object();
        private static List<RuleAppData> _ruleApps = new List<RuleAppData>();
        public static RuleAppData Add(string ruleAppXml)
        {
            XmlDocument d = new XmlDocument();
            d.LoadXml(ruleAppXml);
            XmlNode defTag = d.GetElementsByTagName("RuleApplicationDef")[0];
            string ruleAppGuid = defTag.Attributes["Guid"].Value;
            string ruleAppName = defTag.Attributes["Name"].Value;
            string ruleAppRevision = defTag.Attributes["Revision"].Value;

            RuleAppData appData;
            lock (_ruleAppsLock)
            {
                appData = _ruleApps.FirstOrDefault(a => a.Name == ruleAppName);
                if (appData == null)
                {
                    appData = new RuleAppData();
                    _ruleApps.Add(appData);
                }

                appData.GUID = ruleAppGuid;
                appData.Name = ruleAppName;
                appData.Revision = ruleAppRevision;
                appData.LastUpdated = DateTime.UtcNow;

                foreach (var oldRuleApp in _ruleApps.Where(a => a.LastUpdated < DateTime.UtcNow.AddMinutes(-5)).ToList())
                {
                    _ruleApps.Remove(oldRuleApp);
                }
            }

            return appData;
        }
        public static RuleAppData Get(string guid)
        {
            return _ruleApps.FirstOrDefault(a => a.GUID == guid);
        }
    }
    public class RuleAppData
    {
        public string GUID;
        public string Name;
        public string Revision;
        public DateTime LastUpdated;
    }
}