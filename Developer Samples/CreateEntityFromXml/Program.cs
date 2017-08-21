using System;
using InRule.Runtime;

internal class Program
{
	private static void Main()
	{
		// Load rule application from file system
		using (var session = new RuleSession(new FileSystemRuleApplicationReference("InvoiceLineItem.ruleapp")))
		{
			// Create XML for line items
			var lineItem1Xml = "<LineItem><Quantity>1</Quantity><Price>2</Price></LineItem>";
			var lineItem2Xml = "<LineItem><Quantity>3</Quantity><Price>4</Price></LineItem>";

			// Create Invoice entity
			var invoiceEntity = session.CreateEntity("Invoice");

			// Create the line items using the XML from above
			var lineItem1Entity = session.CreateEntity("LineItem", lineItem1Xml);
			var lineItem2Entity = session.CreateEntity("LineItem", lineItem2Xml);

			// Add the line items to the Invoice
			invoiceEntity.Collections["LineItems"].Add(lineItem1Entity);
			invoiceEntity.Collections["LineItems"].Add(lineItem2Entity);

			// Apply rules
			session.ApplyRules();

			// Get the value of the Total calculation
			var total = invoiceEntity.Fields["Total"].Value;

			Console.WriteLine("Total is: " + total);

			Console.ReadLine();
		}
	}
}