using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using WebTruyenMVC.Entity;
using WebTruyenMVC.Models;

namespace WebTruyenMVC.Controllers
{
    //[ApiController]
    //[Route("api/[controller]")]
    public class ReadingHistoryController : Controller
    {
        private readonly MongoContext mongoContext;
        private readonly ILogger<ReadingHistoryController> logger;

        public ReadingHistoryController(MongoContext mongoContext, ILogger<ReadingHistoryController> logger)
        {
            this.mongoContext = mongoContext;
            this.logger = logger;
        }

        [HttpPost("ListAll")]
        public async Task<IActionResult> GetAll([FromBody] FilterEntity request)
        {
            var csModel = new ReadingHistoryModel(mongoContext, logger);
            var response = await csModel.GetAllReadingHistoryAsync(request);

            return Ok(response);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var csModel = new ReadingHistoryModel(mongoContext, logger);
            var response = await csModel.GetReadingHistoryByIdAsync(id);
            return Ok(response);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] ReadingHistoryEntity newReadingHistory)
        {
            var csModel = new ReadingHistoryModel(mongoContext, logger);
            var response = await csModel.CreateReadingHistoryAsync(newReadingHistory);
            return Ok(response);
        }


        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] ReadingHistoryEntity updateReadingHistory)
        {
            updateReadingHistory.Id = id;
            var csModel = new ReadingHistoryModel(mongoContext, logger);
            var response = await csModel.UpdateReadingHistoryAsync(updateReadingHistory);
            return Ok(response);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var csModel = new ReadingHistoryModel(mongoContext, logger);
            var response = await csModel.DeleteReadingHistoryAsync(id);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Bookmark(string storyId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(storyId))
                return Json(new { success = false, message = "Bạn cần đăng nhập để đánh dấu truyện." });

            var model = new ReadingHistoryModel(mongoContext, logger);

            // Kiểm tra đã đánh dấu chưa
            var collection = mongoContext.GetCollection<ReadingHistoryEntity>("ReadingHistory");
            var existing = await collection.Find(x => x.UserID == userId && x.StoryID == storyId && x.LastReadChapter == 0).FirstOrDefaultAsync();
            if (existing == null)
            {
                var bookmark = new ReadingHistoryEntity
                {
                    UserID = userId,
                    StoryID = storyId,
                    LastReadChapter = 0,
                    LastReadAt = DateTime.UtcNow
                };
                await model.CreateReadingHistoryAsync(bookmark);
                return Json(new { success = true, message = "Đánh dấu truyện thành công!" });
            }
            else
            {
                return Json(new { success = false, message = "Truyện đã được đánh dấu trước đó." });
            }
        }

        public async Task<IActionResult> Bookmarks()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            // Lấy danh sách truyện đã đánh dấu (LastReadChapter == 0)
            var readingHistoryCollection = mongoContext.GetCollection<ReadingHistoryEntity>("ReadingHistory");
            var storyCollection = mongoContext.GetCollection<StoryEntity>("Stories");

            var bookmarks = await readingHistoryCollection.Find(x => x.UserID == userId && x.LastReadChapter == 0).ToListAsync();
            var storyIds = bookmarks.Select(b => b.StoryID).ToList();
            var stories = await storyCollection.Find(x => storyIds.Contains(x.Id)).ToListAsync();

            // Truyền danh sách truyện sang view
            TempData["BookmarkSuccess"] = TempData["BookmarkSuccess"] ?? null;
            return View(stories);
        }
    }
}
