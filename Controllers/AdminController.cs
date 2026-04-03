using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetInt32("IsAdmin") == 1;
        }

        private IActionResult RequireAdmin()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");
            return null!;
        }

        public async Task<IActionResult> Index()
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            ViewBag.TotalProducts = await _db.Products.CountAsync();
            ViewBag.TotalOrders = await _db.Orders.CountAsync();
            ViewBag.TotalUsers = await _db.Users.CountAsync(u => !u.IsAdmin);
            ViewBag.TotalRevenue = await _db.Orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            ViewBag.RecentOrders = await _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return View();
        }

        // Products Management
        public async Task<IActionResult> Products()
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            var products = await _db.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View(new ProductViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductViewModel model)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _db.Categories.ToListAsync();
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Stock = model.Stock,
                ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl,
                CategoryId = model.CategoryId,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Product \"{product.Name}\" created successfully.";
            return RedirectToAction("Products");
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _db.Categories.ToListAsync();
            var vm = new ProductViewModel
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                IsActive = product.IsActive
            };

            ViewBag.ProductId = id;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, ProductViewModel model)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _db.Categories.ToListAsync();
                ViewBag.ProductId = id;
                return View(model);
            }

            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl;
            product.CategoryId = model.CategoryId;
            product.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Product \"{product.Name}\" updated successfully.";
            return RedirectToAction("Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Soft delete
            product.IsActive = false;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Product \"{product.Name}\" has been removed from the store.";
            return RedirectToAction("Products");
        }

        // Orders Management
        public async Task<IActionResult> Orders()
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            var orders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            var order = await _db.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Order #{orderId} status updated to {status}.";
            }

            return RedirectToAction("Orders");
        }
    }
}
