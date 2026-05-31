using InventoryPro.Data;
using InventoryPro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── GET: /Sales/Index ─────────────────────────────────────────────
        // Sales history with date filter
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate,
                                               string? searchProduct)
        {
            var sales = _context.Sales
                .Include(s => s.Product)
                .Include(s => s.User)
                .AsQueryable();

            // Filter by date range
            if (startDate.HasValue)
                sales = sales.Where(s => s.SaleDate.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                sales = sales.Where(s => s.SaleDate.Date <= endDate.Value.Date);

            // Filter by product name
            if (!string.IsNullOrEmpty(searchProduct))
                sales = sales.Where(s => s.Product != null &&
                    s.Product.ProductName.Contains(searchProduct));

            var salesList = await sales.OrderByDescending(s => s.SaleDate).ToListAsync();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.SearchProduct = searchProduct;
            ViewBag.TotalRevenue = salesList.Sum(s => s.TotalPrice);
            ViewBag.TotalItems = salesList.Sum(s => s.QuantitySold);

            return View(salesList);
        }

        // ── GET: /Sales/Create ────────────────────────────────────────────
        // Record a new sale
        public async Task<IActionResult> Create(int? productId)
        {
            // Load all products for the dropdown
            var products = await _context.Products
                .Where(p => p.QuantityInStock > 0)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            ViewBag.Products = products;

            // Pre-select product if passed via query string
            if (productId.HasValue)
            {
                var selectedProduct = await _context.Products.FindAsync(productId.Value);
                ViewBag.SelectedProduct = selectedProduct;
            }

            return View();
        }

        // ── POST: /Sales/Create ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, int quantitySold)
        {
            // Reload products for dropdown in case of error
            var products = await _context.Products
                .Where(p => p.QuantityInStock > 0)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
            ViewBag.Products = products;

            // ── Validation ────────────────────────────────────────────────
            if (productId <= 0)
            {
                ModelState.AddModelError("", "Please select a product.");
                return View();
            }

            if (quantitySold <= 0)
            {
                ModelState.AddModelError("", "Quantity sold must be at least 1.");
                return View();
            }

            // Get product from DB
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                ModelState.AddModelError("", "Product not found.");
                return View();
            }

            // Check sufficient stock
            if (quantitySold > product.QuantityInStock)
            {
                ModelState.AddModelError("",
                    $"Insufficient stock. Only {product.QuantityInStock} unit(s) available.");
                ViewBag.SelectedProduct = product;
                return View();
            }

            // Get current user ID from claims
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (userIdClaim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim);

            // ── Create Sale record ────────────────────────────────────────
            var sale = new Sale
            {
                ProductID = productId,
                QuantitySold = quantitySold,
                TotalPrice = quantitySold * product.UnitPrice,
                UserID = userId,
                SaleDate = DateTime.Now
            };

            // ── Deduct stock ──────────────────────────────────────────────
            product.QuantityInStock -= quantitySold;
            product.LastUpdated = DateTime.Now;

            // ── Save both changes in one transaction ──────────────────────
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Sales.Add(sale);
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("",
                    "An error occurred while recording the sale. Please try again.");
                return View();
            }

            TempData["SuccessMessage"] = "Sale recorded successfully!";

            // Redirect to receipt page
            return RedirectToAction(nameof(Receipt), new { id = sale.SaleID });
        }

        // ── GET: /Sales/Receipt/1001 ──────────────────────────────────────
        public async Task<IActionResult> Receipt(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Product)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SaleID == id);

            if (sale == null) return NotFound();

            return View(sale);
        }

        // ── GET: /Sales/Details/1001 ──────────────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Product)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SaleID == id);

            if (sale == null) return NotFound();

            return View(sale);
        }

        // ── GET: /Sales/GetProductDetails/101 ────────────────────────────
        // AJAX endpoint — returns product price + stock as JSON
        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                unitPrice = product.UnitPrice,
                quantityInStock = product.QuantityInStock,
                productName = product.ProductName,
                isLowStock = product.IsLowStock
            });
        }
    }
}