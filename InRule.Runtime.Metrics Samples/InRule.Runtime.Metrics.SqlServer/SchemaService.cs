using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.IO;
using InRule.Runtime.Engine.State;
using InRule.Runtime.Metrics.SqlServer.Logging;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using DataType = InRule.Repository.DataType;
using System.Data;

namespace InRule.Runtime.Metrics.SqlServer
{
    internal class SchemaService : IDisposable
    {
        private enum SchemaStatus
        {
            Unchanged,
            New,
            Changed,
        }

        private readonly Dictionary<Type, Microsoft.SqlServer.Management.Smo.DataType> _frameworkTypeToSmoTypeMap = new Dictionary<Type, Microsoft.SqlServer.Management.Smo.DataType>
        {
            {typeof(string), Microsoft.SqlServer.Management.Smo.DataType.NVarCharMax},
            {typeof(Int32), Microsoft.SqlServer.Management.Smo.DataType.Int},
            {typeof(DateTime), Microsoft.SqlServer.Management.Smo.DataType.DateTime},
            {typeof(decimal), Microsoft.SqlServer.Management.Smo.DataType.Decimal(30, 20)},
            {typeof(bool), Microsoft.SqlServer.Management.Smo.DataType.Bit},
            {typeof(Guid), Microsoft.SqlServer.Management.Smo.DataType.NVarCharMax},
        };

        private readonly Dictionary<DataType, Microsoft.SqlServer.Management.Smo.DataType> _inRuleToSqlTypeMap = new Dictionary<DataType, Microsoft.SqlServer.Management.Smo.DataType>
        {
            {DataType.String, Microsoft.SqlServer.Management.Smo.DataType.NVarCharMax},
            {DataType.Integer, Microsoft.SqlServer.Management.Smo.DataType.Int},
            {DataType.Date, Microsoft.SqlServer.Management.Smo.DataType.DateTime},
            {DataType.DateTime, Microsoft.SqlServer.Management.Smo.DataType.DateTime},
            {DataType.Number, Microsoft.SqlServer.Management.Smo.DataType.Decimal(16, 38)},
            {DataType.Boolean, Microsoft.SqlServer.Management.Smo.DataType.Bit}
        };

        private readonly List<(string, Type)> _commonColumns = new List<(string, Type)>
        {
            ("MetricSchemaVersion", typeof(int)),
            ("ServiceName", typeof(string)),
            ("RuleApplicationName", typeof(string)),
            ("SessionId", typeof(string)),
            ("EntityId", typeof(string))
        };

        private readonly HashSet<MetricSchemaKey> _ruleAppEntityToSchemaHashMap = new HashSet<MetricSchemaKey>();

        private readonly string _connectionString;
        private static readonly ILog Log = LogProvider.For<SchemaService>();
        private SqlConnection _connection;


        public SchemaService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection Connection
        {
            get
            {
                if (_connection == null || _connection.State == ConnectionState.Closed)
                {
                    _connection = new SqlConnection(_connectionString);
                }

                return _connection;
            }
        }

        public void UpdateSchema(string ruleApplicationName, Dictionary<string, MetricSchema> entitiesInMetrics)
        {
            using (Connection)
            {
                foreach (var tableToValidateSchema in entitiesInMetrics)
                {
                    var tableName = tableToValidateSchema.Key;
                    var metricSchema = tableToValidateSchema.Value;

                    UpdateSchema(ruleApplicationName, tableName, metricSchema);
                }
            }
        }

