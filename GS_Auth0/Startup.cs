using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Social.Notifications;
using GS.Auth0.Components;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Provider;
using Owin;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

[assembly: Microsoft.Owin.OwinStartupAttribute(Constants.PROVIDER_NAME, typeof(GS.Auth0.Startup))]

namespace GS.Auth0
{
    public class Startup
    {
        private static readonly ILog logger = LoggerSource.Instance.GetLogger(typeof(Startup));

        public void Configuration(IAppBuilder app)
        {
            try
            {
                #region "SSL settings"
                // Remove insecure protocols (SSL3, TLS 1.0, TLS 1.1)
                ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
                ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls;
                ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls11;
                // Add TLS 1.2
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                #endregion

                var config = Auth0ConfigBase.GetConfig(Constants.PROVIDER_NAME, Helpers.FirstPortalID);

                System.Web.Helpers.AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

                // Configure Auth0 parameters
                foreach (var property in app.Properties)
                {
                    logger.Error($"startup, app property key is {property.Key}");
                }
                
                string auth0Domain = config.Domain;
                string auth0ClientId = config.ClientID;

                // Enable the Cookie saver middleware to work around a bug in the OWIN implementation
                app.UseKentorOwinCookieSaver();
                //var CookieManager = new SystemWebCookieManager();

                // Set Cookies as default authentication type
                app.SetDefaultSignInAsAuthenticationType(Constants.AUTH_TYPE);
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = Constants.AUTH_TYPE,
                    CookieName = Constants.AUTH_COOKIE_NAME,
                    CookieSameSite = Microsoft.Owin.SameSiteMode.None,
                    CookieSecure = CookieSecureOption.Always
                    //CookieManager = CookieManager,
                });


                // Configure Auth0 authentication
                app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                {
                    AuthenticationType = Constants.AUTH_TYPE,
                    Authority = $"https://{auth0Domain}",
                    ClientId = auth0ClientId,
                    Scope = "openid profile email",
                    ResponseType = OpenIdConnectResponseType.CodeIdToken,
                    //CookieManager = CookieManager,

                    TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = ClaimTypes.NameIdentifier,
                    },

                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        RedirectToIdentityProvider = notification =>
                        {
                            var providerConfig = GetProviderConfig(notification);

                            if (providerConfig != null && notification.ProtocolMessage.RequestType != OpenIdConnectRequestType.Logout)
                            {
                                UpdateForCurrentPortal(notification, notification.ProtocolMessage, providerConfig, true);
                                logger.Error($"Current metadata address is {notification.Options.MetadataAddress}");
                                notification.HandleResponse();
                            }

                            else if (providerConfig != null && notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.Logout)
                            {
                                var logoutUri = $"https://{providerConfig.Domain}/v2/logout?client_id={providerConfig.ClientID}";
                                var postLogoutUri = providerConfig.PostLogoutRedirectUri;
                                if (!string.IsNullOrEmpty(postLogoutUri))
                                {
                                    if (postLogoutUri.StartsWith("/"))
                                    {
                                        // transform to absolute
                                        var request = notification.Request;
                                        postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                                    }
                                    logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
                                }
                                notification.Response.Redirect(logoutUri);
                                notification.HandleResponse();
                            }

                            if (providerConfig != null && providerConfig.IsDiagnosticModeEnabled)
                            {
                                logger.Debug($"Redirecting to '{notification.Options.Authority}' using following coordinates:");
                                logger.Debug("Client id: " + notification.Options.ClientId);
                                logger.Debug("Redirect uri: " + notification.Options.RedirectUri);
                                logger.Debug("Callback path: " + notification.Options.CallbackPath);
                            }

                            return Task.FromResult(0);
                        },

