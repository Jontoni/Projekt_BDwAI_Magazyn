using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Projekt_BDwAI_Magazyn.ViewModels
{
    public class OrderCreateVm
    {
        [StringLength(200)]
        public string? Notes { get; set; }

        public List<OrderItemCreateVm> Items { get; set; } = new();
        public List<SelectListItem> Products { get; set; } = new();
    }

    public class OrderItemCreateVm
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, 100000)]
        public int Quantity { get; set; } = 1;
    }
}
