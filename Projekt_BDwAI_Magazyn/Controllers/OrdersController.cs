using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt_BDwAI_Magazyn.Data;
using Projekt_BDwAI_Magazyn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Projekt_BDwAI_Magazyn.Controllers
{
    [Authorize] // Wymaga zalogowania do wszystkich akcji
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders
        public async Task<IActionResult> Index(string statusFilter = "")  
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            IQueryable<Order> ordersQuery = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product);

            // FILTROWANIE PO STATUSIE ← DODAJ TEN BLOK
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<OrderStatus>(statusFilter, out var status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status);
            }

            // Jeśli nie jest adminem, pokazuje tylko swoje zamówienia
            if (!isAdmin)
            {
                ordersQuery = ordersQuery.Where(o => o.UserId == userId);
            }

            // Przekazanie danych do widoku 
            ViewBag.Statuses = Enum.GetValues<OrderStatus>();
            ViewBag.CurrentFilter = statusFilter;

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Set<Order>()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Sprawdź czy użytkownik ma dostęp (admin lub właściciel)
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && order.UserId != userId)
            {
                return Forbid(); 
            }

            return View(order);
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            var products = await _context.Products
                .Where(p => p.QuantityInStock > 0)
                .ToListAsync();

            ViewBag.Products = products;
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order, int[] productIds, int[] quantities)
        {
            if (productIds == null || quantities == null || productIds.Length == 0)
            {
                ModelState.AddModelError("", "Wybierz co najmniej jeden produkt.");
                ViewBag.Products = await _context.Products.Where(p => p.QuantityInStock > 0).ToListAsync();
                return View(order);
            }

            
            for (int i = 0; i < productIds.Length; i++)
            {
                if (quantities[i] <= 0)
                {
                    ModelState.AddModelError("", "Ilość musi być większa niż 0.");
                    ViewBag.Products = await _context.Products.Where(p => p.QuantityInStock > 0).ToListAsync();
                    return View(order);
                }
            }

            // Pobierz aktualnego użytkownika
            var userId = _userManager.GetUserId(User);
            order.UserId = userId!;
            order.OrderDate = DateTime.Now;
            order.Status = OrderStatus.New; 

            // Sprawdź dostępność produktów i odejmij z magazynu
            var orderItems = new List<OrderItem>();
            for (int i = 0; i < productIds.Length; i++)
            {
                var product = await _context.Products.FindAsync(productIds[i]);
                if (product == null)
                {
                    ModelState.AddModelError("", $"Produkt o ID {productIds[i]} nie istnieje.");
                    ViewBag.Products = await _context.Products.Where(p => p.QuantityInStock > 0).ToListAsync();
                    return View(order);
                }

                if (product.QuantityInStock < quantities[i])
                {
                    ModelState.AddModelError("", $"Niewystarczająca ilość produktu: {product.Name}. Dostępne: {product.QuantityInStock}");
                    ViewBag.Products = await _context.Products.Where(p => p.QuantityInStock > 0).ToListAsync();
                    return View(order);
                }

                // Odejmij z magazynu
                product.QuantityInStock -= quantities[i];

                // Dodaj pozycję zamówienia
                var orderItem = new OrderItem
                {
                    ProductId = productIds[i],
                    Quantity = quantities[i],
                    UnitPrice = product.Price
                };
                orderItems.Add(orderItem);
            }

            // Dodaj pozycje do zamówienia
            order.OrderItems = orderItems;

            // Zapisz zamówienie
            _context.Add(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Zamówienie zostało złożone pomyślnie!";
            return RedirectToAction(nameof(Index));
        }

        // ★★★★ DODAJ TE METODY ★★★★

        // POST: Orders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.Set<Order>()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Sprawdź czy można anulować (tylko zamówienia "Nowe")
            if (order.Status != OrderStatus.New)
            {
                TempData["ErrorMessage"] = "Można anulować tylko zamówienia ze statusem 'Nowe'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Zwróć produkty do magazynu
            foreach (var item in order.OrderItems)
            {
                if (item.Product is Product product)
                {
                    product.QuantityInStock += item.Quantity;
                }
            }

            // Zmień status na anulowany
            order.Status = OrderStatus.Cancelled;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Zamówienie zostało anulowane. Produkty zwrócone do magazynu.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Orders/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Complete(int id)
        {
            var order = await _context.Set<Order>()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Można oznaczyć jako zrealizowane tylko "Nowe" zamówienia
            if (order.Status != OrderStatus.New)
            {
                TempData["ErrorMessage"] = "Tylko zamówienia 'Nowe' można oznaczyć jako zrealizowane.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = OrderStatus.Completed;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Zamówienie zostało oznaczone jako zrealizowane.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}