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

        var collection = _mongoContext.GetCollection<ChapterEntity>("Chapter");
        var filter = Builders<ChapterEntity>.Filter.Eq(c => c.StoryId, storyId);
        var chapters = await collection.Find(filter).SortBy(c => c.ChapterNumber).ToListAsync();

        ViewBag.StoryId = storyId;
        return View(chapters);
    }

    // GET: Chapter/Details/5
    public async Task<IActionResult> Details(string id)
    {
        var collection = _mongoContext.GetCollection<ChapterEntity>("Chapter");
        var chapter = await collection.Find(c => c.Id == id).FirstOrDefaultAsync();

        if (chapter == null)
            return NotFound();

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
            var collection = _mongoContext.GetCollection<ChapterEntity>("Chapter");
            await collection.InsertOneAsync(chapter);
            return RedirectToAction("Index", new { storyId = chapter.StoryId });
        }
        return View(chapter);
    }
}
