using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryPro.Models
{
    /// <summary>
    /// Represents a recorded sale transaction
    /// </summary>
    public class Sale
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SaleID { get; set; }

        [Display(Name = "Sale Date")]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Quantity sold is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity sold must be at least 1")]
        [Display(Name = "Qty Sold")]
        public int QuantitySold { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than 0")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total Price (R)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [Display(Name = "Served By")]
        public int UserID { get; set; }

        // Navigation properties
        [ForeignKey("ProductID")]
        public Product? Product { get; set; }

        [ForeignKey("UserID")]
        public User? User { get; set; }
    }
}