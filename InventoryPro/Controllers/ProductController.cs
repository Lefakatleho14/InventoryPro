using InventoryPro.Data;
using InventoryPro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Controllers
{
    [Authorize] // All actions require login
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── GET: /Product/Index ───────────────────────────────────────────
        // View all products with search/filter
        public async Task<IActionResult> Index(string? searchString, string? stockFilter)
        {
            var products = _context.Products.AsQueryable();

            // Search by name or ID
            if (!string.IsNullOrEmpty(searchString))
            {
                // Try parse as int for ID search
                if (int.TryParse(searchString, out int productId))
                    products = products.Where(p => p.ProductID == productId);
                else
                    products = products.Where(p =>
                        p.ProductName.Contains(searchString) ||
                        (p.Supplier != null && p.Supplier.Contains(searchString)));
            }

            // Filter by stock status
            if (stockFilter == "low")
                products = products.Where(p => p.QuantityInStock <= p.ReorderLevel);
            else if (stockFilter == "ok")
                products = products.Where(p => p.QuantityInStock > p.ReorderLevel);

            ViewBag.SearchString = searchString;
            ViewBag.StockFilter = stockFilter;
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.LowStockCount = await _context.Products
                .CountAsync(p => p.QuantityInStock <= p.ReorderLevel);

            return View(await products.OrderBy(p => p.ProductName).ToListAsync());
        }

        // ── GET: /Product/Details/101 ─────────────────────────────────────
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Sales)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // ── GET: /Product/Create ──────────────────────────────────────────
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult Create()
        {
            return View();
        }

        // ── POST: /Product/Create ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOnly")]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate product name
                bool nameExists = await _context.Products
                    .AnyAsync(p => p.ProductName.ToLower() == product.ProductName.ToLower());

                if (nameExists)
                {
                    ModelState.AddModelError("ProductName",
                        "A product with this name already exists.");
                    return View(product);
                }

                product.LastUpdated = DateTime.Now;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    $"Product '{product.ProductName}' added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // ── GET: /Product/Edit/101 ────────────────────────────────────────
        [Authorize(Policy = "ManagerOnly")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // ── POST: /Product/Edit/101 ───────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOnly")]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.ProductID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate name (exclude current product)
                    bool nameExists = await _context.Products
                        .AnyAsync(p => p.ProductName.ToLower() == product.ProductName.ToLower()
                                    && p.ProductID != id);

                    if (nameExists)
                    {
                        ModelState.AddModelError("ProductName",
                            "A product with this name already exists.");
                        return View(product);
                    }

                    product.LastUpdated = DateTime.Now;
                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] =
                        $"Product '{product.ProductName}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductID))
                        return NotFound();
                    throw;
                }
            }
            return View(product);
        }

        // ── GET: /Product/Delete/101 ──────────────────────────────────────
        [Authorize(Policy = "ManagerOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            // Check if product has sales records
            var salesCount = await _context.Sales
                .CountAsync(s => s.ProductID == id);
            ViewBag.SalesCount = salesCount;

            return View(product);
        }

        // ── POST: /Product/Delete/101 ─────────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Prevent delete if sales records exist
            var salesCount = await _context.Sales
                .CountAsync(s => s.ProductID == id);

            if (salesCount > 0)
            {
                TempData["ErrorMessage"] =
                    $"Cannot delete '{product.ProductName}' — it has {salesCount} " +
                    $"sale record(s). Consider editing the product instead.";
                return RedirectToAction(nameof(Index));
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Product '{product.ProductName}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── Helper ────────────────────────────────────────────────────────
        private bool ProductExists(int id) =>
            _context.Products.Any(p => p.ProductID == id);
    }
}
