using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InRule.Repository;
using InRule.Runtime;
using Serilog;

namespace LibraryLoggerSampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            Log.Logger = log;

            var ruleAppDef = new RuleApplicationDef();
            var entity1Def = ruleAppDef.Entities.Add(new EntityDef("entity1"));
            var fieldDef = new FieldDef();
            

            fieldDef.IsCalculated = true;
            fieldDef.Calc.FormulaText = "1 + 1";

            entity1Def.Fields.Add(fieldDef);

            using (var session = new RuleSession(ruleAppDef))
            {
                var entity1 = session.CreateEntity(entity1Def.Name);
                session.ApplyRules();
                Console.WriteLine(entity1.Fields[fieldDef.Name].Value.ToString());
            }

            
            Console.ReadLine();

        }
    }
}
