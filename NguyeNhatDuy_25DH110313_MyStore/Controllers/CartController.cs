
using NguyeNhatDuy_25DH110313_MyStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

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

        // Hiển thị giỏ hàng
        public ActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var product = db.Products.Find(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            if (quantity < 1)
            {
                quantity = 1;
            }

            if (quantity > 99)
            {
                quantity = 99;
            }

            var cart = GetCart();

            var existingItem = cart.FirstOrDefault(
                x => x.ProductID == id
            );

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;

                if (existingItem.Quantity > 99)
                {
                    existingItem.Quantity = 99;
                }
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

        // Xóa một sản phẩm khỏi giỏ hàng
        [HttpPost]
        public ActionResult RemoveFromCart(int productID)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(
                x => x.ProductID == productID
            );

            if (item != null)
            {
                cart.Remove(item);
            }

            Session["Cart"] = cart;

            return RedirectToAction("Index");
        }

        // Xóa toàn bộ giỏ hàng
        [HttpPost]
        public ActionResult ClearCart()
        {
            Session["Cart"] = new List<CartItem>();

            return RedirectToAction("Index");
        }

        // GET: Cart/Checkout
        // Hiển thị trang xác nhận đặt hàng
        public ActionResult Checkout()
        {
            // Kiểm tra đăng nhập
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
                ModelState.AddModelError(
                    "",
                    "Không tìm thấy thông tin khách hàng."
                );

                return View(customer);
            }

            // Kiểm tra thông tin giao hàng
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

                return View(customer);
            }

            try
            {
                decimal totalAmount = 0;

                var orderDetails = new List<OrderDetail>();

                // Lấy giá sản phẩm mới nhất từ database
                foreach (var item in cart)
                {
                    if (item.Quantity < 1 || item.Quantity > 99)
                    {
                        ModelState.AddModelError(
                            "",
                            "Số lượng sản phẩm không hợp lệ."
                        );

                        return View(customer);
                    }

                    var product = db.Products.Find(item.ProductID);

                    if (product == null)
                    {
                        ModelState.AddModelError(
                            "",
                            "Một sản phẩm trong giỏ hàng không còn tồn tại."
                        );

                        return View(customer);
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

                // Cập nhật thông tin khách hàng
                customer.CustomerName = model.CustomerName.Trim();
                customer.CustomerPhone = model.CustomerPhone.Trim();
                customer.CustomerEmail = model.CustomerEmail.Trim();
                customer.CustomerAddress = model.CustomerAddress.Trim();

                // Tạo đơn hàng
                Order order = new Order
                {
                    CustomerID = customer.CustomerID,
                    OrderDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    PaymentStatus = "Chưa thanh toán",
                    AddressDelivery = model.CustomerAddress.Trim()
                };

                // Gắn các chi tiết đơn hàng
                foreach (var detail in orderDetails)
                {
                    order.OrderDetails.Add(detail);
                }

                db.Orders.Add(order);

                // Lưu đơn hàng và chi tiết đơn hàng
                db.SaveChanges();

                // Chỉ xóa giỏ hàng sau khi lưu thành công
                Session["Cart"] = new List<CartItem>();

                TempData["OrderSuccess"] =
                    "Đặt hàng thành công! Mã đơn hàng: " + order.OrderID;

                return RedirectToAction("OrderSuccess");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "",
                    "Đã xảy ra lỗi khi đặt hàng. Vui lòng thử lại."
                );

                return View(customer);
            }
        }

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
