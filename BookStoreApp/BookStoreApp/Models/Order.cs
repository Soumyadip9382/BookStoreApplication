using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BookStoreApp.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public string UserUserID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        [ValidateNever]
        public User User { get; set; }
        [ValidateNever]
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }

}
