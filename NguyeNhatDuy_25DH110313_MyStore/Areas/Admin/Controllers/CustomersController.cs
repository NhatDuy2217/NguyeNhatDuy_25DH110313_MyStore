
using System;
using System.Linq;
using System.Net;
using System.Data.Entity;
using System.Web.Mvc;
using NguyeNhatDuy_25DH110313_MyStore.Models;

namespace NguyeNhatDuy_25DH110313_MyStore.Areas.Admin.Controllers
{
    public class CustomersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Customers
        public ActionResult Index(
            string searchString,
            string sortOrder,
            int? page,
            int? pageSize)
        {
            ViewBag.CurrentSort = sortOrder;

            ViewBag.NameSortParm =
                sortOrder == "name_asc" ? "name_desc" : "name_asc";

            ViewBag.PhoneSortParm =
                sortOrder == "phone_asc" ? "phone_desc" : "phone_asc";

            ViewBag.CurrentFilter = searchString;

            var customers = db.Customers
                .Include(c => c.User)
                .AsQueryable();

            // Tìm kiếm theo tên khách hàng
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                customers = customers.Where(c =>
                    c.CustomerName.Contains(searchString));
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "name_asc":
                    customers = customers.OrderBy(c => c.CustomerName);
                    break;

                case "name_desc":
                    customers = customers.OrderByDescending(c => c.CustomerName);
                    break;

                case "phone_asc":
                    customers = customers.OrderBy(c => c.CustomerPhone);
                    break;

                case "phone_desc":
                    customers = customers.OrderByDescending(c => c.CustomerPhone);
                    break;

                default:
                    customers = customers.OrderBy(c => c.CustomerID);
                    break;
            }

            // Chọn số lượng khách hàng mỗi trang
            int selectedPageSize = pageSize ?? 5;

            if (selectedPageSize != 5 &&
                selectedPageSize != 10 &&
                selectedPageSize != 20)
            {
                selectedPageSize = 5;
            }

            // Tính số trang
            int pageNumber = page ?? 1;

            if (pageNumber < 1)
                pageNumber = 1;

            int totalItems = customers.Count();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    (double)totalItems / selectedPageSize
                )
            );

            if (pageNumber > totalPages)
                pageNumber = totalPages;

            // Gửi thông tin sang View
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = selectedPageSize;

            var result = customers
                .Skip((pageNumber - 1) * selectedPageSize)
                .Take(selectedPageSize)
                .ToList();

            return View(result);
        }


        // GET: Admin/Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Customer customer = db.Customers
                .Include(c => c.User)
                .FirstOrDefault(c => c.CustomerID == id.Value);

            if (customer == null)
            {
                return HttpNotFound();
            }

            return View(customer);
        }


        // GET: Admin/Customers/Create
        public ActionResult Create()
        {
            ViewBag.Username = new SelectList(
                db.Users,
                "Username",
                "Username"
            );

            return View();
        }


        // POST: Admin/Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "CustomerID,CustomerName,CustomerPhone,CustomerEmail,CustomerAddress,Username")]
            Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Customers.Add(customer);
                    db.SaveChanges();

                    TempData["CreateSuccess"] =
                        "Thêm khách hàng thành công!";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể thêm khách hàng: " +
                        ex.GetBaseException().Message
                    );
                }
            }

            ViewBag.Username = new SelectList(
                db.Users,
                "Username",
                "Username",
                customer.Username
            );

            return View(customer);
        }


        // GET: Admin/Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Customer customer = db.Customers.Find(id);

            if (customer == null)
            {
                return HttpNotFound();
            }

            return View(customer);
        }


        // POST: Admin/Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "CustomerID,CustomerName,CustomerPhone,CustomerEmail,CustomerAddress")]
            Customer customer)
        {
            if (customer == null)
            {
                return HttpNotFound();
            }

            // Lấy khách hàng hiện tại trong database
            Customer existingCustomer = db.Customers.Find(
                customer.CustomerID
            );

            if (existingCustomer == null)
            {
                return HttpNotFound();
            }

            // Form không chỉnh sửa Username
            ModelState.Remove("Username");

            if (ModelState.IsValid)
            {
                try
                {
                    // Chỉ cập nhật các thông tin được phép sửa
                    existingCustomer.CustomerName =
                        customer.CustomerName;

                    existingCustomer.CustomerPhone =
                        customer.CustomerPhone;

                    existingCustomer.CustomerEmail =
                        customer.CustomerEmail;

                    existingCustomer.CustomerAddress =
                        customer.CustomerAddress;

                    // Giữ nguyên Username và tài khoản liên kết
                    db.SaveChanges();

                    TempData["EditSuccess"] =
                        "Cập nhật thông tin khách hàng thành công!";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể cập nhật khách hàng: " +
                        ex.GetBaseException().Message
                    );
                }
            }

            return View(customer);
        }


        // GET: Admin/Customers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Customer customer = db.Customers
                .Include(c => c.User)
                .FirstOrDefault(c => c.CustomerID == id.Value);

            if (customer == null)
            {
                return HttpNotFound();
            }

            // Đếm số đơn hàng của khách hàng
            int orderCount = db.Orders.Count(
                o => o.CustomerID == id.Value
            );

            ViewBag.OrderCount = orderCount;

            return View(customer);
        }


        // POST: Admin/Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customers.Find(id);

            if (customer == null)
            {
                return HttpNotFound();
            }

            try
            {
                // Kiểm tra số đơn hàng đang liên kết
                int orderCount = db.Orders.Count(
                    o => o.CustomerID == id
                );

                ViewBag.OrderCount = orderCount;

                // Không cho xóa nếu khách hàng đã có đơn hàng
                if (orderCount > 0)
                {
                    ViewBag.DeleteError =
                        "Không thể xóa khách hàng này vì đang có " +
                        orderCount +
                        " đơn hàng trong hệ thống. " +
                        "Vui lòng giữ lại khách hàng để bảo toàn lịch sử mua hàng.";

                    return View("Delete", customer);
                }

                // Xóa khách hàng nếu không có đơn hàng
                db.Customers.Remove(customer);
                db.SaveChanges();

                TempData["DeleteSuccess"] =
                    "Xóa khách hàng thành công!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.DeleteError =
                    "Không thể xóa khách hàng: " +
                    ex.GetBaseException().Message;

                try
                {
                    ViewBag.OrderCount = db.Orders.Count(
                        o => o.CustomerID == id
                    );
                }
                catch
                {
                    ViewBag.OrderCount = -1;
                }

                return View("Delete", customer);
            }
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
