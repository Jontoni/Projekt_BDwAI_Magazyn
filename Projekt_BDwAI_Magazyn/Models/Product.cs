using System.ComponentModel.DataAnnotations;

namespace Projekt_BDwAI_Magazyn.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Sku { get; set; } = string.Empty;

        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Range(0, 100000)]
        public int QuantityInStock { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}