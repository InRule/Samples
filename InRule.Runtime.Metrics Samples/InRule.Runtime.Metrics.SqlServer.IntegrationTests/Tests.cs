using InRule.Repository;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DataType = InRule.Repository.DataType;

namespace InRule.Runtime.Metrics.SqlServer.IntegrationTests
{
    [TestFixture]
    public sealed class Tests
    {
        private const string ServerConnectionString = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true";

        private const string IntegrationTestDatabaseName = "MetricTesting";

        private const string DatabaseConnectionString = ServerConnectionString + ";Initial Catalog=" + IntegrationTestDatabaseName;

        private static Table GetTable(RuleApplicationDef ruleAppDef, string name)
        {
            var sqlConnection = new Microsoft.Data.SqlClient.SqlConnection(ServerConnectionString);
            var server = new Server(new ServerConnection(sqlConnection));

            var database = server.Databases[IntegrationTestDatabaseName];
            var table = database.Tables[name, ruleAppDef.Name];

            return table;
        }

        [SetUp]
        public void Setup()
        {
            var sqlConnection = new Microsoft.Data.SqlClient.SqlConnection(ServerConnectionString);
            var server = new Server(new ServerConnection(sqlConnection));

            if (!server.Databases.Contains(IntegrationTestDatabaseName))
            {
                var database = new Database(server, IntegrationTestDatabaseName);
                database.Create();
            }
        }

        [TearDown]
        public void TearDown()
        {
            var sqlConnection = new Microsoft.Data.SqlClient.SqlConnection(ServerConnectionString);
            var server = new Server(new ServerConnection(sqlConnection));

            if (server.Databases.Contains(IntegrationTestDatabaseName))
            {
                server.KillAllProcesses(IntegrationTestDatabaseName);
                server.KillDatabase(IntegrationTestDatabaseName);
            }
        }

        [Test]
        [Explicit]
        public void Adhoc_PerformanceTest_Harness()
        {
            var ruleAppDef = RuleApplicationDef.Load("./InRule.Runtime.Metrics.SqlServer.IntegrationTests/InvoiceForKpis.ruleappx");

            var entityState = new string[100];

            for (var i = 1; i < 101; i++)
            {
                entityState[i - 1] = File.ReadAllText($"./InRule.Runtime.Metrics.SqlServer.IntegrationTests/InvoiceJsonFiles\\Invoice{i}.json");
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            for (var i = 1; i < 101; i++)
            {
                using (var session = new RuleSession(ruleAppDef))
                {
                    session.Settings.MetricLogger = new MetricLogger(DatabaseConnectionString);
                    session.Settings.MetricServiceName = "Integration Tests";

                    var invoice = session.CreateEntity("Invoice");
                    invoice.ParseJson(entityState[i - 1]);

                    session.ApplyRules();
                }
            }

            stopWatch.Stop();
            Console.WriteLine("Execution Time:" + stopWatch.ElapsedMilliseconds);
        }

        [Test]
        public void GivenMetricsWithoutServiceName_MetricsAreStored()
        {
            var ra = new RuleApplicationDef("TestRuleApplication");
            var e1Def = ra.AddEntity("e1");
            var f1Def = e1Def.AddField("f1", DataType.Integer, "1");
            f1Def.IsMetric = true;

            using (var session = new RuleSession(ra))
            {
                session.Settings.MetricLogger = new MetricLogger(DatabaseConnectionString);

                session.CreateEntity(e1Def.Name);

                session.ApplyRules();
            }

            using (var connection = new SqlConnection(DatabaseConnectionString))
            using (var command = new SqlCommand())
            {
                var sql = $"SELECT {f1Def.Name}_{f1Def.DataType} FROM {ra.Name}.{e1Def.Name}";
                command.CommandText = sql;
                command.Connection = connection;
                connection.Open();
                var metricValue = command.ExecuteScalar();

                Assert.That(metricValue, Is.EqualTo(1));
            }
        }

        [Test]
        public void GivenMetrics_MetricsAreStored()
        {
            var ra = new RuleApplicationDef("TestRuleApplication");
            var e1Def = ra.AddEntity("e1");
            var e2Def = ra.AddEntity("e2");
            var ec1Def = e1Def.AddEntityCollection("ec1", e2Def.Name);
            var f1Def = e1Def.AddField("f1", DataType.Integer, "123");
            f1Def.IsMetric = true;
            var f2Def = e2Def.AddField("f2", DataType.Integer);
            var f3Def = e2Def.AddCalcField("f3", DataType.String, $"Concat(\"Test\", {f2Def.Name})");
            f2Def.IsMetric = true;
            f3Def.IsMetric = true;

            using (var session = new RuleSession(ra))
            {
                session.Settings.MetricLogger = new MetricLogger(DatabaseConnectionString);
                session.Settings.MetricServiceName = "Integration Tests";

                var e1 = session.CreateEntity(e1Def.Name);
                var ec1 = e1.Collections[ec1Def.Name];
                ec1.Add().Fields[f2Def.Name].SetValue(1);
                ec1.Add().Fields[f2Def.Name].SetValue(2);
                ec1.Add().Fields[f2Def.Name].SetValue(3);
                session.ApplyRules();
            }

            using (var connection = new SqlConnection(DatabaseConnectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.CommandText = $"SELECT {f1Def.Name}_{f1Def.DataType} FROM {ra.Name}.{e1Def.Name}";
                    command.Connection = connection;
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        Assert.That(reader.Read(), Is.True);
                        Assert.That(reader.GetInt32(0), Is.EqualTo(123));
                    }
                }

                using (var command = new SqlCommand())
                {
                    command.CommandText =
                        $"SELECT {f2Def.Name}_{f2Def.DataType}, {f3Def.Name}_{f3Def.DataType} FROM {ra.Name}.{e2Def.Name}";
                    command.Connection = connection;
                    using (var reader = command.ExecuteReader())
                    {
                        Assert.That(reader.Read(), Is.True);
                        Assert.That(reader.GetInt32(0), Is.EqualTo(1));
                        Assert.That(reader.GetString(1), Is.EqualTo("Test1"));
                        Assert.That(reader.Read(), Is.True);
                        Assert.That(reader.GetInt32(0), Is.EqualTo(2));
                        Assert.That(reader.GetString(1), Is.EqualTo("Test2"));
                        Assert.That(reader.Read(), Is.True);
                        Assert.That(reader.GetInt32(0), Is.EqualTo(3));
                        Assert.That(reader.GetString(1), Is.EqualTo("Test3"));
                        Assert.That(reader.Read(), Is.False);
                    }
                }
            }
        }

