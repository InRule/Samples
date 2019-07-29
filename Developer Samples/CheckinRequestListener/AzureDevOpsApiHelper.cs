using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CheckinRequestListener
{
    public class AzureDevOpsApiHelper
    {
        public static async void QueuePipelineBuild(string organization, string project, string pipelineId, string token)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", token))));

                    string jsonBody = "{\"definition\": { \"id\": " + pipelineId + " } }";
                    HttpContent content = new StringContent(jsonBody, Encoding.ASCII, "application/json");

                    using (HttpResponseMessage response = await client.PostAsync($"https://dev.azure.com/{organization}/{project}/_apis/build/builds?api-version=5.0", content))
                    {
                        try
                        {
                            response.EnsureSuccessStatusCode();
                            string responseBody = await response.Content.ReadAsStringAsync();
                            Console.WriteLine(responseBody);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to initiate build: ", ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to make request to initiate build: ", ex.Message);
            }
        }
    }
}
