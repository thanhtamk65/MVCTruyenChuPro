using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text.Json;
using WebTruyenMVC.Entity;
using WebTruyenMVC.Models;

namespace WebTruyenMVC.Controllers
{
    public class StoryController : Controller
    {
        private readonly MongoContext _mongoContext;
        private readonly ILogger<StoryController> _logger;

        public StoryController(MongoContext mongoContext, ILogger<StoryController> logger)
        {
            _mongoContext = mongoContext;
            _logger = logger;
        }

        // Danh sách truyện
        public async Task<IActionResult> Index()
        {
            var model = new StoryModel(_mongoContext, _logger);
            var response = await model.GetAllStoryAsync(new FilterEntity());

            if (response.Code != 200 || response.Data == null)
                return View(new List<StoryEntity>());

            var json = JsonSerializer.Serialize(response.Data);
            var parsed = JsonSerializer.Deserialize<StoryListResponse>(json);

            return View(parsed?.ListData ?? new List<StoryEntity>());
        }

        public async Task<IActionResult> Details(string id)
        {
            var storyModel = new StoryModel(_mongoContext, _logger);
            var authorModel = new AuthorModel(_mongoContext, _logger);

            var storyResponse = await storyModel.GetStoryByIdAsync(id);

            if (storyResponse.Code != 200 || storyResponse.Data == null)
                return NotFound();

            var story = storyResponse.Data as StoryEntity;

            if (story == null)
                return NotFound();

            if (!string.IsNullOrEmpty(story.AuthorId))
            {
                var authorResponse = await authorModel.GetAuthorByIdAsync(story.AuthorId);
                if (authorResponse.Code == 200 && authorResponse.Data is AuthorEntity author)
                {
                    story.AuthorInfo = author;
                }
            }

            return View(story);
        }



        public IActionResult Create()
        {
            var authorModel = new AuthorModel(_mongoContext, _logger);
            var authorsResponse = authorModel.GetAllAuthorAsync(new FilterEntity()).Result;

            List<AuthorEntity> authors = new List<AuthorEntity>();
            if (authorsResponse.Code == 200 && authorsResponse.Data != null)
            {
                var json = JsonSerializer.Serialize(authorsResponse.Data);
                var parsed = JsonSerializer.Deserialize<AuthorListResponse>(json);
                authors = parsed?.ListData ?? new List<AuthorEntity>();
            }

            ViewBag.Authors = authors;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoryEntity story)
        {
            var authorModel = new AuthorModel(_mongoContext, _logger);
            var authorsResponse = await authorModel.GetAllAuthorAsync(new FilterEntity());

            List<AuthorEntity> authors = new List<AuthorEntity>();
            if (authorsResponse.Code == 200 && authorsResponse.Data != null)
            {
                var json = JsonSerializer.Serialize(authorsResponse.Data);
                var parsed = JsonSerializer.Deserialize<AuthorListResponse>(json);
                authors = parsed?.ListData ?? new List<AuthorEntity>();
            }
            ViewBag.Authors = authors;

            if (ModelState.IsValid)
            {
                var model = new StoryModel(_mongoContext, _logger);
                var response = await model.CreateStoryAsync(story);
                if (response.Code == 201)
                    return RedirectToAction(nameof(Index));
            }
            return View(story);
        }
        public class StoryDetailsViewModel
        {
            public StoryEntity Story { get; set; } = null!;
            public AuthorEntity? Author { get; set; }
        }


        // Form cập nhật
        public async Task<IActionResult> Edit(string id)
        {
            var model = new StoryModel(_mongoContext, _logger);
            var response = await model.GetStoryByIdAsync(id);

            if (response.Code != 200 || response.Data == null)
                return NotFound();

            var story = JsonSerializer.Deserialize<StoryEntity>(response.Data.ToString()!);
            return View(story);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, StoryEntity story)
        {
            if (ModelState.IsValid)
            {
                story.Id = id;
                var model = new StoryModel(_mongoContext, _logger);
                var response = await model.UpdateStoryAsync(story);

                if (response.Code == 200)
                    return RedirectToAction(nameof(Index));
            }
            return View(story);
        }

        // Form xác nhận xóa
        public async Task<IActionResult> Delete(string id)
        {
            var model = new StoryModel(_mongoContext, _logger);
            var response = await model.GetStoryByIdAsync(id);

            if (response.Code != 200 || response.Data == null)
                return NotFound();

            var story = JsonSerializer.Deserialize<StoryEntity>(response.Data.ToString()!);
            return View(story);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var model = new StoryModel(_mongoContext, _logger);
            var response = await model.DeleteStoryAsync(id);

            if (response.Code == 200)
                return RedirectToAction(nameof(Index));

            return NotFound();
        }

        // Truyện được đánh giá cao
        public async Task<IActionResult> TopRated()
        {
            var model = new StoryModel(_mongoContext, _logger);
            var response = await model.GetTopRatedStoriesAsync();

            if (response.Code != 200 || response.Data == null)
                return View(new List<StoryEntity>());

            var json = JsonSerializer.Serialize(response.Data);
            var list = JsonSerializer.Deserialize<List<StoryEntity>>(json);

            return View(list ?? new List<StoryEntity>());
        }
    }

    // Phục vụ cho GetAllStoryAsync → lấy ListData từ MessagesResponse.Data
    public class StoryListResponse
    {
        public long TotalItemCounts { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<StoryEntity> ListData { get; set; } = new();
    }
}
