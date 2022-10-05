namespace OnlineSignature.Models
{
    public abstract class DsAccessInfo
    {
        public string AccessToken { get; set; }
        public string BasePath { get; set; }
        public string AccountId { get; set; }
    }
}
