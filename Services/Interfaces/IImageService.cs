using System.IO;

namespace MyManual.Services.Interfaces
{
    public interface IImageService
    {
        /// <summary>
        /// 이미지를 Azure Blob Storage에 업로드
        /// </summary>
        /// <param name="imageStream">이미지 스트림</param>
        /// <param name="fileName">원본 파일명</param>
        /// <returns>업로드된 Blob의 URL</returns>
        Task<string> UploadImageAsync(Stream imageStream, string fileName);

        /// <summary>
        /// Azure Blob Storage에서 이미지 삭제
        /// </summary>
        /// <param name="blobUrl">삭제할 Blob URL</param>
        /// <returns>삭제 성공 여부</returns>
        Task<bool> DeleteImageAsync(string blobUrl);

        /// <summary>
        /// 여러 이미지를 한 번에 업로드
        /// </summary>
        /// <param name="images">이미지 스트림과 파일명 목록</param>
        /// <returns>업로드된 Blob URL 목록</returns>
        Task<List<string>> UploadImagesAsync(List<(Stream stream, string fileName)> images);

        /// <summary>
        /// 여러 이미지를 한 번에 삭제
        /// </summary>
        /// <param name="blobUrls">삭제할 Blob URL 목록</param>
        Task DeleteImagesAsync(IEnumerable<string> blobUrls);
    }
}
