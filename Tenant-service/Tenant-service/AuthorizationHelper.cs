using Microsoft.IdentityModel.Protocols;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Configuration;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Tenant_service.AuthConfig;

namespace Tenant_service
{
    public static class AuthorizationHelper
    {
        public static string GetRequestSub(HttpRequest request)
        {
            string authHeader = request.Headers.Authorization;

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.ReadJwtToken(authHeader.Split(' ')[1]);

            Claim? claim = token.Claims.FirstOrDefault(x => x.Type == "sub");

            if (claim == null)
                return "";

            return claim.Value;
        }

        public static async Task<string> GetManagementToken(ManagementAPIConfig managementAPIConfig)
        {             
            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri($"https://{managementAPIConfig.Domain}"),               
            };
            

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "oauth/token");
            request.Headers.Add("content-type", "application/json");
            request.Content = JsonContent.Create(new { client_id = managementAPIConfig.ClientID, client_secret = managementAPIConfig.ClientSecret, audiance = managementAPIConfig.Audiance, grant_type = "client_credentials" });

            HttpResponseMessage response = await httpClient.SendAsync(request);
            
            return JsonConvert.DeserializeObject<Dictionary<string,string>>(await response.Content.ReadAsStringAsync())["access_token"];
        }
    }
}