        [TestCase(DataType.Boolean, "true", SqlDataType.Bit)]
        [TestCase(DataType.Integer, "123", SqlDataType.Int)]
        [TestCase(DataType.Number, "123.123", SqlDataType.Decimal)]
        [TestCase(DataType.Date, "#2019-01-02#", SqlDataType.DateTime)]
        [TestCase(DataType.DateTime, "#2019-01-02T01:02:03#", SqlDataType.DateTime)]
        [TestCase(DataType.String, "\"Test\"", SqlDataType.NVarCharMax)]
        public void MetricDataTypes_CreatesCorrectSqlDataTypes(DataType dataType, string defaultValue, SqlDataType sqlDataType)
        {
            var ruleAppDef = new RuleApplicationDef("TestRuleApplication");
            var entity1Def = ruleAppDef.AddEntity("Entity1");
            var field1Def = entity1Def.AddField("Field1", dataType, defaultValue);
            field1Def.IsMetric = true;

            using (var session = new RuleSession(ruleAppDef))
            {
                session.Settings.MetricLogger = new MetricLogger(DatabaseConnectionString);
                session.Settings.MetricServiceName = "Integration Tests";

                session.CreateEntity(entity1Def.Name);

                session.ApplyRules();
            }

            var table = GetTable(ruleAppDef, entity1Def.Name);
            var column = table.Columns[field1Def.Name + "_" + dataType];

            Assert.That(column.DataType.SqlDataType, Is.EqualTo(sqlDataType));
        }

        [TestCase(DataType.Boolean, "true", SqlDataType.Bit)]
        [TestCase(DataType.Integer, "123", SqlDataType.Int)]
        [TestCase(DataType.Number, "123.123", SqlDataType.Decimal)]
        [TestCase(DataType.Date, "#2019-01-02#", SqlDataType.DateTime)]
        [TestCase(DataType.DateTime, "#2019-01-02T01:02:03#", SqlDataType.DateTime)]
        [TestCase(DataType.String, "\"Test\"", SqlDataType.NVarCharMax)]
        public async Task MetricDataTypes_CreatesCorrectSqlDataTypes_Async(DataType dataType, string defaultValue, SqlDataType sqlDataType)
        {
            var ruleAppDef = new RuleApplicationDef("TestRuleApplication");
            var entity1Def = ruleAppDef.AddEntity("Entity1");
            var field1Def = entity1Def.AddField("Field1", dataType, defaultValue);
            field1Def.IsMetric = true;

            using (var session = new RuleSession(ruleAppDef))
            {
                session.Settings.MetricLogger = new MetricLogger(DatabaseConnectionString);
                session.Settings.MetricServiceName = "Integration Tests";

                session.CreateEntity(entity1Def.Name);

                await session.ApplyRulesAsync();
            }

            var table = GetTable(ruleAppDef, entity1Def.Name);
            var column = table.Columns[field1Def.Name + "_" + dataType];

            Assert.That(column.DataType.SqlDataType, Is.EqualTo(sqlDataType));
        }

