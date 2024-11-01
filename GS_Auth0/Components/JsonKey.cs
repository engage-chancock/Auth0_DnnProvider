using System.Collections.Generic;

namespace GS.Auth0.Components
{
    public class JsonKey
    {
        public string kty { get; set; }

        public string use { get; set; }

        public string n { get; set; }

        public string e { get; set; }

        public string kid { get; set; }

        public string x5t { get; set; }

        public IEnumerable<string> x5c { get; set; }

        public string alg { get; set; }
    }
}