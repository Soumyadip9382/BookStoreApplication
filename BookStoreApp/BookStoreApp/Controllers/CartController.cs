using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreApp.Models;
using BookStoreApp.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly BookStoreAppContext _context;

    public CartController(ICartService cartService, BookStoreAppContext context)
    {
        _cartService = cartService;
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string userId = GetCurrentUserId();
        var carts = await _context.Carts
            .Where(c => c.UserUserID == userId)
            .Include(c => c.Book)
            .ToListAsync();

        return View(carts);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int id)
    {
        string userId = GetCurrentUserId();

        var wishlistItem = await _context.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId && w.BookID == id);
        if (wishlistItem != null)
        {
            await _cartService.AddToCartAsync(userId, id);
            _context.Wishlists.Remove(wishlistItem);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index","WishList");
        }

        await _cartService.AddToCartAsync(userId, id);
        return RedirectToAction("Index", "HomeCard");
    }
   
    private string GetCurrentUserId()
    {
        var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userid;
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int id, int quantity)
    {
        var userid=GetCurrentUserId();
        var bookid = id;
        var cartItem = await _context.Carts.FirstOrDefaultAsync(c => c.BookID == id && c.UserUserID==userid);

        if (cartItem == null)
        {
            return NotFound();
        }

        cartItem.Quantity = quantity;
        _context.Carts.Update(cartItem);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int id)
    {
        var userid = GetCurrentUserId();
        var cartItem = await _context.Carts.FirstOrDefaultAsync(c => c.BookID == id && c.UserUserID==userid);

        if (cartItem == null)
        {
            return NotFound();
        }

        _context.Carts.Remove(cartItem);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}
