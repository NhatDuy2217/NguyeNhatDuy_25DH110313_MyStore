
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Mvc;
using PagedList;
using NguyeNhatDuy_25DH110313_MyStore.Models;

namespace NguyeNhatDuy_25DH110313_MyStore.Controllers
{
    public class PurchaseHistoryController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Kiểm tra tài khoản khách hàng đăng nhập
        private Customer GetCurrentCustomer()
        {
            if (Session["Username"] == null ||
                Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Customer")
            {
                return null;
            }

            string username = Session["Username"].ToString();

            return db.Customers.FirstOrDefault(
                c => c.Username == username
            );
        }

        // GET: PurchaseHistory/Index
        // Danh sách đơn hàng của khách hàng
        public ActionResult Index(int? page, string status, string searchString)
        {
            var customer = GetCurrentCustomer();

            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int pageNumber = page ?? 1;
            int pageSize = 6;

            var customerOrders = db.Orders
                .Where(o => o.CustomerID == customer.CustomerID);

            // Thống kê số lượng đơn hàng theo từng trạng thái
            ViewBag.TotalOrders = customerOrders.Count();

            ViewBag.UnpaidOrders = customerOrders.Count(o =>
                o.PaymentStatus == "Chưa thanh toán");

            ViewBag.ProcessingOrders = customerOrders.Count(o =>
                o.OrderStatus == "Đang xử lý");

            ViewBag.ShippingOrders = customerOrders.Count(o =>
                o.OrderStatus == "Đang vận chuyển");

            ViewBag.DeliveredOrders = customerOrders.Count(o =>
                o.OrderStatus == "Đã giao");

            ViewBag.CancelledOrders = customerOrders.Count(o =>
                o.OrderStatus == "Đã hủy");

            // Lọc đơn hàng theo trạng thái
            var orders = customerOrders
                .Include(o => o.OrderDetails.Select(od => od.Product));

            if (!string.IsNullOrWhiteSpace(status))
            {
                switch (status)
                {
                    case "unpaid":
                        orders = orders.Where(o =>
                            o.PaymentStatus == "Chưa thanh toán");
                        break;

                    case "processing":
                        orders = orders.Where(o =>
                            o.OrderStatus == "Đang xử lý");
                        break;

                    case "shipping":
                        orders = orders.Where(o =>
                            o.OrderStatus == "Đang vận chuyển");
                        break;

                    case "delivered":
                        orders = orders.Where(o =>
                            o.OrderStatus == "Đã giao");
                        break;

                    case "cancelled":
                        orders = orders.Where(o =>
                            o.OrderStatus == "Đã hủy");
                        break;
                }
            }

            // Tìm kiếm theo mã đơn hàng hoặc tên sản phẩm
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                int orderId;
                bool isOrderId = int.TryParse(
                    searchString.Replace("#", ""),
                    out orderId
                );

                orders = orders.Where(o =>
                    (isOrderId && o.OrderID == orderId) ||
                    o.OrderDetails.Any(od =>
                        od.Product != null &&
                        od.Product.ProductName.Contains(searchString)
                    )
                );
            }

            ViewBag.CurrentStatus = status ?? "";
            ViewBag.SearchString = searchString;

            // Sắp xếp đơn hàng mới nhất lên đầu
            orders = orders
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderID);

            return View(orders.ToPagedList(pageNumber, pageSize));
        }

        // GET: PurchaseHistory/Details/5
        // Chi tiết đơn hàng
        public ActionResult Details(int? id)
        {
            var customer = GetCurrentCustomer();

            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return HttpNotFound();
            }

            var order = db.Orders
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .FirstOrDefault(o =>
                    o.OrderID == id.Value &&
                    o.CustomerID == customer.CustomerID
                );

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: PurchaseHistory/BuyAgain/5
        // Mua lại các sản phẩm trong đơn hàng với số lượng luôn bằng 1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuyAgain(int id)
        {
            var customer = GetCurrentCustomer();

            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy đơn hàng thuộc khách hàng đang đăng nhập
            var order = db.Orders
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .FirstOrDefault(o =>
                    o.OrderID == id &&
                    o.CustomerID == customer.CustomerID
                );

            if (order == null)
            {
                return HttpNotFound();
            }

            // Lấy giỏ hàng hiện tại
            var cart = Session["Cart"] as List<CartItem>;

            if (cart == null)
            {
                cart = new List<CartItem>();
            }

            // Lấy danh sách sản phẩm còn kinh doanh
            // Đồng thời loại bỏ sản phẩm trùng trong đơn hàng cũ
            var products = order.OrderDetails
                .Where(d => d.Product != null && d.Product.IsActive)
                .Select(d => d.Product)
                .GroupBy(p => p.ProductID)
                .Select(g => g.First())
                .ToList();

            int addedCount = 0;

            // Duyệt qua từng sản phẩm trong đơn hàng cũ
            foreach (var product in products)
            {
                // Xóa tất cả dòng sản phẩm trùng trong giỏ hàng
                cart.RemoveAll(x =>
                    x.ProductID == product.ProductID
                );

                // Thêm lại sản phẩm với số lượng đúng bằng 1
                cart.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ProductImage = product.ProductImage,
                    UnitPrice = product.ProductPrice,
                    Quantity = 1
                });

                addedCount++;
            }

            // Lưu giỏ hàng vào Session
            Session["Cart"] = cart;

            // Thông báo kết quả
            if (addedCount == 0)
            {
                TempData["CartError"] =
                    "Không có sản phẩm nào trong đơn hàng cũ còn kinh doanh.";
            }
            else
            {
                TempData["CartSuccess"] =
                    "Đã thêm " + addedCount +
                    " sản phẩm vào giỏ hàng. Mỗi sản phẩm có số lượng là 1.";
            }

            return RedirectToAction("Index", "Cart");
        }

        // Giải phóng database
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
