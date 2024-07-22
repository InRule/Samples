using System;
using System.Collections.Generic;
using System.Data;
using InRule.Repository;
using InRule.Runtime.Engine.State;

namespace InRule.Runtime.Metrics.SqlServer
{
    public static class MetricsToDataTableConverter
    {
        private static readonly Dictionary<DataType, Type> InRuleToFrameworkTypeMap = new Dictionary<DataType, Type>
        {
            {DataType.String, typeof(string)},
            {DataType.Integer, typeof(int)},
            {DataType.Date, typeof(DateTime)},
            {DataType.DateTime, typeof(DateTime)},
            {DataType.Number, typeof(decimal)},
            {DataType.Boolean, typeof(bool)}
        };

        private static readonly List<(string, Type)> CommonColumns = new List<(string, Type)>
        {
			("MetricSchemaVersion", typeof(int)),
			("ServiceName", typeof(string)),
            ("RuleApplicationName", typeof(string)),
			("SessionId", typeof(string)),
			("EntityId", typeof(string)),
        };


        public static Dictionary<string, MetricSchema> ConvertMetricsToDataTables(string serviceName, string ruleApplicationName, Guid sessionId, IEnumerable<Metric> metrics, Dictionary<string, DataTable> entityToDataTableMap)
        {
            Dictionary<string, MetricSchema> mapOfTablesToCheck = new Dictionary<string, MetricSchema>();
            foreach (Metric metric in metrics)
            {
                if (GetOrCreateDataTable(entityToDataTableMap, metric, out DataTable dataTable))
                {
                    mapOfTablesToCheck.Add(metric.EntityName, metric.Schema);
                }

                var metricRow = dataTable.NewRow();
				metricRow["MetricSchemaVersion"] = metric.Schema.Version;
				metricRow["ServiceName"] = serviceName;
                metricRow["RuleApplicationName"] = ruleApplicationName;
				metricRow["SessionId"] = sessionId.ToString();
				metricRow["EntityId"] = metric.EntityId;

				foreach (MetricProperty metricProperty in metric.Schema.Properties)
                {
                    var value = metric[metricProperty] ?? DBNull.Value;
                    metricRow[metricProperty.GetMetricColumnName()] = value;
                }

                dataTable.Rows.Add(metricRow);
            }

            return mapOfTablesToCheck;
        }



        /// <summary>
        /// Returns true if the DataTable needed to be created, false if the DataTable already existed.
        /// </summary>
        /// <param name="entityToDataTableMap"></param>
        /// <param name="metric"></param>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private static bool GetOrCreateDataTable(Dictionary<string, DataTable> entityToDataTableMap, Metric metric, out DataTable dataTable)
        {
            if (entityToDataTableMap.TryGetValue(metric.EntityName, out dataTable))
            {
                return false;
            }

            dataTable = new DataTable();

            foreach (var (columnName, type) in CommonColumns)
            {
                dataTable.Columns.Add(columnName, type);
            }

            foreach (var metricProperty in metric.Schema)
            {
                dataTable.Columns.Add(metricProperty.GetMetricColumnName(), InRuleToFrameworkTypeMap[metricProperty.DataType]);
            }

            entityToDataTableMap.Add(metric.EntityName, dataTable);
            return true;

        }

    }
}