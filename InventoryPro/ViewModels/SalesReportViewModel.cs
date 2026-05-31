using InventoryPro.Models;

namespace InventoryPro.ViewModels
{
    /// <summary>
    /// Used for the sales report page (date filter + results)
    /// </summary>
    public class SalesReportViewModel
    {
        // Filter inputs from the form
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Results
        public List<Sale> Sales { get; set; } = new List<Sale>();

        // Summary totals
        public decimal TotalRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalItemsSold { get; set; }
    }
}