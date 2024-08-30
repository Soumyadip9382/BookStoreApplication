using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BookStoreApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookStoreAppContext _context;

        public HomeController(ILogger<HomeController> logger,BookStoreAppContext context)
        {
            _logger = logger;
            this._context = context;
        }

        //[HttpGet]
        //public IActionResult SignUp()
        //{
        //    return View("SignUp");
        //}

        //[HttpPost]
        //public IActionResult SignUp(User obj)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        obj.CreatedDate = DateTime.Now;
        //        obj.UpdatedDate = DateTime.Now;
        //        _context.Users.Add(obj);
        //        _context.SaveChanges();
        //        return RedirectToAction("Index","HomeCard");
        //    }
        //    return View();
        //}

        //[HttpGet]
        //public async Task<IActionResult> AllUser()
        //{
        //    var bookStoreAppContext = _context.Users;
        //    return View(await bookStoreAppContext.ToListAsync());
        //}


        //[HttpGet]
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var book = await _context.Users
        //        .FirstOrDefaultAsync(m => m.UserID == id);
        //    if (book == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(book);
        //}

        //// POST: Books/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var user = await _context.Users.FindAsync(id);
        //    if (user != null)
        //    {
        //        _context.Users.Remove(user);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
