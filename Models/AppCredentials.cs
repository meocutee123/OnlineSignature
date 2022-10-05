using System.Collections.Generic;

namespace OnlineSignature.Models
{
    public class AppCredentials
    {
        public string UserId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthorizationEndpoint { get; set; }
        public string OAuthBasePath { get; set; }
        public string TokenEndpoint { get; set; }
        public string PrivateKey { get; set; }
        public string RedirectUri { get; set; }
        public string DemoEnv { get; set; }
        public string ProdEnv { get; set; }
        public List<string> Scopes { get; set; }
    }
}
