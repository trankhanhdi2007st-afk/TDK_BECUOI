
using TKD_BECUOI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace TKD_BECUOI.Controllers
{
    public class CartController : Controller
    {
        private TKD_BEEntities db = new TKD_BEEntities();

        // Lấy giỏ hàng
        private List<CartItem> LayGioHang()
        {
            List<CartItem> gioHang =
                Session["Cart"] as List<CartItem>;

            if (gioHang == null)
            {
                gioHang = new List<CartItem>();
                Session["Cart"] = gioHang;
            }

            return gioHang;
        }

        // Hiển thị giỏ hàng
        public ActionResult Index()
        {
            return View(LayGioHang());
        }

        // Thêm sản phẩm vào giỏ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int productID)
        {
            var sanPham = db.Product.Find(productID);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            var gioHang = LayGioHang();

            var item = gioHang.FirstOrDefault(
                x => x.ProductID == productID
            );

            if (item != null)
            {
                item.Quantity++;
            }
            else
            {
                gioHang.Add(new CartItem
                {
                    ProductID = sanPham.ProductID,
                    ProductName = sanPham.ProductName,
                    ProductImage = sanPham.ProductImage,
                    ProductPrice = sanPham.ProductPrice,
                    Quantity = 1
                });
            }

            Session["Cart"] = gioHang;

            TempData["ThongBao"] =
                "Đã thêm sản phẩm vào giỏ hàng!";

            return RedirectToAction("Index");
        }

        // Cập nhật số lượng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(int productID, int quantity)
        {
            var gioHang = LayGioHang();

            var item = gioHang.FirstOrDefault(
                x => x.ProductID == productID
            );

            if (item != null)
            {
                if (quantity <= 0)
                {
                    gioHang.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }

            Session["Cart"] = gioHang;

            return RedirectToAction("Index");
        }

        // Xóa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int productID)
        {
            var gioHang = LayGioHang();

            gioHang.RemoveAll(
                x => x.ProductID == productID
            );

            Session["Cart"] = gioHang;

            return RedirectToAction("Index");
        }
        // GET: Cart/Checkout
        [HttpGet]
        public ActionResult Checkout()
        {
            var gioHang = LayGioHang();

            if (gioHang == null || gioHang.Count == 0)
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        // POST: Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(
            string hoTen,
            string soDienThoai,
            string email,
            string diaChi,
            string phuongThucThanhToan)
        {
            var gioHang = LayGioHang();

            if (gioHang == null || gioHang.Count == 0)
            {
                return RedirectToAction("Index");
            }

            if (String.IsNullOrWhiteSpace(hoTen) ||
                String.IsNullOrWhiteSpace(soDienThoai) ||
                String.IsNullOrWhiteSpace(diaChi) ||
                String.IsNullOrWhiteSpace(phuongThucThanhToan))
            {
                ViewBag.Loi = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Tạo tài khoản hệ thống tự động cho khách mua vãng lai.
                    // Khách không cần đăng nhập hay đặt mật khẩu.
                    string username = "guest_" + Guid.NewGuid().ToString("N");

                    User user = new User
                    {
                        Username = username,
                        Password = Guid.NewGuid().ToString("N").PadRight(50, '0'),
                        UserRole = "C"
                    };

                    db.User.Add(user);

                    Customer customer = new Customer
                    {
                        CustomerName = hoTen.Trim(),
                        CustomerPhone = soDienThoai.Trim(),
                        CustomerEmail = email,
                        CustomerAddress = diaChi.Trim(),
                        Username = username
                    };

                    db.Customer.Add(customer);

                    db.SaveChanges();

                    // Tính tổng tiền
                    decimal tongTien = gioHang.Sum(
                        x => x.ProductPrice * x.Quantity
                    );

                    // Tạo đơn hàng
                    Order donHang = new Order
                    {
                        CustomerID = customer.CustomerID,
                        OrderDate = DateTime.Today,
                        TotalAmount = tongTien,
                        PaymentStatus = "Chưa thanh toán",
                        OrderStatus = "Chờ thanh toán",
                        AddressDelivery = diaChi.Trim(),
                        PaymentMethod = phuongThucThanhToan,
                        DeliveryMethod = "Giao hàng"
                    };

                    db.Order.Add(donHang);
                    db.SaveChanges();

                    // Lưu từng sản phẩm vào chi tiết đơn hàng
                    foreach (var item in gioHang)
                    {
                        OrderDetail chiTiet = new OrderDetail
                        {
                            OrderID = donHang.OrderID,
                            ProductID = item.ProductID,
                            Quantity = item.Quantity,
                            UnitPrice = item.ProductPrice
                        };

                        db.OrderDetail.Add(chiTiet);
                    }

                    db.SaveChanges();

                    // Ghi nhớ khách hàng hiện tại để xem lịch sử mua hàng
                    Session["GuestCustomerID"] = customer.CustomerID;

                    // Xóa giỏ hàng sau khi đặt thành công
                    Session["Cart"] = null;

                    transaction.Commit();

                    return RedirectToAction(
                        "Success",
                        new { id = donHang.OrderID }
                    );
                }
                catch
                {
                    transaction.Rollback();

                    ViewBag.Loi = "Đặt hàng thất bại. Vui lòng thử lại.";
                    return View();
                }
            }
        }

        // Trang đặt hàng thành công
        public ActionResult Success(int id)
        {
            ViewBag.MaDonHang = id;
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
