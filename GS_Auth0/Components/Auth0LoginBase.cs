using DotNetNuke.Services.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GS.Auth0.Components
{
    /// <summary>
    /// Class Auth0LoginBase.
    /// Implements the <see cref="DotNetNuke.Services.Authentication.AuthenticationLoginBase" />
    /// </summary>
    /// <seealso cref="DotNetNuke.Services.Authentication.AuthenticationLoginBase" />
    public class Auth0LoginBase : AuthenticationLoginBase
    {
        public override bool Enabled
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}