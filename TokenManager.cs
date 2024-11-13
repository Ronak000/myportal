using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace MyPortal
{
    public class TokenManager
    {
        private static string accessToken;
        private static DateTime tokenExpiry;

        public static async Task<string> GetAccessTokenAsync(string tenantId, string clientId, string clientSecret)
        {
            if (string.IsNullOrEmpty(accessToken) || tokenExpiry <= DateTime.UtcNow)
            {
                // Get a new access token
                var newTokenResponse = await GetNewAccessTokenAsync(tenantId, clientId, clientSecret);
                accessToken = newTokenResponse;
                // tokenExpiry = DateTime.UtcNow.AddSeconds(newTokenResponse.expires_in - 60); // Token expiry buffer
            }
            return accessToken;
        }

        private static async Task<string> GetNewAccessTokenAsync(string tenantId, string clientId, string clientSecret)
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
        static async Task<string> GetTokenAsync(string tenantId, string clientId, string clientSecret)
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
                    var tokenResult = System.Text.Json.JsonDocument.Parse(json);
                    return tokenResult.RootElement.GetProperty("access_token").GetString();
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
