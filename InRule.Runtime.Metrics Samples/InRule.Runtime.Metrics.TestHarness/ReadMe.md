# InRule Metrics Sample
The purpose of this sample is to show an end to end, simple implentation of the InRule Metrics product offering. Typically, Metrics will be logged to a data store (e.g. SQL Server, Azure Table Storage, etc.), however, for this sample, we are writing to to a file on disk in a comma separated values format (CSV).

The sample includes the following:
* Invoice.ruleappx
  * Located in the RuleApps folder of the TestHarness project
  * Simple invoice rule application (Top level entity has a collection of Lineitems)
  * Fields on the Invoice entity have been flagged as Metrics, which means when the engine runs, it will capture these values
* InRule.Runtime.Metrics.TestHarness
  * This project has a WPF Window that will initiate the processing of rules. 
  * There are 100 data files in the Data directory of the project. The engine will emit the metric values for each transaction as a "row" of output per InRule Entity. In this sample, metrics are only captured on the Invoice entity and therefore there will be a single row of output per execution.
  * The Metrics will be output to the Output directory in this project. 
* InRule.Runtime.Metrics.Csv
  * This project is used to consume the metrics that are emitted by the rule engine.
  * It implements the IMetricLogger interface.
  * Customers can create their own custom metric loggers by implementing this interface and store the metric values whereever they need.
  * There is a synchronous and asynchronous methods for logging.
    * Synchronous - <Explain here>
    * Asynchronous - <Explain here>

To run the sample:
* Clone the GitHub repo
* Use NuGet to get the InRule assemblies
* Run the application
* Click the "Run Rules" button
  * The grid we populate with the metrics values for the Invoice rule application

# TODO
* Enter desciptions of Sync and Asyc
* Have somebody run through the instructions to find any gotchas
* Don't publish until the new NuGet packages are published