        [Test]
        public void EntitySchemaChanges_WithNewLogger_DatabaseTablesSchemaChanges()
        {
            var ruleAppDef = new RuleApplicationDef("TestRuleApplication");
            var entity1Def = ruleAppDef.AddEntity("Entity1");
            var field1Def = entity1Def.AddField("Field1", DataType.Integer, "1");
            field1Def.IsMetric = true;

            using (var session = new RuleSession(ruleAppDef))
            {
                session.Settings.MetricLogger = new MetricLogger(DatabaseConnectionString);
                session.Settings.MetricServiceName = "Integration Tests";

                session.CreateEntity(entity1Def.Name);

                session.ApplyRules();
            }

            var table = GetTable(ruleAppDef, entity1Def.Name);
            var column = table.Columns[field1Def.Name + "_" + DataType.Integer];

            Assert.That(column.DataType.SqlDataType, Is.EqualTo(SqlDataType.Int));

            field1Def.DataType = DataType.String;

            using (var session = new RuleSession(ruleAppDef))
            {
                session.Settings.MetricLogger = new MetricLogger(DatabaseConnectionString);
                session.Settings.MetricServiceName = "Integration Tests";

                session.CreateEntity(entity1Def.Name);

                session.ApplyRules();
            }

            column = table.Columns[$"{field1Def.Name}_{DataType.String}"];

            Assert.That(column.DataType.SqlDataType, Is.EqualTo(SqlDataType.NVarCharMax));
        }

        [Test]
        public void EntitySchemaChanges_ReusingLoggerInstance_DatabaseTablesSchemaChanges()
        {
            var ruleAppDef = new RuleApplicationDef("TestRuleApplication");
            var entity1Def = ruleAppDef.AddEntity("Entity1");
            var field1Def = entity1Def.AddField("Field1", DataType.Integer, "1");
            field1Def.IsMetric = true;

            var metricLogger = new MetricLogger(DatabaseConnectionString);
            using (var session = new RuleSession(ruleAppDef))
            {
                session.Settings.MetricLogger = metricLogger;
                session.Settings.MetricServiceName = "Integration Tests";

                session.CreateEntity(entity1Def.Name);

                session.ApplyRules();
            }

            var table = GetTable(ruleAppDef, entity1Def.Name);
            var column = table.Columns[$"{field1Def.Name}_{DataType.Integer}"];

            Assert.That(column.DataType.SqlDataType, Is.EqualTo(SqlDataType.Int));

            field1Def.DataType = DataType.String;

            using (var session = new RuleSession(ruleAppDef))
            {
                session.Settings.MetricLogger = metricLogger;
                session.Settings.MetricServiceName = "Integration Tests";

                session.CreateEntity(entity1Def.Name);

                session.ApplyRules();
            }

            column = table.Columns[$"{field1Def.Name}_{DataType.String}"];

            Assert.That(column.DataType.SqlDataType, Is.EqualTo(SqlDataType.NVarCharMax));
        }

