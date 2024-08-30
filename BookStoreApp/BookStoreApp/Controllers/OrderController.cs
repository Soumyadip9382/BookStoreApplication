using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Security.Claims;

namespace BookStoreApp.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {

        private readonly BookStoreAppContext _context;

        public OrderController(BookStoreAppContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            string userId = GetCurrentUserId();

            // Get orders filtered by user ID
            IQueryable<Order> ordersQuery = _context.Orders
                .Where(u => u.UserUserID == userId)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book);

            // Apply date filters if provided
            if (startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate <= endDate.Value);
            }

            // Apply ordering
            IOrderedQueryable<Order> orderedOrdersQuery = ordersQuery.OrderByDescending(o => o.OrderDate);

            // Execute query and get orders
            var orders = await orderedOrdersQuery.ToListAsync();

            return View(orders);
        }

        private string GetCurrentUserId()
        {
            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userid;
        }

        [HttpGet]
        public async Task<IActionResult> GenerateInvoice(DateTime? startDate, DateTime? endDate)
        {
            string userId = GetCurrentUserId();
            IQueryable<Order> ordersQuery = _context.Orders
                .Where(u => u.UserUserID == userId)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.OrderDate);

            if (startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate <= endDate.Value);
            }

            var orders = await ordersQuery.ToListAsync();

            byte[] fileContents = GenerateInvoiceAsExcel(orders);

            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "invoice.xlsx");
        }

        private byte[] GenerateInvoiceAsExcel(List<Order> orders)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Invoice");
                var userid=GetCurrentUserId();
                var user=_context.Users.FirstOrDefault(u => u.Id == userid);


                // Add headers
                worksheet.Cells[1, 1].Value = "Full Name";
                worksheet.Cells[2, 1].Value = "Address";
                worksheet.Cells[3, 1].Value = "Phone Number";
                worksheet.Cells[5, 1].Value = "Book Name";
                worksheet.Cells[5, 2].Value = "Quantity";
                worksheet.Cells[5, 3].Value = "Unit Price";
                worksheet.Cells[5, 4].Value = "Total Amount";
                worksheet.Cells[5, 5].Value = "Order Date";
                worksheet.Cells[5, 6].Value = "Status";

                string fullName = user.FullName;
                string address = user.Address;
                string phoneNumber = user.PhoneNumber;
                worksheet.Cells[1, 2].Value = fullName;
                worksheet.Cells[2, 2].Value = address;
                worksheet.Cells[3, 2].Value = phoneNumber;

                // Add data
                int row = 6;
                foreach (var order in orders)
                {
                    
                    foreach (var orderDetail in order.OrderDetails)
                    {
                        
                        worksheet.Cells[row, 1].Value = orderDetail.Book.Title;
                        worksheet.Cells[row, 2].Value = orderDetail.Quantity;
                        worksheet.Cells[row, 3].Value = orderDetail.UnitPrice;
                        worksheet.Cells[row, 4].Value = order.TotalAmount;
                        worksheet.Cells[row, 5].Value = order.OrderDate.ToShortDateString();
                        worksheet.Cells[row, 6].Value = order.Status;
                        row++;
                    }
                }

                return package.GetAsByteArray();
            }
        }
    }
}
