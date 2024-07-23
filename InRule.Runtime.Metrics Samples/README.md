# InRule.Runtime.Metrics

With InRule Metrics, enterprises can track field values, execution frequency, evaluated criteria, and results to improve declarative logic and deliver better business outcomes. 
The adapter-based model allows users to write data to various storage targets including Azure Table Storage, SQL Server, or a CSV file.  This logged information is valuable to understand how rules are performing over time and can be used to measure decision automation KPIs.
 
How it works:
In irAuthor, users flag fields and certain rule types as metrics. During execution, the engine will collect the field or rule name and its value. The engine will then output the selected metrics to a logger. Values are output and organized according to the data structure established in the rule application. For example, if the Product and Total fields are flagged as metrics on an Invoice entity, the rule engine will output a metric for Invoice with the name and values for Product and Total.

The adapter model for InRule Metrics is a .NET assembly made available to the rule engine that implements the IMetricLogger interface. When this assembly exists, the engine will call out to the required methods in the assembly to perform logging. This enables users to write metrics to the location desired for their implementation.

Currently, there are three available adapters:
- Azure Table Storage 
- SQL Server
- CSV
