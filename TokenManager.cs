using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MyPortal
{
    public class TokenManager
    {
        public static async Task<Token> GetNewAccessTokenAsync(string tenantId, string clientId, string clientSecret)
        {
            var token = await GetTokenAsync(tenantId, clientId, clientSecret);

            if (token != null)
            {
                return token;
            }
            else
            {
                Console.WriteLine("Failed to get access token.");
                return null;
            }
        }
        static async Task<Token> GetTokenAsync(string tenantId, string clientId, string clientSecret)
        {
            var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            using (HttpClient client = new HttpClient())
            {
                var parameters = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "https://api.businesscentral.dynamics.com/.default")
            });

                HttpResponseMessage response = await client.PostAsync(tokenEndpoint, parameters);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tokenResult = JsonConvert.DeserializeObject<Token>(json);
                    return tokenResult;
                }
                else
                {
                    Console.WriteLine("Error getting token: " + response.StatusCode);
                    return null;
                }
            }
        }
    }
}
public class Token
{
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string  access_token{ get; set; }
}