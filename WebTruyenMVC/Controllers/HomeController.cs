using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;
using WebTruyenMVC.Entity;
using WebTruyenMVC.Models;

namespace WebTruyenMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly MongoContext _mongoContext;
        private readonly ILogger<HomeController> logger;
        private readonly IMongoCollection<StoryEntity> _stories;

        public HomeController(MongoContext mongoContext, ILogger<HomeController> logger)
        {
            _mongoContext = mongoContext;
            _stories = mongoContext.GetCollection<StoryEntity>("Stories");
            this.logger = logger;

        }

        public async Task<IActionResult> Index(string search)
        {
            var builder = Builders<StoryEntity>.Filter;
            var filter = string.IsNullOrEmpty(search)
                ? builder.Empty
                : builder.Regex(x => x.Title, new MongoDB.Bson.BsonRegularExpression(search, "i"));

            var allStories = await _stories.Find(filter).SortByDescending(x => x.Created).ToListAsync();
            var newestStories = allStories.Take(6).ToList();
            var recommendedStories = allStories.OrderByDescending(x => x.Views).Take(5).ToList();

            ViewBag.Recommended = recommendedStories;
            ViewBag.SearchKeyword = search;

            return View(newestStories);
        }

        public async Task<IActionResult> History()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var readingHistoryCollection = _mongoContext.GetCollection<ReadingHistoryEntity>("ReadingHistory");
            var storyCollection = _mongoContext.GetCollection<StoryEntity>("Stories");

            // Lấy truyện đã đánh dấu (LastReadChapter == 0)
            var bookmarks = await readingHistoryCollection.Find(x => x.UserID == userId && x.LastReadChapter == 0).ToListAsync();
            var bookmarkObjectIds = bookmarks
                .Where(b => !string.IsNullOrEmpty(b.StoryID))
                .Select(b => MongoDB.Bson.ObjectId.Parse(b.StoryID))
                .ToList();
            var bookmarkIdStrings = bookmarkObjectIds.Select(oid => oid.ToString()).ToList();
            var bookmarkStories = bookmarkIdStrings.Any()
                ? await storyCollection.Find(x => bookmarkIdStrings.Contains(x.Id)).ToListAsync()
                : new List<StoryEntity>();

            // Lấy lịch sử đọc (LastReadChapter > 0)
            var histories = await readingHistoryCollection.Find(x => x.UserID == userId && x.LastReadChapter > 0).ToListAsync();
            var historyObjectIds = histories
                .Where(h => !string.IsNullOrEmpty(h.StoryID))
                .Select(h => MongoDB.Bson.ObjectId.Parse(h.StoryID))
                .ToList();
            var historyIdStrings = historyObjectIds.Select(oid => oid.ToString()).ToList();
            var historyStories = historyIdStrings.Any()
                ? await storyCollection.Find(x => historyIdStrings.Contains(x.Id)).ToListAsync()
                : new List<StoryEntity>();

            var historyList = histories
                .Select(h => (historyStories.FirstOrDefault(s => s.Id == h.StoryID), h.LastReadChapter))
                .Where(x => x.Item1 != null)
                .ToList();

            return View("History", (bookmarkStories.AsEnumerable(), historyList.AsEnumerable()));
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}
