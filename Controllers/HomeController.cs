using ECommerceApp.Data;
using ECommerceApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var products = _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p =>
                    p.Name.Contains(search) ||
                    (p.Description != null && p.Description.Contains(search)));
            }

            var vm = new HomeViewModel
            {
                FeaturedProducts = await products.OrderByDescending(p => p.Id).Take(8).ToListAsync(),
                Categories = await _db.Categories.ToListAsync(),
                SearchQuery = search
            };

            return View(vm);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
