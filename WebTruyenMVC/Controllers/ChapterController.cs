using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebTruyenMVC;
using WebTruyenMVC.Entity;

public class ChapterController : Controller
{
    private readonly MongoContext _mongoContext;
    private readonly ILogger<ChapterController> _logger;

    public ChapterController(MongoContext mongoContext, ILogger<ChapterController> logger)
    {
        _mongoContext = mongoContext;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string storyId)
    {
        if (string.IsNullOrEmpty(storyId))
            return BadRequest("Missing storyId");

        var collection = _mongoContext.GetCollection<ChapterEntity>("Chapters");
        var filter = Builders<ChapterEntity>.Filter.Eq(c => c.StoryId, storyId);
        var chapters = await collection.Find(filter).SortBy(c => c.ChapterNumber).ToListAsync();

        ViewBag.StoryId = storyId;
        return View(chapters);
    }

    // GET: Chapter/Details/5
    public async Task<IActionResult> Details(string id)
    {
        var collection = _mongoContext.GetCollection<ChapterEntity>("Chapters");
        var chapter = await collection.Find(c => c.Id == id).FirstOrDefaultAsync();

        if (chapter == null)
            return NotFound();

        // Find previous chapter
        var prevChapter = await collection
            .Find(c => c.StoryId == chapter.StoryId && c.ChapterNumber < chapter.ChapterNumber)
            .SortByDescending(c => c.ChapterNumber)
            .FirstOrDefaultAsync();

        // Find next chapter
        var nextChapter = await collection
            .Find(c => c.StoryId == chapter.StoryId && c.ChapterNumber > chapter.ChapterNumber)
            .SortBy(c => c.ChapterNumber)
            .FirstOrDefaultAsync();

        ViewBag.PrevChapterId = prevChapter?.Id;
        ViewBag.NextChapterId = nextChapter?.Id;

        // --- Bổ sung cập nhật lịch sử đọc ---
        var userId = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrEmpty(userId))
        {
            var readingHistoryCollection = _mongoContext.GetCollection<ReadingHistoryEntity>("ReadingHistory");
            var filter = Builders<ReadingHistoryEntity>.Filter.Where(x => x.UserID == userId && x.StoryID == chapter.StoryId);
            var history = await readingHistoryCollection.Find(filter).FirstOrDefaultAsync();
            if (history == null)
            {
                // Tạo mới lịch sử đọc
                var newHistory = new ReadingHistoryEntity
                {
                    UserID = userId,
                    StoryID = chapter.StoryId,
                    LastReadChapter = chapter.ChapterNumber,
                    LastReadAt = DateTime.UtcNow
                };
                await readingHistoryCollection.InsertOneAsync(newHistory);
            }
            else
            {
                // Cập nhật chương mới nhất đã đọc
                var update = Builders<ReadingHistoryEntity>.Update
                    .Set(x => x.LastReadChapter, chapter.ChapterNumber)
                    .Set(x => x.LastReadAt, DateTime.UtcNow);
                await readingHistoryCollection.UpdateOneAsync(filter, update);
            }
        }
        // --- Kết thúc bổ sung ---

        return View(chapter);
    }

    // GET: Chapter/Create
    public IActionResult Create(string storyId)
    {
        if (string.IsNullOrEmpty(storyId))
            return BadRequest("Missing storyId");

        var chapter = new ChapterEntity { StoryId = storyId };
        return View(chapter);
    }

    // POST: Chapter/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChapterEntity chapter)
    {
        if (ModelState.IsValid)
        {
            chapter.Created = DateTime.UtcNow;
            var collection = _mongoContext.GetCollection<ChapterEntity>("Chapters");
            await collection.InsertOneAsync(chapter);
            return RedirectToAction("Index", new { storyId = chapter.StoryId });
        }
        return View(chapter);
    }
}
