using System;
using InRule.Runtime;

internal class Program
{
	private static void Main()
	{
        // Load rule application from catalog
        var ruleappref = new CatalogRuleApplicationReference("http://localhost/InRuleCatalogService/Service.svc", "InvoiceLineItem");
        ruleappref.Credentials = new CatalogCredentials("admin", "password");
        using (var session = new RuleSession(ruleappref))
        {
            // Create Invoice entity
			var invoiceEntity = session.CreateEntity("Invoice");

			// Read the state file into the entity
			invoiceEntity.LoadXml(@"InvoiceLineItem.xml");

			// Apply rules
			session.ApplyRules();

			// Get the value of the Total calculation
			var total = invoiceEntity.Fields["Total"].Value;

			Console.WriteLine("Total is: " + total);
			Console.ReadLine();
		}
	}
}