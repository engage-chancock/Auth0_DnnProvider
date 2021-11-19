using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GS.Auth0.Components
{
    /// <summary>
    /// Class Auth0AuthMessageHandler.
    /// Implements the <see cref="DotNetNuke.Web.Api.Auth.AuthMessageHandlerBase" />
    /// </summary>
    /// <seealso cref="DotNetNuke.Web.Api.Auth.AuthMessageHandlerBase" />
    public class Auth0AuthMessageHandler : DotNetNuke.Web.Api.Auth.AuthMessageHandlerBase
    {
        /// <summary>
        /// Gets the authentication scheme.
        /// </summary>
        /// <value>The authentication scheme.</value>
        public override string AuthScheme => Constants.AUTH_TYPE;

        /// <summary>
        /// Initializes a new instance of the <see cref="Auth0AuthMessageHandler"/> class.
        /// </summary>
        /// <param name="includeByDefault">if set to <c>true</c> [include by default].</param>
        /// <param name="forceSsl">if set to <c>true</c> [force SSL].</param>
        public Auth0AuthMessageHandler(bool includeByDefault, bool forceSsl)
            : base(includeByDefault, forceSsl)
        {
        }
    }
}