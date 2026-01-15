using Microsoft.EntityFrameworkCore;
using Projekt_BDwAI_Magazyn.Models;

namespace Projekt_BDwAI_Magazyn.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // upewnia się, że migracje są zastosowane
            await context.Database.MigrateAsync();

            if (await context.Products.AnyAsync())
                return;

            context.Products.AddRange(
                new Product { Name = "Laptop Dell", Sku = "LAP-DELL-001", Price = 4500m, QuantityInStock = 10 },
                new Product { Name = "Monitor LG", Sku = "MON-LG-002", Price = 1200m, QuantityInStock = 15 },
                new Product { Name = "Klawiatura Logitech", Sku = "KEY-LOG-003", Price = 350m, QuantityInStock = 30 }
            );

            await context.SaveChangesAsync();
        }
    }
}