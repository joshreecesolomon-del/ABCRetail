using System.ComponentModel.DataAnnotations;

namespace ABCRetail.Models
{
    public class OrderModel
    {
        [Required]
        public string OrderId { get; set; } = string.Empty;

        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public string ProductName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}