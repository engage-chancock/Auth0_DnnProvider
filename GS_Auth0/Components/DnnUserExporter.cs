using DotNetNuke.Entities.Users;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DotNetNuke.Services.Log.EventLog;

namespace GS.Auth0.Components
{
    /// <summary>
    /// Class DnnUserExporter.
    /// </summary>
    public class DnnUserExporter
    {
        /// <summary>
        /// Exports users from DNN to Auth0
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public string ExportUsers(int portalId, string authToken)
        {
            var client = new HttpClient();
            var providerConfig = Auth0ConfigBase.GetConfig(Constants.PROVIDER_NAME, portalId);
            var usersInfo = DotNetNuke.Entities.Users.UserController.GetUsers(portalId).Cast<UserInfo>().ToList();
            var successAmount = 0;
            foreach (var user in usersInfo)
            {
                successAmount += CreateAuth0User(client, providerConfig.Domain, authToken, user);
            }
            return GetStatusString(successAmount, usersInfo.Count);
        }

        /// <summary>
        /// Creates an Auth0 user
        /// </summary>
        /// <param name="client"></param>
        /// <param name="baseUrl"></param>
        /// <param name="authToken"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private int CreateAuth0User(HttpClient client, string baseUrl, string authToken, UserInfo user)
        {
            var givenName = user.FirstName ?? "GivenName";
            var familyName = user.LastName ?? "FamilyName";
            var auth0User = new
            {
                connection = "Username-Password-Authentication",
                email = user.Email,
                nickname = user.DisplayName,
                name = $"{givenName} {familyName}",
                given_name = givenName,
                family_name = familyName,
                email_verified = true,
                password = Guid.NewGuid().ToString()+"aB1!", //temp password that should fit most specs
                username = user.Username,
                user_id = $"{user.UserID}",
            };
            var userJson = JsonConvert.SerializeObject(auth0User);
            //await Task.Delay(1000);
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://{baseUrl}/api/v2/users");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            request.Content = new StringContent(userJson, Encoding.UTF8, "application/json");
            var task = Task.Run(() => client.SendAsync(request));
            task.Wait();
            var response = task.Result;
            return response.IsSuccessStatusCode ? 1 : 0;
        }

        /// <summary>
        /// Gets the status string.
        /// </summary>
        /// <param name="successCount">The success count.</param>
        /// <param name="userCount">The user count.</param>
        /// <returns>System.String.</returns>
        private string GetStatusString(int successCount, int userCount)
        {
            if (successCount == userCount)
            {
                return "Export Succeeded!";
            }
            if (successCount > 0 && successCount < userCount)
            {
                return $"{successCount}/{userCount} Exports Succeeded";
            }
            if (successCount == 0)
            {
                return "Export Failed!";
            }
            return string.Empty;
        }

    }
}