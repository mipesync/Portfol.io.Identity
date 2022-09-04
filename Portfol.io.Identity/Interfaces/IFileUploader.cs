namespace Portfol.io.Identity.Interfaces
{
    public interface IFileUploader
    {
        IFormFile File { get; set; }
        string AbsolutePath { get; set; }
        string WebRootPath { get; set; }

        Task<string> UploadFileAsync();
    }
}
