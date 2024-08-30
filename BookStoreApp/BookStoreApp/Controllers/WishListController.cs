using Microsoft.AspNetCore.Mvc;
using BookStoreApp.Models;
using BookStoreApp.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace BookStoreApp.Controllers
{
    [Authorize]
    public class WishListController : Controller
    {
        private readonly BookStoreAppContext _context;

        public WishListController(BookStoreAppContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Retrieve the wishlist items for the current user
            string userId = GetCurrentUserId(); // Implement GetCurrentUserId() to get the current user ID
            var wishlistItems = _context.Wishlists
                .Include(w => w.Book)
                .Where(w => w.UserId == userId)
                .ToList();

            return View(wishlistItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishList(int id)
        {
            string userId = GetCurrentUserId();
            var existingWishlistItem = await _context.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId && w.BookID == id);

            if (existingWishlistItem != null)
            {
                return RedirectToAction("Index", "HomeCard");
            }
            var wishlistItem = new Wishlist
            {
                UserId = userId,
                BookID = id
            };
            
            _context.Wishlists.Add(wishlistItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "HomeCard");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            var wishlistItem = await _context.Wishlists.FindAsync(id);
            if (wishlistItem == null)
            {
                return NotFound();
            }

            _context.Wishlists.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index","WishList");
        }

        private string GetCurrentUserId()
        {
            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userid;
        }
    }
}
