using myshop.Application.Contracts;

namespace myshop.Web.Services
{
    public class LocalFileService : IFileService
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeInBytes = 2 * 1024 * 1024;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public LocalFileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public ImageValidationResult ValidateImage(Stream fileStream, string fileExtension, long fileSizeInBytes)
        {
            var normalizedExtension = fileExtension.ToLowerInvariant();

            if (!AllowedExtensions.Contains(normalizedExtension))
                return ImageValidationResult.InvalidExtension;

            if (fileSizeInBytes > MaxFileSizeInBytes)
                return ImageValidationResult.FileTooLarge;

            if (!MatchesImageSignature(fileStream, normalizedExtension))
                return ImageValidationResult.InvalidContent;

            return ImageValidationResult.Valid;
        }

        private static bool MatchesImageSignature(Stream fileStream, string normalizedExtension)
        {
            var header = new byte[12];
            var originalPosition = fileStream.Position;

            fileStream.Position = 0;
            int bytesRead = fileStream.Read(header, 0, header.Length);
            fileStream.Position = originalPosition;

            return normalizedExtension switch
            {
                ".jpg" or ".jpeg" => bytesRead >= 3
                    && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,

                ".png" => bytesRead >= 8
                    && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                    && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,

                ".webp" => bytesRead >= 12
                    && header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F'
                    && header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P',

                _ => false
            };
        }

        public async Task<string> SaveImageAsync(Stream fileStream, string fileExtension, string folderName)
        {
            string rootPath = _webHostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + fileExtension;
            var uploadPath = Path.Combine(rootPath, folderName);
            var fullPath = Path.Combine(uploadPath, fileName);

            Directory.CreateDirectory(uploadPath);

            using (var outputStream = new FileStream(fullPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(outputStream);
            }

            return Path.Combine(folderName, fileName);
        }

        public void DeleteImage(string imagePath)
        {
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('\\'));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
