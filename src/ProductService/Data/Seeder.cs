using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Products.AnyAsync())
        {
            return;
        }

        var products = new List<Product>
        {
            new() { Name = "Classic White T-Shirt",       Description = "Premium cotton crew-neck tee",          Price = 19.99m,  Stock = 150 },
            new() { Name = "Slim Fit Blue Jeans",         Description = "Stretch denim, mid-rise",               Price = 59.99m,  Stock = 80  },
            new() { Name = "Floral Summer Dress",         Description = "Lightweight chiffon, knee-length",      Price = 44.99m,  Stock = 60  },
            new() { Name = "Graphic Logo Tee",            Description = "100% organic cotton, relaxed fit",      Price = 24.99m,  Stock = 200 },
            new() { Name = "Striped Polo Shirt",          Description = "Pique cotton, classic fit",             Price = 34.99m,  Stock = 95  },
            new() { Name = "Leather Bomber Jacket",       Description = "Genuine leather, zip closure",          Price = 199.99m, Stock = 25  },
            new() { Name = "Down Puffer Jacket",          Description = "Recycled fill, water-resistant shell",  Price = 149.99m, Stock = 40  },
            new() { Name = "Denim Jacket",                Description = "Classic washed denim, button front",    Price = 89.99m,  Stock = 55  },
            new() { Name = "Trench Coat",                 Description = "Double-breasted, belted",               Price = 179.99m, Stock = 20  },
            new() { Name = "Fleece Zip-Up Hoodie",        Description = "Sherpa-lined fleece, kangaroo pocket",  Price = 69.99m,  Stock = 110 },
            new() { Name = "Air Mesh Running Sneakers",   Description = "Breathable upper, cushioned sole",      Price = 89.99m,  Stock = 70  },
            new() { Name = "Classic Canvas Low-Tops",     Description = "Vulcanised rubber sole, lace-up",       Price = 49.99m,  Stock = 90  },
            new() { Name = "Leather Oxford Shoes",        Description = "Full-grain leather, Goodyear welt",     Price = 129.99m, Stock = 30  },
            new() { Name = "High-Top Basketball Shoes",   Description = "Ankle support, non-slip outsole",       Price = 99.99m,  Stock = 45  },
            new() { Name = "Trail Running Shoes",         Description = "Gore-Tex lining, aggressive tread",     Price = 119.99m, Stock = 35  },
            new() { Name = "Chronograph Watch",           Description = "316L steel case, sapphire crystal",     Price = 249.99m, Stock = 15  },
            new() { Name = "Minimalist Leather Watch",    Description = "Italian leather strap, Japanese quartz",Price = 129.99m, Stock = 22  },
            new() { Name = "Woven Canvas Belt",           Description = "Military-style, auto-ratchet buckle",   Price = 29.99m,  Stock = 120 },
            new() { Name = "Wool Beanie Hat",             Description = "Merino wool, ribbed knit",              Price = 19.99m,  Stock = 140 },
            new() { Name = "Sunglasses UV400",            Description = "Polarised lenses, lightweight frame",   Price = 39.99m,  Stock = 75  },
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
