using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MstestWrapperForTestsuite
{
    public class FolderConventionDataSourceAttribute : Attribute, ITestDataSource
    {
        public string RuleAppPath { get; }
        public string TestSuitePath { get; }

        /// <summary>
        /// By convention all the testSuite files related to a ruleapp
        /// will be placed under a subfolder of testSuitePath named after the ruleapp
        /// for example, if ruleAppPath = "ruleapp", testSuitePath = "testsuite"
        /// the folder structure will be as follows
        /// ├───ruleapp
        /// │       RectangleApp.ruleappx
        /// │       SquareApp.ruleappx
        /// │
        /// └───testsuite
        ///     ├───rectangleApp
        ///     │       Rectangle.testsuite
        ///     │
        ///     └───squareApp
        ///             Square.testsuite
        /// </summary>
        /// <param name="ruleAppPath">The path to the ruleApp files</param>
        /// <param name="testSuitePath">The path to the testSuite files</param>
        public FolderConventionDataSourceAttribute(string ruleAppPath, string testSuitePath)
        {
            RuleAppPath = ruleAppPath;
            TestSuitePath = testSuitePath;
        }

        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            DirectoryInfo ruleappDirectory = new DirectoryInfo(RuleAppPath);

            foreach (var ruleAppFile in ruleappDirectory.GetFiles("*.ruleappx"))
            {
                DirectoryInfo testsuiteDirectory = GetConventionTestSuiteDirectory(ruleAppFile);
                if (testsuiteDirectory.Exists)
                {
                    foreach (var testSuiteFile in testsuiteDirectory.GetFiles("*.testsuite"))
                    {
                        yield return new object[] { ruleAppFile.Name, testSuiteFile.Name, ruleAppFile.FullName, testSuiteFile.FullName };
                    }
                }
                else
                {
                    yield return new object[] { ruleAppFile.Name, string.Empty, ruleAppFile.FullName, string.Empty };
                }
            }
        }

        private DirectoryInfo GetConventionTestSuiteDirectory(FileInfo ruleAppFile)
        {
            var ruleAppFileName = Path.GetFileNameWithoutExtension(ruleAppFile.FullName);
            var testSuiteForAppPath = Path.Combine(TestSuitePath, ruleAppFileName);
            DirectoryInfo testsuiteDirectory = new DirectoryInfo(testSuiteForAppPath);
            return testsuiteDirectory;
        }

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            if (data != null)
                return string.Format("{0} ({1}, {2})", methodInfo.Name, data[0], data[1]);

            return null;
        }
    }
}