using ECommerceApp.Data;
using ECommerceApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _db;

        public ProductController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(int? categoryId, string? search)
        {
            var query = _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    (p.Description != null && p.Description.Contains(search)));

            string? categoryName = null;
            if (categoryId.HasValue)
            {
                var cat = await _db.Categories.FindAsync(categoryId.Value);
                categoryName = cat?.Name;
            }

            var vm = new ProductListViewModel
            {
                Products = await query.OrderBy(p => p.Name).ToListAsync(),
                Categories = await _db.Categories.ToListAsync(),
                SelectedCategoryId = categoryId,
                SearchQuery = search,
                CategoryName = categoryName
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
                return NotFound();

            ViewBag.Related = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id && p.IsActive)
                .Take(4)
                .ToListAsync();

            return View(product);
        }
    }
}
