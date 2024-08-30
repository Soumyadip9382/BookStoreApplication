using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BookStoreApp.Models
{
    public class Wishlist
    {
        public int WishlistID { get; set; }
        public string UserId { get; set; }
        public int BookID { get; set; }
        [ValidateNever]
        public User User { get; set; }
        [ValidateNever]
        public Book Book { get; set; }
    }

}
