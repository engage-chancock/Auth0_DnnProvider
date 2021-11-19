using DotNetNuke.Services.Authentication.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace GS.Auth0.Components
{
    /// <summary>
    /// Class Auth0UserData.
    /// Implements the <see cref="DotNetNuke.Services.Authentication.OAuth.UserData" />
    /// </summary>
    /// <seealso cref="DotNetNuke.Services.Authentication.OAuth.UserData" />
    [DataContract]
    public class Auth0UserData : UserData
    {
        #region Overrides

        public override string DisplayName
        {
            get { return ScreenName; }
            set { }
        }

        public override string Locale
        {
            get { return LanguageCode; }
            set { }
        }

        public override string ProfileImage
        {
            get { return ProfileImageUrl; }
            set { }
        }

        public override string Website
        {
            get { return Url; }
            set { }
        }

        #endregion

        [DataMember(Name = "screen_name")]
        public string ScreenName { get; set; }

        [DataMember(Name = "lang")]
        public string LanguageCode { get; set; }

        [DataMember(Name = "profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}