                        AuthorizationCodeReceived = async notification =>
                        {
                            logger.Error("received authorization code");
                            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();

                            if (portalSettings == null
                                && notification.OwinContext.Environment["System.Web.HttpContextBase"] is HttpContextWrapper)
                            {
                                var context = notification.OwinContext.Environment["System.Web.HttpContextBase"] as HttpContextWrapper;
                                if (context?.Items["PortalSettings"] is PortalSettings)
                                {
                                    portalSettings = context.Items["PortalSettings"] as PortalSettings;
                                }
                            }

                            Auth0ConfigBase providerConfig = null;
                            if (portalSettings != null)
                                providerConfig = Auth0ConfigBase.GetConfig(Constants.PROVIDER_NAME, portalSettings.PortalId);
                            else
                                throw new InvalidOperationException("Can't obtain DNN settings, login process terminated!");

                            if (providerConfig == null)
                            {
                                throw new InvalidOperationException("Can't obtain DNN settings, login process terminated!");
                            }

                            if (providerConfig.IsDiagnosticModeEnabled)
                            {
                                logger.Debug("Received authorization code from Auth0.");
                            }

                            var userController = new UserController();

                            //get username assuming username was sent. If not, default to user id from auth0
                            var username = notification.AuthenticationTicket.Identity?
                                .FindFirst(c => c.Type == "preferred_username")?.Value ?? notification.AuthenticationTicket?.Identity?.Name;
                            
                            //get or create DNN user
                            var userInfo = userController.User_Create(username, portalSettings, providerConfig.IsDiagnosticModeEnabled);

                            if (userInfo != null)
                            {
                                //update DNN user profile
                                userController.User_Update(
                                userInfo,
                                notification.AuthenticationTicket?.Identity?.FindFirst(c => c.Type == ClaimTypes.GivenName)?.Value,
                                notification.AuthenticationTicket?.Identity?.FindFirst(c => c.Type == ClaimTypes.Surname)?.Value,
                                notification.AuthenticationTicket?.Identity?.FindFirst(c => c.Type == "name")?.Value,
                                notification.AuthenticationTicket?.Identity?.FindFirst(c => c.Type == ClaimTypes.Email)?.Value,
                                portalSettings.PortalId,
                                providerConfig.IsDiagnosticModeEnabled);

                                var loginStatus = DotNetNuke.Security.Membership.UserLoginStatus.LOGIN_FAILURE;
                                DotNetNuke.Entities.Users.UserController.ValidateUser(portalSettings.PortalId, notification.AuthenticationTicket?.Identity?.Name, "",
                                                                Constants.PROVIDER_NAME, "",
                                                                portalSettings.PortalName, "",
                                                                ref loginStatus);

                                //set type of current authentication provider
                                DotNetNuke.Services.Authentication.AuthenticationController.SetAuthenticationType(Constants.AUTH_TYPE);
                                DotNetNuke.Entities.Users.UserController.UserLogin(portalSettings.PortalId, userInfo, portalSettings.PortalName, notification.OwinContext.Request.RemoteIpAddress, false);
                            }
                            else
                                throw new ArgumentNullException($"Can't create or get user '{notification.AuthenticationTicket?.Identity?.Name}' from DNN.");

                            await Task.FromResult(0);
                        },

                        AuthenticationFailed = async notification =>
                        {
                            //get the error message and send it to the DNN login page  
                            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                            HttpContextWrapper context = null;

                            if (portalSettings == null
                                && notification.OwinContext.Environment["System.Web.HttpContextBase"] is HttpContextWrapper)
                            {
                                context = notification.OwinContext.Environment["System.Web.HttpContextBase"] as HttpContextWrapper;
                                if (context?.Items["PortalSettings"] is PortalSettings)
                                {
                                    portalSettings = context.Items["PortalSettings"] as PortalSettings;
                                }
                            }
                            
                            Auth0ConfigBase providerConfig = null;
                            if (portalSettings != null)
                                providerConfig = Auth0ConfigBase.GetConfig(Constants.PROVIDER_NAME, portalSettings.PortalId);
                            else
                                logger.Error("Can't obtain DNN settings from 'AuthenticationFailed' event, login process terminated!!");

                            if (providerConfig == null)
                            {
                                throw new InvalidOperationException("Can't obtain DNN settings, login process terminated!");
                            }

                            if (providerConfig.IsDiagnosticModeEnabled)
                                logger.Error($"OIDC authentication failed, details: {notification.Exception}");

                            if (providerConfig.AutoRetryOnFailure && notification.Exception.Message.Contains("IDX21323"))
                            {
                                context = notification.OwinContext.Environment["System.Web.HttpContextBase"] as HttpContextWrapper;
                                var returnUri = context?.Request?.QueryString[Constants.OIDC_RETURN_URL] != null
                                    ? context.Request.QueryString[Constants.OIDC_RETURN_URL]
                                    : "/";
                                notification.HandleResponse();
                                notification.OwinContext.Authentication.Challenge(new AuthenticationProperties { RedirectUri = returnUri }, Constants.AUTH_TYPE);
                            }
                            else
                            {
                                var redirectUrl = DotNetNuke.Common.Globals.NavigateURL(portalSettings.LoginTabId, "Login", new string[] { Constants.ALERT_QUERY_STRING + "=" + notification.Exception.Message });
                                notification.Response.Redirect(redirectUrl);
                                notification.HandleResponse();
                            }
                            await Task.FromResult(0);
                        },

