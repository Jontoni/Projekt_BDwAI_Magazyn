using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt_BDwAI_Magazyn.Data;
using Projekt_BDwAI_Magazyn.Models;

namespace Projekt_BDwAI_Magazyn.Controllers.Api
{
    [ApiController]
    [Route("api/products")]
    public class ProductsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // R - Read all
        // GET: /api/products
        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetAll()
        {
            var products = await _context.Products.AsNoTracking().ToListAsync();
            return Ok(products);
        }

        // R - Read by id
        // GET: /api/products/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var product = await _context.Products.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();
            return Ok(product);
        }

        // C - Create
        // POST: /api/products
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Product>> Create([FromBody] Product product)
        {
            // DB nada Id, a CreatedAt ustawiamy "teraz"
            product.Id = 0;
            product.CreatedAt = DateTime.Now;

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        // U - Update
        // PUT: /api/products/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            if (id != product.Id)
                return BadRequest("Id w URL i w body muszą być takie same.");

            var exists = await _context.Products.AnyAsync(p => p.Id == id);
            if (!exists) return NotFound();

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            // (opcjonalnie) nie zmieniamy CreatedAt przez API
            _context.Entry(product).Property(p => p.CreatedAt).IsModified = false;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // D - Delete
        // DELETE: /api/products/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
