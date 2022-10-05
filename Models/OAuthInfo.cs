namespace OnlineSignature.Models
{
    public class OAuthInfo : DsAccessInfo
    {
        public long ExpiresAt { get; set; }
        public string RefreshToken { get; set; }
    }
}
