using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookStoreApp.Data;
using BookStoreApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;

namespace BookStoreApp.Controllers
{
    [Authorize(Roles ="Admin")]
    
    public class BooksController : Controller
    {
        private readonly BookStoreAppContext _context;

        public BooksController(BookStoreAppContext context)
        {
            _context = context;
        }


        //public async Task<IActionResult> Search(string searchTerm)
        //{
            
        //}

        // GET: Books
        public async Task<IActionResult> Index(string searchTerm)
        {
            var booksQuery = _context.Books.AsQueryable();

            // Check if a search term is provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Filter books by title, ISBN, and author name containing the search term
                booksQuery = booksQuery.Where(b =>
                    EF.Functions.Like(b.Title, $"%{searchTerm}%") ||
                    EF.Functions.Like(b.ISBN, $"%{searchTerm}%") ||
                    EF.Functions.Like(b.AuthorName, $"%{searchTerm}%") ||
                    EF.Functions.Like(b.GenreName, $"%{searchTerm}")
                );
            }

            var books = await booksQuery.ToListAsync();

            return View(books);
            //var bookStoreAppContext = _context.Books.Include(b => b.Author).Include(b => b.Genre).Include(b => b.User);
            //return View(await bookStoreAppContext.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
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

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["AuthorID"] = new SelectList(_context.Authors, "AuthorID", "AuthorID");
            ViewData["GenreID"] = new SelectList(_context.Genres, "GenreID", "GenreID");
            //ViewData["UserId"] = new SelectList(_context.Users, "UserID", "UserID");
            ViewData["UserId"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //ViewData["UserId"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            book.CreatedDate = DateTime.Now;
            book.UpdatedDate = DateTime.Now;
            
            if (ModelState.IsValid)
            {
                //var UserUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                //book.UserUserId = "aa";
                // book.UserUserId= User.FindFirstValue(ClaimTypes.NameIdentifier);
                // Check if the author name already exists in the database
                var existingAuthor = await _context.Authors.FirstOrDefaultAsync(a => a.AuthorName == book.AuthorName);
                var existingGenre = await _context.Genres.FirstOrDefaultAsync(a => a.GenreName == book.GenreName);
                // If author name doesn't exist, create a new author
                if (existingAuthor == null)
                {
                    var newAuthor = new Author { AuthorName = book.AuthorName };
                    _context.Authors.Add(newAuthor);
                    await _context.SaveChangesAsync(); // Save changes to generate author ID
                    book.AuthorID = newAuthor.AuthorID; // Assign the generated author ID to the book
                }
                else
                {
                    book.AuthorID = existingAuthor.AuthorID; // Use existing author ID
                }

                if (existingGenre == null)
                {
                    var newGenre = new Genre { GenreName = book.GenreName };
                    _context.Genres.Add(newGenre);
                    await _context.SaveChangesAsync(); // Save changes to generate author ID
                    book.GenreID = newGenre.GenreID; // Assign the generated author ID to the book
                }
                else
                {
                    book.GenreID = existingGenre.GenreID; // Use existing author ID
                }

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0]; // Assuming the data is in the first worksheet

                        int rowCount = worksheet.Dimension.Rows;
                        for (int row = 2; row <= rowCount; row++) // Assuming the first row is header
                        {
                            var isbn = worksheet.Cells[row, 1].Value?.ToString();
                            var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);

                            if (existingBook != null)
                            {
                                // Update the existing book with new details
                                existingBook.Title = worksheet.Cells[row, 2].Value?.ToString();
                                existingBook.AuthorName = worksheet.Cells[row, 3].Value?.ToString();
                                existingBook.GenreName = worksheet.Cells[row, 4].Value?.ToString();
                                existingBook.CoverImage = worksheet.Cells[row, 5].Value?.ToString();
                                existingBook.Price = decimal.Parse(worksheet.Cells[row, 6].Value?.ToString());
                                existingBook.Page = int.Parse(worksheet.Cells[row, 7].Value?.ToString());
                                existingBook.Description = worksheet.Cells[row, 8].Value?.ToString();
                                existingBook.PublicationDate = DateTime.Parse(worksheet.Cells[row, 9].Value?.ToString());
                                existingBook.Availability = bool.Parse(worksheet.Cells[row, 10].Value?.ToString());
                                existingBook.Popularity = decimal.Parse(worksheet.Cells[row, 11].Value?.ToString());
                                existingBook.UserUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                                existingBook.UpdatedDate = DateTime.Now;

                                // Check if author exists, if not create new
                                var existingAuthor = await _context.Authors.FirstOrDefaultAsync(a => a.AuthorName == existingBook.AuthorName);
                                if (existingAuthor == null)
                                {
                                    var newAuthor = new Author { AuthorName = existingBook.AuthorName };
                                    _context.Authors.Add(newAuthor);
                                    await _context.SaveChangesAsync();
                                    existingBook.AuthorID = newAuthor.AuthorID;
                                }
                                else
                                {
                                    existingBook.AuthorID = existingAuthor.AuthorID;
                                }

                                // Check if genre exists, if not create new
                                var existingGenre = await _context.Genres.FirstOrDefaultAsync(g => g.GenreName == existingBook.GenreName);
                                if (existingGenre == null)
                                {
                                    var newGenre = new Genre { GenreName = existingBook.GenreName };
                                    _context.Genres.Add(newGenre);
                                    await _context.SaveChangesAsync();
                                    existingBook.GenreID = newGenre.GenreID;
                                }
                                else
                                {
                                    existingBook.GenreID = existingGenre.GenreID;
                                }

                                // Update the existing book in the database
                                _context.Books.Update(existingBook);
                            }
                            else
                            {
                                // Create a new book if it doesn't exist
                                var book = new Book
                                {
                                    ISBN = isbn,
                                    Title = worksheet.Cells[row, 2].Value?.ToString(),
                                    AuthorName = worksheet.Cells[row, 3].Value?.ToString(),
                                    GenreName = worksheet.Cells[row, 4].Value?.ToString(),
                                    CoverImage = worksheet.Cells[row, 5].Value?.ToString(),
                                    Price = decimal.Parse(worksheet.Cells[row, 6].Value?.ToString()),
                                    Page = int.Parse(worksheet.Cells[row, 7].Value?.ToString()),
                                    Description = worksheet.Cells[row, 8].Value?.ToString(),
                                    PublicationDate = DateTime.Parse(worksheet.Cells[row, 9].Value?.ToString()),
                                    Availability = bool.Parse(worksheet.Cells[row, 10].Value?.ToString()),
                                    Popularity = decimal.Parse(worksheet.Cells[row, 11].Value?.ToString()),
                                    UserUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                                    CreatedDate = DateTime.Now,
                                    UpdatedDate = DateTime.Now
                                };

                                // Check if author exists, if not create new
                                var existingAuthor = await _context.Authors.FirstOrDefaultAsync(a => a.AuthorName == book.AuthorName);
                                if (existingAuthor == null)
                                {
                                    var newAuthor = new Author { AuthorName = book.AuthorName };
                                    _context.Authors.Add(newAuthor);
                                    await _context.SaveChangesAsync();
                                    book.AuthorID = newAuthor.AuthorID;
                                }
                                else
                                {
                                    book.AuthorID = existingAuthor.AuthorID;
                                }

                                // Check if genre exists, if not create new
                                var existingGenre = await _context.Genres.FirstOrDefaultAsync(g => g.GenreName == book.GenreName);
                                if (existingGenre == null)
                                {
                                    var newGenre = new Genre { GenreName = book.GenreName };
                                    _context.Genres.Add(newGenre);
                                    await _context.SaveChangesAsync();
                                    book.GenreID = newGenre.GenreID;
                                }
                                else
                                {
                                    book.GenreID = existingGenre.GenreID;
                                }

                                // Add the new book to the database
                                _context.Books.Add(book);
                            }
                        }

