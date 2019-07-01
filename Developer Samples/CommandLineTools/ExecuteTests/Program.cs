using System;
using InRule.Runtime;
using Mono.Options;
using InRule.Repository.Regression;
using InRule.Runtime.Testing.Regression;
using InRule.Runtime.Testing.Session;
using InRule.Runtime.Testing.Regression.Runtime;
using System.Linq;
using InRule.Repository;
using System.IO;

namespace ExecuteTests
{
    class Program
    {
        static int Main(string[] args)
        {
            bool showHelp = false;
            string TestSuiteFilePath = null;
            //Option 1
            string RuleAppFilePath = null;
            //Option 2
            string CatalogUri = null;
            string CatalogUsername = null;
            string CatalogPassword = null;
            string CatalogRuleAppName = null;
            string CatalogRuleAppLabel = null;
            string CatalogRuleAppRevision = null;

            var clParams = new OptionSet {
                { "h|help", "Display Help.", k => showHelp = true },
                { "t|TestSuitePath=", "The path to the .testsuite file to run.", p => TestSuiteFilePath = p },
                //Option 1
                { "r|RuleAppPath=",  "Path to the Rule Application to be compiled.", p => RuleAppFilePath = p },
                //Option 2
                { "c|CatUri=",  "Web URI for the IrCatalog Service endpoint.", p => CatalogUri = p },
                { "u|CatUsername=",  "IrCatalog Username for authentication.", p => CatalogUsername = p },
                { "p|CatPassword=",  "IrCatalog Password for authentication.", p => CatalogPassword = p },
                { "n|CatRuleAppName=",  "Name of the Rule Application.", p => CatalogRuleAppName = p },
                { "l|CatLabel=",  "Label of the Rule Application to retrieve.", p => CatalogRuleAppLabel = p },
                { "v|CatRevision=",  "Revision of the Rule Application to retrieve.", p => CatalogRuleAppRevision = p },
            };

            try
            {
                clParams.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("Failed parsing execution parameters: " + e.Message);
                showHelp = true;
            }

            if (showHelp)
            {
                ShowHelp(clParams);
                return 2;
            }
            else if (string.IsNullOrEmpty(TestSuiteFilePath))
            {
                Console.WriteLine("Parameter must be specified for the TestSuitePath.");
                return 2;
            }
            else if (!string.IsNullOrEmpty(RuleAppFilePath))
            {
                try
                {
                    if (File.Exists(RuleAppFilePath))
                    {
                        Console.WriteLine("Using Rule App " + RuleAppFilePath);
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Rule App file not found at " + RuleAppFilePath);
                        return 2;
                    }

                    var result = RunTestSuite(new FileSystemRuleApplicationReference(RuleAppFilePath), TestSuiteFilePath);
                    return result;
                }
                catch (IntegrationException ie)
                {
                    Console.WriteLine("Error creating reference to file-based Rule App: " + ie.Message); //Rule App file not found
                    return 2;
                }
            }
            else if (!string.IsNullOrEmpty(CatalogUri)
                && !string.IsNullOrEmpty(CatalogUsername)
                && !string.IsNullOrEmpty(CatalogPassword)
                && !string.IsNullOrEmpty(CatalogRuleAppName))
            {
                try
                {
                    RuleApplicationReference ruleApp;
                    int ruleAppRevision;
                    if (!string.IsNullOrEmpty(CatalogRuleAppLabel))
                    {
                        ruleApp = new CatalogRuleApplicationReference(CatalogUri, CatalogRuleAppName, CatalogUsername, CatalogPassword, CatalogRuleAppLabel);
                        Console.WriteLine($"Loading Rule App {CatalogRuleAppName} with Label {CatalogRuleAppLabel} from {CatalogUri}");
                    }
                    else if (!string.IsNullOrEmpty(CatalogRuleAppRevision) && int.TryParse(CatalogRuleAppRevision, out ruleAppRevision))
                    {
                        ruleApp = new CatalogRuleApplicationReference(CatalogUri, CatalogRuleAppName, CatalogUsername, CatalogPassword, ruleAppRevision);
                        Console.WriteLine($"Loading Rule App {CatalogRuleAppName} with Revision {ruleAppRevision} from {CatalogUri}");
                    }
                    else
                    {
                        ruleApp = new CatalogRuleApplicationReference(CatalogUri, CatalogRuleAppName, CatalogUsername, CatalogPassword);
                        Console.WriteLine($"Loading Rule App {CatalogRuleAppName} from {CatalogUri}");
                    }

                    var result = RunTestSuite(ruleApp, TestSuiteFilePath);
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error creating reference to Catalog-based Rule App: " + ex.Message);
                    return 2;
                }
            }
            else
            {
                Console.WriteLine("You must provide either RuleAppPath or all of CatUri, CatUsername, CatPassword, and CatRuleAppName (with optional CatLabel or CatRevision)");
                return 2;
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine();
            Console.WriteLine("Usage: RunTests.exe [OPTIONS]");
            Console.WriteLine("Executes a Test Suite against a file-based Rule Application.");
            Console.WriteLine();
            Console.WriteLine("All requests must contain RuleAppPath and TestSuitePath.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
        }

        private static int RunTestSuite(RuleApplicationReference ruleApp, string testSuiteFilePath)
        {
            RuleApplicationDef ruleAppDef = null;
            TestSuiteDef suite = null;

            try
            {
                ruleAppDef = ruleApp.GetRuleApplicationDef();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Unable to load Rule App: " + ex.Message);
                return 2;
            }

            try
            {
                if (File.Exists(testSuiteFilePath))
                {
                    Console.WriteLine("Using Test Suite " + testSuiteFilePath);
                }
                else
                {
                    Console.WriteLine("ERROR: Test Suite file not found at " + testSuiteFilePath);
                    throw new FileNotFoundException("ERROR: Test Suite file not found at " + testSuiteFilePath);
                }

                suite = TestSuiteDef.LoadFrom(new ZipFileTestSuitePersistenceProvider(testSuiteFilePath));
                suite.ActiveRuleApplicationDef = ruleAppDef;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Unable to load Test Suite: " + ex.Message);
                return 2;
            }

            try
            {
                TestResultCollection results;
                using (TestingSessionManager manager = new TestingSessionManager(new InProcessConnectionFactory()))
                {
                    var session = new RegressionTestingSession(manager, suite);
                    results = session.ExecuteAllTests();
                }

                bool hadFailures = false;
                foreach (var result in results)
                {
                    if (result.RuntimeErrorMessage != null)
                    {
                        Console.WriteLine($"ERROR: Failed to execute test {result.TestDef.DisplayName}: {result.RuntimeErrorMessage}");
                        hadFailures = true;
                    }
                    else if (result.Passed)
                    {
                        Console.WriteLine($"PASS: {result.TestDef.DisplayName}");
                    }
                    else
                    {
                        hadFailures = true;
                        Console.WriteLine($"FAIL: {result.TestDef.DisplayName}");
                        foreach (var failedAssertionResult in result.AssertionResults.Where(ar => ar.Passed == false))
                        {
                            Console.WriteLine($"  {failedAssertionResult.Target} was {failedAssertionResult.ActualValue}, expected value {failedAssertionResult.ExpectedValue}");
                        }
                    }
                }

                if (hadFailures)
                    return 1;
                else
                    return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Failed to execute test suite: " + ex.Message);
                return 2;
            }
        }
    }
}