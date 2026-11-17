
using NguyeNhatDuy_25DH110313_MyStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Diagnostics;

namespace NguyeNhatDuy_25DH110313_MyStore.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Lấy giỏ hàng từ Session
        private List<CartItem> GetCart()
        {
            if (Session["Cart"] == null)
            {
                Session["Cart"] = new List<CartItem>();
            }

            return (List<CartItem>)Session["Cart"];
        }

        // GET: Cart
        // Hiển thị giỏ hàng
        public ActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // POST: Cart/AddToCart
        // Thêm sản phẩm vào giỏ hàng thông thường
        [HttpPost]
        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var product = db.Products.Find(id);

            if (product == null || !product.IsActive)
            {
                TempData["CartError"] =
                    "Sản phẩm không tồn tại hoặc hiện không còn kinh doanh.";

                return RedirectToAction("Index");
            }

            quantity = Math.Max(1, Math.Min(quantity, 99));

            var cart = GetCart();

            var existingItem = cart.FirstOrDefault(
                x => x.ProductID == id
            );

            if (existingItem != null)
            {
                existingItem.Quantity = Math.Min(
                    existingItem.Quantity + quantity, 99
                );

                existingItem.ProductName = product.ProductName;
                existingItem.ProductImage = product.ProductImage;
                existingItem.UnitPrice = product.ProductPrice;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ProductImage = product.ProductImage,
                    UnitPrice = product.ProductPrice,
                    Quantity = quantity
                });
            }

            Session["Cart"] = cart;

            return RedirectToAction("Index");
        }

        // POST: Cart/BuyAgain/5
        // Mua lại một sản phẩm với số lượng luôn bằng 1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuyAgain(int id)
        {
            var product = db.Products.Find(id);

            if (product == null || !product.IsActive)
            {
                TempData["CartError"] =
                    "Sản phẩm không tồn tại hoặc hiện không còn kinh doanh.";

                return RedirectToAction("Index", "PurchaseHistory");
            }

            var cart = GetCart();

            // Xóa tất cả dòng sản phẩm trùng trong giỏ hàng
            cart.RemoveAll(x => x.ProductID == product.ProductID);

            // Thêm lại sản phẩm với số lượng đúng bằng 1
            cart.Add(new CartItem
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                ProductImage = product.ProductImage,
                UnitPrice = product.ProductPrice,
                Quantity = 1
            });

            Session["Cart"] = cart;

            TempData["CartSuccess"] =
                "Đã thêm sản phẩm vào giỏ hàng với số lượng 1.";

            return RedirectToAction("Index");
        }

        // POST: Cart/UpdateQuantity
        // Cập nhật số lượng sản phẩm
        [HttpPost]
        public ActionResult UpdateQuantity(int productID, int quantity)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(
                x => x.ProductID == productID
            );

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = Math.Min(quantity, 99);
                }
            }

            Session["Cart"] = cart;

            return RedirectToAction("Index");
        }

        // POST: Cart/RemoveFromCart
        // Xóa một sản phẩm khỏi giỏ hàng
        [HttpPost]
        public ActionResult RemoveFromCart(int productID)
        {
            var cart = GetCart();

            cart.RemoveAll(x => x.ProductID == productID);

            Session["Cart"] = cart;

            return RedirectToAction("Index");
        }

        // POST: Cart/ClearCart
        // Xóa toàn bộ giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearCart()
        {
            Session["Cart"] = new List<CartItem>();

            return RedirectToAction("Index");
        }

        // GET: Cart/Checkout
        // Hiển thị trang xác nhận đặt hàng
        public ActionResult Checkout()
        {
            if (Session["Username"] == null ||
                Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCart();

            if (cart.Count == 0)
            {
                TempData["CartError"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index");
            }

            string username = Session["Username"].ToString();

            var customer = db.Customers.FirstOrDefault(
                c => c.Username == username
            );

            if (customer == null)
            {
                TempData["CartError"] =
                    "Không tìm thấy thông tin khách hàng.";

                return RedirectToAction("Index");
            }

            return View(customer);
        }

        // POST: Cart/Checkout
        // Xác nhận và lưu đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(Customer model)
        {
            // Kiểm tra đăng nhập
            if (Session["Username"] == null ||
                Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCart();

            if (cart == null || cart.Count == 0)
            {
                TempData["CartError"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index");
            }

            string username = Session["Username"].ToString();

            var customer = db.Customers.FirstOrDefault(
                c => c.Username == username
            );

            if (customer == null)
            {
                ModelState.AddModelError(
                    "",
                    "Không tìm thấy thông tin khách hàng."
                );

                return View(model);
            }

            // Kiểm tra dữ liệu giao hàng
            if (model == null ||
                string.IsNullOrWhiteSpace(model.CustomerName) ||
                string.IsNullOrWhiteSpace(model.CustomerPhone) ||
                string.IsNullOrWhiteSpace(model.CustomerEmail) ||
                string.IsNullOrWhiteSpace(model.CustomerAddress))
            {
                ModelState.AddModelError(
                    "",
                    "Vui lòng nhập đầy đủ thông tin giao hàng."
                );

                return View(model ?? customer);
            }

            // Kiểm tra số lượng trong giỏ hàng
            if (cart.Any(x => x.Quantity < 1 || x.Quantity > 99))
            {
                ModelState.AddModelError(
                    "",
                    "Số lượng sản phẩm không hợp lệ."
                );

                return View(model);
            }

            try
            {
                decimal totalAmount = 0;

                var orderDetails = new List<OrderDetail>();

                // Lấy thông tin và giá mới nhất từ database
                foreach (var item in cart)
                {
                    var product = db.Products.Find(item.ProductID);

                    if (product == null || !product.IsActive)
                    {
                        ModelState.AddModelError(
                            "",
                            "Sản phẩm \"" + item.ProductName +
                            "\" không còn kinh doanh. Vui lòng kiểm tra lại giỏ hàng."
                        );

                        return View(model);
                    }

                    decimal unitPrice = product.ProductPrice;

                    totalAmount += unitPrice * item.Quantity;

                    orderDetails.Add(new OrderDetail
                    {
                        ProductID = product.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice
                    });
                }

                // Bắt đầu transaction
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Cập nhật thông tin khách hàng
                        customer.CustomerName = model.CustomerName.Trim();
                        customer.CustomerPhone = model.CustomerPhone.Trim();
                        customer.CustomerEmail = model.CustomerEmail.Trim();
                        customer.CustomerAddress = model.CustomerAddress.Trim();

                        // Tạo đơn hàng mới
                        Order order = new Order
                        {
                            CustomerID = customer.CustomerID,
                            OrderDate = DateTime.Now,
                            TotalAmount = totalAmount,
                            PaymentStatus = "Chưa thanh toán",
                            OrderStatus = "Đang xử lý",
                            AddressDelivery = model.CustomerAddress.Trim()
                        };

                        // Thêm chi tiết sản phẩm vào đơn hàng
                        foreach (var detail in orderDetails)
                        {
                            order.OrderDetails.Add(detail);
                        }

                        db.Orders.Add(order);

                        db.SaveChanges();

                        transaction.Commit();

                        // Chỉ xóa giỏ hàng khi lưu thành công
                        Session["Cart"] = new List<CartItem>();

                        TempData["OrderSuccess"] =
                            "Đặt hàng thành công! Mã đơn hàng: " +
                            order.OrderID;

                        return RedirectToAction("OrderSuccess");
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LỖI ĐẶT HÀNG:");
                Debug.WriteLine(ex.ToString());

                ModelState.AddModelError(
                    "",
                    "Đã xảy ra lỗi khi đặt hàng. Vui lòng thử lại."
                );

                return View(model);
            }
        }

        // GET: Cart/OrderSuccess
        // Trang thông báo đặt hàng thành công
        public ActionResult OrderSuccess()
        {
            if (TempData["OrderSuccess"] == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Message = TempData["OrderSuccess"];

            return View();
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
