using System;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using NguyeNhatDuy_25DH110313_MyStore.Models;
using NguyeNhatDuy_25DH110313_MyStore.Models.ViewModel;

namespace NguyeNhatDuy_25DH110313_MyStore.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // 1. Trang chủ BookOra
        public ActionResult Index(string searchString, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 8;

            // Lấy sách bán chạy đang hoạt động
            var featuredQuery = db.Products
                .Where(p => p.IsActive)
                .OrderByDescending(p =>
                    p.OrderDetails.Sum(od => (int?)od.Quantity) ?? 0);

            // Tìm kiếm sách bán chạy
            if (!String.IsNullOrWhiteSpace(searchString))
            {
                featuredQuery = featuredQuery.Where(p =>
                    p.ProductName.Contains(searchString) ||
                    (p.ProductDecription != null &&
                     p.ProductDecription.Contains(searchString)))
                    .OrderByDescending(p =>
                        p.OrderDetails.Sum(od => (int?)od.Quantity) ?? 0);
            }

            var featuredProducts = featuredQuery
                .Take(8)
                .ToList();

            // Lấy sách mới đang hoạt động
            var newQuery = db.Products
                .Where(p => p.IsActive)
                .AsQueryable();

            // Tìm kiếm sách mới
            if (!String.IsNullOrWhiteSpace(searchString))
            {
                newQuery = newQuery.Where(p =>
                    p.ProductName.Contains(searchString) ||
                    (p.ProductDecription != null &&
                     p.ProductDecription.Contains(searchString)));
            }

            var newProducts = newQuery
                .OrderByDescending(p => p.ProductID)
                .Take(20)
                .ToList()
                .ToPagedList(pageNumber, pageSize);

            // Lấy danh sách danh mục đang có
            var categories = db.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();

            // Đưa dữ liệu vào ViewModel
            var model = new HomeProductVM
            {
                FeaturedProducts = featuredProducts,
                NewProducts = newProducts,
                Categories = categories
            };

            ViewBag.SearchString = searchString;

            return View(model);
        }

        // 2. Danh sách sản phẩm theo danh mục
        public ActionResult ProductList(int? categoryID, string searchString, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 6;

            // Chỉ lấy sản phẩm đang hoạt động
            var products = db.Products
                .Where(p => p.IsActive)
                .AsQueryable();

            // Lọc theo danh mục
            if (categoryID.HasValue)
            {
                products = products.Where(p =>
                    p.CategoryID == categoryID.Value);
            }

            // Tìm kiếm theo tên hoặc mô tả
            if (!String.IsNullOrWhiteSpace(searchString))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(searchString) ||
                    (p.ProductDecription != null &&
                     p.ProductDecription.Contains(searchString)));
            }

            // Lấy danh sách danh mục
            ViewBag.Categories = db.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();

            ViewBag.CategoryID = categoryID;
            ViewBag.SearchString = searchString;

            return View(products
                .OrderBy(p => p.ProductName)
                .ToPagedList(pageNumber, pageSize));
        }

        // 3. Chi tiết sản phẩm
        public ActionResult ProductDetail(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            // Chỉ cho phép xem sản phẩm đang hoạt động
            var product = db.Products
                .Include("Category")
                .FirstOrDefault(p =>
                    p.ProductID == id.Value &&
                    p.IsActive);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // 4. Trang About
        [HttpGet]
        public ActionResult About()
        {
            return View();
        }

        // 5. Hiển thị trang Contact
        [HttpGet]
        public ActionResult Contact()
        {
            return View();
        }

        // 6. Xử lý biểu mẫu liên hệ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(
            string contactName,
            string contactEmail,
            string contactSubject,
            string contactMessage)
        {
            // Kiểm tra họ tên
            if (String.IsNullOrWhiteSpace(contactName))
            {
                ModelState.AddModelError(
                    "contactName",
                    "Vui lòng nhập họ và tên.");
            }

            // Kiểm tra email
            if (String.IsNullOrWhiteSpace(contactEmail))
            {
                ModelState.AddModelError(
                    "contactEmail",
                    "Vui lòng nhập email.");
            }
            else
            {
                try
                {
                    var email = new System.Net.Mail.MailAddress(contactEmail);

                    if (email.Address != contactEmail.Trim())
                    {
                        ModelState.AddModelError(
                            "contactEmail",
                            "Địa chỉ email không hợp lệ.");
                    }
                }
                catch
                {
                    ModelState.AddModelError(
                        "contactEmail",
                        "Địa chỉ email không hợp lệ.");
                }
            }

            // Kiểm tra tiêu đề
            if (String.IsNullOrWhiteSpace(contactSubject))
            {
                ModelState.AddModelError(
                    "contactSubject",
                    "Vui lòng nhập tiêu đề.");
            }

            // Kiểm tra nội dung
            if (String.IsNullOrWhiteSpace(contactMessage))
            {
                ModelState.AddModelError(
                    "contactMessage",
                    "Vui lòng nhập nội dung liên hệ.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ContactError =
                    "Vui lòng kiểm tra lại thông tin đã nhập.";

                return View();
            }

            // Chưa tích hợp gửi email hoặc lưu database
            ViewBag.ContactSuccess =
                "Thông tin đã được kiểm tra hợp lệ. " +
                "Chức năng gửi email/lưu yêu cầu sẽ được tích hợp sau.";

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