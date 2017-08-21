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

			// Create a line item collection member via traditional SDK members and set its values
			var lineItem = invoiceEntity.Collections["LineItems"].Add();
			lineItem.Fields["Quantity"].Value = 1;
			lineItem.Fields["Price"].Value = 2;

			// Create a line item collection member with the dynamic keyword, allowing access to fields, calculations
			// and collections without having to use the Fields, Calculations and Colletions properties, respectively.
			dynamic lineItemDynamic = invoiceEntity.Collections["LineItems"].Add();
			lineItemDynamic.Quantity = 3;
			lineItemDynamic.Price = 4;

			// Apply rules
			session.ApplyRules();

			// Get the value of the Total calculation
			var total = invoiceEntity.Fields["Total"].Value;

			Console.WriteLine("Total is: " + total);

			Console.ReadLine();
		}
	}
}