using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BookStoreApp.Models
{
    public class Cart
    {
        public int CartID { get; set; }
        public string UserUserID { get; set; }
        public int BookID { get; set; }
        public int Quantity { get; set; }
        [ValidateNever]
        public User User { get; set; }
        [ValidateNever]
        public Book Book { get; set; }
    }

}
