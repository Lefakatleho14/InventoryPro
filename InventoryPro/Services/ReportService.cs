using InventoryPro.Models;
using InventoryPro.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using CsvHelper;
using System.Globalization;

namespace InventoryPro.Services
{
    /// <summary>
    /// Handles PDF and CSV generation for all reports
    /// </summary>
    public class ReportService
    {
        // ── Shared PDF colours & fonts ────────────────────────────────────
        private static readonly BaseColor PrimaryBlue = new BaseColor(26, 115, 232);
        private static readonly BaseColor DarkGray = new BaseColor(52, 58, 64);
        private static readonly BaseColor LightGray = new BaseColor(248, 249, 250);
        private static readonly BaseColor White = BaseColor.WHITE;
        private static readonly BaseColor DangerRed = new BaseColor(220, 53, 69);
        private static readonly BaseColor SuccessGreen = new BaseColor(40, 167, 69);

        // ── PDF: Sales Report ─────────────────────────────────────────────
        public byte[] GenerateSalesReportPdf(SalesReportViewModel model)
        {
            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A4.Rotate(), 30, 30, 40, 30);
            var writer = PdfWriter.GetInstance(doc, ms);

            doc.Open();

            // Header
            AddPdfHeader(doc, "SALES REPORT",
                $"Period: {model.StartDate?.ToString("dd MMM yyyy") ?? "All"} " +
                $"to {model.EndDate?.ToString("dd MMM yyyy") ?? "All"}");

            // Summary box
            var summaryTable = new PdfPTable(3) { WidthPercentage = 100 };
            summaryTable.SetWidths(new float[] { 1f, 1f, 1f });
            summaryTable.SpacingAfter = 15f;

            AddSummaryCell(summaryTable, "Total Revenue",
                $"R {model.TotalRevenue:N2}", SuccessGreen);
            AddSummaryCell(summaryTable, "Total Transactions",
                model.TotalTransactions.ToString(), PrimaryBlue);
            AddSummaryCell(summaryTable, "Total Items Sold",
                model.TotalItemsSold.ToString(), DarkGray);

            doc.Add(summaryTable);

