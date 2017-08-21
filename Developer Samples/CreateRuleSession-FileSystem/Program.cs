using System;
using InRule.Runtime;

internal class Program
{
	private static void Main()
	{
		// Load rule application from file system
		using (var session = new RuleSession(new FileSystemRuleApplicationReference("InvoiceLineItem.ruleappx")))
		{
			// Create Invoice entity
			var invoiceEntity = session.CreateEntity("Invoice");

			// Read the state file into the entity
			invoiceEntity.LoadXml("InvoiceLineItem.xml");

			// Apply rules
			session.ApplyRules();

			// Get the value of the Total calculation
			var total = invoiceEntity.Fields["Total"].Value;

			Console.WriteLine("Total is: " + total);

			Console.ReadLine();
		}
	}
}