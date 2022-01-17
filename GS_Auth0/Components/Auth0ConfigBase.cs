using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Authentication;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace GS.Auth0.Components
{
    /// <summary>
    /// The Config class provides a central area for management of Module Configuration Settings.
    /// </summary>
    [Serializable]
    public class Auth0ConfigBase : AuthenticationConfigBase
    {
        #region "Constructors"        
        /// <summary>
        /// Initializes a new instance of the <see cref="Auth0ConfigBase"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="portalId">The portal identifier.</param>
        protected Auth0ConfigBase(string service, int portalId)
            : base(portalId)
        {
            Service = service;

            Domain = PortalController.GetPortalSetting(this.Service + "_Domain", portalId, "");
            ClientID = PortalController.GetPortalSetting(Service + "_ClientID", portalId, "");
            ClientSecret = PortalController.GetPortalSetting(Service + "_ClientSecret", portalId, "");
            RedirectUri = PortalController.GetPortalSetting(Service + "_RedirectUri", portalId, "");
            PostLogoutRedirectUri = PortalController.GetPortalSetting(Service + "_PostLogoutRedirectUri", portalId, "");
            IsEnabled = PortalController.GetPortalSettingAsBoolean(Service + "_Enabled", portalId, false);
            IsDiagnosticModeEnabled = PortalController.GetPortalSettingAsBoolean(Service + "_IsDiagnosticModeEnabled", portalId, true);
            SkipLoginPage = PortalController.GetPortalSettingAsBoolean(Service + "_SkipLoginPage", portalId, false);
            AutoRetryOnFailure = PortalController.GetPortalSettingAsBoolean(Service + "_AutoRetryOnFailure", portalId, false);
        }
        #endregion

        #region "Private properties"        
        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>The service.</value>
        private string Service { get; set; }
        #endregion

        #region "Public properties"
        /// <summary>
        /// Flag that determines whether provider is enabled or disabled.
        /// </summary>
        [DotNetNuke.UI.WebControls.SortOrder(0)]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the domain.
        /// </summary>
        /// <value>The domain.</value>
        [DotNetNuke.UI.WebControls.SortOrder(1)]
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>The client identifier.</value>
        [DotNetNuke.UI.WebControls.SortOrder(2)]
        public string ClientID { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        /// <value>The client secret.</value>
        [DotNetNuke.UI.WebControls.SortOrder(3)]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI.
        /// </summary>
        /// <value>The redirect URI.</value>
        [DotNetNuke.UI.WebControls.IsReadOnly(false)]
        [DotNetNuke.UI.WebControls.SortOrder(4)]
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the post logout redirect URI.
        /// </summary>
        /// <value>The post logout redirect URI.</value>
        [DotNetNuke.UI.WebControls.IsReadOnly(false)]
        [DotNetNuke.UI.WebControls.SortOrder(5)]
        public string PostLogoutRedirectUri { get; set; }

        /// <summary>
        /// Flag that determines if diagnostic info should be saved in DNN log file
        /// </summary>
        [DotNetNuke.UI.WebControls.SortOrder(6)]
        public bool IsDiagnosticModeEnabled { get; set; }

        /// <summary>
        /// Skips login page and instead logs in immediately
        /// </summary>
        [DotNetNuke.UI.WebControls.SortOrder(7)]
        public bool SkipLoginPage { get; set; }

        /// <summary>
        /// Whether or not to automatically retry to login if there is a owin cookie related failure
        /// </summary>
        [DotNetNuke.UI.WebControls.SortOrder(8)]
        public bool AutoRetryOnFailure { get; set; }

        /// <summary>
        /// Check if provider has defined all coordinates that are necessary to initialize login flow.
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public bool AreProviderSettingsCorrect
        {
            get
            {
                return !(
                       string.IsNullOrEmpty(Domain)
                    || string.IsNullOrEmpty(ClientID)
                    || string.IsNullOrEmpty(ClientSecret)
                    || string.IsNullOrEmpty(RedirectUri)
                    || string.IsNullOrEmpty(PostLogoutRedirectUri)
                    );
            }
        }

        #endregion

        #region "Public methods"        
        /// <summary>
        /// Clears the configuration.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="portalId">The portal identifier.</param>
        public static void ClearConfig(string service, int portalId)
        {
            DataCache.RemoveCache(GetCacheKey(service, portalId));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service">Unique string required to create cache key</param>
        /// <param name="portalId"></param>
        /// <returns></returns>
        public static Auth0ConfigBase GetConfig(string service, int portalId)
        {
            string key = GetCacheKey(service, portalId);
            object _cachedConfig = DataCache.GetCache(key);
            Auth0ConfigBase config = (Auth0ConfigBase)_cachedConfig;
            if (config == null)
            {
                config = new Auth0ConfigBase(service, portalId);
                DataCache.SetCache(key, config);
            }
            return config;
        }

        /// <summary>
        /// Updates the configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public static void UpdateConfig(Auth0ConfigBase config)
        {
            PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_Domain", config.Domain);
            PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_ClientID", config.ClientID);
            PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_ClientSecret", config.ClientSecret);
            PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_RedirectUri", config.RedirectUri);
            PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_PostLogoutRedirectUri", config.PostLogoutRedirectUri);
            PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_Enabled", config.IsEnabled.ToString(CultureInfo.InvariantCulture));
            PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_IsDiagnosticModeEnabled", config.IsDiagnosticModeEnabled.ToString(CultureInfo.InvariantCulture));
            PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_SkipLoginPage", config.SkipLoginPage.ToString(CultureInfo.InvariantCulture));
            PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_AutoRetryOnFailure", config.AutoRetryOnFailure.ToString(CultureInfo.InvariantCulture));


            ClearConfig(config.Service, config.PortalID);
        }
        #endregion

        #region "Private Methods"        
        /// <summary>
        /// Gets the cache key.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>System.String.</returns>
        private static string GetCacheKey(string service, int portalId)
        {
            const string cacheKey = "Authentication";
            return cacheKey + "." + service + "_" + portalId;
        }
        #endregion

    }
}