using FAPCL.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAPCL.Services
{
    public class NewsService : INewsService
    {
        private readonly BookClassRoomContext _context;

        public NewsService(BookClassRoomContext context)
        {
            _context = context;
        }

        public async Task<News?> GetNewsById(int newsId)
        {
            return await _context.News.FirstOrDefaultAsync(x => x.NewsId ==newsId);
        }

        public async Task<(IEnumerable<News> News, int TotalPages)> GetAllNews(int pageNumber, int pageSize, string? title = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.News.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(n => n.Title.Contains(title));
            }

            if (startDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt <= endDate.Value);
            }
           
            int totalNews = await query.CountAsync();


            int totalPages = (int)Math.Ceiling(totalNews / (double)pageSize);
            var news = await query
                .Skip((pageNumber -1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return(news, totalPages);
        }

        public async Task<(IEnumerable<News> News, int TotalPages)> GetPublishedNews(int pageNumber, int pageSize)
        {
            var query = _context.News
                .Where(n => n.IsPublished == true);
            int totalNews = await query.CountAsync();


            int totalPages = (int)Math.Ceiling(totalNews / (double)pageSize);
            var news = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (news, totalPages);

        }

        public async Task<News> AddNews(News news)
        {
            news.CreatedAt = DateTime.UtcNow;
            _context.News.Add(news);
            await _context.SaveChangesAsync();
            return news;
        }

        public async Task<News?> UpdateNews(int newsId, News updatedNews)
        {
            var existingNews = await _context.News.FindAsync(newsId);
            if (existingNews == null)
            {
                return null;
            }

            existingNews.Title = updatedNews.Title;
            existingNews.Content = updatedNews.Content;
            existingNews.IsPublished = updatedNews.IsPublished;
            existingNews.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingNews;
        }



        public async Task<bool> DeleteNews(int newsId)
        {
            var news = await _context.News.FindAsync(newsId);
            if (news == null)
            {
                return false;
            }

            _context.News.Remove(news);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}