        public void UpdateSchema(string ruleApplicationName, string tableName, MetricSchema metricSchema)
        {
            using (Connection)
            {
                var metricSchemaKey = new MetricSchemaKey(ruleApplicationName, tableName, metricSchema);
                if (_ruleAppEntityToSchemaHashMap.Contains(metricSchemaKey))
                {
                    // seen this schema before in this session
                    return;
                }

                var schemaStatus = GetSchemaStatus(ruleApplicationName, tableName, metricSchema);
                if (schemaStatus == SchemaStatus.Unchanged)
                    return;

                Log.Info("Schema has changed");

                try
                {
                    var server = new Server(new ServerConnection(new SqlConnection(_connectionString)));
                    var database = server.Databases[Connection.Database];
                    var schema = GetOrAddSchema(ruleApplicationName, database);
                    var table = GetOrAddTable(database, schema.Name, tableName);

                    foreach (var metricProperty in metricSchema)
                    {
                        GetOrAddColumn(metricProperty, table);
                    }

                    SerializeSchemaToDbCache(ruleApplicationName, tableName, metricSchema.GetJson(), schemaStatus);

                    _ruleAppEntityToSchemaHashMap.Add(metricSchemaKey);

                }
                catch (Exception e)
                {
                    Log.Error(e, "Error updating schema!");
                }
            }
        }

        private static Schema GetOrAddSchema(string ruleApplicationName, Database database)
        {
            if (database.Schemas.Contains(ruleApplicationName))
                return database.Schemas[ruleApplicationName];

            var schema = new Schema(database, ruleApplicationName);
            schema.Create();

            return schema;
        }

        private Column GetOrAddColumn(MetricProperty metricProperty, Table table)
        {
            var columnName = metricProperty.GetMetricColumnName();

            if (table.Columns.Contains(columnName))
                return table.Columns[columnName];

            var column = new Column(table, columnName, _inRuleToSqlTypeMap[metricProperty.DataType]);
            column.Create();

            return column;
        }

        private Table GetOrAddTable(Database database, string schemaName, string tableName)
        {
            if (database.Tables.Contains(tableName, schemaName))
                return database.Tables[tableName, schemaName];

            var table = new Table(database, tableName);
            table.Schema = schemaName;

            foreach (var (columnName, type) in _commonColumns)
            {
                var column = new Column(table, columnName, _frameworkTypeToSmoTypeMap[type]);
                table.Columns.Add(column);
            }

            table.Create();

            return table;
        }

        private SchemaStatus GetSchemaStatus(string ruleAppName, string entityName, MetricSchema newMetricSchema)
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            var selectFromMetricSchema = "SELECT MetricSchema from MetricSchemaStore where RuleAppName = @RuleAppName AND EntityName = @EntityName";

            var command = Connection.CreateCommand();
            command.CommandText = selectFromMetricSchema;
            command.Parameters.Add("@RuleAppName", SqlDbType.NVarChar).Value = ruleAppName;
            command.Parameters.Add("@EntityName", SqlDbType.NVarChar).Value = entityName;

            var metricJson = command.ExecuteScalar()?.ToString();
            if (metricJson == null)
            {
                return SchemaStatus.New;
            }

            var oldSchema = MetricSchema.FromJson(new StringReader(metricJson));

            if (oldSchema.GetHashCode() == newMetricSchema.GetHashCode() && oldSchema.Equals(newMetricSchema))
            {
                return SchemaStatus.Unchanged;
            }

            return SchemaStatus.Changed;

        }

        private void SerializeSchemaToDbCache(string ruleAppName, string entityName, string metricJson, SchemaStatus schemaStatus)
        {
            var storageSql = schemaStatus == SchemaStatus.New
                ? "INSERT INTO MetricSchemaStore (RuleAppName, EntityName, MetricSchema) VALUES(@RuleAppName,@EntityName, @MetricSchema)"
                : "UPDATE MetricSchemaStore SET MetricSchema = @MetricSchema where RuleAppName = @RuleAppName AND EntityName = @EntityName";

            var command = Connection.CreateCommand();
            command.CommandText = storageSql;
            command.Parameters.Add("@RuleAppName", SqlDbType.NVarChar).Value = ruleAppName;
            command.Parameters.Add("@EntityName", SqlDbType.NVarChar).Value = entityName;
            command.Parameters.Add("@MetricSchema", SqlDbType.NVarChar).Value = metricJson;

            command.ExecuteNonQuery();
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}