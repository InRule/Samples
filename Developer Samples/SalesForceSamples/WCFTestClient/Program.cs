using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
namespace WCFTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Task t = new Task(HTTP_POST);
            t.Start();
            Console.ReadLine();
        }
        static async void HTTP_POST()
        {
            var TARGETURL = "http://a65ec13f693948c396221b51bc484e17.cloudapp.net/MortgageDecision.svc/presentXML";
            //var TARGETURL = "http://localhost:50564/MortgageDecision.svc/presentXML";
            string instance = "" +
            //"<Decision xmlns=\"http://schemas.datacontract.org/2004/07/AzureDecisionServiceWebRole\">" +
            "<Request>" +
            "<MortgageApplication>" +
            "<![CDATA[<Case>" +
            "<Contact>" +
                   "<MailingState>CA</MailingState>" +
            "</Contact>" +
            "<cberg__Loan_Type__c /><cberg__Loan_Stage__c /><cberg__Loan_Channel__c>Mobile</cberg__Loan_Channel__c>" +
            "</Case>]]>" +
            "</MortgageApplication>" +
            "</Request>";
            Console.WriteLine(instance);
            StringContent sc = new StringContent(instance,Encoding.UTF8, "application/xml");
            HttpClientHandler handler = new HttpClientHandler();
            Console.WriteLine("POST: + " + TARGETURL);           
            HttpClient client = new HttpClient(handler);
            HttpResponseMessage response = await client.PostAsync(TARGETURL,sc);
            HttpContent content = response.Content;
            // ... Check Status Code                                
            Console.WriteLine("Response StatusCode: " + (int)response.StatusCode);
            // ... Read the string.
            string result = await content.ReadAsStringAsync();
            // ... Display the result.
                Console.WriteLine(result);
                
        }
    }
}
