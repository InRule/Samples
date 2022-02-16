using System;
using System.IO;
using System.Net;
using System.Text;
using InRule.Repository;
using InRule.Repository.EndPoints;
using InRule.Repository.RuleElements;
using InRule.Runtime;

namespace RuntimeOverridesViaConfig
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var ruleAppDef = CreateRuleApplication(out var operationDef, out var e1Def);

			using (new InProcessWebServer(ProcessRequest))
			{
				Console.WriteLine("--- Config Overrides ----------------------------");
				using (RuleSession session = new RuleSession(ruleAppDef))
				{
					session.CreateEntity(e1Def.Name);
					session.ApplyRules();
				}
			}
		}

		private static RuleApplicationDef CreateRuleApplication(out DataElementDef restOperationDef, out EntityDef e1Def)
		{
			var ruleAppDef = new RuleApplicationDef();
			var restServiceDef = ruleAppDef.EndPoints.Add(new RestServiceDef("service1") { RootUrl = InProcessWebServer.RootUrl });
			restOperationDef = ruleAppDef.DataElements.Add(new RestOperationDef("operation1")
														   {
															   RestService = restServiceDef.Name,
															   Verb = RestOperationVerb.Post,
															   UriPrototype = "api/v1/test",
															   BodyFormat = RestOperationBodyFormat.Json,
															   BodyPrototype = "{}",
															   Inputs =
															   {
																   new RestOperationInputParameterDef { Name = "p1", DataType = DataType.String },
																   new RestOperationInputParameterDef { Name = "p2", DataType = DataType.String }
															   }
														   });
			e1Def = ruleAppDef.Entities.Add(new EntityDef("e1"));
			var rs1Def = (RuleSetDef)e1Def.RuleElements.Add(new RuleSetDef("rs1") { FireMode = RuleSetFireMode.Auto, RunMode = RuleSetRunMode.SequentialRunOnce });
			rs1Def.Rules.Add(new ExecuteRestServiceActionDef("exec1")
							 {
								 Operation = restOperationDef.Name,
								 InputValues =
								 {
									 new ExecuteRestServiceActionParameterValueDef("p1", "\"value1\""),
									 new ExecuteRestServiceActionParameterValueDef("p2", "\"value2\"")
								 }
							 });
			return ruleAppDef;
		}

		private static void ProcessRequest(HttpListenerContext context)
		{
			Console.WriteLine($"Request URI: {context.Request.Url}");
			Console.WriteLine($"Verb: {context.Request.HttpMethod}");
			Console.WriteLine("Request Headers:");
			foreach (var headerName in context.Request.Headers.AllKeys)
			{
				Console.WriteLine($"\t- {headerName}: {context.Request.Headers[headerName]}");
			}

			using (var response = new StreamReader(context.Request.InputStream))
			{
				Console.WriteLine($"Request Body: {response.ReadToEnd()}");
			}

			var responseBuffer = Encoding.UTF8.GetBytes("{}");
			context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
			Console.WriteLine();
		}

	}
}
