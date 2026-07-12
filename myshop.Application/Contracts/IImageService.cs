using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Application.Contracts
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(Stream fileStream, string fileExtension, string folderName);
        void DeleteImage(string imagePath);
    }
}
