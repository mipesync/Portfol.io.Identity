namespace Portfol.io.Identity.ViewModels.ResponseModels.AuthResponseModels
{
    public class LoginResponse
    {
        public string? access_token { get; set; }
        public DateTime expires { get; set; }
        public string? refresh_token { get; set; }
        public DateTime? refresh_token_expires { get; set; }
        public string? returnUrl { get; set; }
    }
}
