using Microsoft.IdentityModel.Protocols;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Configuration;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Tenant_service.AuthConfig;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Azure.Core;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Text;

namespace Tenant_service
{
    public static class AuthorizationHelper
    {
        public static async Task<string> GetManagementToken(ManagementAPIConfig managementAPIConfig)
        {
            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri($"https://{managementAPIConfig.Domain}"),
            };

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            List<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>>();
            data.Add(new KeyValuePair<string, string>("client_id", managementAPIConfig.ClientID));
            data.Add(new KeyValuePair<string, string>("client_secret", managementAPIConfig.ClientSecret));
            data.Add(new KeyValuePair<string, string>("audience", managementAPIConfig.Audience));
            data.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "oauth/token") { Content = new FormUrlEncodedContent(data) };
            HttpResponseMessage response = await httpClient.SendAsync(request);

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync())["access_token"] ?? throw new UnauthorizedAccessException("Cannot get management token from Auth0 API");
        }

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

        public static async Task<string?> GetSubByEmail(ManagementAPIConfig managementAPIConfig, string email) 
        {
            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri($"https://{managementAPIConfig.Domain}"),
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetManagementToken(managementAPIConfig));


            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"api/v2/users-by-email?email={email}");
            HttpResponseMessage response = await httpClient.SendAsync(request);

            Dictionary<string, object>[] users = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(await response.Content.ReadAsStringAsync());
            if (users.Length == 0) return null;
            return users.First()["user_id"].ToString();
        }        

        public static async Task<Dictionary<string,object>> GetUserInformation(ManagementAPIConfig managementAPIConfig, string userID)
        {
            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri($"https://{managementAPIConfig.Domain}"),
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetManagementToken(managementAPIConfig));


            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"api/v2/users/{userID}");
            HttpResponseMessage response = await httpClient.SendAsync(request);

            string content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(content) ?? throw new UnauthorizedAccessException("Cannot get management token from Auth0 API");
        }

        public static async Task<Dictionary<string,object>> GetUserAppMetadata(ManagementAPIConfig managementAPIConfig, string userID)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>((await GetUserInformation(managementAPIConfig, userID))["app_metadata"].ToString());
        }

        public static async Task<Dictionary<string, List<string>>> GetUserBoards(ManagementAPIConfig managementAPIConfig, string userID)
        {
            Dictionary<string, List<string>> boards = new Dictionary<string, List<string>>();
            boards.Add("boards", JsonConvert.DeserializeObject<List<string>>((await GetUserAppMetadata(managementAPIConfig, userID))["boards"].ToString()));
            boards.Add("admin_boards", JsonConvert.DeserializeObject<List<string>>((await GetUserAppMetadata(managementAPIConfig, userID))["admin_boards"].ToString()));

            return boards;
        }

        public static async Task<bool> AddUserToBoard(ManagementAPIConfig managementAPIConfig, string userID, int boardID) 
        {
            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri($"https://{managementAPIConfig.Domain}"),
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetManagementToken(managementAPIConfig));

            Dictionary<string, List<string>> boards = (await GetUserBoards(managementAPIConfig, userID));
            if (!boards["boards"].Contains(boardID.ToString()))
                boards["boards"].Add(boardID.ToString());

            Dictionary<string, object> userDetails = new Dictionary<string, object>();
            userDetails.Add("app_metadata", boards);


            string json = JsonConvert.SerializeObject(userDetails);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, $"api/v2/users/{userID}") { Content = new StringContent(json, Encoding.UTF8, "application/json") };

            HttpResponseMessage response = await httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> AddAdminToBoard(ManagementAPIConfig managementAPIConfig, string userID, int boardID) 
        {
            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri($"https://{managementAPIConfig.Domain}"),
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetManagementToken(managementAPIConfig));

            Dictionary<string, List<string>> boards = (await GetUserBoards(managementAPIConfig, userID));
            if (!boards["boards"].Contains(boardID.ToString()))
                boards["boards"].Add(boardID.ToString());

            if (!boards["admin_boards"].Contains(boardID.ToString()))
                boards["admin_boards"].Add(boardID.ToString());

            Dictionary<string, object> userDetails = new Dictionary<string, object>();
            userDetails.Add("app_metadata", boards);


            string json = JsonConvert.SerializeObject(userDetails);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, $"api/v2/users/{userID}") { Content = new StringContent(json, Encoding.UTF8, "application/json") };
            
            HttpResponseMessage response = await httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> CheckAdminPermission(ManagementAPIConfig managementAPIConfig, string userID, int boardID)
        {
            return (await GetUserBoards(managementAPIConfig, userID))["admin_boards"].Contains(boardID.ToString());
        }

        public static async Task<bool> CheckBoardAccess(ManagementAPIConfig managementAPIConfig, string userID, int boardID)
        {
            return (await GetUserBoards(managementAPIConfig, userID))["boards"].Contains(boardID.ToString());
        }

        public static async Task<bool> RemoveUserFromBoard(ManagementAPIConfig managementAPIConfig, string userID, int boardID)
        {
            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri($"https://{managementAPIConfig.Domain}"),
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetManagementToken(managementAPIConfig));

            Dictionary<string, List<string>> boards = (await GetUserBoards(managementAPIConfig, userID));
            if (boards["boards"].Contains(boardID.ToString()))
                boards["boards"].Remove(boardID.ToString());

            Dictionary<string, object> userDetails = new Dictionary<string, object>();
            userDetails.Add("app_metadata", boards);


            string json = JsonConvert.SerializeObject(userDetails);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, $"api/v2/users/{userID}") { Content = new StringContent(json, Encoding.UTF8, "application/json") };

            HttpResponseMessage response = await httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> RemoveAdminFromBoard(ManagementAPIConfig managementAPIConfig, string userID, int boardID)
        {
            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri($"https://{managementAPIConfig.Domain}"),
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetManagementToken(managementAPIConfig));

            Dictionary<string, List<string>> boards = (await GetUserBoards(managementAPIConfig, userID));
            if (boards["admin_boards"].Contains(boardID.ToString()))
                boards["admin_boards"].Remove(boardID.ToString());

            Dictionary<string, object> userDetails = new Dictionary<string, object>();
            userDetails.Add("app_metadata", boards);


            string json = JsonConvert.SerializeObject(userDetails);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, $"api/v2/users/{userID}") { Content = new StringContent(json, Encoding.UTF8, "application/json") };

            HttpResponseMessage response = await httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }
}