using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.EntityFrameworkCore;

public interface ICartService
{
    Task AddToCartAsync(string userId, int bookId);
}

public class CartService : ICartService
{
    private readonly BookStoreAppContext _dbContext;

    public CartService(BookStoreAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddToCartAsync(string userId, int bookId)
    {
        // Check if the item already exists in the cart
        var existingCartItem = await _dbContext.Carts
            .FirstOrDefaultAsync(c => c.UserUserID == userId && c.BookID == bookId);

        if (existingCartItem != null)
        {
            // If the item exists, increase its quantity
            existingCartItem.Quantity++;
        }
        else
        {
            // If the item doesn't exist, create a new cart item
            var newCartItem = new Cart
            {
                UserUserID = userId,
                BookID = bookId,
                Quantity = 1 // Initial quantity is 1
            };

            _dbContext.Carts.Add(newCartItem);
        }

        await _dbContext.SaveChangesAsync();
    }
}
