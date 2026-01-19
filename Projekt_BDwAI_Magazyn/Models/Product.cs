using System.ComponentModel.DataAnnotations;

namespace Projekt_BDwAI_Magazyn.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Sku { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int QuantityInStock { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relacja do OrderItem (opcjonalnie)
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}