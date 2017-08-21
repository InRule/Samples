using System;
using InRule.Repository;
using InRule.Repository.RuleElements;
using InRule.Runtime;
 
namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var ruleAppDef = new RuleApplicationDef("CallWebServiceExample1");

            //Create a new Web Service Endpoint
            WebServiceDef webServiceDef = new WebServiceDef();
            webServiceDef.Name = "WebService1";
            webServiceDef.WsdlUri = "http://localhost/WcfService1/Service1.svc?wsdl";
            webServiceDef.PortName = "BasicHttpBinding_IService1";
            webServiceDef.Operations.Add(new WebServiceDef.OperationDef("GetData"));

            //Add the new Web Service Endpoint to the Rule Application
            ruleAppDef.EndPoints.Add(webServiceDef);

            //Add an Entity for output
            EntityDef entity1 = ruleAppDef.Entities.Add(new EntityDef("Entity1"));
            //Add field to the new Entity that will contain the output value
            FieldDef resultField1 = entity1.Fields.Add(new FieldDef("InputData", DataType.Integer));
            FieldDef resultField2 = entity1.Fields.Add(new FieldDef("OutputData", DataType.String));

            //Create a new Execute Web Service Action
            ExecuteWebServiceOperationActionDef executeWebServiceActionDef = new ExecuteWebServiceOperationActionDef();
            executeWebServiceActionDef.Name = "ExecuteWebService1";
            executeWebServiceActionDef.WebServiceEndPointName = "WebService1";
            executeWebServiceActionDef.OperationName = "GetData";

            var inputMapping1 = new TypeMapping("value", "Int32");
            inputMapping1.FieldMappings.Add(new FieldMapping("InputData", "value", "Integer", false, false));
            executeWebServiceActionDef.Inputs.Add(inputMapping1);
            var outputMapping1 = new TypeMapping("[return]", "String");
            outputMapping1.FieldMappings.Add(new FieldMapping("OutputData", "[return]", "Text", false, false));
            executeWebServiceActionDef.Outputs.Add(outputMapping1);

            //Create a Rule Set container to hold the Action
            RuleSetDef ruleSetDef = new RuleSetDef("RuleSet");
            ruleSetDef.FireMode = RuleSetFireMode.Auto;
            ruleSetDef.RunMode = RuleSetRunMode.Sequential;

            //Add the Rule Set to the Entity
            entity1.RuleElements.Add(ruleSetDef);
            //Add the Action to the Rule Set
            ruleSetDef.Rules.Add(executeWebServiceActionDef);

            ruleAppDef.SaveToFile(@"c:\temp\GeneratedRuleApp");

            //Confirm the Rule Application works by creating a Rule Session
            var session = new RuleSession(ruleAppDef);
            //Create a session Entity using the output Entity
            var entity1Runtime = session.CreateEntity("Entity1");
            entity1Runtime.Fields["InputData"].Value = "123";
            //Apply the Rule
            session.ApplyRules();

            //Get the result from the Entity used for the session and return the result from the Result Field
            string result = entity1Runtime.Fields["OutputData"];

            //Verify. Copy the TinyURL from the command and paste into a browser to confirm it takes you to http://www.inrule.com
            Console.WriteLine("Result: {0}", result);
            Console.Read();
        }
    }
}

