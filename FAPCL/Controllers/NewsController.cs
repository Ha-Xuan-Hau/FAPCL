using FAPCL.DTO;
using FAPCL.Model;
using FAPCL.Services;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;
        public NewsController(INewsService newsService) { 
            _newsService = newsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNews(int currentPage = 1)
        {
            var result = await _newsService.GetPublishedNews(currentPage, 10);

            if (result.News == null || !result.News.Any())
            {
                return NotFound("No rooms found");
            }

            return Ok(new { result.News, result.TotalPages });
        }
        [HttpGet("managerNews/{newsId}")]
        public async Task<IActionResult> GetNewsDetail(int newsId)
        {
            var resutl = await _newsService.GetNewsById(newsId);
            if (resutl == null)
            {
                return NotFound($"Not found news with id: {newsId}");
            }
            return Ok(resutl);
        }
        [HttpGet("managerNews")]
        public async Task<IActionResult> GetManagerNews(DateTime? startDate, DateTime? toDate, String? Title, int currentPage = 1)
        {
            var result = await _newsService.GetAllNews(currentPage, 10,Title,startDate, toDate);
            return Ok(new
            {
                News = result.News ?? new List<News>(), // Nếu `result.News` là null thì trả về danh sách rỗng []
                TotalPages = result.TotalPages
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddNews([FromBody] NewsDTO news)
        {
            var createdNews = await _newsService.AddNews(new News {
                Title = news.title,
                Content = news.content,
                IsPublished = news.isPublished,
                CreatedBy = news.createdby,
            });
            return CreatedAtAction(nameof(GetNewsDetail), new { newsId = createdNews.NewsId }, createdNews);
        }

        // Cập nhật tin
        [HttpPut("{newsId}")]
        public async Task<IActionResult> UpdateNews(int newsId, [FromBody] NewsDTO updatedNews)
        {
            var result = await _newsService.UpdateNews(newsId, new News
            {
                Title = updatedNews.title,
                Content = updatedNews.content,
                IsPublished = updatedNews.isPublished,
            });
            return result != null ? Ok(result) : NotFound();
        }
        [HttpDelete("{newsId}")]
        public async Task<IActionResult> DeleteRoom(int newsId)
        {
            var isDeleted = await _newsService.DeleteNews(newsId);
            return isDeleted ? Ok($"News {newsId} deleted successfully") : NotFound($"News {newsId} not found");
        }
    }
}
