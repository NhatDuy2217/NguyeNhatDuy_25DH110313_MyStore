
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
            {
                pageNumber = 1;
            }

            int totalItems = customers.Count();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    (double)totalItems / selectedPageSize
                )
            );

            if (pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

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
            LoadUserList(null);

            return View();
        }

        // POST: Admin/Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "CustomerID,CustomerName,CustomerPhone,CustomerEmail,CustomerAddress,Username")]
            Customer customer)
        {
            NormalizeUsername(customer);

            ValidateUsername(customer.Username);

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

            LoadUserList(customer.Username);

            return View(customer);
        }

        // GET: Admin/Customers/Edit/5
        public ActionResult Edit(int? id)
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

            // Lấy Username hiện tại của khách hàng
            string currentUsername = customer.Username;

            if (string.IsNullOrWhiteSpace(currentUsername) && customer.User != null)
            {
                currentUsername = customer.User.Username;
            }

            // Gán lại vào Model để giữ nguyên thông tin
            customer.Username = currentUsername;

            // Tạo danh sách tài khoản và đánh dấu tài khoản hiện tại
            var users = db.Users
                .OrderBy(u => u.Username)
                .ToList();

            var userList = users.Select(u => new SelectListItem
            {
                Text = u.Username,
                Value = u.Username,
                Selected = u.Username == currentUsername
            }).ToList();

            // Nếu tài khoản hiện tại không còn trong danh sách Users,
            // vẫn hiển thị tên tài khoản đang liên kết
            if (!string.IsNullOrWhiteSpace(currentUsername) &&
                !users.Any(u => u.Username == currentUsername))
            {
                userList.Insert(0, new SelectListItem
                {
                    Text = currentUsername,
                    Value = currentUsername,
                    Selected = true
                });
            }

            ViewBag.Username = userList;

            return View(customer);
        }

        // POST: Admin/Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "CustomerID,CustomerName,CustomerPhone,CustomerEmail,CustomerAddress,Username")]
            Customer customer)
        {
            NormalizeUsername(customer);

            ValidateUsername(customer.Username);

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy khách hàng hiện tại trong database
                    Customer existingCustomer = db.Customers
                        .FirstOrDefault(c =>
                            c.CustomerID == customer.CustomerID);

                    if (existingCustomer == null)
                    {
                        return HttpNotFound();
                    }

                    // Cập nhật thông tin khách hàng
                    existingCustomer.CustomerName = customer.CustomerName;
                    existingCustomer.CustomerPhone = customer.CustomerPhone;
                    existingCustomer.CustomerEmail = customer.CustomerEmail;
                    existingCustomer.CustomerAddress = customer.CustomerAddress;

                    // Cập nhật tài khoản liên kết
                    // Nếu Username = null thì bỏ liên kết tài khoản
                    existingCustomer.Username = customer.Username;

                    db.SaveChanges();

                    TempData["SuccessMessage"] =
                        "Cập nhật thông tin khách hàng thành công.";

                    return RedirectToAction("Index");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError(
                                validationError.PropertyName,
                                validationError.ErrorMessage
                            );
                        }
                    }
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

            // Tải lại danh sách tài khoản khi có lỗi
            LoadUserList(customer.Username);

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

        // Chuẩn hóa Username
        private void NormalizeUsername(Customer customer)
        {
            if (customer == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(customer.Username))
            {
                customer.Username = null;
            }
            else
            {
                customer.Username = customer.Username.Trim();
            }
        }

        // Kiểm tra tài khoản liên kết có tồn tại
        private void ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            bool userExists = db.Users.Any(
                u => u.Username == username
            );

            if (!userExists)
            {
                ModelState.AddModelError(
                    "Username",
                    "Tài khoản được chọn không tồn tại."
                );
            }
        }

        // Tải danh sách tài khoản và chọn tài khoản hiện tại
        private void LoadUserList(string selectedUsername)
        {
            var users = db.Users
                .OrderBy(u => u.Username)
                .ToList();

            ViewBag.Username = new SelectList(
                users,
                "Username",
                "Username",
                selectedUsername
            );
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
