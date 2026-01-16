using System.ComponentModel.DataAnnotations;

namespace Projekt_BDwAI_Magazyn.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; } = default!;

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;

        [Range(1, 100000)]
        public int Quantity { get; set; }

        [Range(0, 1000000)]
        public decimal UnitPrice { get; set; }
    }
}