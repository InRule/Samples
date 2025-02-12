using InRule.Repository.Client;
using InRule.Repository;
using System;
using Mono.Options;
using System.Net.Http;
using System.Linq;
using InRule.Repository.Service.Data;
using System.Net.Http.Headers;

namespace OverwriteRuleAppFromUrl
{
    // Example Command: 
    // .\OverwriteRuleAppFromUrl.exe -Label="LIVE" -Comment="This is my COMMENT" -DestCatUri="http://localhost/InRuleCatalogService_v5.8.1/Service.svc" -DestCatUser="Admin" -DestCatPass="password" -SrcRuleAppUri="https://dev.azure.com/InRuleDevOps/.../items?path=%2FDevOpsRuleApps%2FStringSplitWithDecision.ruleapp..." -AzureGitToken="xxxxxxxxxxxxxxxxxxxxxxxxxx"
    //  .\OverwriteRuleAppFromUrl.exe -Label="LIVE" -Comment="This is my COMMENT" -DestCatUri="http://localhost/InRuleCatalogService_v5.8.1/Service.svc" -DestCatUser="Admin" -DestCatPass="password" -SrcRuleAppUri="https://raw.githubusercontent.com/.../e2eTest.ruleapp" -GitHubToken="ghp_xxxxxxxxxxxxx"

    internal class Program
    {
        static int Main(string[] args)
        {
            bool showHelp = false;

            string label = null;
            string comment = null;

            string sourceRuleAppGitHubToken = null;
            string sourceRuleAppAzureGitToken = null;
            string sourceRuleAppUri = null;

            string destCatalogUrl = null;
            string destCatalogUsername = null;
            string destCatalogPassword = null;

            var clParams = new OptionSet {
                { "h|help", "Display Help.", k => showHelp = true },
                // Rule App Config
                { "l|Label=",  "Label to assign to the promoted Rule App.", l => label = l },
                { "m|Comment=",  "Comment to be associated with the commit.", c => comment = c },
                //Source
                { "a|SrcRuleAppUri=",  "Web URI where we can download the source Rule Application from.", c => sourceRuleAppUri = c },
                { "c|AzureGitToken=",  "Azure Git token used to authenticate with the Organization.", c => sourceRuleAppAzureGitToken = c },
                { "i|GitHubToken=",  "Azure Git token used to authenticate with the Organization.", c => sourceRuleAppGitHubToken = c },
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
            else if (string.IsNullOrEmpty(sourceRuleAppUri)
                || string.IsNullOrEmpty(destCatalogUrl) || string.IsNullOrEmpty(destCatalogUsername) || string.IsNullOrEmpty(destCatalogPassword))
            {
                Console.WriteLine("Parameters must be specified for the Rule App name as well as the URI, username, and password for both the source and destination irCatalog instances.");
                return 1;
            }
            else
            {
                string ruleAppXml = null;
                try
                {
                    using (var client = new HttpClient())
                    {
                        if (!string.IsNullOrEmpty(sourceRuleAppAzureGitToken))
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", sourceRuleAppAzureGitToken))));
                        }
                        else if (!string.IsNullOrEmpty(sourceRuleAppGitHubToken))
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", sourceRuleAppGitHubToken);
                        }
                        var fileResponse = client.GetAsync(sourceRuleAppUri).Result;
                        if (fileResponse.IsSuccessStatusCode)
                        {
                            ruleAppXml = fileResponse.Content.ReadAsStringAsync().Result;
                        }
                        else
                        {
                            Console.WriteLine($"Received error response code {fileResponse.StatusCode} while attempting to download RuleApp XML: " + fileResponse.ReasonPhrase);
                            return 1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving source Rule Application XML: " + ex.Message);
                    return 1;
                }

                RuleApplicationDef sourceRuleAppDef;
                try
                {
                    sourceRuleAppDef = (RuleApplicationDef)RuleApplicationDef.LoadFromXml(ruleAppXml, typeof(RuleApplicationDef));
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Received invalid response content when requesting RuleApp XML: {ex.Message}.  {Environment.NewLine}{ruleAppXml}");
                    return 1;
                }

                var destCatCon = new RuleCatalogConnection(new Uri(destCatalogUrl), TimeSpan.FromSeconds(60), destCatalogUsername, destCatalogPassword, RuleCatalogAuthenticationType.BuiltIn);
                RuleAppInfo ruleAppToOverwrite;
                try
                {
                    var ruleApps = destCatCon.GetRuleAppSummary(false);
                    ruleAppToOverwrite = ruleApps.FirstOrDefault(ra => ra.Name == sourceRuleAppDef.Name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error connecting to the target Catalog: " + ex.Message);
                    return 1;
                }

                RuleApplicationDef promotedDef;
                if (ruleAppToOverwrite != null)
                {
                    // Overwriting existing Rule Application
                    try
                    {
                        promotedDef = destCatCon.OverwriteRuleApplication(ruleAppToOverwrite.AppGuid, sourceRuleAppDef, true, comment);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error overwriting Rule Application in target catalog: " + ex.Message);
                        return 1;
                    }
                }
                else
                {
                    // Checking into the target Catalog for the first time
                    try
                    {
                        promotedDef = destCatCon.CreateRuleApplication(sourceRuleAppDef, comment);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error overwriting Rule Application in target catalog: " + ex.Message);
                        return 1;
                    }
                }

                if(!string.IsNullOrEmpty(label))
                {
                    try
                    {
                        var existingLabels = destCatCon.GetAllLabels();
                        if(!existingLabels.Keys.Any(l => l.Label == label))
                        {
                            destCatCon.CreateLabel(label, "");
                        }
                        destCatCon.ApplyLabel(promotedDef, label);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error applying Label to Rule Application in target catalog: " + ex.Message);
                        return 1;
                    }
                }

                Console.WriteLine("Success!");
                return 0;
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine();
            Console.WriteLine("Usage: OverwriteRuleAppFromUrl.exe [OPTIONS]");
            Console.WriteLine("Overwrites a Rule Application in a destination Catalog with the Rule Application XML retrieved from Git.");
            Console.WriteLine();
            Console.WriteLine("All requests must contain SrcRuleAppUri, and connection information for destination Catalog.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
        }
    }
}
