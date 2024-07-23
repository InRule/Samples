using InRule.Runtime.Engine.State;
using Azure.Data.Tables;
using System;
using Azure;

namespace InRule.Runtime.Metrics.AzureTableStorage
{
	public sealed class MetricEntity : ITableEntity
	{
		public MetricEntity(string serviceName, string ruleApplicationName, string sessionId, string entityId, string entityName, string metricJson)
		{
			PartitionKey = sessionId;
			RowKey = entityId;
			Version = MetricSchema.CurrentVersion;
			ServiceName = serviceName;
			RuleApplicationName = ruleApplicationName;
			EntityName = entityName;
			MetricJson = metricJson;
			Timestamp = DateTimeOffset.UtcNow;
			ETag = new ETag("*");
		}

		public MetricEntity()
		{
		}

        public string PartitionKey { get; set; }
        
		public string RowKey { get; set; }
        
		public int Version { get; set; }

		public string ServiceName { get; set; }

		public string RuleApplicationName { get; set; }

		public string EntityName { get; set; }

		public string MetricJson { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
        
		public ETag ETag { get; set ; }
    }
}