                        // Save changes to the database
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["Message"] = "File uploaded successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while importing the file: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["AuthorID"] = new SelectList(_context.Authors, "AuthorID", "AuthorID", book.AuthorID);
            ViewData["GenreID"] = new SelectList(_context.Genres, "GenreID", "GenreID", book.GenreID);
            ViewData["UserId"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,Book book)
        {
            if (id != book.BookID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAuthor = await _context.Authors.FirstOrDefaultAsync(a => a.AuthorName == book.AuthorName);
                    var existingGenre = await _context.Genres.FirstOrDefaultAsync(a => a.GenreName == book.GenreName);
                    // If author name doesn't exist, create a new author
                    if (existingAuthor == null)
                    {
                        var newAuthor = new Author { AuthorName = book.AuthorName };
                        _context.Authors.Add(newAuthor);
                        await _context.SaveChangesAsync(); // Save changes to generate author ID
                        book.AuthorID = newAuthor.AuthorID; // Assign the generated author ID to the book
                    }
                    else
                    {
                        book.AuthorID = existingAuthor.AuthorID; // Use existing author ID
                    }

                    if (existingGenre == null)
                    {
                        var newGenre = new Genre { GenreName = book.GenreName };
                        _context.Genres.Add(newGenre);
                        await _context.SaveChangesAsync(); // Save changes to generate author ID
                        book.GenreID = newGenre.GenreID; // Assign the generated author ID to the book
                    }
                    else
                    {
                        book.GenreID = existingGenre.GenreID; // Use existing author ID
                    }
                    var existingBook = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.BookID == id);

                    // Copy the CreatedDate from the existing book to the edited book
                    book.CreatedDate = existingBook.CreatedDate;
                    book.UpdatedDate = DateTime.Now;
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.BookID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorID"] = new SelectList(_context.Authors, "AuthorID", "AuthorID", book.AuthorID);
            ViewData["GenreID"] = new SelectList(_context.Genres, "GenreID", "GenreID", book.GenreID);
            ViewData["UserId"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookID == id);
        }

        public IActionResult GetGenreName(int genreId)
        {
            // Find the genre with the given ID in the database
            var genre = _context.Genres.Find(genreId);

            // Check if the genre exists
            if (genre != null)
            {
                // Return the genre name as JSON
                return Json(new { genreName = genre.GenreName });
            }
            else
            {
                // If genre is not found, return an empty response or appropriate error
                return NotFound();
            }
        }

        public IActionResult GetAuthorName(int authorId)
        {
            // Find the genre with the given ID in the database
            var author = _context.Authors.Find(authorId);

            // Check if the genre exists
            if (author != null)
            {
                // Return the genre name as JSON
                return Json(new { AuthorName = author.AuthorName });
            }
            else
            {
                // If genre is not found, return an empty response or appropriate error
                return NotFound();
            }
        }

    }
}
