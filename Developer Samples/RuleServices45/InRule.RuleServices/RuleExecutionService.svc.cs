using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using InRule.Repository;
using InRule.Repository.RuleElements;
using InRule.RuleServices.Common.DataObjects;
using InRule.Runtime;

namespace InRule.RuleServices
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class RuleExecutionService : IRuleExecutionService
    {
        /// <summary>
        ///    Method used when performing a GET request to execute auto fire rule sets.  
        ///    Builds a RuleExecutionRequest and calls the ExecuteRuleRequest method where all rule processing is handled.
        /// </summary>
        /// <param name="ruleApp">The rule application to load from the catalog</param>
        /// <param name="entity">The name of the entity to load into the rulesession</param>
        /// <param name="entityXml">The state XML to load into the entity</param>
        /// <param name="returnEntity">The entity whose XML will be returned in the response</param>
        /// <param name="responseType">Specifies what will be returned in the response</param>
        /// <returns>Stream (supports variable response types)</returns>
        public Stream ApplyRules(string ruleApp, string entity, string entityXml, string returnEntity, string responseType)
        {
            var request = new RuleExecutionRequest
                              {
                                  RuleApp = ruleApp,
                                  Entity = entity,
                                  EntityXml = entityXml,
                                  ReturnEntity = returnEntity,
                                  ResponseType = responseType
                              };

            var response = ExecuteRuleRequest(request);
            return new MemoryStream(Encoding.UTF8.GetBytes(response.ResponseText));
        }

        /// <summary>
        ///    Method used when performing a GET request to execute an explicit or independent rule set.  
        ///    Builds a RuleExecutionRequest and calls the ExecuteRuleRequest method where all rule processing is handled.
        /// </summary>
        /// <param name="ruleApp">The rule application to load from the catalog</param>
        /// <param name="ruleset">The explicit rule set that will be executed</param>
        /// <param name="entity">The name of the entity to load into the rulesession</param>
        /// <param name="entityXml">The state XML to load into the entity</param>
        /// <param name="returnEntity">The enitity whose XML will be returned in the response</param>
        /// <param name="responseType">Specifies what will be returned in the response</param>
        /// <returns>Stream (supports variable response types)</returns>
        public Stream ExecuteRuleSet(string ruleApp, string ruleset, string entity, string entityXml, string returnEntity, string responseType)
        {
            var request = new RuleExecutionRequest
                              {
                                  RuleApp = ruleApp,
                                  RuleSet = ruleset,
                                  Entity = entity,
                                  EntityXml = entityXml,
                                  ReturnEntity = returnEntity,
                                  Parameters = GetParameters(),
                                  ResponseType = responseType
                              };

            var response = ExecuteRuleRequest(request);
            return new MemoryStream(Encoding.UTF8.GetBytes(response.ResponseText));
        }

        /// <summary>
        ///    Method used when performing a POST request and by public GET methods to centralize all rule execution.
        ///    Auto, explicit and independent rules can be executed in the method, based on the type of request specified.
        /// </summary>
        /// <param name="request">A RuleExecutionRequest that contains all execution information required to run rules.</param>
        /// <returns>RuleExecutionResponse</returns>
        public RuleExecutionResponse ExecuteRuleRequest(RuleExecutionRequest request)
        {
            var response = new RuleExecutionResponse();

            try
            {
                // get rule application using settings from web.config
                var ruleApp = GetRuleApp(request.RuleApp);

                using (var session = new RuleSession(ruleApp))
                {
                    // override end points 
                    RuleServiceHelper.OverrideEndPoints(session, request);

                    // if the performance stats report was requested, turn on the details
                    if (request.ResponseType != null && request.ResponseType.Contains(RuleExecutionResponseType.PerformanceStatisticsReport.ToString()))
                    {
                        session.Settings.LogOptions = EngineLogOptions.SummaryStatistics | EngineLogOptions.DetailStatistics;
                    }
                    
                    // if the execution report or execution log was requested, turn on the execution and state change settings
                    if (request.ResponseType != null && (request.ResponseType.Contains(RuleExecutionResponseType.RuleExecutionReport.ToString()) ||
                        request.ResponseType.Contains(RuleExecutionResponseType.ExecutionLogText.ToString()) ||
                        request.ResponseType.Contains(RuleExecutionResponseType.ExecutionLogXml.ToString())))
                    {
                        session.Settings.LogOptions = EngineLogOptions.Execution | EngineLogOptions.StateChanges;
                    }

                    response.ResponseType = request.ResponseType;
                    
                    Entity entity = null;
                    
                    if (!String.IsNullOrEmpty(request.Entity))
                    {
                        // if an entity was specified, this is an entity based rule set
                        entity = session.CreateEntity(request.Entity);

                        // if state was passed in, load it
                        if (!String.IsNullOrEmpty(request.EntityXml))
                        {
                            entity.ParseXml(request.EntityXml);
                        }

                        // if an explicit rule set was not specified, call ApplyRules
                        if (String.IsNullOrEmpty(request.RuleSet))
                        {
                            session.ApplyRules();
                        }
                        else
                        {

                            // this is an explicit rule set, if parameters were passed in, pass them to the explicit rule set
                            if (request.Parameters.Count > 0)
                            {
                                var parameters = BuildRuleSetParameters(request, session);
                                entity.ExecuteRuleSet(request.RuleSet, parameters.ToArray());
                            }
                            else
                            {
                                entity.ExecuteRuleSet(request.RuleSet);
                            }
                        }
                    }
                    else
                    {
                        // if no entity name was passed in, this is an independent rule set, pass parameters if applicable
                        if (request.Parameters.Count > 0)
                        {
                            var parameters = BuildRuleSetParameters(request, session); 
                            session.ExecuteIndependentRuleSet(request.RuleSet, parameters.ToArray());
                        }
                        else
                        {
                            session.ExecuteIndependentRuleSet(request.RuleSet);
                        }
                    }
                    
                    // if output entity was specified, 
                    Entity returnEntity;
                    if (string.IsNullOrEmpty(request.ReturnEntity))
                    {
                        returnEntity = entity;
                    }
                    else
                    {
                        // retrieve output entity 
                        returnEntity = RuleServiceHelper.GetEntityFromRuleSession(session, request.ReturnEntity);
                    }

                    // set the response text based on the request type
                    RuleServiceHelper.SetResponseText(session, returnEntity, response);
                }
            }
            catch (Exception ex)
            {
                RuleServiceHelper.HandleWebException(ex, response);
            }
            return response;
        }

        private static List<object> BuildRuleSetParameters(RuleExecutionRequest request, RuleSession session)
        {
            RuleSetDef ruleSetDef;

            // get the rule set definition so we can get the names of the parameters
            if (String.IsNullOrEmpty(request.Entity))
            {
                // independent rule set 
                ruleSetDef = session.GetRuleApplicationDef().RuleSets[request.RuleSet] as RuleSetDef;
            }
            else
            {
                // entity rule set
                ruleSetDef = session.GetRuleApplicationDef().Entities[request.Entity].GetRuleSet(request.RuleSet);
            }

            var parameters = new List<object>();

            var parmCount = request.Parameters.Count - 1;

            var parms = new Object[parmCount - 1];

            var i = 0;
            foreach (var parameter in request.Parameters)
            {
                var parmDef = ruleSetDef.Parameters[parameter.Name];
                if (parmDef != null && parmDef.DataType == DataType.Entity)
                {
                    // if parm is an entity type, create the entity using the value as the XML andd add
                    var entity = session.CreateEntity(parmDef.DataTypeEntityName, parameter.Value);
                    parms[i] = entity;
                }
                else
                {
                    // add the value based parameter
                    parms[i] = parameter.Value;
                }

                i++;
            }

            return parameters;
        }
        
        private static List<RuleExecutionParameter> GetParameters()
        {
            var parameters = new List<RuleExecutionParameter>();
            var queryParameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
            var boundVariables = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BoundVariables;

            foreach (var key in queryParameters.AllKeys)
            {
                if (key != null)
                {
                    if (!boundVariables.ContainsKey(key.ToUpper()))
                    {
                        var parameter = new RuleExecutionParameter
                                            {
                                                Name = key,
                                                Value = queryParameters[key]
                                            };
                        parameters.Add(parameter);
                    }
                }
            }
            return parameters;
        }

        private static RuleApplicationReference GetRuleApp(string name)
        {
            var useCatalogRaw = ConfigurationManager.AppSettings[Constants.UseInRuleCatalog];
            bool useCatalog;
            bool.TryParse(useCatalogRaw, out useCatalog);

            // Use the catatlog
            if (useCatalog)
            {
                var uri = ConfigurationManager.AppSettings[Constants.InRuleCatalogUri];
                var label = ConfigurationManager.AppSettings[Constants.InRuleCatalogLabel];

                // use SSO? (active directory)
                var useSsoRaw = ConfigurationManager.AppSettings[Constants.InRuleCatalogSso];
                bool useSso;
                bool.TryParse(useSsoRaw, out useSso);
                if (useSso)
                {
                    // credentials from application pool will be used for AD
                    return String.IsNullOrWhiteSpace(label) ? 
                        new CatalogRuleApplicationReference(uri, name)
                        : new CatalogRuleApplicationReference(uri, name, label);
                }

                // no SSO, pass username and password -- log in using InRule security or AD
                var catalogUser = ConfigurationManager.AppSettings[Constants.InRuleCatalogUser];
                var catalogPassword = ConfigurationManager.AppSettings[Constants.InRuleCatalogPassword];
                return String.IsNullOrWhiteSpace(label)
                    ? new CatalogRuleApplicationReference(uri, name, catalogUser, catalogPassword)
                    : new CatalogRuleApplicationReference(uri, name, catalogUser, catalogPassword, label);
            }

            // no catalog -- use the local file system
            var filename = Path.Combine(ConfigurationManager.AppSettings[Constants.InRuleRuleAppDirectory], name + ".ruleapp");
            return new FileSystemRuleApplicationReference(filename);
        }
    }
}
