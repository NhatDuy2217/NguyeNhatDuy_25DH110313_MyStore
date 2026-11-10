
using System;
using System.Linq;
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
        public ActionResult Index(int? page)
        {
            var customer = GetCurrentCustomer();

            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int pageNumber = page ?? 1;
            int pageSize = 6;

            var orders = db.Orders
                .Where(o => o.CustomerID == customer.CustomerID)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderID)
                .ToPagedList(pageNumber, pageSize);

            return View(orders);
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
                .Include("OrderDetails.Product")
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
