using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projekt_BDwAI_Magazyn.Data;
using Projekt_BDwAI_Magazyn.Models;
using Projekt_BDwAI_Magazyn.ViewModels;

namespace Projekt_BDwAI_Magazyn.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Orders
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var isAdmin = User.IsInRole("Admin");

            var q = _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.Id);

            var list = isAdmin
                ? await q.ToListAsync()
                : await q.Where(o => o.UserId == userId).ToListAsync();

            return View(list);
        }

        // GET: /Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;
            var isAdmin = User.IsInRole("Admin");

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            if (!isAdmin && order.UserId != userId) return Forbid();

            return View(order);
        }

        // GET: /Orders/Create
        public async Task<IActionResult> Create()
        {
            var vm = new OrderCreateVm();
            vm.Items.Add(new OrderItemCreateVm());
            await FillProducts(vm);
            return View(vm);
        }

        // POST: /Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateVm vm)
        {
            vm.Items = vm.Items.Where(x => x.ProductId != 0 && x.Quantity > 0).ToList();

            if (vm.Items.Count == 0)
                ModelState.AddModelError("", "Dodaj co najmniej jedną pozycję zamówienia.");

            vm.Items = vm.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new OrderItemCreateVm { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();

            if (!ModelState.IsValid)
            {
                await FillProducts(vm);
                return View(vm);
            }

            var userId = _userManager.GetUserId(User)!;

            var productIds = vm.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var item in vm.Items)
                if (!products.ContainsKey(item.ProductId))
                    ModelState.AddModelError("", $"Produkt o ID={item.ProductId} nie istnieje.");

            if (!ModelState.IsValid)
            {
                await FillProducts(vm);
                return View(vm);
            }

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in vm.Items)
                {
                    var p = products[item.ProductId];
                    if (p.QuantityInStock < item.Quantity)
                        ModelState.AddModelError("", $"Brak stanu: {p.Name} (dostępne {p.QuantityInStock}, żądane {item.Quantity})");
                }

                if (!ModelState.IsValid)
                {
                    await tx.RollbackAsync();
                    await FillProducts(vm);
                    return View(vm);
                }

                foreach (var item in vm.Items)
                {
                    var p = products[item.ProductId];
                    p.QuantityInStock -= item.Quantity;
                }

                var order = new Order
                {
                    UserId = userId,
                    Notes = vm.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                foreach (var item in vm.Items)
                {
                    var p = products[item.ProductId];
                    order.Items.Add(new OrderItem
                    {
                        ProductId = p.Id,
                        Quantity = item.Quantity,
                        UnitPrice = p.Price
                    });
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }
            catch
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", "Wystąpił błąd podczas zapisu zamówienia.");
                await FillProducts(vm);
                return View(vm);
            }
        }

        private async Task FillProducts(OrderCreateVm vm)
        {
            var products = await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name, p.Sku, p.QuantityInStock })
                .ToListAsync();

            vm.Products = products.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Name} ({p.Sku}) - stan: {p.QuantityInStock}"
            }).ToList();
        }
    }
}
