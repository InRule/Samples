using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MstestWrapperForTestsuite
{
    [TestClass]
    public class DataDrivenWrapperForTestsuite : DataDrivenWrapperForTestsuiteBase
    {
        [DataTestMethod]
        [DataSource(
            "Microsoft.VisualStudio.TestTools.DataSource.CSV",
            "|DataDirectory|\\data\\testsuitelist.csv",
            "testsuitelist#csv",
            DataAccessMethod.Sequential)]
        [DeploymentItem("..\\..\\data")]
        public void TestInRuleTestSuiteFromCsvDataSource()
        {
            var ruleAppFilePath = TestContext.DataRow["RuleAppFilePath"].ToString();
            var testSuiteFilePath = TestContext.DataRow["TestSuiteFilePath"].ToString();

            ExecuteInRuleTestSuite(ruleAppFilePath, testSuiteFilePath);
        }

        [DataTestMethod]
        [FolderConventionDataSource(ruleAppPath: "ruleapp", testSuitePath: "testsuite")]
        [DeploymentItem("..\\..\\data")]
        public void TestInRuleFromFolderConvention(string ruleApp, string testSuite, string ruleAppFilePath, string testSuiteFilePath)
        {
            TestContext.WriteLine($"{ruleApp} - {testSuite}");

            ExecuteInRuleTestSuite(ruleAppFilePath, testSuiteFilePath);
        }
    }
}
