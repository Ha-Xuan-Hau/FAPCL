using FAPCL.Model;

namespace FAPCL.Services
{
    public interface INewsService
    {
        Task<News?> GetNewsById(int newsId);
        Task<(IEnumerable<News> News, int TotalPages)> GetAllNews(int pageNumber, int pageSize, string? title = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<(IEnumerable<News> News, int TotalPages)> GetPublishedNews(int pageNumber, int pageSize);
        Task<News> AddNews(News news);
        Task<News?> UpdateNews(int newsId, News updatedNews);
        Task<bool> DeleteNews(int newsId);
    }
}
