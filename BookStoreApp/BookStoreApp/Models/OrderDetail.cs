using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BookStoreApp.Models
{
    public class OrderDetail
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }
        public int BookID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        [ValidateNever]
        public Order Order { get; set; }
        [ValidateNever]
        public Book Book { get; set; }
    }

}
