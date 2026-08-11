using System.ComponentModel.DataAnnotations;

namespace ABCRetail.Models
{
    public class InventoryModel
    {
        [Required]
        public string ProductName { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}