using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _db;

        public CartController(AppDbContext db)
        {
            _db = db;
        }

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account", new { returnUrl = "/Cart" });

            var cart = await _db.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var vm = new CartViewModel
            {
                CartItems = cart?.CartItems ?? new List<CartItem>(),
                Total = cart?.CartItems.Sum(ci => ci.Quantity * ci.Product.Price) ?? 0
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Product/Details/{productId}" });

            var product = await _db.Products.FindAsync(productId);
            if (product == null || !product.IsActive)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index", "Product");
            }

            if (quantity < 1) quantity = 1;
            if (quantity > product.Stock)
            {
                TempData["Error"] = $"Only {product.Stock} units available.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            var cart = await _db.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId.Value, UpdatedAt = DateTime.UtcNow };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                if (cartItem.Quantity > product.Stock)
                    cartItem.Quantity = product.Stock;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"\"{product.Name}\" added to cart!";
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cartItem = await _db.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

            if (cartItem == null)
            {
                TempData["Error"] = "Cart item not found.";
                return RedirectToAction("Index");
            }

            if (quantity <= 0)
            {
                _db.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = Math.Min(quantity, cartItem.Product.Stock);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cartItem = await _db.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

            if (cartItem != null)
            {
                _db.CartItems.Remove(cartItem);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Item removed from cart.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Count()
        {
            var userId = GetUserId();
            if (userId == null) return Json(0);

            var count = await _db.CartItems
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart.UserId == userId)
                .SumAsync(ci => ci.Quantity);

            return Json(count);
        }
    }
}
