using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GS.Auth0.Components
{
    public class JsonKeyResponse
    {
        public IEnumerable<JsonKey> keys { get; set; }
    }
}