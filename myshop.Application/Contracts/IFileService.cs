using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Application.Contracts
{
    public enum ImageValidationResult
    {
        Valid,
        InvalidExtension,
        FileTooLarge,
        InvalidContent
    }

    public interface IFileService
    {
        Task<string> SaveImageAsync(Stream fileStream, string fileExtension, string folderName);
        void DeleteImage(string imagePath);
        ImageValidationResult ValidateImage(Stream fileStream, string fileExtension, long fileSizeInBytes);
    }
}
