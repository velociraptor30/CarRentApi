using Microsoft.AspNetCore.Http;

namespace RentCarApi.Repository.Abstract
{
    public interface IFileService
    {
        Task<List<string>> SaveImages(List<IFormFile> imageFiles);
        Task<bool> DeleteImage(string imageUrl);
    }
}