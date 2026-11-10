
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyeNhatDuy_25DH110313_MyStore.Models;

namespace NguyeNhatDuy_25DH110313_MyStore.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Product
        public ActionResult Index(
            string searchString,
            string sortOrder,
            int? page,
            int? pageSize)
        {
            ViewBag.CurrentSort = sortOrder;

            ViewBag.NameSortParm =
                sortOrder == "name_asc" ? "name_desc" : "name_asc";

            ViewBag.PriceSortParm =
                sortOrder == "price_asc" ? "price_desc" : "price_asc";

            ViewBag.CurrentFilter = searchString;

            // Chỉ lấy sản phẩm đang kinh doanh
            var products = db.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                products = products.Where(p =>
                    p.ProductName.Contains(searchString));
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.ProductName);
                    break;

                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;

                case "price_asc":
                    products = products.OrderBy(p => p.ProductPrice);
                    break;

                case "price_desc":
                    products = products.OrderByDescending(p => p.ProductPrice);
                    break;

                default:
                    products = products.OrderBy(p => p.ProductID);
                    break;
            }

            // Phân trang
            int selectedPageSize = pageSize ?? 5;

            if (selectedPageSize != 5 &&
                selectedPageSize != 10 &&
                selectedPageSize != 20)
            {
                selectedPageSize = 5;
            }

            int pageNumber = page ?? 1;

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            int totalItems = products.Count();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    (double)totalItems / selectedPageSize));

            if (pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = selectedPageSize;

            var result = products
                .Skip((pageNumber - 1) * selectedPageSize)
                .Take(selectedPageSize)
                .ToList();

            return View(result);
        }

        // GET: Admin/Product/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Products.Find(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // GET: Admin/Product/Create
        public ActionResult Create()
        {
            var databaseName = db.Database.Connection.Database;
            var serverName = db.Database.Connection.DataSource;

            var categories = db.Categories.ToList();

            ViewBag.SoDanhMuc = categories.Count;
            ViewBag.TenDatabase = databaseName;
            ViewBag.TenServer = serverName;

            ViewBag.CategoryID = new SelectList(
                categories,
                "CategoryID",
                "CategoryName"
            );

            return View();
        }

        // POST: Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "ProductID,CategoryID,ProductName,ProductDecription,ProductPrice,ProductImage")]
            Product product)
        {
            if (product != null && ModelState.IsValid)
            {
                try
                {
                    // Sản phẩm mới mặc định đang kinh doanh
                    product.IsActive = true;

                    db.Products.Add(product);
                    db.SaveChanges();

                    TempData["DeleteSuccess"] =
                        "Thêm sản phẩm thành công!";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.GetBaseException().Message;

                    ModelState.AddModelError(
                        "",
                        "Lỗi khi thêm sản phẩm: " + errorMessage
                    );
                }
            }

            // Hiển thị lỗi kiểm tra dữ liệu
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                ViewBag.ValidationErrors = string.Join(" | ", errors);
            }

            // Nạp lại danh mục khi form có lỗi
            ViewBag.CategoryID = new SelectList(
                db.Categories.ToList(),
                "CategoryID",
                "CategoryName",
                product != null ? product.CategoryID : 0
            );

            return View(product);
        }

        // GET: Admin/Product/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Products.Find(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách danh mục và chọn đúng danh mục hiện tại
            ViewBag.Categories = new SelectList(
                db.Categories.ToList(),
                "CategoryID",
                "CategoryName",
                product.CategoryID
            );

            return View(product);
        }


        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "ProductID,CategoryID,ProductName,ProductDecription,ProductPrice,ProductImage")]
    Product product)
        {
            if (product == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy dữ liệu gốc từ database
                    Product existingProduct = db.Products.Find(product.ProductID);

                    if (existingProduct == null)
                    {
                        return HttpNotFound();
                    }

                    // Chỉ cập nhật các trường thông tin sản phẩm
                    existingProduct.CategoryID = product.CategoryID;
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.ProductDecription = product.ProductDecription;
                    existingProduct.ProductPrice = product.ProductPrice;
                    existingProduct.ProductImage = product.ProductImage;

                    // Lưu thay đổi
                    db.SaveChanges();

                    TempData["DeleteSuccess"] =
                        "Cập nhật sản phẩm thành công!";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        "Lỗi khi cập nhật sản phẩm: " +
                        ex.GetBaseException().Message
                    );
                }
            }

            // Nếu có lỗi, nạp lại danh mục và giữ lựa chọn hiện tại
            ViewBag.Categories = new SelectList(
                db.Categories.ToList(),
                "CategoryID",
                "CategoryName",
                product.CategoryID
            );

            return View(product);
        }

        // GET: Admin/Product/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Products.Find(id);

            if (product == null || !product.IsActive)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Product product = db.Products.Find(id);

                if (product == null)
                {
                    TempData["DeleteError"] =
                        "Không tìm thấy sản phẩm cần xóa.";

                    return RedirectToAction("Index");
                }

                if (!product.IsActive)
                {
                    TempData["DeleteError"] =
                        "Sản phẩm này đã được ngừng kinh doanh.";

                    return RedirectToAction("Index");
                }

                // Ngừng kinh doanh sản phẩm được chọn
                // Không xóa OrderDetail để bảo toàn lịch sử đơn hàng
                product.IsActive = false;

                db.SaveChanges();

                TempData["DeleteSuccess"] =
                    "Đã xóa sản phẩm khỏi danh sách kinh doanh thành công!";
            }
            catch (Exception ex)
            {
                TempData["DeleteError"] =
                    "Lỗi khi xóa sản phẩm: " +
                    ex.GetBaseException().Message;
            }

            return RedirectToAction("Index");
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
