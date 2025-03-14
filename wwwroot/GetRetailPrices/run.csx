#r "Newtonsoft.Json"

using System.Net;
using Newtonsoft.Json;
using System.Net.Http;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, ILogger log)
{
    string endpoint = Environment.GetEnvironmentVariable("AZURE_RETAIL_API_PRICES_ENDPOINT");
    using (HttpClient client = new HttpClient())
    {
        HttpResponseMessage response = await client.GetAsync(endpoint);
        string content = await response.Content.ReadAsStringAsync();
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
        };
    }
}