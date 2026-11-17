using System;
using System.Linq;
using System.Web.Mvc;
using TKD_BECUOI.Models;
using System.Data.Entity;

namespace KTBE_DHQH.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {



        private TKD_BECUOI.Models.TKD_BEEntities db = new TKD_BECUOI.Models.TKD_BEEntities();

        public ActionResult Index()
        {
            // Tổng số đơn hàng
            ViewBag.TongDonHang = db.Order.Count();

            // Tổng số khách hàng
            ViewBag.TongKhachHang = db.Customer.Count();

            // Tổng số sản phẩm
            ViewBag.TongSanPham = db.Product.Count();

            // Số đơn hàng đã giao
            ViewBag.DonDaGiao = db.Order.Count(
                x => x.OrderStatus == "Đã giao"
            );

            // Tổng doanh thu từ đơn đã giao
            ViewBag.DoanhThu = db.Order
                .Where(x => x.OrderStatus == "Đã giao")
                .Select(x => (decimal?)x.TotalAmount)
                .DefaultIfEmpty(0)
                .Sum();

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