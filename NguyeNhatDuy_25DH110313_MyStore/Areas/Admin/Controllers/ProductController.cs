
using System;
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
            int? categoryId,
            int? page,
            int? pageSize)
        {
            // Chỉ hiển thị sản phẩm đang kinh doanh
            var products = db.Products
                             .Where(p => p.IsActive)
                             .Include(p => p.Category)
                             .AsQueryable();

            // Lấy danh sách danh mục cho dropdown
            var categories = db.Categories
                               .OrderBy(c => c.CategoryName)
                               .ToList();

            ViewBag.Categories = new SelectList(
                categories,
                "CategoryID",
                "CategoryName",
                categoryId
            );

            // Lưu thông tin tìm kiếm, sắp xếp và danh mục
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentCategoryId = categoryId;

            ViewBag.NameSortParm = sortOrder == "name_asc"
                ? "name_desc" : "name_asc";

            ViewBag.PriceSortParm = sortOrder == "price_asc"
                ? "price_desc" : "price_asc";

            // Tìm kiếm theo tên sản phẩm
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(searchString));
            }

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                products = products.Where(p =>
                    p.CategoryID == categoryId.Value);
            }

            // Sắp xếp sản phẩm
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

            // Đếm tổng số sản phẩm sau khi lọc
            int totalItems = products.Count();

            // Số sản phẩm hiển thị trên mỗi trang
            int currentPageSize = pageSize ?? 5;

            if (currentPageSize != 5 &&
                currentPageSize != 10 &&
                currentPageSize != 20)
            {
                currentPageSize = 5;
            }

            // Trang hiện tại
            int currentPage = page ?? 1;

            if (currentPage < 1)
            {
                currentPage = 1;
            }

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling(
                (double)totalItems / currentPageSize
            );

            // Kiểm tra trang hiện tại
            if (totalPages > 0 && currentPage > totalPages)
            {
                currentPage = totalPages;
            }

            // Lưu thông tin phân trang
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = currentPageSize;

            // Lấy sản phẩm theo trang
            products = products
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize);

            return View(products.ToList());
        }

        // GET: Admin/Product/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }

            Product product = db.Products
                                .Include(p => p.Category)
                                .FirstOrDefault(p => p.ProductID == id);

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
                    ModelState.AddModelError(
                        "",
                        "Lỗi khi thêm sản phẩm: " +
                        ex.GetBaseException().Message
                    );
                }
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                ViewBag.ValidationErrors =
                    string.Join(" | ", errors);
            }

            // Nạp lại danh mục nếu form có lỗi
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
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }

            Product product = db.Products.Find(id);

            if (product == null)
            {
                return HttpNotFound();
            }

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
                    Product existingProduct =
                        db.Products.Find(product.ProductID);

                    if (existingProduct == null)
                    {
                        return HttpNotFound();
                    }

                    existingProduct.CategoryID = product.CategoryID;
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.ProductDecription =
                        product.ProductDecription;
                    existingProduct.ProductPrice = product.ProductPrice;
                    existingProduct.ProductImage = product.ProductImage;

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

            // Nạp lại danh mục nếu form có lỗi
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
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }

            Product product = db.Products
                                .Include(p => p.Category)
                                .FirstOrDefault(p => p.ProductID == id);

            // Không tìm thấy hoặc sản phẩm đã ngừng kinh doanh
            if (product == null || !product.IsActive)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost]
        [ActionName("Delete")]
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

                // Xóa mềm: ngừng kinh doanh, giữ lịch sử đơn hàng
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
