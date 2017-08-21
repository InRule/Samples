using System;
using InRule.Repository;
using InRule.Repository.EndPoints;
using InRule.Repository.RuleElements;
using InRule.Runtime;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create a new Rule Application
            var ruleAppDef = new RuleApplicationDef("RESTService");

            //Create a new REST Service Endpoint
            RestServiceDef restServiceDef = new RestServiceDef();
            restServiceDef.Name = "RestService";
            restServiceDef.AuthenticationType = RestServiceAuthenticationType.None;
            restServiceDef.RootUrl = "http://tinyurl.com/";

            //Add the new REST Service Endpoint to the Rule Application
            ruleAppDef.EndPoints.Add(restServiceDef);

            //Create a new REST Resource Template
            RestOperationDef restOperationDef = new RestOperationDef();
            restOperationDef.Name = "RestResourceTemplate";
            restOperationDef.RestService = restServiceDef.Name;
            restOperationDef.Verb = RestOperationVerb.Get;
            restOperationDef.WaitForResponse = true;
            restOperationDef.UriPrototype = "/api-create.php?url=http://www.inrule.com/";

            //Add the new REST Resaource Template to the Rule Application
            ruleAppDef.DataElements.Add(restOperationDef);
            
            //Add an Entity for output
            EntityDef tinyUrlOutput = ruleAppDef.Entities.Add(new EntityDef("TinyUrlOutput"));
            //Add field to the new Entity that will contain the output value
            FieldDef resultField = tinyUrlOutput.Fields.Add(new FieldDef("Result", DataType.String));
            
            //Create a new Execute REST Service Action
            ExecuteRestServiceActionDef executeRestServiceActionDef = new ExecuteRestServiceActionDef();
            executeRestServiceActionDef.Name = "ExecuteRestService";
            executeRestServiceActionDef.Operation = restOperationDef.Name;
            executeRestServiceActionDef.AssignReturnValueToField = new CalcDef(DataType.String, resultField.Name);

            //Create a Rule Set container to hold the Action
            RuleSetDef ruleSetDef = new RuleSetDef("RuleSet");
            ruleSetDef.FireMode = RuleSetFireMode.Auto;
            ruleSetDef.RunMode = RuleSetRunMode.Sequential;
            
            //Add the Rule Set to the Entity
            tinyUrlOutput.RuleElements.Add(ruleSetDef);
            //Add the Action to the Rule Set
            ruleSetDef.Rules.Add(executeRestServiceActionDef);
            
            //Confirm the Rule Application works by creating a Rule Session
            var session = new RuleSession(ruleAppDef);
            //Create a session Entity using the output Entity
            var entityWithEntityFieldRef = session.CreateEntity(tinyUrlOutput.Name);
            //Apply the Rule 
            session.ApplyRules();

            //Get the result from the Entity used for the session and return the result from the Result Field
            string result = entityWithEntityFieldRef.Fields["Result"];

            //Verify. Copy the TinyURL from the command and paste into a browser to confirm it takes you to http://www.inrule.com
            Console.WriteLine(String.Format("Result: {0}", result));
            Console.Read();
        }
    }
}