        [Test]
        public void GreaterThan1000Metrics_WritesSqlViaBulkCopy()
        {
            var ra = new RuleApplicationDef(nameof(GreaterThan1000Metrics_WritesSqlViaBulkCopy));
            var e1Def = ra.AddEntity("e1");
            var e2Def = ra.AddEntity("e2");
            var ec1Def = e1Def.AddEntityCollection("ec1", e2Def.Name);
            var f2Def = e2Def.AddField("f2", DataType.Integer);
            var f3Def = e2Def.AddCalcField("f3", DataType.String, $"Concat(\"Test\", {f2Def.Name})");
            f2Def.IsMetric = true;
            f3Def.IsMetric = true;
            e1Def.AddAutoSeqRuleSet("rs1")
                .AddSimpleRule("ifThen1", $"Count({ec1Def.Name}) < {MetricLogger.BulkCopyMetricsThreshold}")
                .AddAddCollectionMemberAction("add1", ec1Def.Name,
                    new NameExpressionPairDef(f2Def.Name, $"Count({ec1Def.Name})"));

            using (var session = new RuleSession(ra))
            {
                session.Settings.MetricLogger = new MetricLogger(DatabaseConnectionString);
                session.Settings.MetricServiceName = "Integration Tests";

                var e1 = session.CreateEntity(e1Def.Name);
                var ec1 = e1.Collections[ec1Def.Name];
                session.ApplyRules();

                Assert.That(ec1.Count, Is.EqualTo(MetricLogger.BulkCopyMetricsThreshold));
                Assert.That(ec1[0].Fields[f2Def.Name].Value.ToInt32(), Is.EqualTo(1));
                Assert.That(ec1[999].Fields[f2Def.Name].Value.ToInt32(), Is.EqualTo(1000));
            }

            using (var connection = new SqlConnection(DatabaseConnectionString))
            using (var command = new SqlCommand())
            {
                var sql =
                    $"SELECT {f2Def.Name}_{f2Def.DataType}, {f3Def.Name}_{f3Def.DataType} FROM {ra.Name}.{e2Def.Name}";
                command.CommandText = sql;
                command.Connection = connection;
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    Assert.That(reader.GetInt32(0), Is.EqualTo(1));

                    var rowCount = 1;
                    var lastRowIntValue = -1;
                    string lastRowStringValue = null;
                    while (reader.Read())
                    {
                        lastRowIntValue = reader.GetInt32(0);
                        lastRowStringValue = reader.GetString(1);
                        rowCount++;
                    }

                    Assert.That(rowCount, Is.EqualTo(MetricLogger.BulkCopyMetricsThreshold));
                    Assert.That(lastRowIntValue, Is.EqualTo(1000));
                    Assert.That(lastRowStringValue, Is.EqualTo("Test1000"));
                }
            }
        }

        [Test]
        public void ComplexField_CreatesValidTableName()
        {
            var ra = new RuleApplicationDef();
            var e1Def = ra.AddEntity("e1");
            var cf1Def = e1Def.AddComplexField("cf1");
            var f1Def = cf1Def.AddCalcField("f1", DataType.Integer, "123");
            f1Def.IsMetric = true;

            var metricLogger = new MetricLogger(DatabaseConnectionString);

            using (RuleSession session = new RuleSession(ra))
            {
                session.Settings.MetricLogger = metricLogger;
                session.Settings.MetricServiceName = "Integration Tests";

                session.CreateEntity(e1Def.Name);

                session.ApplyRules();
            }

            string expectedTableName = $"{e1Def.Name}/{cf1Def.Name}";

            var table = GetTable(ra, expectedTableName);
            Assert.That(table, Is.Not.Null);
            Assert.That(table.Name, Is.EqualTo(expectedTableName));

            using (var connection = new SqlConnection(DatabaseConnectionString))
            using (var command = new SqlCommand())
            {
                var sql = $"SELECT {f1Def.Name}_{f1Def.DataType} FROM [{ra.Name}].[{expectedTableName}]";
                command.CommandText = sql;
                command.Connection = connection;
                connection.Open();
                var metricValue = command.ExecuteScalar();

                Assert.That(metricValue, Is.EqualTo(123));
            }
        }

        [Test]
        public void ComplexCollection_CreatesValidTableName()
        {
            var ra = new RuleApplicationDef();
            var e1Def = ra.AddEntity("e1");
            var cf1Def = e1Def.AddComplexField("cf1");
            var cc1Def = cf1Def.AddComplexCollection("cc1");
            var f1Def = cc1Def.AddCalcField("f1", DataType.Integer, "123");
            f1Def.IsMetric = true;

            var metricLogger = new MetricLogger(DatabaseConnectionString);

            using (RuleSession session = new RuleSession(ra))
            {
                session.Settings.MetricLogger = metricLogger;
                session.Settings.MetricServiceName = "Integration Tests";

                var e1 = session.CreateEntity(e1Def.Name);
                var cf1 = e1.Fields[cf1Def.Name];
                var cc1 = cf1.Collections[cc1Def.Name];
                cc1.Add();

                session.ApplyRules();
            }

            string expectedTableName = $"{e1Def.Name}/{cf1Def.Name}/{cc1Def.Name}";

            var table = GetTable(ra, expectedTableName);
            Assert.That(table, Is.Not.Null);
            Assert.That(table.Name, Is.EqualTo(expectedTableName));

            using (var connection = new SqlConnection(DatabaseConnectionString))
            using (var command = new SqlCommand())
            {
                var sql = $"SELECT {f1Def.Name}_{f1Def.DataType} FROM [{ra.Name}].[{expectedTableName}]";
                command.CommandText = sql;
                command.Connection = connection;
                connection.Open();
                var metricValue = command.ExecuteScalar();

                Assert.That(metricValue, Is.EqualTo(123));
            }
        }
    }
}