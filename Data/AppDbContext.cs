using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User unique email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Cart - one per user
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId);

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", Description = "Gadgets, devices and tech accessories" },
                new Category { Id = 2, Name = "Clothing", Description = "Fashion for all occasions" },
                new Category { Id = 3, Name = "Books", Description = "Fiction, non-fiction and educational" },
                new Category { Id = 4, Name = "Home & Garden", Description = "Everything for your living space" },
                new Category { Id = 5, Name = "Sports", Description = "Equipment and apparel for athletes" }
            );

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Wireless Noise-Cancelling Headphones", Description = "Premium over-ear headphones with 30-hour battery life, active noise cancellation, and crystal-clear sound. Perfect for travel, work, or relaxation.", Price = 249.99m, Stock = 45, CategoryId = 1, ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400&h=300&fit=crop", IsActive = true },
                new Product { Id = 2, Name = "4K Ultra HD Smart TV - 55\"", Description = "Stunning 4K display with built-in streaming apps, Dolby Vision HDR, and voice assistant support. Transform your living room.", Price = 699.99m, Stock = 12, CategoryId = 1, ImageUrl = "https://images.unsplash.com/photo-1593359677879-a4bb92f829d1?w=400&h=300&fit=crop", IsActive = true },
                new Product { Id = 3, Name = "Mechanical Gaming Keyboard", Description = "RGB backlit mechanical keyboard with tactile switches, anti-ghosting, and a durable aluminum frame. Built for serious gamers.", Price = 129.99m, Stock = 67, CategoryId = 1, ImageUrl = "https://images.unsplash.com/photo-1541140532154-b024d705b90a?w=400&h=300&fit=crop", IsActive = true },
                new Product { Id = 4, Name = "Slim Fit Chino Pants", Description = "Classic slim-fit chinos made from premium stretch cotton. Available in multiple colors, perfect for office or weekend wear.", Price = 59.99m, Stock = 120, CategoryId = 2, ImageUrl = "https://images.unsplash.com/photo-1506629082955-511b1aa562c8?w=400&h=300&fit=crop", IsActive = true },
                new Product { Id = 5, Name = "Merino Wool Crew-Neck Sweater", Description = "Luxuriously soft merino wool sweater with a relaxed fit. Naturally temperature-regulating and odor-resistant.", Price = 89.99m, Stock = 85, CategoryId = 2, ImageUrl = "https://images.unsplash.com/photo-1434389677669-e08b4cac3105?w=400&h=300&fit=crop", IsActive = true },
                new Product { Id = 6, Name = "The Art of Clean Code", Description = "A practical guide to writing maintainable, readable, and professional software. Essential for every developer's bookshelf.", Price = 34.99m, Stock = 200, CategoryId = 3, ImageUrl = "https://images.unsplash.com/photo-1532012197267-da84d127e765?w=400&h=300&fit=crop", IsActive = true },
                new Product { Id = 7, Name = "Atomic Habits", Description = "Tiny changes, remarkable results. The definitive guide to building good habits and breaking bad ones. A #1 NY Times bestseller.", Price = 19.99m, Stock = 350, CategoryId = 3, ImageUrl = "https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=400&h=300&fit=crop", IsActive = true },
                new Product { Id = 8, Name = "Ergonomic Office Chair", Description = "Fully adjustable lumbar support, breathable mesh back, and 4D armrests. Your back will thank you after long work sessions.", Price = 349.99m, Stock = 28, CategoryId = 4, ImageUrl = "https://images.unsplash.com/photo-1580480055273-228ff5388ef8?w=400&h=300&fit=crop", IsActive = true },
                new Product { Id = 9, Name = "Ceramic Plant Pot Set (3-Pack)", Description = "Hand-crafted ceramic pots in three sizes, with drainage holes and matching saucers. Perfect for indoor succulents and herbs.", Price = 44.99m, Stock = 75, CategoryId = 4, ImageUrl = "https://images.unsplash.com/photo-1485955900006-10f4d324d411?w=400&h=300&fit=crop", IsActive = true },
                new Product { Id = 10, Name = "Yoga Mat Premium Non-Slip", Description = "6mm thick eco-friendly TPE mat with superior grip, alignment markings, and a carrying strap. Ideal for yoga, pilates, and stretching.", Price = 69.99m, Stock = 95, CategoryId = 5, ImageUrl = "https://images.unsplash.com/photo-1599901860904-17e6ed7083a0?w=400&h=300&fit=crop", IsActive = true },
                new Product { Id = 11, Name = "Smart Fitness Tracker Watch", Description = "24/7 heart rate monitoring, GPS, sleep tracking, and 7-day battery life. Water-resistant to 50m. Your complete health companion.", Price = 189.99m, Stock = 52, CategoryId = 5, ImageUrl = "https://images.unsplash.com/photo-1575311373937-040b8e1fd5b6?w=400&h=300&fit=crop", IsActive = true },
                new Product { Id = 12, Name = "Portable Bluetooth Speaker", Description = "360-degree surround sound, 20-hour playtime, IPX7 waterproof rating. The ultimate outdoor audio companion.", Price = 89.99m, Stock = 88, CategoryId = 1, ImageUrl = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=400&h=300&fit=crop", IsActive = true }
            );

            // Seed Admin User (password: Admin@123)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "Admin User",
                    Email = "admin@ecommerce.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    IsAdmin = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
