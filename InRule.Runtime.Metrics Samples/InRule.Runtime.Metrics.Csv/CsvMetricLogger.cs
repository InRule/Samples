using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InRule.Runtime.Metrics.Csv
{
	public sealed class CsvMetricLogger : IMetricLogger
	{
		public async Task LogMetricsAsync(string serviceId, string ruleApplicationName, Guid sessionId, Metric[] metrics)
		{
			// loop through all of the metrics that are emitted by the rules engine
			// there will be one metric per entity
			foreach (var metricGroup in metrics.GroupBy(metric => metric.EntityName))
			{
				// save them to disk in a csv file per Entity
				string filePath = $"{ConfigurationManager.AppSettings["OutputDirectory"]}{metricGroup.Key.Replace('/', '+')}.csv";

				using (FileStream stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None))
				using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
				{
					foreach (Metric metric in metricGroup)
					{
						// write headers if this is a new file
						if (stream.Position == 0)
						{
							await writer.WriteLineAsync(String.Join(",", metric.Schema.Properties.Select(property => property.Name)));
						}

						await writer.WriteLineAsync(String.Join(",", metric.Schema.Properties.Select(property => metric[property].ToString())));
					}
				}
			}
		}

		public void LogMetrics(string serviceId, string ruleApplicationName, Guid sessionId, Metric[] metrics)
		{
			// loop through all of the metrics that are emitted by the rules engine
			// there will be one metric per entity
			foreach (var metricGroup in metrics.GroupBy(metric => metric.EntityName))
			{
				// save them to disk in a csv file per Entity
				string filePath = $"{ConfigurationManager.AppSettings["OutputDirectory"]}\\{metricGroup.Key.Replace('/', '+')}.csv";

				using (FileStream stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None))
				using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
				{
					foreach (Metric metric in metricGroup)
					{
						// write headers if this is a new file
						if (stream.Position == 0)
						{
							writer.WriteLine(String.Join(",", metric.Schema.Properties.Select(property => property.Name)));
						}

						writer.WriteLine(String.Join(",", metric.Schema.Properties.Select(property => metric[property]?.ToString())));
					}
				}
			}
		}
	}
}
