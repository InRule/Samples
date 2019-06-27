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

            string ruleAppFilePath = null;
            string testSuiteFilePath = null;

            var clParams = new OptionSet {
                { "h|help", "Display Help.", k => showHelp = true },
                { "r|RuleAppPath=",  "Path to the Rule Application to be tested.", p => ruleAppFilePath = p },
                { "n|TestSuitePath=", "The path to the .testsuite file to run.", p => testSuiteFilePath = p },
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
            else if (string.IsNullOrEmpty(ruleAppFilePath) || string.IsNullOrEmpty(testSuiteFilePath))
            {
                Console.WriteLine("Parameters must be specified for both the RuleAppPath as well as the TestSuitePath.");
                return 2;
            }
            else
            {
                var result = RunTestSuite(ruleAppFilePath, testSuiteFilePath);
                return result;
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

        private static int RunTestSuite(string ruleAppFilePath, string testSuiteFilePath)
        {
            RuleApplicationDef ruleAppDef = null;
            TestSuiteDef suite = null;

            try
            {
                if (File.Exists(ruleAppFilePath))
                {
                    Console.WriteLine("Using Rule App " + ruleAppFilePath);
                }
                else
                {
                    Console.WriteLine("ERROR: Rule App file not found at " + ruleAppFilePath);
                    throw new FileNotFoundException("ERROR: Rule App file not found at " + ruleAppFilePath);
                }
                if (File.Exists(testSuiteFilePath))
                {
                    Console.WriteLine("Using Test Suite " + ruleAppFilePath);
                }
                else
                {
                    Console.WriteLine("ERROR: Test Suite file not found at " + ruleAppFilePath);
                    throw new FileNotFoundException("ERROR: Rule App file not found at " + ruleAppFilePath);
                }

                var ruleApp = new FileSystemRuleApplicationReference(ruleAppFilePath);
                ruleAppDef = ruleApp.GetRuleApplicationDef();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Unable to load Rule App: " + ex.Message);
                return 2;
            }

            try
            {
                if (ruleAppDef != null)
                {
                    suite = TestSuiteDef.LoadFrom(new ZipFileTestSuitePersistenceProvider(testSuiteFilePath));
                    suite.ActiveRuleApplicationDef = ruleAppDef;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Unable to load Test Suite: " + ex.Message);
                return 2;
            }

            try
            {
                if (suite != null)
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
                else
                {
                    return 2;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Failed to execute test suite: " + ex.Message);
                return 2;
            }
        }
    }
}