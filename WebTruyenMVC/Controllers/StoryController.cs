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
        public async Task<IActionResult> Index(string? OrderBy = "Created", bool OrderByDescending = true, int page = 1, int pageSize = 24)
        {
            var model = new StoryModel(_mongoContext, _logger);
            var filter = new FilterEntity
            {
                Page = page,
                PageSize = pageSize,
                OrderBy = OrderBy ?? "Created",
                OrderByDescending = OrderByDescending
            };
            var response = await model.GetAllStoryAsync(filter);

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

        public async Task<IActionResult> Create()
        {
            // Lấy danh sách tác giả
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

            // Lấy danh sách thể loại
            var categoryModel = new CategoryModel(_mongoContext, _logger);
            var categoriesResponse = await categoryModel.GetAllCategoryAsync(new FilterEntity());
            List<CategoryEntity> categories = new List<CategoryEntity>();
            if (categoriesResponse.Code == 200 && categoriesResponse.Data != null)
            {
                var json = JsonSerializer.Serialize(categoriesResponse.Data);
                var parsed = JsonSerializer.Deserialize<CategoryListResponse>(json);
                categories = parsed?.ListData ?? new List<CategoryEntity>();
            }
            ViewBag.Category = categories;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(StoryEntity story, IFormFile CoverImageFile)
        {
            // Lấy danh sách tác giả
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

            // Lấy danh sách thể loại
            var categoryModel = new CategoryModel(_mongoContext, _logger);
            var categoriesResponse = await categoryModel.GetAllCategoryAsync(new FilterEntity());
            List<CategoryEntity> categories = new List<CategoryEntity>();
            if (categoriesResponse.Code == 200 && categoriesResponse.Data != null)
            {
                var json = JsonSerializer.Serialize(categoriesResponse.Data);
                var parsed = JsonSerializer.Deserialize<CategoryListResponse>(json);
                categories = parsed?.ListData ?? new List<CategoryEntity>();
            }
            ViewBag.Category = categories;

            // Xử lý upload ảnh bìa (giữ nguyên phần này)
            if (CoverImageFile != null && CoverImageFile.Length > 0)
            {
                var fileName = Path.GetFileNameWithoutExtension(CoverImageFile.FileName);
                var extension = Path.GetExtension(CoverImageFile.FileName);
                var newFileName = $"{fileName}_{Guid.NewGuid()}{extension}";
                var imagePath = Path.Combine("wwwroot", "img", newFileName);

                Directory.CreateDirectory(Path.GetDirectoryName(imagePath)!);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await CoverImageFile.CopyToAsync(stream);
                }

                story.CoverImage = "img/" + newFileName;
            }

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

        public async Task<IActionResult> DetailStory(string id, int page = 1)
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

            // Lấy danh sách chương phân trang
            var chapterResponse = await storyModel.GetChaptersByStoryIdAsync(id, page, 100);
            var chapters = new List<ChapterEntity>();
            long total = 0;
            if (chapterResponse.Code == 200 && chapterResponse.Data != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(chapterResponse.Data);
                var parsed = System.Text.Json.JsonSerializer.Deserialize<ChapterListResponse>(json);
                chapters = parsed?.ListData ?? new List<ChapterEntity>();
                total = parsed?.TotalItemCounts ?? 0;
            }

            var vm = new StoryDetailViewModel
            {
                Story = story,
                Chapters = chapters,
                Page = page,
                PageSize = 100,
                TotalItemCounts = total
            };

            return View(vm);
        }

        public async Task<IActionResult> Latest(int page = 1, int pageSize = 20)
        {
            var model = new StoryModel(_mongoContext, _logger);
            var filter = new FilterEntity
            {
                Page = page,
                PageSize = pageSize,
                OrderBy = "Created",
                OrderByDescending = true
            };
            var response = await model.GetAllStoryAsync(filter);

            var json = JsonSerializer.Serialize(response.Data);
            var parsed = JsonSerializer.Deserialize<StoryListResponse>(json);

            int totalPages = (int)Math.Ceiling((parsed?.TotalItemCounts ?? 0) / (double)pageSize);

            var vm = new LatestStoriesViewModel
            {
                Stories = parsed?.ListData ?? new List<StoryEntity>(),
                CurrentPage = page,
                TotalPages = totalPages
            };
            return View(vm);
        }

        public async Task<IActionResult> MostViewed(int page = 1, int pageSize = 20)
        {
            var model = new StoryModel(_mongoContext, _logger);
            var filter = new FilterEntity
            {
                Page = page,
                PageSize = pageSize,
                OrderBy = "Views",
                OrderByDescending = true
            };
            var response = await model.GetAllStoryAsync(filter);

            var json = JsonSerializer.Serialize(response.Data);
            var parsed = JsonSerializer.Deserialize<StoryListResponse>(json);

            int totalPages = (int)Math.Ceiling((parsed?.TotalItemCounts ?? 0) / (double)pageSize);

            var vm = new LatestStoriesViewModel
            {
                Stories = parsed?.ListData ?? new List<StoryEntity>(),
                CurrentPage = page,
                TotalPages = totalPages
            };
            return View(vm);
        }

        public async Task<IActionResult> ByCategory(string categoryId, int page = 1, int pageSize = 24)
        {
            var model = new StoryModel(_mongoContext, _logger);
            var filter = new FilterEntity
            {
                Page = page,
                PageSize = pageSize,
                filter = new Filter { CategoryId = categoryId }
            };
            var response = await model.GetAllStoryAsync(filter);

            // Xử lý dữ liệu trả về
            var stories = new List<StoryEntity>();
            long total = 0;
            if (response.Code == 200 && response.Data != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(response.Data);
                var parsed = System.Text.Json.JsonSerializer.Deserialize<StoryListResponse>(json);
                stories = parsed?.ListData ?? new List<StoryEntity>();
                total = parsed?.TotalItemCounts ?? 0;
            }

            int totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var vm = new LatestStoriesViewModel
            {
                Stories = stories,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View("Latest", vm);
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

    // Phục vụ cho GetChaptersByStoryIdAsync
    public class ChapterListResponse
    {
        public long TotalItemCounts { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<ChapterEntity> ListData { get; set; } = new();
    }
}

public class LatestStoriesViewModel
{
    public IEnumerable<WebTruyenMVC.Entity.StoryEntity> Stories { get; set; } = new List<WebTruyenMVC.Entity.StoryEntity>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
