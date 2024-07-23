CREATE TABLE MetricSchemaStore
(
	Id int NOT NULL IDENTITY PRIMARY KEY,
	RuleAppName nvarchar(MAX) NOT NULL,
	EntityName nvarchar(MAX) NOT NULL,
	MetricSchema nvarchar(MAX) NOT NULL,
)