using Portfol.io.Identity.Interfaces;

namespace Portfol.io.Identity.Common.Services
{
    public class FileUploader : IFileUploader
    {
        public IFormFile File { get; set; } = null!;
        public string AbsolutePath { get; set; } = null!;

        public async Task<string> UploadFileAsync()
        {
            var fileExtension = Path.GetExtension(File.FileName);
            var fileNameHash = Guid.NewGuid().ToString();
            string path = $"{AbsolutePath}/{fileNameHash}{fileExtension}";

            Directory.CreateDirectory(AbsolutePath);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await File.CopyToAsync(fileStream);
            }

            return path;
        }
    }
}
