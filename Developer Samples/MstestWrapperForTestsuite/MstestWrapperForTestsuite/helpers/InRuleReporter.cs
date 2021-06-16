using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MstestWrapperForTestsuite
{
    public static class InRuleReporter
    {
        private static readonly string identSpacer = "    ";

        public static void ReportAssertionResultsToContext(this InRule.Runtime.Testing.Regression.TestResult result, TestContext testContext)
        {
            if (result.Passed)
            {
                testContext.WriteLine($"PASS: {result.TestDef.DisplayName}");
            }
            else
            {
                testContext.WriteLine($"FAIL: {result.TestDef.DisplayName}");
            }

            foreach (var assertionResult in result.AssertionResults)
            {
                testContext.WriteLine($"{SpacersLimit.One.AddSpacers()}Target: {assertionResult.Target}");
                testContext.WriteLine($"{SpacersLimit.Two.AddSpacers()}Assertion: {assertionResult.DisplayText}");

                var assertionResultMessage = assertionResult.Passed ? "PASSED" : "FAILED";
                testContext.WriteLine($"{SpacersLimit.Two.AddSpacers()}{assertionResultMessage}: {assertionResult.Target} was {assertionResult.ActualValue}, expected value {assertionResult.ExpectedValue}");
            }
        }

        public static string AddSpacers(this SpacersLimit spacerCount)
        {
            return string.Concat(Enumerable.Repeat(identSpacer, Convert.ToByte(spacerCount)));
        }
    }
}