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


    }
}
