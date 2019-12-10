using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using InRule.Repository;
using InRule.Repository.Decisions;
using InRule.Repository.RuleElements;
using Newtonsoft.Json;

namespace ExecuteDecisionSample
{
	class Program
	{
		static async Task Main(string[] args)
		{
			const string tokenUrl = "https://auth.inrulecloud.com/oauth/token";
			const string publishUrl = "https://[your-decision-runtime-hostname]/api/decisions";	// Change this to match the URL of your Decision Runtime
			const string clientId = "xxxxxxxxxxxxxxxxxxxxxxx";	// Contact inrule for the client_id for your application
			const string clientSecret = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";    // Contact inrule for the client_secret for your application

			try
			{
				var ruleApplication = new RuleApplicationDef("InterestRateApp");

				// Define 'CalculateLoanPayment' decision
				var calculateLoanPayment = ruleApplication.Decisions.Add(new DecisionDef("CalculateLoanPayment"));
				{
					var presentValue = calculateLoanPayment.Fields.Add(new DecisionFieldDef("PresentValue", DecisionFieldType.Input, DataType.Number));
					var interestRate = calculateLoanPayment.Fields.Add(new DecisionFieldDef("InterestRate", DecisionFieldType.Input, DataType.Number));
					var periods = calculateLoanPayment.Fields.Add(new DecisionFieldDef("Periods", DecisionFieldType.Input, DataType.Integer));
					var payment = calculateLoanPayment.Fields.Add(new DecisionFieldDef("Payment", DecisionFieldType.Output, DataType.Number));
					calculateLoanPayment.DecisionStart.Rules.Add(new SetValueActionDef(payment.Name, $"{interestRate.Name} * {presentValue.Name} / (1 - ((1 + {interestRate.Name}) ^ -{periods.Name}))"));
				}

				// Define 'CalculateFutureValue' decision
				var calculateFutureValue = ruleApplication.Decisions.Add(new DecisionDef("CalculateFutureValue"));
				{
					var presentValue = calculateFutureValue.Fields.Add(new DecisionFieldDef("PresentValue", DecisionFieldType.Input, DataType.Number));
					var interestRate = calculateFutureValue.Fields.Add(new DecisionFieldDef("InterestRate", DecisionFieldType.Input, DataType.Number));
					var periods = calculateFutureValue.Fields.Add(new DecisionFieldDef("Periods", DecisionFieldType.Input, DataType.Integer));
					var futureValue = calculateFutureValue.Fields.Add(new DecisionFieldDef("FutureValue", DecisionFieldType.Output, DataType.Number));
					calculateFutureValue.DecisionStart.Rules.Add(new SetValueActionDef(futureValue.Name, $"{presentValue.Name} * (1 + {interestRate.Name}) ^ {periods.Name}"));
				}

				// Get bearer token from authentication service
				string bearerToken = await GetBearerToken(tokenUrl, clientId, clientSecret);
				if (bearerToken == null) return;
				Console.WriteLine();

				string ruleApplicationXml = ruleApplication.GetXml();

				// Publish CalculateLoanPayment Decision
				if (!await PublishDecision(publishUrl, calculateLoanPayment.Name, ruleApplicationXml, bearerToken)) return;

				// Publish CalculateFutureValue Decision
				if (!await PublishDecision(publishUrl, calculateFutureValue.Name, ruleApplicationXml, bearerToken)) return;

				string inputJson = JsonConvert.SerializeObject(new
															   {
																   PresentValue = 20000.00m,
																   InterestRate = 0.05m,
																   Periods = 5
															   });

				// Execute CalculateLoanPayment Decision Service
				Console.WriteLine();
				if (!await ExecuteDecisionService(publishUrl, calculateLoanPayment.Name, inputJson, bearerToken)) return;

				// Execute CalculateFutureValue Decision Service
				Console.WriteLine();
				if (!await ExecuteDecisionService(publishUrl, calculateFutureValue.Name, inputJson, bearerToken)) return;
			}
			catch (Exception ex)
			{
				Console.WriteLine();
				Console.WriteLine("An unexpected error occurred: " + ex);
			}
			finally
			{
				Console.WriteLine("Press any key to exit.");
				Console.ReadKey();
			}
		}

		private static async Task<bool> PublishDecision(string publishUrl, string decisionName, string ruleApplicationXml, string bearerToken)
		{
			Console.Write($"Publishing Decision '{decisionName}'...");

			using (HttpClient client = new HttpClient())
			{
				string decisionJson = JsonConvert.SerializeObject(new
																  {
																	  name = decisionName,
																	  ruleApplication = ruleApplicationXml
																  });

				var request = new HttpRequestMessage(HttpMethod.Post, publishUrl);
				request.Headers.Add("Authorization", $"Bearer {bearerToken}");
				request.Content = new StringContent(decisionJson, Encoding.UTF8, "application/json");

				var response = await client.SendAsync(request);
				if (!response.IsSuccessStatusCode)
				{
					string reason = await response.Content.ReadAsStringAsync();
					Console.WriteLine($"Error {(int) response.StatusCode} ({response.StatusCode.ToString()}): {reason}");
					return false;
				}
			}

			Console.WriteLine("Success.");
			return true;
		}

		private static async Task<bool> ExecuteDecisionService(string publishUrl, string decisionName, string inputJson, string bearerToken)
		{
			Console.WriteLine($"Executing '{decisionName}' Decision Service...");
			Console.WriteLine($"{decisionName} inputs: {inputJson}");

			using (HttpClient client = new HttpClient())
			{
				var request = new HttpRequestMessage(HttpMethod.Post, $"{publishUrl}/{decisionName}");
				request.Headers.Add("Authorization", $"Bearer {bearerToken}");
				request.Content = new StringContent(inputJson, Encoding.UTF8, "application/json");

				var response = await client.SendAsync(request);
				if (!response.IsSuccessStatusCode)
				{
					string reason = await response.Content.ReadAsStringAsync();
					Console.WriteLine($"Error {(int) response.StatusCode} ({response.StatusCode.ToString()}): {reason}");
					return false;
				}

				string outputJson = await response.Content.ReadAsStringAsync();
				Console.WriteLine($"{decisionName} output: {outputJson}");
			}

			return true;
		}

		private static async Task<string> GetBearerToken(string tokenUrl, string clientId, string clientSecret)
		{
			Console.Write("Retrieving bearer token from authentication service...");
			using (HttpClient client = new HttpClient())
			{
				var request = new FormUrlEncodedContent(new[]
														{
															new KeyValuePair<string, string>("grant_type", "client_credentials"),
															new KeyValuePair<string, string>("client_id", clientId),
															new KeyValuePair<string, string>("client_secret", clientSecret),
															new KeyValuePair<string, string>("audience", "master_service"),
														});

				var response = await client.PostAsync(tokenUrl, request);
				if (!response.IsSuccessStatusCode)
				{
					string reason = await response.Content.ReadAsStringAsync();
					Console.WriteLine($"Error retreiving token: {(int) response.StatusCode} ({response.StatusCode.ToString()}): {reason}");
					return null;
				}

				string contentJson = await response.Content.ReadAsStringAsync();
				dynamic content = JsonConvert.DeserializeObject(contentJson);
				Console.WriteLine("Success.");
				return content.access_token;
			}
		}
	}
}
