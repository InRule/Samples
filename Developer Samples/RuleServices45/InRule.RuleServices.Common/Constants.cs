namespace InRule.RuleServices.Common
{
    public class Constants
    {
        // uri builders
        public const string AppyRulesUriTemplate = "{0}/ApplyRules?ruleApp={1}&entity={2}&entityXml={3}&returnEntity={4}&responseType={5}";
        public const string ExecuteRuleSetUriTemplate = "{0}/ExecuteRuleSet?ruleApp={1}&ruleSet={2}&entity={3}&entityXml={4}&returnEntity={5}&responseType={6}";
        public const string QueryStringItemTemplate = "&{0}={1}";
        public const string ExecuteRuleRequestTemplate = "{0}/ExecuteRuleRequest";

        // XML serialization
        public const string XmlNamespace = "http://inrule.com/RuleServices";

        // config file app setting
        public const string InRuleRuleServiceUri = "RuleServiceUri";
    }
}
