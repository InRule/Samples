using System;
using InRule.Repository;
using InRule.Runtime;

internal class Program
{
	private static void Main()
	{
		//
		// Create the rule application
		//
		var ruleAppDef = new RuleApplicationDef();

		// Add Entity
		var entityDef = new EntityDef("Rectangle");
		ruleAppDef.Entities.Add(entityDef);

		// Add Fields
		entityDef.Fields.Add(new FieldDef("Width", DataType.Number));
		entityDef.Fields.Add(new FieldDef("Length", DataType.Number));
		entityDef.Fields.Add(new FieldDef("Area", "Width * Length", DataType.Number));

		//
		// Run rules
		//
		using (var ruleSession = new RuleSession(ruleAppDef))
		{
			// create default entity state
			var entity = ruleSession.CreateEntity("Rectangle");

			entity.Fields["Width"].Value = 20;
			entity.Fields["Length"].Value = 10;
			ruleSession.ApplyRules();

			Console.WriteLine("The area is: " + entity.Fields["Area"].Value);
			Console.ReadLine();
		}
	}
}