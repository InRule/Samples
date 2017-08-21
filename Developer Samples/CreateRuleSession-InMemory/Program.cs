using System;
using InRule.Repository;
using InRule.Runtime;

internal class Program
{
	private static void Main()
	{
		// Create rule application definition in memory
		var ruleApp = new RuleApplicationDef();
		var entity = ruleApp.Entities.Add(new EntityDef("Entity"));
		entity.Fields.Add(new FieldDef("Field1"));
		entity.Fields.Add(new FieldDef("Field2"));
		entity.Fields.Add(new FieldDef("Total", "Field1 + Field2"));

		// Create the rule session from the in-memory rule application
		using (var session = new RuleSession(new InMemoryRuleApplicationReference(ruleApp)))
		{
			// Create entity
			var invoiceEntity = session.CreateEntity("Entity");

			// Set values
			invoiceEntity.Fields["Field1"].Value = 2;
			invoiceEntity.Fields["Field2"].Value = 3;

			// Apply rules
			session.ApplyRules();

			// Get the value of the Total calculation
			var total = invoiceEntity.Fields["Total"].Value;

			Console.WriteLine("Total is: " + total);
			Console.ReadLine();
		}
	}
}