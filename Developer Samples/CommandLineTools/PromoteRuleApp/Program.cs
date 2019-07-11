using System;
using InRule.Runtime;
using InRule.Repository.Client;
using Mono.Options;
using InRule.Repository;

namespace PromoteRuleApp
{
    class Program
    {
        static int Main(string[] args)
        {
            bool showHelp = false;

            string ruleAppName = null;
            string label = "LIVE";
            string comment = "";

            string sourceCatalogUrl = null;
            string sourceCatalogUsername = null;
            string sourceCatalogPassword = null;

            string destCatalogUrl = null;
            string destCatalogUsername = null;
            string destCatalogPassword = null;

            var clParams = new OptionSet {
                { "h|help", "Display Help.", k => showHelp = true },
                { "n|RuleAppName=", "The name of the Rule App to promote.", n => ruleAppName = n },
                { "l|Label=",  "Label assigned to the desired version of the Rule App.", l => label = l },
                { "m|Comment=",  "Comment to be associated with the promotion commit.", c => comment = c },
                //Source
                { "a|SrcCatUri=",  "Web URI for the source IrCatalog Service endpoint.", c => sourceCatalogUrl = c },
                { "b|SrcCatUser=",  "IrCatalog Username for authentication .", u => sourceCatalogUsername = u },
                { "c|SrcCatPass=",  "IrCatalog Password for authentication.", p => sourceCatalogPassword = p },
                //Dest
                { "d|DestCatUri=",  "Web URI for the target IrCatalog Service endpoint.", c => destCatalogUrl = c },
                { "e|DestCatUser=",  "IrCatalog Username for authentication.", u => destCatalogUsername = u },
                { "f|DestCatPass=",  "IrCatalog Password for authentication.", p => destCatalogPassword = p },
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
                return 1;
            }
            else if (string.IsNullOrEmpty(ruleAppName) 
                || string.IsNullOrEmpty(sourceCatalogUrl) || string.IsNullOrEmpty(sourceCatalogUsername) || string.IsNullOrEmpty(sourceCatalogPassword) 
                || string.IsNullOrEmpty(destCatalogUrl) || string.IsNullOrEmpty(destCatalogUsername) || string.IsNullOrEmpty(destCatalogPassword))
            {
                Console.WriteLine("Parameters must be specified for the Rule App name as well as the URI, username, and password for both the source and destination irCatalog instances.");
                return 1;
            }
            else
            {
                RuleApplicationDef sourceRuleAppDef = null;
                try
                {
                    CatalogRuleApplicationReference sourceRuleApp;
                    if (string.IsNullOrEmpty(label))
                    {
                        sourceRuleApp = new CatalogRuleApplicationReference(sourceCatalogUrl, ruleAppName, sourceCatalogUsername, sourceCatalogPassword);
                    }
                    else
                    {
                        sourceRuleApp = new CatalogRuleApplicationReference(sourceCatalogUrl, ruleAppName, sourceCatalogUsername, sourceCatalogPassword, label);
                    }
                    sourceRuleAppDef = sourceRuleApp.GetRuleApplicationDef();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error retrieving source Rule App: " + ex.Message);
                    return 1;
                }

                try
                {
                    if (sourceRuleAppDef != null)
                    {
                        var destCatCon = new RuleCatalogConnection(new Uri(destCatalogUrl), TimeSpan.FromSeconds(60), destCatalogUsername, destCatalogPassword);
                        var promotedDef = destCatCon.PromoteRuleApplication(sourceRuleAppDef, comment);
                        Console.WriteLine("Success!");
                        return 0;
                    }
                    else
                    {
                        Console.WriteLine("Source Rule App was unable to be retrieved.");
                        return 1;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error promoting Rule App: " + ex.Message);
                    return 1;
                }
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine();
            Console.WriteLine("Usage: PromoteRuleApp.exe [OPTIONS]");
            Console.WriteLine("Promotes a Rule Application from one catalog into another.");
            Console.WriteLine();
            Console.WriteLine("All requests must contain RuleAppName and connection information for both source and destination Catalogs.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
        }
    }
}
