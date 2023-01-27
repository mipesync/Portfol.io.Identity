namespace Portfol.io.Identity.ViewModels.ResponseModels.AuthResponseModels
{
    public class RefreshTokenResponse
    {
        public string? access_token { get; set; }
        public DateTime expire { get; set; }
        public string? refresh_token { get; set; }
        public DateTime refresh_token_expire { get; set; }
    }
}
