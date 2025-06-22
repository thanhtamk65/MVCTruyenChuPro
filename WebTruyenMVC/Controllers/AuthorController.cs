using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebTruyenMVC.Entity;
using WebTruyenMVC.Models;
using System.Text.Json;

namespace WebTruyenMVC.Controllers
{
    public class AuthorController : Controller
    {
        private readonly MongoContext _mongoContext;
        private readonly ILogger<AuthorController> _logger;

        public AuthorController(MongoContext mongoContext, ILogger<AuthorController> logger)
        {
            _mongoContext = mongoContext;
            _logger = logger;
        }

        // Danh sách tất cả tác giả
        public async Task<IActionResult> Index()
        {
            var model = new AuthorModel(_mongoContext, _logger);
            var response = await model.GetAllAuthorAsync(new FilterEntity());

            if (response.Code != 200 || response.Data == null)
                return View(new List<AuthorEntity>());

            var json = JsonSerializer.Serialize(response.Data);
            var parsed = JsonSerializer.Deserialize<AuthorListResponse>(json);

            return View(parsed?.ListData ?? new List<AuthorEntity>());
        }

        // Chi tiết tác giả
        public async Task<IActionResult> Details(string id)
        {
            var model = new AuthorModel(_mongoContext, _logger);
            var response = await model.GetAuthorByIdAsync(id);

            if (response.Code != 200 || response.Data == null)
                return NotFound();

            var author = JsonSerializer.Deserialize<AuthorEntity>(response.Data.ToString()!);
            return View(author);
        }

        // Form tạo mới tác giả
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuthorEntity author)
        {
            if (ModelState.IsValid)
            {
                var model = new AuthorModel(_mongoContext, _logger);
                var response = await model.CreateAuthorAsync(author);
                if (response.Code == 201)
                    return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // Form chỉnh sửa
        public async Task<IActionResult> Edit(string id)
        {
            var model = new AuthorModel(_mongoContext, _logger);
            var response = await model.GetAuthorByIdAsync(id);

            if (response.Code != 200 || response.Data == null)
                return NotFound();

            var author = JsonSerializer.Deserialize<AuthorEntity>(response.Data.ToString()!);
            return View(author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AuthorEntity author)
        {
            if (ModelState.IsValid)
            {
                author.Id = id;
                var model = new AuthorModel(_mongoContext, _logger);
                var response = await model.UpdateAuthorAsync(author);

                if (response.Code == 200)
                    return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // Xác nhận xóa
        public async Task<IActionResult> Delete(string id)
        {
            var model = new AuthorModel(_mongoContext, _logger);
            var response = await model.GetAuthorByIdAsync(id);

            if (response.Code != 200 || response.Data == null)
                return NotFound();

            var author = JsonSerializer.Deserialize<AuthorEntity>(response.Data.ToString()!);
            return View(author);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var model = new AuthorModel(_mongoContext, _logger);
            var response = await model.DeleteAuthorAsync(id);

            if (response.Code == 200)
                return RedirectToAction(nameof(Index));

            return NotFound();
        }
    }

    // Dùng để map lại dữ liệu trả về từ MessagesResponse.Data
    public class AuthorListResponse
    {
        public long TotalItemCounts { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<AuthorEntity> ListData { get; set; } = new();
    }
}
