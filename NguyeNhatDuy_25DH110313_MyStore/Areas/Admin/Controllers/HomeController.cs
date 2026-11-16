
using System;
using System.Linq;
using System.Web.Mvc;
using NguyeNhatDuy_25DH110313_MyStore.Models;

namespace NguyeNhatDuy_25DH110313_MyStore.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Home
        public ActionResult Index()
        {
            // 1. THỐNG KÊ TỔNG QUAN

            // Chỉ đếm sản phẩm đang hoạt động
            ViewBag.TotalProducts = db.Products
                .Where(p => p.IsActive == true)
                .Count();

            ViewBag.TotalCategories = db.Categories.Count();

            ViewBag.TotalCustomers = db.Customers.Count();

            ViewBag.TotalUsers = db.Users.Count();

            ViewBag.TotalOrders = db.Orders.Count();


            // 2. THỐNG KÊ THANH TOÁN

            ViewBag.PaidOrders = db.Orders
                .Count(o => o.PaymentStatus == "Đã thanh toán");

            ViewBag.UnpaidOrders = db.Orders
                .Count(o => o.PaymentStatus == "Chưa thanh toán");


            // 3. TỔNG DOANH THU
            // Chỉ tính đơn hàng đã thanh toán

            ViewBag.TotalRevenue = db.Orders
                .Where(o => o.PaymentStatus == "Đã thanh toán")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;


            // 4. DOANH THU THEO 12 THÁNG TRONG NĂM HIỆN TẠI

            int currentYear = DateTime.Now.Year;

            var monthlyRevenue = db.Orders
                .Where(o =>
                    o.PaymentStatus == "Đã thanh toán" &&
                    o.OrderDate.Year == currentYear)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .ToList();

            string[] monthLabels = new string[12];
            decimal[] monthlyRevenueValues = new decimal[12];

            for (int i = 0; i < 12; i++)
            {
                monthLabels[i] = "Tháng " + (i + 1);

                var monthData = monthlyRevenue
                    .FirstOrDefault(x => x.Month == i + 1);

                monthlyRevenueValues[i] = monthData != null
                    ? monthData.Revenue
                    : 0;
            }

            ViewBag.MonthlyRevenueLabels = monthLabels;
            ViewBag.MonthlyRevenueValues = monthlyRevenueValues;


            // 5. TOP 5 SÁCH BÁN CHẠY
            // Chỉ thống kê chi tiết thuộc đơn hàng đã thanh toán

            var topSellingProducts = db.OrderDetails
                .Where(od => od.Order.PaymentStatus == "Đã thanh toán")
                .GroupBy(od => new
                {
                    od.ProductID,
                    od.Product.ProductName
                })
                .Select(g => new
                {
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(od => (int?)od.Quantity) ?? 0
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ThenBy(x => x.ProductName)
                .Take(5)
                .ToList();

            ViewBag.TopSellingLabels = topSellingProducts
                .Select(x => x.ProductName)
                .ToArray();

            ViewBag.TopSellingValues = topSellingProducts
                .Select(x => x.TotalQuantity)
                .ToArray();


            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
