
using NguyeNhatDuy_25DH110313_MyStore.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NguyeNhatDuy_25DH110313_MyStore.Areas.Admin.Controllers
{
    public class CategoriesController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Categories
        // Hiển thị danh sách danh mục
        public ActionResult Index()
        {
            return View(db.Categories.ToList());
        }

        // GET: Admin/Categories/Details/5
        // Xem chi tiết danh mục
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // GET: Admin/Categories/Create
        // Hiển thị form thêm danh mục
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        // Lưu danh mục mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Categories.Add(category);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể thêm danh mục: " +
                        ex.GetBaseException().Message
                    );
                }
            }

            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        // Hiển thị form sửa danh mục
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // Lưu thông tin danh mục sau khi sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(category).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể sửa danh mục: " +
                        ex.GetBaseException().Message
                    );
                }
            }

            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        // Hiển thị thông tin trước khi xóa
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra số sản phẩm đang sử dụng danh mục này
            int productCount = db.Products.Count(
                p => p.CategoryID == id.Value
            );

            ViewBag.ProductCount = productCount;

            return View(category);
        }


        // POST: Admin/Categories/Delete/5
        // Xác nhận và thực hiện xóa danh mục
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            try
            {
                // Kiểm tra sản phẩm đang sử dụng danh mục
                int productCount = db.Products.Count(
                    p => p.CategoryID == id
                );

                ViewBag.ProductCount = productCount;

                // Không cho xóa nếu vẫn còn sản phẩm
                if (productCount > 0)
                {
                    ViewBag.DeleteError =
                        "Không thể xóa danh mục này vì đang có " +
                        productCount +
                        " sản phẩm sử dụng. Vui lòng chuyển các sản phẩm sang danh mục khác trước khi xóa.";

                    return View("Delete", category);
                }

                // Xóa danh mục nếu không còn sản phẩm liên kết
                db.Categories.Remove(category);
                db.SaveChanges();

                TempData["DeleteSuccess"] =
                    "Xóa danh mục thành công!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ProductCount = db.Products.Count(
                    p => p.CategoryID == id
                );

                ViewBag.DeleteError =
                    "Không thể xóa danh mục: " +
                    ex.GetBaseException().Message;

                return View("Delete", category);
            }
        }

        // Giải phóng tài nguyên database
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
