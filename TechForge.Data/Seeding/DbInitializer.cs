using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TechForge.Core.Entities;

namespace TechForge.Data.Seeding;

public static class DbInitializer
{
    public const string AdminRole = "Admin";
    public const string CustomerRole = "Customer";

    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        var adminId = await SeedAdminAsync(userManager, configuration);
        var demoUserIds = await SeedDemoUsersAsync(userManager);
        await SeedCategoriesAsync(context);
        await SeedProductsAsync(context);
        await SeedReviewsAsync(context, demoUserIds);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { AdminRole, CustomerRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task<string> SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var email = configuration["AdminUser:Email"] ?? "admin@techforge.local";
        var password = configuration["AdminUser:Password"] ?? "Admin#12345";
        var firstName = configuration["AdminUser:FirstName"] ?? "Admin";
        var lastName = configuration["AdminUser:LastName"] ?? "TechForge";

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                Address = "1 Forge Street, Tech City",
                CreatedOn = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, AdminRole);
            }
        }

        return admin.Id;
    }

    private static async Task<List<string>> SeedDemoUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var demoUsers = new[]
        {
            new { Email = "alex@demo.local", First = "Alex", Last = "Rivera", Address = "12 Pixel Ave" },
            new { Email = "jordan@demo.local", First = "Jordan", Last = "Lee", Address = "44 Cache Lane" },
            new { Email = "sam@demo.local", First = "Sam", Last = "Patel", Address = "8 Render Road" }
        };

        var ids = new List<string>();

        foreach (var u in demoUsers)
        {
            var existing = await userManager.FindByEmailAsync(u.Email);
            if (existing is null)
            {
                existing = new ApplicationUser
                {
                    UserName = u.Email,
                    Email = u.Email,
                    EmailConfirmed = true,
                    FirstName = u.First,
                    LastName = u.Last,
                    Address = u.Address,
                    CreatedOn = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(existing, "Demo#12345");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(existing, CustomerRole);
                }
            }

            ids.Add(existing.Id);
        }

        return ids;
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var categories = new List<Category>
        {
            new() { Name = "Graphics Cards", Description = "High-performance GPUs for gaming, creation and AI." },
            new() { Name = "Processors", Description = "Desktop and workstation CPUs from Intel and AMD." },
            new() { Name = "Motherboards", Description = "ATX, micro-ATX and mini-ITX boards for every build." },
            new() { Name = "Memory", Description = "DDR4 and DDR5 kits for speed and capacity." },
            new() { Name = "Storage", Description = "NVMe SSDs and high-capacity drives." },
            new() { Name = "Peripherals", Description = "Keyboards, mice, headsets and more." },
            new() { Name = "Laptops", Description = "Gaming and productivity notebooks." },
            new() { Name = "Monitors", Description = "High refresh-rate and ultrawide displays." }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(ApplicationDbContext context)
    {
        if (await context.Products.AnyAsync())
        {
            return;
        }

        var byName = await context.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);

        var products = new List<Product>
        {
            new()
            {
                Name = "ForgeVision RTX 4080 Super", Brand = "NVIDIA",
                Description = "Flagship 16GB GDDR6X graphics card built for 4K gaming and ray tracing.",
                Price = 1199.99m, StockQuantity = 25, CategoryId = byName["Graphics Cards"],
                Rating = 0, IsFeatured = true, ReleaseDate = new DateTime(2024, 1, 31),
                Specifications = "16GB GDDR6X; 2550 MHz boost; 320W TDP; PCIe 4.0",
                ImageUrl = "https://images.unsplash.com/photo-1624701928517-44c8ac49d93c?w=600&h=450&fit=crop"
            },
            new()
            {
                Name = "Radeon RX 7900 XTX", Brand = "AMD",
                Description = "24GB enthusiast GPU with RDNA 3 architecture for high-fps gaming.",
                Price = 949.99m, StockQuantity = 18, CategoryId = byName["Graphics Cards"],
                Rating = 0, IsFeatured = true, ReleaseDate = new DateTime(2022, 12, 13),
                Specifications = "24GB GDDR6; 2500 MHz; 355W TDP; PCIe 4.0",
                ImageUrl = "https://images.unsplash.com/photo-1591488320449-011701bb6704?w=600&h=450&fit=crop"
            },
            new()
            {
                Name = "Core i9-14900K", Brand = "Intel",
                Description = "24-core flagship desktop processor for gaming and content creation.",
                Price = 569.99m, StockQuantity = 40, CategoryId = byName["Processors"],
                Rating = 0, IsFeatured = true, ReleaseDate = new DateTime(2023, 10, 17),
                Specifications = "24 cores / 32 threads; up to 6.0 GHz; LGA1700; 125W base",
                ImageUrl = "https://images.unsplash.com/photo-1625315714730-d0830cd368bd?w=600&h=450&fit=crop"
            },
            new()
            {
                Name = "Ryzen 9 7950X", Brand = "AMD",
                Description = "16-core Zen 4 powerhouse for demanding multi-threaded workloads.",
                Price = 549.99m, StockQuantity = 35, CategoryId = byName["Processors"],
                Rating = 0, IsFeatured = false, ReleaseDate = new DateTime(2022, 9, 27),
                Specifications = "16 cores / 32 threads; up to 5.7 GHz; AM5; 170W TDP",
                ImageUrl = "https://images.unsplash.com/photo-1591799264318-7e6ef8ddb7ea?w=600&h=450&fit=crop"
            },
            new()
            {
                Name = "ROG Strix Z790-E", Brand = "ASUS",
                Description = "Premium LGA1700 motherboard with DDR5, PCIe 5.0 and Wi-Fi 6E.",
                Price = 499.99m, StockQuantity = 22, CategoryId = byName["Motherboards"],
                Rating = 0, IsFeatured = false, ReleaseDate = new DateTime(2022, 11, 3),
                Specifications = "ATX; LGA1700; 4x DDR5; PCIe 5.0; Wi-Fi 6E",
                ImageUrl = "https://images.unsplash.com/photo-1712701815718-29f5fe510c0e?w=600&h=450&fit=crop"
            },
            new()
            {
                Name = "Vengeance DDR5 32GB 6000MHz", Brand = "Corsair",
                Description = "32GB (2x16GB) DDR5 kit tuned for AMD and Intel platforms.",
                Price = 124.99m, StockQuantity = 80, CategoryId = byName["Memory"],
                Rating = 0, IsFeatured = true, ReleaseDate = new DateTime(2023, 3, 15),
                Specifications = "2x16GB; DDR5-6000; CL30; 1.35V",
                ImageUrl = "https://images.unsplash.com/photo-1592664474505-51c549ad15c5?w=600&h=450&fit=crop"
            },
            new()
            {
                Name = "990 PRO 2TB NVMe SSD", Brand = "Samsung",
                Description = "PCIe 4.0 NVMe SSD with blazing sequential read speeds.",
                Price = 169.99m, StockQuantity = 60, CategoryId = byName["Storage"],
                Rating = 0, IsFeatured = false, ReleaseDate = new DateTime(2023, 1, 10),
                Specifications = "2TB; PCIe 4.0; up to 7450 MB/s read; M.2 2280",
                ImageUrl = "https://images.unsplash.com/photo-1628557118391-56cd62c9f2cb?w=600&h=450&fit=crop"
            },
            new()
            {
                Name = "Apex Pro TKL Keyboard", Brand = "SteelSeries",
                Description = "Adjustable mechanical keyboard with OLED display.",
                Price = 189.99m, StockQuantity = 50, CategoryId = byName["Peripherals"],
                Rating = 0, IsFeatured = true, ReleaseDate = new DateTime(2023, 6, 1),
                Specifications = "TKL; OmniPoint switches; per-key RGB; OLED smart display",
                ImageUrl = "https://images.unsplash.com/photo-1635987391914-cb84b567e68f?w=600&h=450&fit=crop"
            },
            new()
            {
                Name = "G Pro X Superlight 2", Brand = "Logitech",
                Description = "Ultra-light wireless gaming mouse for esports.",
                Price = 159.99m, StockQuantity = 70, CategoryId = byName["Peripherals"],
                Rating = 0, IsFeatured = false, ReleaseDate = new DateTime(2023, 9, 21),
                Specifications = "60g; HERO 2 sensor; up to 95h battery; LIGHTSPEED",
                ImageUrl = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?w=600&h=450&fit=crop"
            },
            new()
            {
                Name = "Blade 16 Gaming Laptop", Brand = "Razer",
                Description = "16-inch gaming laptop with RTX graphics and a dual-mode display.",
                Price = 2799.99m, StockQuantity = 12, CategoryId = byName["Laptops"],
                Rating = 0, IsFeatured = true, ReleaseDate = new DateTime(2024, 2, 14),
                Specifications = "16\" dual-mode; Core i9; RTX 4080; 32GB; 1TB SSD",
                ImageUrl = "https://images.unsplash.com/photo-1630794180018-433d915c34ac?w=600&h=450&fit=crop"
            },
            new()
            {
                Name = "Odyssey G9 49\" Ultrawide", Brand = "Samsung",
                Description = "49-inch curved 240Hz ultrawide gaming monitor.",
                Price = 1299.99m, StockQuantity = 15, CategoryId = byName["Monitors"],
                Rating = 0, IsFeatured = false, ReleaseDate = new DateTime(2023, 5, 5),
                Specifications = "49\"; 5120x1440; 240Hz; 1ms; 1000R curve",
                ImageUrl = "https://images.unsplash.com/photo-1593305841991-05c297ba4575?w=600&h=450&fit=crop"
            },
            new()
            {
                Name = "27\" 4K 144Hz IPS Monitor", Brand = "LG",
                Description = "Sharp 4K IPS display with high refresh rate for gaming and work.",
                Price = 649.99m, StockQuantity = 28, CategoryId = byName["Monitors"],
                Rating = 0, IsFeatured = true, ReleaseDate = new DateTime(2023, 8, 19),
                Specifications = "27\"; 3840x2160; 144Hz; IPS; HDR400",
                ImageUrl = "https://images.unsplash.com/photo-1534423861386-85a16f5d13fd?w=600&h=450&fit=crop"
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }

    private static async Task SeedReviewsAsync(ApplicationDbContext context, List<string> userIds)
    {
        if (await context.Reviews.AnyAsync() || userIds.Count == 0)
        {
            return;
        }

        var products = await context.Products.OrderBy(p => p.Id).Take(6).ToListAsync();
        if (products.Count == 0)
        {
            return;
        }

        var comments = new[]
        {
            "Absolutely fantastic, exceeded my expectations.",
            "Great value for the performance you get.",
            "Solid build quality, would buy again.",
            "Works perfectly, fast shipping too.",
            "Good product but runs a little warm under load."
        };

        var ratings = new[] { 5, 4, 5, 4, 3 };
        var random = new Random(42);
        var reviews = new List<Review>();

        foreach (var product in products)
        {
            var count = random.Next(2, 4);
            for (var i = 0; i < count; i++)
            {
                var idx = random.Next(comments.Length);
                reviews.Add(new Review
                {
                    ProductId = product.Id,
                    UserId = userIds[random.Next(userIds.Count)],
                    Rating = ratings[idx],
                    Comment = comments[idx],
                    CreatedOn = DateTime.UtcNow.AddDays(-random.Next(1, 60))
                });
            }
        }

        await context.Reviews.AddRangeAsync(reviews);
        await context.SaveChangesAsync();

        foreach (var product in products)
        {
            var productReviews = reviews.Where(r => r.ProductId == product.Id).ToList();
            if (productReviews.Count > 0)
            {
                product.Rating = Math.Round(productReviews.Average(r => r.Rating), 1);
            }
        }

        await context.SaveChangesAsync();
    }
}
