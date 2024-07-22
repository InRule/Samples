using System.IO;
using System.Text;
using System.Threading.Tasks;
using InRule.Runtime.Metrics.Csv;

namespace InRule.Runtime.Metrics.TestHarness
{
    class RuleExecution
    {
        public static async Task RunRules(string ruleAppPath, string dataFilePath, string entityName)
        {
            // loop throught the data files provided in the sample
            foreach (string file in Directory.GetFiles(dataFilePath))
            {
				using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
				{
					// read in the JSON from the file
					string data = await reader.ReadToEndAsync();

					// create a rule session
					using (var session = new RuleSession(ruleAppPath))
					{
						// set the metric logger to a new instance of the logger
						session.Settings.MetricLogger = new CsvMetricLogger();

						// create the entity passing in the JSON
						session.CreateEntity(entityName, data, EntityStateType.Json);

						// run rules, this will emit the values when the logger is present
						await session.ApplyRulesAsync();
					}
				}
			}
        }
    }
}
