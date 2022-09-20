using ECDBugTracker.Services.Interfaces;

namespace ECDBugTracker.Services
{
    public class ImageService : IImageService
    {
        private readonly string _defaultBlogPostImageSrc = "/img/defaultBlog.jpg";
        private readonly string _defaultCategoryImageSrc = "/img/defaultCategory.png";
        private readonly string _defaultUserImageSrc = "/img/defaultProfile.png";

        //TODO: Blog Customizations
        public string ConvertByteArrayToFile(byte[] fileData, string extension, int? imageType)
        {
            if ((fileData == null || fileData.Length == 0) && imageType != null)
            {
                switch (imageType)
                {
                    // BlogUser Image based on the 'DefaultImage' Enum
                    case 1: return _defaultUserImageSrc;
                    // BlogPost Image based on the 'DefaultImage' Enum
                    case 2: return _defaultBlogPostImageSrc;
                    // Category Image based on the 'DefaultImage' Enum
                    case 3: return _defaultCategoryImageSrc;
                }
            }
            try
            {
                string ImageBase64Data = Convert.ToBase64String(fileData!);
                return string.Format($"data:{extension};base64,{ImageBase64Data}");
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                using MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();

                return byteFile;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
