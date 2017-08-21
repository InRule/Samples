using System;
using InRule.Runtime;

internal class Program
{
	private static void Main()
	{
		// Load rule application from file system
		using (var session = new RuleSession(new FileSystemRuleApplicationReference("InvoiceLineItem.ruleappx")))
		{
			// Logging is turned off by default, so the engine must be instructed
			// to log value changes
            session.Settings.LogOptions = EngineLogOptions.StateChanges;

			// Create Invoice entity
			var invoiceEntity = session.CreateEntity("Invoice");

			// Read the state file into the entity
			invoiceEntity.LoadXml("InvoiceLineItem.xml");

			// Apply rules 
			var executionLog = session.ApplyRules();

			// Iterate through the value change messages
			foreach (var valueChangeMessage in executionLog.StateValueChanges)
			{
				Console.WriteLine("The value for '{0}' changed to: {1}", 
				                  valueChangeMessage.ElementId,
				                  valueChangeMessage.NewValue);
			}

			Console.ReadLine();
		}
	}
}