            // Sales table
            var table = new PdfPTable(6) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 0.8f, 1.5f, 2.5f, 0.8f, 1.2f, 1.2f });

            string[] headers = { "Sale ID", "Date & Time", "Product",
                                  "Qty", "Unit Price", "Total" };
            foreach (var h in headers)
                AddTableHeader(table, h);

            foreach (var sale in model.Sales)
            {
                AddTableCell(table, $"#{sale.SaleID}");
                AddTableCell(table, sale.SaleDate.ToString("dd MMM yyyy HH:mm"));
                AddTableCell(table, sale.Product?.ProductName ?? "—");
                AddTableCell(table, sale.QuantitySold.ToString(), center: true);
                AddTableCell(table, $"R {sale.Product?.UnitPrice:N2}", right: true);
                AddTableCell(table, $"R {sale.TotalPrice:N2}", right: true,
                             bold: true, color: SuccessGreen);
            }

            // Total row
            var totalCell = new PdfPCell(new Phrase("TOTAL",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, White)))
            {
                BackgroundColor = DarkGray,
                Colspan = 5,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 6f
            };
            table.AddCell(totalCell);

            var totalValueCell = new PdfPCell(new Phrase(
                $"R {model.TotalRevenue:N2}",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, White)))
            {
                BackgroundColor = SuccessGreen,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 6f
            };
            table.AddCell(totalValueCell);

            doc.Add(table);
            AddPdfFooter(doc);
            doc.Close();

            return ms.ToArray();
        }

        // ── PDF: Inventory Valuation Report ───────────────────────────────
        public byte[] GenerateValuationReportPdf(List<Product> products)
        {
            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A4, 30, 30, 40, 30);
            var writer = PdfWriter.GetInstance(doc, ms);

            doc.Open();

            decimal totalValue = products.Sum(p => p.StockValue);
            int totalQty = products.Sum(p => p.QuantityInStock);

            AddPdfHeader(doc, "INVENTORY VALUATION REPORT",
                $"Generated: {DateTime.Now:dd MMM yyyy HH:mm}");

            // Summary
            var summaryTable = new PdfPTable(3) { WidthPercentage = 100 };
            summaryTable.SetWidths(new float[] { 1f, 1f, 1f });
            summaryTable.SpacingAfter = 15f;

            AddSummaryCell(summaryTable, "Total Products",
                products.Count.ToString(), PrimaryBlue);
            AddSummaryCell(summaryTable, "Total Qty In Stock",
                totalQty.ToString(), DarkGray);
            AddSummaryCell(summaryTable, "Total Stock Value",
                $"R {totalValue:N2}", SuccessGreen);

            doc.Add(summaryTable);

            // Products table
            var table = new PdfPTable(6) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 0.6f, 2.5f, 1.2f, 1.2f, 1.2f, 1.5f });

            string[] headers = { "ID", "Product Name", "Qty In Stock",
                                  "Unit Price", "Stock Value", "Status" };
            foreach (var h in headers)
                AddTableHeader(table, h);

            foreach (var p in products.OrderBy(x => x.ProductName))
            {
                AddTableCell(table, p.ProductID.ToString());
                AddTableCell(table, p.ProductName);
                AddTableCell(table, p.QuantityInStock.ToString(),
                             center: true,
                             color: p.IsLowStock ? DangerRed : null);
                AddTableCell(table, $"R {p.UnitPrice:N2}", right: true);
                AddTableCell(table, $"R {p.StockValue:N2}", right: true, bold: true);

                var statusCell = new PdfPCell(new Phrase(
                    p.IsLowStock ? "LOW STOCK" : "OK",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8,
                        p.IsLowStock ? DangerRed : SuccessGreen)))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Padding = 5f
                };
                table.AddCell(statusCell);
            }

            // Total row
            var blankCell = new PdfPCell(
                new Phrase("TOTAL INVENTORY VALUE",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, White)))
            {
                BackgroundColor = DarkGray,
                Colspan = 4,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 6f
            };
            table.AddCell(blankCell);

            var totalCell = new PdfPCell(
                new Phrase($"R {totalValue:N2}",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, White)))
            {
                BackgroundColor = SuccessGreen,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 6f
            };
            table.AddCell(totalCell);
            table.AddCell(new PdfPCell(new Phrase("")) { BackgroundColor = DarkGray });

            doc.Add(table);
            AddPdfFooter(doc);
            doc.Close();

            return ms.ToArray();
        }

        // ── PDF: Low Stock Report ─────────────────────────────────────────
        public byte[] GenerateLowStockReportPdf(List<Product> lowStockProducts)
        {
            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A4, 30, 30, 40, 30);
            var writer = PdfWriter.GetInstance(doc, ms);

            doc.Open();

            AddPdfHeader(doc, "LOW STOCK REPORT",
                $"Generated: {DateTime.Now:dd MMM yyyy HH:mm} — " +
                $"{lowStockProducts.Count} item(s) need restocking");

            var table = new PdfPTable(6) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 0.6f, 2.5f, 1.2f, 1.2f, 1.5f, 1.5f });

            string[] headers = { "ID", "Product Name", "In Stock",
                                  "Reorder At", "Supplier", "Order Qty" };
            foreach (var h in headers)
                AddTableHeader(table, h);

            foreach (var p in lowStockProducts.OrderBy(x => x.QuantityInStock))
            {
                int recommendedOrder = (p.ReorderLevel * 3) - p.QuantityInStock;

                AddTableCell(table, p.ProductID.ToString());
                AddTableCell(table, p.ProductName);
                AddTableCell(table, p.QuantityInStock.ToString(),
                             center: true, color: DangerRed, bold: true);
                AddTableCell(table, p.ReorderLevel.ToString(), center: true);
                AddTableCell(table, p.Supplier ?? "—");
                AddTableCell(table, recommendedOrder.ToString(),
                             center: true, bold: true, color: PrimaryBlue);
            }

            doc.Add(table);
            AddPdfFooter(doc);
            doc.Close();

            return ms.ToArray();
        }

        // ── CSV: Sales Report ─────────────────────────────────────────────
        public byte[] GenerateSalesReportCsv(SalesReportViewModel model)
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // Write headers
            csv.WriteField("Sale ID");
            csv.WriteField("Date");
            csv.WriteField("Time");
            csv.WriteField("Product ID");
            csv.WriteField("Product Name");
            csv.WriteField("Quantity Sold");
            csv.WriteField("Unit Price (R)");
            csv.WriteField("Total Price (R)");
            csv.WriteField("Served By");
            csv.NextRecord();

            // Write data rows
            foreach (var sale in model.Sales)
            {
                csv.WriteField(sale.SaleID);
                csv.WriteField(sale.SaleDate.ToString("dd/MM/yyyy"));
                csv.WriteField(sale.SaleDate.ToString("HH:mm"));
                csv.WriteField(sale.ProductID);
                csv.WriteField(sale.Product?.ProductName ?? "—");
                csv.WriteField(sale.QuantitySold);
                csv.WriteField(sale.Product?.UnitPrice.ToString("N2") ?? "—");
                csv.WriteField(sale.TotalPrice.ToString("N2"));
                csv.WriteField(sale.User?.FullName ?? "—");
                csv.NextRecord();
            }

            // Summary rows
            csv.NextRecord();
            csv.WriteField("SUMMARY");
            csv.NextRecord();
            csv.WriteField("Total Revenue");
            csv.WriteField($"R {model.TotalRevenue:N2}");
            csv.NextRecord();
            csv.WriteField("Total Transactions");
            csv.WriteField(model.TotalTransactions);
            csv.NextRecord();
            csv.WriteField("Total Items Sold");
            csv.WriteField(model.TotalItemsSold);
            csv.NextRecord();

            writer.Flush();
            return ms.ToArray();
        }

        // ── CSV: Inventory Valuation ──────────────────────────────────────
        public byte[] GenerateValuationCsv(List<Product> products)
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteField("Product ID");
            csv.WriteField("Product Name");
            csv.WriteField("Description");
            csv.WriteField("Qty In Stock");
            csv.WriteField("Unit Price (R)");
            csv.WriteField("Stock Value (R)");
            csv.WriteField("Reorder Level");
            csv.WriteField("Status");
            csv.WriteField("Supplier");
            csv.NextRecord();

            foreach (var p in products.OrderBy(x => x.ProductName))
            {
                csv.WriteField(p.ProductID);
                csv.WriteField(p.ProductName);
                csv.WriteField(p.Description ?? "");
                csv.WriteField(p.QuantityInStock);
                csv.WriteField(p.UnitPrice.ToString("N2"));
                csv.WriteField(p.StockValue.ToString("N2"));
                csv.WriteField(p.ReorderLevel);
                csv.WriteField(p.IsLowStock ? "LOW STOCK" : "OK");
                csv.WriteField(p.Supplier ?? "");
                csv.NextRecord();
            }

            // Total
            csv.NextRecord();
            csv.WriteField("TOTAL INVENTORY VALUE");
            csv.WriteField("");
            csv.WriteField("");
            csv.WriteField(products.Sum(p => p.QuantityInStock));
            csv.WriteField("");
            csv.WriteField($"R {products.Sum(p => p.StockValue):N2}");
            csv.NextRecord();

            writer.Flush();
            return ms.ToArray();
        }

        // ── Private PDF Helper Methods ────────────────────────────────────

        private void AddPdfHeader(Document doc, string title, string subtitle)
        {
            // Company name
            var companyFont = FontFactory.GetFont(
                FontFactory.HELVETICA_BOLD, 18, PrimaryBlue);
            var company = new Paragraph("InventoryPro", companyFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 2f
            };
            doc.Add(company);

            var subCompany = new Paragraph(
                "Durban Small Business Supplies",
                FontFactory.GetFont(FontFactory.HELVETICA, 10, DarkGray))
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10f
            };
            doc.Add(subCompany);

            // Title bar
            var titleTable = new PdfPTable(1) { WidthPercentage = 100 };
            titleTable.SpacingAfter = 15f;
            var titleCell = new PdfPCell(new Phrase(title,
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, White)))
            {
                BackgroundColor = PrimaryBlue,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 10f,
                Border = Rectangle.NO_BORDER
            };
            titleTable.AddCell(titleCell);
            doc.Add(titleTable);

            // Subtitle
            var sub = new Paragraph(subtitle,
                FontFactory.GetFont(FontFactory.HELVETICA, 9, DarkGray))
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10f
            };
            doc.Add(sub);
        }

        private void AddPdfFooter(Document doc)
        {
            doc.Add(new Paragraph(" "));
            var footer = new Paragraph(
                $"Generated on {DateTime.Now:dd MMMM yyyy} at {DateTime.Now:HH:mm} " +
                $"| InventoryPro — CodeCrafters",
                FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8, DarkGray))
            {
                Alignment = Element.ALIGN_CENTER
            };
            doc.Add(footer);
        }

        private void AddTableHeader(PdfPTable table, string text)
        {
            var cell = new PdfPCell(new Phrase(text,
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, White)))
            {
                BackgroundColor = PrimaryBlue,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 6f
            };
            table.AddCell(cell);
        }

        private void AddTableCell(PdfPTable table, string text,
            bool center = false, bool right = false,
            bool bold = false, BaseColor? color = null)
        {
            var font = bold
                ? FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9,
                    color ?? DarkGray)
                : FontFactory.GetFont(FontFactory.HELVETICA, 9,
                    color ?? DarkGray);

            var cell = new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = right ? Element.ALIGN_RIGHT :
                                      center ? Element.ALIGN_CENTER :
                                      Element.ALIGN_LEFT,
                Padding = 5f
            };
            table.AddCell(cell);
        }

        private void AddSummaryCell(PdfPTable table, string label,
            string value, BaseColor bgColor)
        {
            var inner = new PdfPTable(1);

            var labelCell = new PdfPCell(new Phrase(label,
                FontFactory.GetFont(FontFactory.HELVETICA, 9, White)))
            {
                BackgroundColor = bgColor,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Border = Rectangle.NO_BORDER,
                Padding = 4f
            };

            var valueCell = new PdfPCell(new Phrase(value,
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, White)))
            {
                BackgroundColor = bgColor,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Border = Rectangle.NO_BORDER,
                Padding = 6f
            };

            inner.AddCell(labelCell);
            inner.AddCell(valueCell);

            var outer = new PdfPCell(inner)
            {
                Padding = 0f,
                Border = Rectangle.NO_BORDER
            };
            table.AddCell(outer);
        }
    }
}