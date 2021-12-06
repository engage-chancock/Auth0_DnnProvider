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
using DotNetNuke.Instrumentation;

namespace GS.Auth0.Components
{
    /// <summary>
    /// Enum UserExportStatus
    /// </summary>
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
        private static readonly ILog logger = LoggerSource.Instance.GetLogger(typeof(DnnUserExporter));

        /// <summary>
        /// Exports users from DNN to Auth0
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="authToken"></param>
        /// <param name="domain"></param>
        /// <param name="isDiagnosticModeEnabled"></param>
        /// <param name="successCount"></param>
        /// <param name="userCount"></param>
        /// <returns></returns>
        public UserExportStatus ExportUsers(int portalId, string authToken, string domain, bool isDiagnosticModeEnabled, out int successCount, out int userCount)
        {
            var client = new HttpClient();
            var usersInfo = DotNetNuke.Entities.Users.UserController.GetUsers(portalId).Cast<UserInfo>().ToList();
            //super users aren't included in original collection, so need to get those separately and add in
            var superUserInfo = DotNetNuke.Entities.Users.UserController.GetUsers(false, true, DotNetNuke.Common.Utilities.Null.NullInteger).Cast<UserInfo>().ToList();
            usersInfo.AddRange(superUserInfo);
            successCount = 0;
            foreach (var user in usersInfo)
            {
                successCount += CreateAuth0User(client, domain, authToken, user, isDiagnosticModeEnabled);
            }
            userCount = usersInfo.Count;
            if (isDiagnosticModeEnabled)
                logger.Debug(string.Format("{0}/{1} Auth0 users exported successfully", successCount, userCount));
            return GetStatus(successCount, userCount);
        }

        /// <summary>
        /// Creates an Auth0 user
        /// </summary>
        /// <param name="client"></param>
        /// <param name="baseUrl"></param>
        /// <param name="authToken"></param>
        /// <param name="user"></param>
        /// <param name="isDiagnosticModeEnabled"></param>
        /// <returns></returns>
        private int CreateAuth0User(HttpClient client, string baseUrl, string authToken, UserInfo user, bool isDiagnosticModeEnabled)
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
                username = user.Username.Trim(), //trim leading and trailing white spaces because Auth0 does not allow spaces in usernames
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
            if (isDiagnosticModeEnabled)
                logger.Debug(string.Format("User '{0}' creation in Auth0 success?: {1}", user.Username, response.IsSuccessStatusCode));
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