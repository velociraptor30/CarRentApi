using Microsoft.AspNetCore.Hosting;
using RentCarApi.Repository.Abstract;

namespace RentCarApi.Repository.Implementation
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<List<string>> SaveImages(List<IFormFile> imageFiles)
        {
            var contentPath = _environment.ContentRootPath;
            var uploadPath = Path.Combine(contentPath, "Images");
            Directory.CreateDirectory(uploadPath); // Creates if doesn’t exist

            var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
            var imageUrls = new List<string>();

            foreach (var imageFile in imageFiles)
            {
                if (imageFile == null || imageFile.Length == 0)
                    continue;

                var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                    throw new ArgumentException($"Invalid file extension: {ext}");

                var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var baseUrl = "/Images/"; // Relative path for now
                imageUrls.Add(baseUrl + uniqueFileName);
            }

            return imageUrls;
        }

        public async Task<bool> DeleteImage(string imageUrl)
        {
            try
            {
                var fileName = Path.GetFileName(imageUrl); // Extract filename from URL
                var filePath = Path.Combine(_environment.ContentRootPath, "Images", fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}