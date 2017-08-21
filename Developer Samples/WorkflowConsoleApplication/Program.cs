using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Activities;
using System.Activities.Statements;
using System.Xml.Serialization;
using InRule.Runtime;

namespace WorkflowConsoleApplication
{

    class Program
    {
        static void Main(string[] args)
        {
            CallInRule();
        }

        static void CallInRule()
        {
            Console.WriteLine("Start Workflow Execution");

            Console.Write("Enter your name: ");

            var name = Console.ReadLine();

            //Create collection of arguments to be passed into the workflow.
            //Arguments are defined in the workflow xaml.
            var wfArgs = new Dictionary<string, object>();

            //Option 1: pass in this xml as an argument
            //var xml = @"<Person><FirstName>Mike</FirstName></Person>";

            //Option 2: Create and instance of an object and serialize it to an xml string
            //var person = new Person { FirstName = "Mike" };
            //var xml = SerializeToString(person);

            //Option 3: Create and instance of an object and pass it in as an argument.
            //The argument (as defined in the workflow designer) will need to be updated
            //to reflect the object type being passed in.
            var person = new Model.Person { FirstName = name.Trim() };
            wfArgs["varStateXml"] = person;

            Workflow1 workflow = new Workflow1();

            var result = WorkflowInvoker.Invoke(workflow, wfArgs);

            Console.WriteLine(person.Salutation);

            var ruleExecutionLog = result["varRuleExecutionLog"] as RuleExecutionLog;

            Console.WriteLine("Workflow Execution Complete");

            Console.ReadLine();
        }

        static string SerializeToString(object obj)
        {
            var serializer = new XmlSerializer(obj.GetType());

            using (StringWriter w = new StringWriter())
            {
                serializer.Serialize(w, obj);

                return w.ToString();
            }
        }
    }
}
