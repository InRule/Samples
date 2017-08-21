using System;
using System.Collections.Generic;
using InRule.Runtime;

internal class Program
{
	private static void Main()
	{
		// Load rule application from file system
		using (var session = new RuleSession(new FileSystemRuleApplicationReference("InvoiceLineItem.ruleappx")))
		{
			// Create a string->object Dictionary for backing state of the Entity
			var dictionary = new Dictionary<string, object>();

			// Create Invoice entity, passing in the Expando as the bound state
			var invoiceEntity = session.CreateEntity("Invoice", dictionary);

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

			// Examine the dictionary to make sure it was updated during execution
			var totalFromDictionary = dictionary["Total"];

			Console.WriteLine("Total from the Dictionary is: " + totalFromDictionary);

			// For good measure, let's check out the line items
			var lineItems = (IEnumerable<Dictionary<string, object>>)dictionary["LineItems"];

			foreach (var item in lineItems)
			{
				Console.WriteLine("Line item from Dictionary Quantity is: " + item["Quantity"]);
				Console.WriteLine("Line item from Dictionary Price is: " + item["Price"]);
			}

			Console.ReadLine();
		}
	}
}