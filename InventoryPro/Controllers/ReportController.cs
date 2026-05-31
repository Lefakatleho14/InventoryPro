using InventoryPro.Data;
using InventoryPro.Services;
using InventoryPro.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Controllers
{
    [Authorize(Policy = "ManagerOnly")] // Reports are Manager-only
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ReportService _reportService;

        public ReportController(ApplicationDbContext context,
                                ReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        // ── GET: /Report/Sales ────────────────────────────────────────────
        public async Task<IActionResult> Sales(
            DateTime? startDate, DateTime? endDate)
        {
            var model = new SalesReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var sales = _context.Sales
                .Include(s => s.Product)
                .Include(s => s.User)
                .AsQueryable();

            if (startDate.HasValue)
                sales = sales.Where(s => s.SaleDate.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                sales = sales.Where(s => s.SaleDate.Date <= endDate.Value.Date);

            model.Sales = await sales.OrderByDescending(s => s.SaleDate).ToListAsync();
            model.TotalRevenue = model.Sales.Sum(s => s.TotalPrice);
            model.TotalTransactions = model.Sales.Count;
            model.TotalItemsSold = model.Sales.Sum(s => s.QuantitySold);

            return View(model);
        }

        // ── GET: /Report/ExportSalesPdf ───────────────────────────────────
        public async Task<IActionResult> ExportSalesPdf(
            DateTime? startDate, DateTime? endDate)
        {
            var model = new SalesReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var sales = _context.Sales
                .Include(s => s.Product)
                .Include(s => s.User)
                .AsQueryable();

            if (startDate.HasValue)
                sales = sales.Where(s => s.SaleDate.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                sales = sales.Where(s => s.SaleDate.Date <= endDate.Value.Date);

            model.Sales = await sales.OrderByDescending(s => s.SaleDate).ToListAsync();
            model.TotalRevenue = model.Sales.Sum(s => s.TotalPrice);
            model.TotalTransactions = model.Sales.Count;
            model.TotalItemsSold = model.Sales.Sum(s => s.QuantitySold);

            var pdfBytes = _reportService.GenerateSalesReportPdf(model);
            var fileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        // ── GET: /Report/ExportSalesCsv ───────────────────────────────────
        public async Task<IActionResult> ExportSalesCsv(
            DateTime? startDate, DateTime? endDate)
        {
            var model = new SalesReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var sales = _context.Sales
                .Include(s => s.Product)
                .Include(s => s.User)
                .AsQueryable();

            if (startDate.HasValue)
                sales = sales.Where(s => s.SaleDate.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                sales = sales.Where(s => s.SaleDate.Date <= endDate.Value.Date);

            model.Sales = await sales.OrderByDescending(s => s.SaleDate).ToListAsync();
            model.TotalRevenue = model.Sales.Sum(s => s.TotalPrice);
            model.TotalTransactions = model.Sales.Count;
            model.TotalItemsSold = model.Sales.Sum(s => s.QuantitySold);

            var csvBytes = _reportService.GenerateSalesReportCsv(model);
            var fileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmm}.csv";

            return File(csvBytes, "text/csv", fileName);
        }

        // ── GET: /Report/Valuation ────────────────────────────────────────
        public async Task<IActionResult> Valuation()
        {
            var products = await _context.Products
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            ViewBag.TotalValue = products.Sum(p => p.StockValue);
            ViewBag.TotalQty = products.Sum(p => p.QuantityInStock);
            ViewBag.LowStockCount = products.Count(p => p.IsLowStock);

            return View(products);
        }

        // ── GET: /Report/ExportValuationPdf ──────────────────────────────
        public async Task<IActionResult> ExportValuationPdf()
        {
            var products = await _context.Products
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            var pdfBytes = _reportService.GenerateValuationReportPdf(products);
            var fileName = $"InventoryValuation_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        // ── GET: /Report/ExportValuationCsv ──────────────────────────────
        public async Task<IActionResult> ExportValuationCsv()
        {
            var products = await _context.Products
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            var csvBytes = _reportService.GenerateValuationCsv(products);
            var fileName = $"InventoryValuation_{DateTime.Now:yyyyMMdd_HHmm}.csv";

            return File(csvBytes, "text/csv", fileName);
        }

        // ── GET: /Report/LowStock ─────────────────────────────────────────
        public async Task<IActionResult> LowStock()
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.QuantityInStock <= p.ReorderLevel)
                .OrderBy(p => p.QuantityInStock)
                .ToListAsync();

            ViewBag.TotalLowStock = lowStockProducts.Count;

            return View(lowStockProducts);
        }

        // ── GET: /Report/ExportLowStockPdf ────────────────────────────────
        public async Task<IActionResult> ExportLowStockPdf()
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.QuantityInStock <= p.ReorderLevel)
                .OrderBy(p => p.QuantityInStock)
                .ToListAsync();

            var pdfBytes = _reportService.GenerateLowStockReportPdf(lowStockProducts);
            var fileName = $"LowStockReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}