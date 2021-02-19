using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MstestWrapperForTestsuite
{
    [TestClass]
    public class DataDrivenWrapperForTestsuite : DataDrivenWrapperForTestsuiteBase
    {
        [TestMethod]
        [DataSource(
            "Microsoft.VisualStudio.TestTools.DataSource.CSV",
            "|DataDirectory|\\data\\testsuitelist.csv",
            "testsuitelist#csv",
            DataAccessMethod.Sequential)]
        [DeploymentItem("data")]
        public void TestInRuleTestSuite()
        {
            var ruleAppFilePath = TestContext.DataRow["RuleaAppFilePath"].ToString();
            var testSuiteFilePath = TestContext.DataRow["TestSuiteFilePath"].ToString();

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
