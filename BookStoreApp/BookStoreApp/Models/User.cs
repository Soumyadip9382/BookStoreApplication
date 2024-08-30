using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BookStoreApp.Models
{
    public class User:IdentityUser
    {
        //public int UserUserID { get; set; }
        //public string UserUsername { get; set; }
        //public string Password { get; set; }
        //public string Email { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string UserType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        [ValidateNever]
        public ICollection<Cart> Carts { get; set; }
        [ValidateNever]
        public ICollection<Order> Orders { get; set; }
        [ValidateNever]
        public ICollection<Wishlist> Wishlists { get; set; }
    }

}
