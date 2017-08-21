using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using InRule.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.IO;
using System.Xml;
namespace AzureDecisionServiceWebRole
{
    public class MortgageDecision : AzureDecisionServiceWebRole.IMortgageDecision
    {
        public Response process(Request req)
        {
            Response resp = new Response();
            string text;
            try
            {
                
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("test");
                CloudBlockBlob blockBlob = container.GetBlockBlobReference("MortgageProcess.ruleapp");
                using (var memoryStream = new MemoryStream())
                {
                    blockBlob.DownloadToStream(memoryStream);
                    text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray()).Substring(1); //substring removes a bad character from blob storage...probably due to my tool
                }
            }
            catch (Exception e)
            {
                resp.MortgageApplication = "<ERROR>" + "There was a problem retrieving your ruleapp from blob storage: " + e.Message + e.StackTrace + "</ERROR>";
                return resp;
            }
            try
            {
                //You can just as easily use the catalog hosted on Azure with the following line of code.
                //using (var session = new RuleSession(new CatalogRuleApplicationReference("https://inrule-sfdemocat.cloudapp.net/Service.svc", "ruleAppName", "[user name]", "[user password]")))
                using (var session = new RuleSession(new InMemoryRuleApplicationReference(InRule.Repository.RuleApplicationDef.LoadXml(text))))
                {
                    //prepare rule set parameters 
                    var instance = session.CreateEntity("Case", req.MortgageApplication);
                    session.ApplyRules();
                    //SF does not support CDATA for safe payloads which could easily handle a complete XML document
                    //We remove the XML element and replace with a safe element name for parssing within the Apex class environment
                    string safe = instance.GetXml();
                    safe = safe.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", ""); //remove xml 
                    safe = safe.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");  //Remove reference
                    resp.MortgageApplication = safe;
                    return resp;
                }
            }
            catch (Exception e)
            {
                resp.MortgageApplication = "<ERROR>" + "There was a problem executing your ruleset: " + e.Message + e.StackTrace + "</ERROR>";
                return resp;
            }
        }
    }
}
