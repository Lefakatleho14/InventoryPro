using InventoryPro.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Controllers
{
    [Authorize] // All dashboard actions require login
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── GET: /Dashboard/Index ─────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            // ── 1. Low stock products (below reorder level) ───────────────
            var lowStockProducts = await _context.Products
                .Where(p => p.QuantityInStock <= p.ReorderLevel)
                .OrderBy(p => p.QuantityInStock)
                .ToListAsync();

            // ── 2. Total inventory valuation ──────────────────────────────
            var totalValuation = await _context.Products
                .SumAsync(p => p.QuantityInStock * p.UnitPrice);

            // ── 3. Today's sales total ────────────────────────────────────
            var today = DateTime.Today;
            var todaysSalesTotal = await _context.Sales
                .Where(s => s.SaleDate.Date == today)
                .SumAsync(s => (decimal?)s.TotalPrice) ?? 0;

            // ── 4. Today's transaction count ──────────────────────────────
            var todaysTransactionCount = await _context.Sales
                .Where(s => s.SaleDate.Date == today)
                .CountAsync();

            // ── 5. Total products count ───────────────────────────────────
            var totalProducts = await _context.Products.CountAsync();

            // ── 6. Recent sales (last 5) ──────────────────────────────────
            var recentSales = await _context.Sales
                .Include(s => s.Product)
                .Include(s => s.User)
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .ToListAsync();

            // ── 7. Pass data to view via ViewBag ──────────────────────────
            ViewBag.LowStockProducts = lowStockProducts;
            ViewBag.LowStockCount = lowStockProducts.Count;
            ViewBag.TotalValuation = totalValuation;
            ViewBag.TodaysSalesTotal = todaysSalesTotal;
            ViewBag.TodaysTransactionCount = todaysTransactionCount;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.RecentSales = recentSales;

            return View();
        }
    }
}