using DotNetNuke.Entities.Users;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DotNetNuke.Services.Log.EventLog;
using System.Threading;
using DotNetNuke.Services.Localization;

namespace GS.Auth0.Components
{
    public enum UserExportStatus
    { 
        Success = 0,
        Mixed = 1,
        Failure = 2,
    }

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
        /// <param name="successCount"></param>
        /// <param name="userCount"></param>
        /// <returns></returns>
        public UserExportStatus ExportUsers(int portalId, string authToken, out int successCount, out int userCount)
        {
            var client = new HttpClient();
            var providerConfig = Auth0ConfigBase.GetConfig(Constants.PROVIDER_NAME, portalId);
            var usersInfo = DotNetNuke.Entities.Users.UserController.GetUsers(portalId).Cast<UserInfo>().ToList();
            successCount = 0;
            foreach (var user in usersInfo)
            {
                successCount += CreateAuth0User(client, providerConfig.Domain, authToken, user);
            }
            userCount = usersInfo.Count;
            return GetStatus(successCount, userCount);
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
            var fullName = user.FirstName == null && user.LastName == null ? $"{givenName} {familyName}" : $"{user.FirstName} {user.LastName}";
            var auth0User = new
            {
                connection = "Username-Password-Authentication",
                email = user.Email,
                nickname = user.DisplayName,
                name = fullName,
                given_name = givenName,
                family_name = familyName,
                email_verified = true,
                password = DotNetNuke.Entities.Users.UserController.GeneratePassword(13) + "aB1!", //temp password that should fit most specs
                username = user.Username,
                user_id = $"{user.UserID}",
            };
            var userJson = JsonConvert.SerializeObject(auth0User);
            //Management API requests are rate limited to 2 per second at most, so wait between each request
            Thread.Sleep(550);
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://{baseUrl}/api/v2/users");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            request.Content = new StringContent(userJson, Encoding.UTF8, "application/json");
            var task = Task.Run(() => client.SendAsync(request));
            task.Wait();
            var response = task.Result;
            return response.IsSuccessStatusCode ? 1 : 0;
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <param name="successCount">The success count.</param>
        /// <param name="userCount">The user count.</param>
        /// <returns>System.String.</returns>
        private UserExportStatus GetStatus(int successCount, int userCount)
        {
            if (successCount == userCount)
            {
                return UserExportStatus.Success;
            }
            if (successCount > 0 && successCount < userCount)
            {
                return UserExportStatus.Mixed;
            }
            return UserExportStatus.Failure;
        }

    }
}