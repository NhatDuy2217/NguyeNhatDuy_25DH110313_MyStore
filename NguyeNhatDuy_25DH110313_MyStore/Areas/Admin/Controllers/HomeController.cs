
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
            // =====================================
            // 1. THỐNG KÊ SỐ LƯỢNG
            // =====================================

            // Chỉ đếm sản phẩm đang hoạt động
            // Không tính sản phẩm đã xóa mềm (IsActive = false)
            ViewBag.TotalProducts = db.Products
                .Where(p => p.IsActive == true)
                .Count();

            // Tổng danh mục
            ViewBag.TotalCategories = db.Categories.Count();

            // Tổng khách hàng
            ViewBag.TotalCustomers = db.Customers.Count();

            // Tổng tài khoản
            ViewBag.TotalUsers = db.Users.Count();

            // Tổng đơn hàng
            ViewBag.TotalOrders = db.Orders.Count();


            // =====================================
            // 2. THỐNG KÊ TRẠNG THÁI THANH TOÁN
            // =====================================

            // Đơn hàng đã thanh toán
            ViewBag.PaidOrders = db.Orders.Count(
                o => o.PaymentStatus == "Đã thanh toán"
            );

            // Đơn hàng chưa thanh toán
            ViewBag.UnpaidOrders = db.Orders.Count(
                o => o.PaymentStatus == "Chưa thanh toán"
            );


            // =====================================
            // 3. TỔNG DOANH THU
            // =====================================

            // Chỉ tính doanh thu từ đơn hàng đã thanh toán
            ViewBag.TotalRevenue = db.Orders
                .Where(o => o.PaymentStatus == "Đã thanh toán")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;


            return View();
        }

        // Giải phóng kết nối database
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
