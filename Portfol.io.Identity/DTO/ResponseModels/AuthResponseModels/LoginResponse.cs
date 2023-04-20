namespace Portfol.io.Identity.DTO.ResponseModels.AuthResponseModels
{
    public class LoginResponse
    {
        public string userId { get; set; } = "";
        public string access_token { get; set; } = "";
        public long expires { get; set; }
        public string? refresh_token { get; set; }
        public long? refresh_token_expires { get; set; }
        public string? returnUrl { get; set; }
    }
}
