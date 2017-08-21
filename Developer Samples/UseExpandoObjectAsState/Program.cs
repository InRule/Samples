using System;
using System.Collections.Generic;
using System.Dynamic;
using InRule.Runtime;

internal class Program
{
	private static void Main()
	{
		// Load rule application from file system
		using (var session = new RuleSession(new FileSystemRuleApplicationReference("InvoiceLineItem.ruleappx")))
		{
			// Create an Expando object (standard .NET dynamic object) to use as bound state for the Entity
			dynamic expando = new ExpandoObject();

			// Create Invoice entity, passing in the Expando as the bound state
			var invoiceEntity = session.CreateEntity("Invoice", expando);

			// Create a line item collection member via traditional SDK members and set its values
			var lineItem = invoiceEntity.Collections["LineItems"].Add();
			lineItem.Fields["Quantity"].Value = 1;
			lineItem.Fields["Price"].Value = 2;

			var lineItem2 = invoiceEntity.Collections["LineItems"].Add();
			lineItem2.Fields["Quantity"].Value = 7;
			lineItem2.Fields["Price"].Value = 45.50;

			// Apply rules
			session.ApplyRules();

			// Get the value of the Total calculation
			var total = invoiceEntity.Fields["Total"].Value;

			Console.WriteLine("Total is: " + total);

			// Examine the Expando to make sure it was updated during execution
			var totalFromExpando = expando.Total;

			Console.WriteLine("Total from the Expando object is: " + totalFromExpando);

			// For good measure, let's check out the line items
			var lineItems = (IEnumerable<dynamic>)expando.LineItems;

			foreach (var item in lineItems)
			{
				Console.WriteLine("Line item from Expando Quantity is: " + item.Quantity);
				Console.WriteLine("Line item from Expando Price is: " + item.Price);
			}

			Console.ReadLine();
		}
	}
}