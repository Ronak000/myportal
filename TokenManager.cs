using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MyPortal
{
    public class TokenManager
    {
        public static async Task<TokenDTO> GetNewAccessTokenAsync(string MicrosoftUrl, string TenantId, string ClientId, string ClientSecret)
        {
            var token = await GetTokenAsync(MicrosoftUrl, TenantId, ClientId, ClientSecret);

            if (token != null)
            {
                return token;
            }
            else
            {
                Console.WriteLine("Failed to get access token.");
                return token;
            }
        }
        static async Task<TokenDTO> GetTokenAsync(string MicrosoftUrl, string TenantId, string ClientId, string ClientSecret)
        {
            var tokenEndpoint = $"{MicrosoftUrl}/{TenantId}/oauth2/v2.0/token";

            using (HttpClient client = new HttpClient())
            {
                var parameters = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", ClientId),
                    new KeyValuePair<string, string>("client_secret", ClientSecret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", "https://api.businesscentral.dynamics.com/.default")
                });

                HttpResponseMessage response = await client.PostAsync(tokenEndpoint, parameters);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tokenResult = JsonConvert.DeserializeObject<TokenDTO>(json);
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
public class TokenDTO
{
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string access_token { get; set; }
}