                        SecurityTokenReceived = notification =>
                        {
                            var providerConfig = GetProviderConfig(notification);

                            if (providerConfig != null && notification.ProtocolMessage.RequestType != OpenIdConnectRequestType.Logout)
                            {
                                UpdateForCurrentPortal(notification, notification.ProtocolMessage, providerConfig, false);
                            }

                            logger.Error("Updated notification with updated config");
                            return Task.FromResult(0);
                        },

                        #region "Rest of 'Notification' methods, not in use for now."
                        //SecurityTokenValidated = notification =>
                        //{
                        //    return Task.FromResult(0);
                        //},
                        //MessageReceived = (context) =>
                        //{

                        //    return Task.FromResult(0);
                        //},
                        #endregion
                    },


                });
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private static Auth0ConfigBase GetProviderConfig(BaseContext<OpenIdConnectAuthenticationOptions> notification)
        { 
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings == null
                && notification.OwinContext.Environment["System.Web.HttpContextBase"] is HttpContextWrapper)
            {
                var context = notification.OwinContext.Environment["System.Web.HttpContextBase"] as HttpContextWrapper;
                if (context?.Items["PortalSettings"] is PortalSettings)
                {
                    portalSettings = context.Items["PortalSettings"] as PortalSettings;
                }
            }

            if (portalSettings != null)
                return Auth0ConfigBase.GetConfig(Constants.PROVIDER_NAME, portalSettings.PortalId);
            
            logger.Debug("Can't obtain DNN settings, login process terminated!!");
            throw new InvalidOperationException();
        }

        private static void UpdateForCurrentPortal(BaseContext<OpenIdConnectAuthenticationOptions> notification, OpenIdConnectMessage protocolMessage, Auth0ConfigBase providerConfig, bool includeRedirect)
        {
            notification.Options.Authority = $"https://{providerConfig.Domain}";
            notification.Options.ClientId = providerConfig.ClientID;
            notification.Options.ClientSecret = providerConfig.ClientSecret;
            notification.Options.RedirectUri = providerConfig.RedirectUri;
            notification.Options.MetadataAddress = $"https://{providerConfig.Domain}/.well-known/openid-configuration";

            notification.Options.TokenValidationParameters = new TokenValidationParameters()
            {
                NameClaimType = ClaimTypes.NameIdentifier,
                ValidAudience = "https://authtest.local",
                ValidIssuer = $"https://{providerConfig.Domain}",
            };


            protocolMessage.ClientId = providerConfig.ClientID;
            protocolMessage.ClientSecret = providerConfig.ClientSecret;
            protocolMessage.RedirectUri = providerConfig.RedirectUri;
            protocolMessage.IssuerAddress = $"https://{providerConfig.Domain}/authorize";

            if (includeRedirect)
            {
                var redirectUri = protocolMessage.BuildRedirectUrl();
                logger.Error($"redirect url built is {redirectUri}");
                notification.Response.Redirect($"{redirectUri}");
            }
        }
    }
}