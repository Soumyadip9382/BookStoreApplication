using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookStoreApp.Controllers
{
    [Authorize]
    public class HomeCardController : Controller
    {
        private readonly BookStoreAppContext _context;
        private readonly ICartService _cartService;

        public HomeCardController(BookStoreAppContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: Books
        //public async Task<IActionResult> Index(int? page)
        //{
        //    int pageSize = 4;
        //    int pageNumber = page ?? 1;

        //    var books = await _context.Books
        //        .Include(b => b.Author)
        //        .Include(b => b.Genre)
        //        .Include(b => b.User)
        //        .ToListAsync();

        //    var paginatedBooks = PaginatedList<Book>.Create(books.AsQueryable(), pageNumber, pageSize);

        //    string currentUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
        //    ViewData["CurrentUrl"] = currentUrl;

        //    string userId = GetCurrentUserId();
        //    var wishlistBookIds = await _context.Wishlists
        //        .Where(w => w.UserId == userId)
        //        .Select(w => w.BookID)
        //        .ToListAsync();

        //    ViewData["WishlistBookIds"] = wishlistBookIds;

        //    return View(paginatedBooks);
        //}

        public async Task<IActionResult> Index(string searchQuery, int? page)
        {
            int pageSize = 4;
            int pageNumber = page ?? 1;

            // Retrieve all books from the database
            var booksQuery = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Include(b => b.User)
                .AsQueryable();

            // If a search query is provided, filter the books based on the search query
            if (!string.IsNullOrEmpty(searchQuery))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(searchQuery) ||
                    b.Author.AuthorName.Contains(searchQuery) ||
                    b.Genre.GenreName.Contains(searchQuery)
                );
            }

            var books = await booksQuery.ToListAsync();

            // Create a paginated list of books
            var paginatedBooks = PaginatedList<Book>.Create(books.AsQueryable(), pageNumber, pageSize);

            // Pass the current search query to the view
            ViewData["CurrentSearchQuery"] = searchQuery;

            // Retrieve wishlist book IDs for the current user
            string userId = GetCurrentUserId();
            var wishlistBookIds = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Select(w => w.BookID)
                .ToListAsync();

            ViewData["WishlistBookIds"] = wishlistBookIds;

            return View(paginatedBooks);
        }


        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (book == null)
            {
                return NotFound();
            }
            string currentUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
            ViewData["CurrentUrl"] = currentUrl;
            string userId = GetCurrentUserId();
            var wishlistBookIds = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Select(w => w.BookID)
                .ToListAsync();

            ViewData["WishlistBookIds"] = wishlistBookIds;
            return View(book);
        }
        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, string returnUrl)
        {
            string userId = GetCurrentUserId();
            await _cartService.AddToCartAsync(userId, id);

            // If the return URL is not provided, redirect to the default page
            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index", "HomeCard");
            }

            // Otherwise, redirect back to the same page
            return Redirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishList(int id, string returnUrl)
        {
            string userId = GetCurrentUserId();
            var existingWishlistItem = await _context.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId && w.BookID == id);

            if (existingWishlistItem != null)
            {
                _context.Wishlists.Remove(existingWishlistItem);
                await _context.SaveChangesAsync();
                // If the return URL is not provided, redirect to the default page
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction("Index", "HomeCard");
                }
                return Redirect(returnUrl);
            }

            var wishlistItem = new Wishlist
            {
                UserId = userId,
                BookID = id
            };
            _context.Wishlists.Add(wishlistItem);
            await _context.SaveChangesAsync();
            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index", "HomeCard");
            }

            return Redirect(returnUrl);
        }

        private string GetCurrentUserId()
        {
            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userid;
        }
    }



    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        public static PaginatedList<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
