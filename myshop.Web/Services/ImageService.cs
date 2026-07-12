using myshop.Application.Contracts;

namespace myshop.Web.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveImageAsync(Stream fileStream, string fileExtension, string folderName)
        {
            string rootPath = _webHostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + fileExtension;
            var uploadPath = Path.Combine(rootPath, folderName);
            var fullPath = Path.Combine(uploadPath, fileName);

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
