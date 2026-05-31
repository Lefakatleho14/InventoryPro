using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryPro.Models
{
    /// <summary>
    /// Represents a product in the inventory
    /// </summary>
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        [Display(Name = "Qty In Stock")]
        public int QuantityInStock { get; set; } = 0;

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Unit Price (R)")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Reorder level is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative")]
        [Display(Name = "Reorder Level")]
        public int ReorderLevel { get; set; } = 5;

        [StringLength(100)]
        [Display(Name = "Supplier")]
        public string? Supplier { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation property — one product can appear in many sales
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();

        // Computed property — not stored in DB
        [NotMapped]
        public bool IsLowStock => QuantityInStock <= ReorderLevel;

        // Computed property — total stock value for this product
        [NotMapped]
        [Display(Name = "Stock Value (R)")]
        public decimal StockValue => QuantityInStock * UnitPrice;
    }
}