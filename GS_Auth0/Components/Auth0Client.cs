using DotNetNuke.Services.Authentication.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GS.Auth0.Components
{
    /// <summary>
    /// Class Auth0Client.
    /// Implements the <see cref="DotNetNuke.Services.Authentication.OAuth.OAuthClientBase" />
    /// </summary>
    /// <seealso cref="DotNetNuke.Services.Authentication.OAuth.OAuthClientBase" />
    public class Auth0Client : OAuthClientBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Auth0Client"/> class.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="mode">The mode.</param>
        public Auth0Client(int portalId, DotNetNuke.Services.Authentication.AuthMode mode)
            : base(portalId, mode, "xx")
        {
            Service = Constants.PROVIDER_NAME;
        }
    }
}