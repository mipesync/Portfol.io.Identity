namespace Portfol.io.Identity.DTO
{
    public class ErrorDto
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}