using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebTruyenMVC.Entity;
using WebTruyenMVC.Models;
using System.Text.Json;

namespace WebTruyenMVC.Controllers
{
    public class CategoryController : Controller
    {
        private readonly MongoContext _mongoContext;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(MongoContext mongoContext, ILogger<CategoryController> logger)
        {
            _mongoContext = mongoContext;
            _logger = logger;
        }

        // Danh sách tất cả thể loại
        public async Task<IActionResult> Index()
        {
            var model = new CategoryModel(_mongoContext, _logger);
            var response = await model.GetAllCategoryAsync(new FilterEntity());

            if (response.Code != 200 || response.Data == null)
                return View(new List<CategoryEntity>());

            var json = JsonSerializer.Serialize(response.Data);
            var parsed = JsonSerializer.Deserialize<CategoryListResponse>(json);

            var categories = parsed?.ListData ?? new List<CategoryEntity>();
            return View(categories);
        }

        // Chi tiết thể loại
        public async Task<IActionResult> Details(string id)
        {
            var model = new CategoryModel(_mongoContext, _logger);
            var response = await model.GetCategoryByIdAsync(id);

            if (response.Code != 200 || response.Data == null)
                return NotFound();

            var category = JsonSerializer.Deserialize<CategoryEntity>(response.Data.ToString()!);
            return View(category);
        }

        // Form tạo mới thể loại
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryEntity category)
        {
            if (ModelState.IsValid)
            {
                var model = new CategoryModel(_mongoContext, _logger);
                var response = await model.CreateCategoryAsync(category);
                if (response.Code == 201)
                    return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // Form chỉnh sửa
        public async Task<IActionResult> Edit(string id)
        {
            var model = new CategoryModel(_mongoContext, _logger);
            var response = await model.GetCategoryByIdAsync(id);

            if (response.Code != 200 || response.Data == null)
                return NotFound();

            var category = response.Data as CategoryEntity;
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CategoryEntity category)
        {
            if (ModelState.IsValid)
            {
                category.Id = id;
                var model = new CategoryModel(_mongoContext, _logger);
                var response = await model.UpdateCategoryAsync(category);

                if (response.Code == 200)
                    return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // Xác nhận xóa
        public async Task<IActionResult> Delete(string id)
        {
            var model = new CategoryModel(_mongoContext, _logger);
            var response = await model.GetCategoryByIdAsync(id);

            if (response.Code != 200 || response.Data == null)
                return NotFound();

            var category = response.Data as CategoryEntity;
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var model = new CategoryModel(_mongoContext, _logger);
            var response = await model.DeleteCategoryAsync(id);

            if (response.Code == 200)
                return RedirectToAction(nameof(Index));

            return NotFound();
        }
    }

    // Dùng để map lại dữ liệu trả về từ MessagesResponse.Data
    public class CategoryListResponse
    {
        public long TotalItemCounts { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<CategoryEntity> ListData { get; set; } = new();
    }
}