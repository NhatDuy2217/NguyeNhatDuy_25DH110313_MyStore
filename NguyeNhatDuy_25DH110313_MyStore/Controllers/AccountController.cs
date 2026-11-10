using NguyeNhatDuy_25DH110313_MyStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NguyeNhatDuy_25DH110313_MyStore.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(
            Customer model,
            string password,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(model.Username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập tài khoản và mật khẩu.");
                return View(model);
            }

            if (password.Length > 50)
            {
                ModelState.AddModelError("", "Mật khẩu không được vượt quá 50 ký tự.");
                return View(model);
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                return View(model);
            }

            string username = model.Username.Trim();

            bool exists = db.Users.Any(u => u.Username == username);

            if (exists)
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                return View(model);
            }

            User user = new User
            {
                Username = username,
                Password = password,
                UserRole = "Customer"
            };

            model.Username = username;
            model.User = user;

            user.Customers.Add(model);

            db.Users.Add(user);
            db.SaveChanges();

            TempData["Success"] = "Đăng ký tài khoản thành công!";
            return RedirectToAction("Login");
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tài khoản và mật khẩu.";
                return View();
            }

            username = username.Trim();

            var user = db.Users.FirstOrDefault(u =>
                u.Username == username &&
                u.Password.Trim() == password);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                return View();
            }

            Session["Username"] = user.Username;
            Session["UserRole"] = user.UserRole.Trim();

            if (user.UserRole.Trim() == "Admin")
            {
                return RedirectToAction(
                    "Index", "Home", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Home");
        }

        // Đăng xuất
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Index", "Home");
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