using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using MyManual.Exceptions;
using MyManual.Services.Interfaces;

namespace MyManual.Services
{
    public class ImageService : IImageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly string _containerName;
        private readonly string _connectionString;

        public ImageService(string connectionString, string containerName = "manual-images")
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Azure Storage 연결 문자열이 필요합니다.", nameof(connectionString));
            }

            _connectionString = connectionString;
            _containerName = containerName;

            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // 컨테이너가 없으면 생성 (Private - 공용 액세스 없음)
            _containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
        {
            try
            {
                // 고유한 Blob 이름 생성
                var blobName = $"{Guid.NewGuid()}_{SanitizeFileName(fileName)}";
                var blobClient = _containerClient.GetBlobClient(blobName);

                // Content-Type 설정
                var contentType = GetContentType(fileName);
                var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };

                // 업로드
                await blobClient.UploadAsync(imageStream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });

                // SAS 토큰이 포함된 URL 생성 (1년 유효)
                var sasUrl = GenerateSasUrl(blobClient);

                System.Diagnostics.Debug.WriteLine($"[ImageService] 이미지 업로드 완료: {sasUrl}");

                return sasUrl;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageService] 이미지 업로드 실패: {ex.Message}");
                throw new DatabaseException("이미지 업로드 중 오류가 발생했습니다.", ex);
            }
        }

        /// <summary>
        /// SAS 토큰이 포함된 URL 생성 (읽기 전용, 1년 유효)
        /// </summary>
        private string GenerateSasUrl(BlobClient blobClient)
        {
            // BlobClient에서 SAS 생성이 가능한지 확인
            if (blobClient.CanGenerateSasUri)
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerName,
                    BlobName = blobClient.Name,
                    Resource = "b", // blob
                    ExpiresOn = DateTimeOffset.UtcNow.AddYears(1)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                return blobClient.GenerateSasUri(sasBuilder).ToString();
            }

            // SAS 생성이 안 되면 기본 URI 반환 (공용 액세스 필요)
            return blobClient.Uri.ToString();
        }

        public async Task<bool> DeleteImageAsync(string blobUrl)
        {
            try
            {
                var blobName = GetBlobNameFromUrl(blobUrl);
                if (string.IsNullOrEmpty(blobName))
                {
                    return false;
                }

                var blobClient = _containerClient.GetBlobClient(blobName);
                var response = await blobClient.DeleteIfExistsAsync();

                System.Diagnostics.Debug.WriteLine($"[ImageService] 이미지 삭제: {blobName}, 결과: {response.Value}");

                return response.Value;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageService] 이미지 삭제 실패: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> UploadImagesAsync(List<(Stream stream, string fileName)> images)
        {
            var uploadedUrls = new List<string>();

            foreach (var (stream, fileName) in images)
            {
                var url = await UploadImageAsync(stream, fileName);
                uploadedUrls.Add(url);
            }

            return uploadedUrls;
        }

        public async Task DeleteImagesAsync(IEnumerable<string> blobUrls)
        {
            var deleteTasks = blobUrls.Select(url => DeleteImageAsync(url));
            await Task.WhenAll(deleteTasks);
        }

        private string GetBlobNameFromUrl(string blobUrl)
        {
            try
            {
                var uri = new Uri(blobUrl);
                var segments = uri.AbsolutePath.Split('/');

                // URL 형식: /container-name/blob-name
                if (segments.Length >= 3)
                {
                    return segments[^1]; // 마지막 세그먼트가 blob 이름
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            // 파일명에서 허용되지 않는 문자 제거
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            return sanitized.Length > 100 ? sanitized[..100] : sanitized;
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
