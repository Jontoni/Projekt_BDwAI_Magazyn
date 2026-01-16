using System.ComponentModel.DataAnnotations;

namespace Projekt_BDwAI_Magazyn.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; } = default!;

        [MaxLength(200)]
        public string? Notes { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
