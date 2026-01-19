using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity; 

namespace Projekt_BDwAI_Magazyn.Models
{
    public class Order
    {
        public int Id { get; set; } 

        [Required]
        public required string UserId { get; set; } 

        [Required]
        [Display(Name = "Data zamówienia")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "Status")]
        public OrderStatus Status { get; set; } = OrderStatus.New;

     
        public required IdentityUser User { get; set; } 
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    
    public enum OrderStatus
    {
        [Display(Name = "Nowe")]
        New,

        [Display(Name = "W trakcie")]
        InProgress,

        [Display(Name = "Zrealizowane")]
        Completed,

        [Display(Name = "Anulowane")]
        Cancelled
    }
}