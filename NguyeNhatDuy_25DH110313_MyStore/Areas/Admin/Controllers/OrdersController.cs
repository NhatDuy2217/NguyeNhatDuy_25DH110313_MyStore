
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NguyeNhatDuy_25DH110313_MyStore.Models;

namespace NguyeNhatDuy_25DH110313_MyStore.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Orders
        public ActionResult Index()
        {
            var orders = db.Orders
                           .Include(o => o.Customer)
                           .OrderByDescending(o => o.OrderDate);

            return View(orders.ToList());
        }

        // GET: Admin/Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Order order = db.Orders
                            .Include(o => o.Customer)
                            .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // GET: Admin/Orders/Create
        public ActionResult Create()
        {
            ViewBag.CustomerID = new SelectList(
                db.Customers,
                "CustomerID",
                "CustomerName"
            );

            return View();
        }

        // POST: Admin/Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "OrderID,CustomerID,OrderDate,TotalAmount,PaymentStatus,AddressDelivery")]
            Order order)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Orders.Add(order);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Thêm đơn hàng thành công!";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể thêm đơn hàng: " +
                        ex.GetBaseException().Message
                    );
                }
            }

            ViewBag.CustomerID = new SelectList(
                db.Customers,
                "CustomerID",
                "CustomerName",
                order.CustomerID
            );

            return View(order);
        }

        // GET: Admin/Orders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Order order = db.Orders.Find(id);

            if (order == null)
            {
                return HttpNotFound();
            }

            ViewBag.CustomerID = new SelectList(
                db.Customers,
                "CustomerID",
                "CustomerName",
                order.CustomerID
            );

            return View(order);
        }

        // POST: Admin/Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "OrderID,CustomerID,OrderDate,TotalAmount,PaymentStatus,AddressDelivery")]
            Order order)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var orderInDb = db.Orders.Find(order.OrderID);

                    if (orderInDb == null)
                    {
                        return HttpNotFound();
                    }

                    orderInDb.CustomerID = order.CustomerID;
                    orderInDb.OrderDate = order.OrderDate;
                    orderInDb.TotalAmount = order.TotalAmount;
                    orderInDb.PaymentStatus = order.PaymentStatus;
                    orderInDb.AddressDelivery = order.AddressDelivery;

                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Cập nhật đơn hàng thành công!";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể cập nhật đơn hàng: " +
                        ex.GetBaseException().Message
                    );
                }
            }

            ViewBag.CustomerID = new SelectList(
                db.Customers,
                "CustomerID",
                "CustomerName",
                order.CustomerID
            );

            return View(order);
        }

        // GET: Admin/Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Order order = db.Orders
                            .Include(o => o.Customer)
                            .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra số chi tiết sản phẩm trong đơn hàng
            int orderDetailCount = db.OrderDetails
                                     .Count(d => d.OrderID == id.Value);

            ViewBag.OrderDetailCount = orderDetailCount;

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders
                            .Include(o => o.Customer)
                            .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            try
            {
                // Kiểm tra đơn hàng có chi tiết sản phẩm hay không
                int orderDetailCount = db.OrderDetails
                                         .Count(d => d.OrderID == id);

                ViewBag.OrderDetailCount = orderDetailCount;

                // Nếu đơn hàng có chi tiết thì không cho xóa
                if (orderDetailCount > 0)
                {
                    ViewBag.DeleteError =
                        "Không thể xóa đơn hàng này vì đang có " +
                        orderDetailCount +
                        " chi tiết sản phẩm liên kết. " +
                        "Vui lòng giữ lại đơn hàng để bảo toàn lịch sử mua hàng.";

                    return View("Delete", order);
                }

                // Nếu không có chi tiết thì xóa đơn hàng
                db.Orders.Remove(order);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Xóa đơn hàng thành công!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.OrderDetailCount = db.OrderDetails
                                             .Count(d => d.OrderID == id);

                ViewBag.DeleteError =
                    "Không thể xóa đơn hàng: " +
                    ex.GetBaseException().Message;

                return View("Delete", order);
            }
        }

        // Giải phóng tài nguyên
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
