
using System;
using System.Linq;
using System.Web.Mvc;
using TKD_BECUOI.Models;

namespace TDK_BECUOI.Controllers
{
    public class AccountController : Controller
    {
        private TKD_BEEntities db = new TKD_BEEntities();

        // GET: Account/Login
        public ActionResult Login()
        {
            if (Session["Username"] != null)
                return RedirectToAction("Index", "Home");

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

            var user = db.User.FirstOrDefault(u =>
                u.Username == username.Trim() &&
                u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng.";
                return View();
            }

            Session["Username"] = user.Username;
            Session["UserRole"] = user.UserRole;

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            if (Session["Username"] != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(
            string username,
            string password,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            username = username.Trim();

            if (username.Length < 3)
            {
                ViewBag.Error = "Tên đăng nhập phải có ít nhất 3 ký tự.";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            try
            {
                bool exists = db.User.Any(u =>
                    u.Username == username);

                if (exists)
                {
                    ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                    return View();
                }

                var user = new User
                {
                    Username = username,
                    Password = password,
                    UserRole = "C"
                };

                db.User.Add(user);
                db.SaveChanges();

                TempData["Success"] =
                    "Đăng ký thành công! Hãy đăng nhập.";

                return RedirectToAction("Login");
            }

            catch (Exception ex)
            {
                var dbEx = ex as
                    System.Data.Entity.Validation.DbEntityValidationException;

                if (dbEx != null)
                {
                    var errors = dbEx.EntityValidationErrors
                        .SelectMany(e => e.ValidationErrors)
                        .Select(e => e.PropertyName + ": " + e.ErrorMessage);

                    ViewBag.Error = "Lỗi dữ liệu: " +
                        string.Join(" | ", errors);
                }
                else
                {
                    var error = ex;

                    while (error.InnerException != null)
                        error = error.InnerException;

                    ViewBag.Error = "Lỗi database: " + error.Message;
                }

                return View();
            }

        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Remove("Username");
            Session.Remove("UserRole");

            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
