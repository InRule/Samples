using InRule.Runtime;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;
using System.IO;
using InRule.Repository.Service;
using Newtonsoft.Json;

namespace BuildIrJsRuleApp
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            //Required Parameters
            bool showHelp = false;
            string irDistributionKey = null;
            string OutputFilePath = null;
            //Option 1
            string RuleAppFilePath = null;
            //Option 2
            string CatalogUri = null;
            string CatalogUsername = null;
            string CatalogPassword = null;
            string CatalogRuleAppName = null;
            string CatalogRuleAppLabel = "LIVE";

            var clParams = new OptionSet {
                { "h|help", "Display Help.", k => showHelp = true },
                { "k|DistributionKey=", "The irDistributionKey for your account.", k => irDistributionKey = k },
                { "o|OutputPath=",  "Desired Path for the compiled output library.", p => OutputFilePath = p },
                //Option 1
                { "r|RuleAppPath=",  "Path to the Rule Application to be compiled.", p => RuleAppFilePath = p },
                //Option 2
                { "c|CatUri=",  "Web URI for the IrCatalog Service endpoint.", p => CatalogUri = p },
                { "u|CatUsername=",  "IrCatalog Username for authentication.", p => CatalogUsername = p },
                { "p|CatPassword=",  "IrCatalog Password for authentication.", p => CatalogPassword = p },
                { "n|CatRuleAppName=",  "Name of the Rule Application.", p => CatalogRuleAppName = p },
                { "l|CatLabel=",  "Label of the Rule Application to retrieve (LIVE).", p => CatalogRuleAppLabel = p },
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

            RuleApplicationReference ruleApp = null;
            if (showHelp)
            {
                ShowHelp(clParams);
                return 1;
            }
            else if (string.IsNullOrEmpty(irDistributionKey))
            {
                Console.WriteLine("Error: Missing required parameter DistributionKey.");
                return 1;
            }
            else if (string.IsNullOrEmpty(OutputFilePath))
            {
                Console.WriteLine("Error: Missing required parameter OutputPath.");
                return 1;
            }

            if (!string.IsNullOrEmpty(RuleAppFilePath))
            {
                try
                {
                    ruleApp = new FileSystemRuleApplicationReference(RuleAppFilePath);
                }
                catch (IntegrationException ie)
                {
                    Console.WriteLine("Error creating reference to file-based Rule App: " + ie.Message); //Rule App file not found
                }
            }

            if (!string.IsNullOrEmpty(CatalogUri)
                && !string.IsNullOrEmpty(CatalogUsername)
                && !string.IsNullOrEmpty(CatalogPassword)
                && !string.IsNullOrEmpty(CatalogRuleAppName))
            {
                if (ruleApp != null)
                {
                    Console.WriteLine("Error: Parameters were provided for both File- and Catalog-based Rule App; only one may be specified.");
                    return 1;
                }
                else
                {
                    ruleApp = new CatalogRuleApplicationReference(CatalogUri, CatalogRuleAppName, CatalogUsername, CatalogPassword, CatalogRuleAppLabel);
                }
            }

            if (ruleApp == null)
            {
                Console.WriteLine("You must provide either RuleAppPath or all of CatUri, CatUsername, CatPassword, and CatRuleAppName (with optional CatLabel)");
                return 1;
            }
            else
            {
                var success = await RetrieveAndWriteIrJSFromDistributionService(ruleApp, irDistributionKey, OutputFilePath);
                if (success)
                    return 0;
                else
                    return 1;
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine();
            Console.WriteLine("Usage: BuildIrJsRuleApp.exe [OPTIONS]");
            Console.WriteLine("Compiles a Rule Application into an irJS executable library using the irDistribution service.");
            Console.WriteLine();
            Console.WriteLine("All requests must contain DistributionKey and OutputPath.");
            Console.WriteLine("   File-based Rule Apps should be specified with RuleAppPath.");
            Console.WriteLine("   Catalog-based Rule Apps should be specified with CatalogUri, CatalogUsername, CatalogPassword, and CatalogRuleAppName (optionally CatalogRuleAppLabel).");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
        }
        private static async Task<bool> RetrieveAndWriteIrJSFromDistributionService(RuleApplicationReference ruleRef, string distroKey, string outputPath)
        {
            Console.WriteLine();

            Console.WriteLine("Requesting compiled JS library from Distribution Service...");
            var js = await CallDistributionServiceAsync(ruleRef, "https://api.distribution.inrule.com/", distroKey);

            if (!string.IsNullOrEmpty(js))
            {
                Console.WriteLine("Received compiled library, writing out to " + outputPath);
                try
                {
                    File.WriteAllText(outputPath, js);
                    Console.WriteLine("Compiled and wrote out Javascript Rule App");
                    Console.WriteLine();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error writing out compiled JavaScript file: " + ex.Message);
                }
            }
            return false;
        }
        public static async Task<string> CallDistributionServiceAsync(RuleApplicationReference ruleApplicationRef, string serviceUri, string subscriptionKey)
        {
            using (var client = new HttpClient())
            using (var requestContent = new MultipartFormDataContent())
            {
                HttpResponseMessage result = null;
                try
                {
                    client.BaseAddress = new Uri(serviceUri);

                    // Build up our request by reading in the rule application
                    var ruleApplication = ruleApplicationRef.GetRuleApplicationDef();
                    var httpContent = new ByteArrayContent(Encoding.UTF8.GetBytes(ruleApplication.GetXml()));
                    requestContent.Add(httpContent, "ruleApplication", ruleApplication.Name + ".ruleapp");

                    // Tell the server we are sending form data
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
                    client.DefaultRequestHeaders.Add("subscription-key", subscriptionKey);

                    // Post the rule application to the irDistribution service API,
                    // enabling Execution and State Change logging, Display Name metadata, and the developer example.
                    var distributionUrl = "package?logOption=Execution&logOption=StateChanges&subscriptionkey=" + subscriptionKey;
                    result = await client.PostAsync(distributionUrl, requestContent).ConfigureAwait(false);

                    // Get the return package from the result

                    dynamic returnPackage = JObject.Parse(await result.Content.ReadAsStringAsync());
                    var errors = new StringBuilder();
                    if (returnPackage.Status.ToString() == "Fail")
                    {
                        foreach (var error in returnPackage.Errors)
                        {
                            // Handle errors
                            errors.AppendLine("* " + error.Description.ToString());
                        }
                        foreach (var unsupportedError in returnPackage.UnsupportedFeatures)
                        {
                            // Handle errors
                            errors.AppendLine("* " + unsupportedError.Feature.ToString());
                        }
                        // Still need to stop processing
                        return errors.ToString();
                    }

                    // Build the download url of the file
                    var downloadUrl = returnPackage.PackagedApplicationDownloadUrl.ToString();

                    // Get the contents
                    HttpResponseMessage resultDownload = await client.GetAsync(downloadUrl);
                    if (!resultDownload.IsSuccessStatusCode)
                    {
                        // Handle errors
                        errors.AppendLine(await resultDownload.Content.ReadAsStringAsync());
                        return errors.ToString();
                    }
                    return await resultDownload.Content.ReadAsStringAsync();
                }
                catch (InRuleCatalogException icex)
                {
                    Console.WriteLine("Error retrieving Rule Application to compile: " + icex.Message);
                    return null;
                }
                catch (JsonReaderException)
                {
                    if (result != null)
                        Console.WriteLine("Error requesting compiled Rule Application: " + await result.Content.ReadAsStringAsync());
                    else
                        Console.WriteLine("Error requesting compiled Rule Application.");

                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error requesting compiled Rule Application.");
                    Console.WriteLine(ex.ToString());
                    return null;
                }
            }
        }
    }
}
