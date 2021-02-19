using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

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
                else if (result.Passed)
                {
                    TestContext.WriteLine($"PASS: {result.TestDef.DisplayName}");
                }
                else
                {
                    TestContext.WriteLine($"FAIL: {result.TestDef.DisplayName}");
                    foreach (var failedAssertionResult in result.AssertionResults.Where(ar => ar.Passed == false))
                    {
                        TestContext.WriteLine($"  {failedAssertionResult.Target} was {failedAssertionResult.ActualValue}, expected value {failedAssertionResult.ExpectedValue}");
                    }
                }

                Assert.AreEqual(true, result.Passed);
                Assert.IsNull(result.RuntimeErrorMessage);
            }
        }
    }
}
