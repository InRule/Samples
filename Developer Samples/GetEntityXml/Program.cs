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

			// Get the XML representing the invoice entity
			var xml = invoiceEntity.GetXml();

			Console.WriteLine(xml);
			Console.ReadLine();
		}
	}
}