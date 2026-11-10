
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyeNhatDuy_25DH110313_MyStore.Models;

namespace NguyeNhatDuy_25DH110313_MyStore.Areas.Admin.Controllers
{
    public class OrderDetailsController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/OrderDetails
        public ActionResult Index()
        {
            var orderDetails = db.OrderDetails
                                 .Include(o => o.Order)
                                 .Include(o => o.Product)
                                 .OrderByDescending(o => o.OrderID);

            return View(orderDetails.ToList());
        }

        // GET: Admin/OrderDetails/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            OrderDetail orderDetail = db.OrderDetails
                                        .Include(o => o.Order)
                                        .Include(o => o.Product)
                                        .FirstOrDefault(o => o.ID == id);

            if (orderDetail == null)
                return HttpNotFound();

            return View(orderDetail);
        }

        // GET: Admin/OrderDetails/Create
        public ActionResult Create()
        {
            ViewBag.OrderID = new SelectList(
                db.Orders.OrderByDescending(o => o.OrderID),
                "OrderID", "OrderID");

            ViewBag.ProductID = new SelectList(
                db.Products.OrderBy(p => p.ProductName),
                "ProductID", "ProductName");

            return View();
        }

        // POST: Admin/OrderDetails/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,ProductID,OrderID,Quantity,UnitPrice")] OrderDetail orderDetail)
        {
            // Kiểm tra số lượng
            if (orderDetail.Quantity < 1)
            {
                ModelState.AddModelError("Quantity", "Số lượng phải lớn hơn 0.");
            }

            // Kiểm tra đơn giá
            if (orderDetail.UnitPrice < 0)
            {
                ModelState.AddModelError("UnitPrice", "Đơn giá không được âm.");
            }

            if (ModelState.IsValid)
            {
                var product = db.Products.Find(orderDetail.ProductID);

                if (product == null)
                    return HttpNotFound();

                var order = db.Orders.Find(orderDetail.OrderID);

                if (order == null)
                    return HttpNotFound();

                // Lưu đơn giá người quản trị đã nhập hoặc tùy chỉnh
                db.OrderDetails.Add(orderDetail);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.OrderID = new SelectList(
                db.Orders.OrderByDescending(o => o.OrderID),
                "OrderID", "OrderID", orderDetail.OrderID);

            ViewBag.ProductID = new SelectList(
                db.Products.OrderBy(p => p.ProductName),
                "ProductID", "ProductName", orderDetail.ProductID);

            return View(orderDetail);
        }

        // GET: Admin/OrderDetails/GetProductPrice/5
        public JsonResult GetProductPrice(int? id)
        {
            if (id == null)
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var product = db.Products.Find(id);

            if (product == null)
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                success = true,
                price = product.ProductPrice.ToString(
                    System.Globalization.CultureInfo.InvariantCulture)
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/OrderDetails/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            OrderDetail orderDetail = db.OrderDetails.Find(id);

            if (orderDetail == null)
                return HttpNotFound();

            ViewBag.OrderID = new SelectList(
                db.Orders.OrderByDescending(o => o.OrderID),
                "OrderID", "OrderID", orderDetail.OrderID);

            ViewBag.ProductID = new SelectList(
                db.Products.OrderBy(p => p.ProductName),
                "ProductID", "ProductName", orderDetail.ProductID);

            return View(orderDetail);
        }

        // POST: Admin/OrderDetails/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,ProductID,OrderID,Quantity,UnitPrice")] OrderDetail orderDetail)
        {
            if (orderDetail.Quantity < 1)
            {
                ModelState.AddModelError("Quantity", "Số lượng phải lớn hơn 0.");
            }

            if (orderDetail.UnitPrice < 0)
            {
                ModelState.AddModelError("UnitPrice", "Đơn giá không được âm.");
            }

            if (ModelState.IsValid)
            {
                var orderDetailInDb = db.OrderDetails.Find(orderDetail.ID);

                if (orderDetailInDb == null)
                    return HttpNotFound();

                var product = db.Products.Find(orderDetail.ProductID);

                if (product == null)
                    return HttpNotFound();

                var order = db.Orders.Find(orderDetail.OrderID);

                if (order == null)
                    return HttpNotFound();

                // Cập nhật thông tin chi tiết đơn hàng
                orderDetailInDb.ProductID = orderDetail.ProductID;
                orderDetailInDb.OrderID = orderDetail.OrderID;
                orderDetailInDb.Quantity = orderDetail.Quantity;

                // Lưu đơn giá đã tùy chỉnh
                orderDetailInDb.UnitPrice = orderDetail.UnitPrice;

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.OrderID = new SelectList(
                db.Orders.OrderByDescending(o => o.OrderID),
                "OrderID", "OrderID", orderDetail.OrderID);

            ViewBag.ProductID = new SelectList(
                db.Products.OrderBy(p => p.ProductName),
                "ProductID", "ProductName", orderDetail.ProductID);

            return View(orderDetail);
        }

        // GET: Admin/OrderDetails/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            OrderDetail orderDetail = db.OrderDetails
                                        .Include(o => o.Order)
                                        .Include(o => o.Product)
                                        .FirstOrDefault(o => o.ID == id);

            if (orderDetail == null)
                return HttpNotFound();

            return View(orderDetail);
        }

        // POST: Admin/OrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            OrderDetail orderDetail = db.OrderDetails.Find(id);

            if (orderDetail == null)
                return HttpNotFound();

            db.OrderDetails.Remove(orderDetail);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
