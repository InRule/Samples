using InRule.Repository;
using InRule.Repository.Regression;
using InRule.Runtime;
using InRule.Runtime.Testing.Regression;
using InRule.Runtime.Testing.Regression.Runtime;
using InRule.Runtime.Testing.Session;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace MstestWrapperForTestsuite
{
    [TestClass]
    public abstract class DataDrivenWrapperForTestsuiteBase
    {
        public TestContext TestContext { get; set; }

        protected TestResultCollection RunTestSuite(string ruleAppFilePath, string testSuiteFilePath)
        {
            if (!File.Exists(ruleAppFilePath))
            {
                throw new FileNotFoundException("ERROR: Rule App file not found at " + ruleAppFilePath);
            }
            if (!File.Exists(testSuiteFilePath))
            {
                throw new FileNotFoundException("ERROR: Test Suite file not found at " + testSuiteFilePath);
            }

            var ruleApp = new FileSystemRuleApplicationReference(ruleAppFilePath);
            RuleApplicationDef ruleAppDef = ruleApp.GetRuleApplicationDef();

            TestSuiteDef suite = TestSuiteDef.LoadFrom(new ZipFileTestSuitePersistenceProvider(testSuiteFilePath));
            suite.ActiveRuleApplicationDef = ruleAppDef;

            TestResultCollection results;
            using (TestingSessionManager manager = new TestingSessionManager(new InProcessConnectionFactory()))
            {
                var session = new RegressionTestingSession(manager, suite);
                results = session.ExecuteAllTests();
            }

            return results;
        }

        protected void ExecuteInRuleTestSuite(string ruleAppFilePath, string testSuiteFilePath)
        {
            var testResultCollection = RunTestSuite(ruleAppFilePath, testSuiteFilePath);

            TestContext.WriteLine("Using Rule App " + ruleAppFilePath);
            TestContext.WriteLine("Using Test Suite " + testSuiteFilePath);

            foreach (var result in testResultCollection)
            {
                if (result.RuntimeErrorMessage != null)
                {
                    TestContext.WriteLine($"ERROR: Failed to execute test {result.TestDef.DisplayName}: {result.RuntimeErrorMessage}");
                }
                else
                {
                    result.ReportAssertionResultsToContext(TestContext);
                }

                Assert.AreEqual(true, result.Passed);
                Assert.IsNull(result.RuntimeErrorMessage);
            }
        }
    }
}
