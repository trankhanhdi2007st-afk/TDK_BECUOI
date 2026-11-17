using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using TKD_BECUOI.Models;

namespace TKD_BECUOI.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {



        private TKD_BECUOI.Models.TKD_BEEntities db = new TKD_BECUOI.Models.TKD_BEEntities();

        // Danh sách trạng thái đơn hàng
        private List<string> LayDanhSachTrangThai()
        {
            return new List<string>
            {
                "Chờ thanh toán",
                "Đang xử lý",
                "Đang vận chuyển",
                "Đã giao",
                "Đã hủy"
            };
        }

        // GET: Admin/Orders
        public ActionResult Index(
            string tenKhachHang,
            string trangThaiThanhToan,
            DateTime? ngayDatHang,
            string sapXep,
            int trang = 1)
        {
            int soDonMoiTrang = 15;

            // Lấy toàn bộ đơn hàng và thông tin khách hàng
            var danhSach = db.Order
                .Include(x => x.Customer)
                .AsQueryable();

            // Tìm theo tên khách hàng
            if (!String.IsNullOrEmpty(tenKhachHang))
            {
                danhSach = danhSach.Where(x =>
                    x.Customer.CustomerName.Contains(tenKhachHang));
            }

            // Tìm theo trạng thái thanh toán
            if (!String.IsNullOrEmpty(trangThaiThanhToan))
            {
                danhSach = danhSach.Where(x =>
                    x.PaymentStatus != null &&
                    x.PaymentStatus.Contains(trangThaiThanhToan));
            }

            // Tìm theo ngày đặt hàng
            if (ngayDatHang.HasValue)
            {
                DateTime ngay = ngayDatHang.Value.Date;

                danhSach = danhSach.Where(x =>
                    DbFunctions.TruncateTime(x.OrderDate) == ngay);
            }

            // Sắp xếp
            switch (sapXep)
            {
                case "cu":
                    danhSach = danhSach.OrderBy(x => x.OrderDate);
                    break;

                case "tongtien_tang":
                    danhSach = danhSach.OrderBy(x => x.TotalAmount);
                    break;

                case "tongtien_giam":
                    danhSach = danhSach.OrderByDescending(x => x.TotalAmount);
                    break;

                default:
                    danhSach = danhSach.OrderByDescending(x => x.OrderDate);
                    break;
            }

            // Tổng số đơn hàng sau khi tìm kiếm
            int tongSoDon = danhSach.Count();

            // Tổng số trang
            int tongSoTrang = (int)Math.Ceiling(
                (double)tongSoDon / soDonMoiTrang
            );

            // Không cho số trang nhỏ hơn 1
            if (trang < 1)
            {
                trang = 1;
            }

            if (tongSoTrang > 0 && trang > tongSoTrang)
            {
                trang = tongSoTrang;
            }

            // Lấy 15 đơn hàng của trang hiện tại
            var ketQua = danhSach
                .Skip((trang - 1) * soDonMoiTrang)
                .Take(soDonMoiTrang)
                .ToList();

            ViewBag.TenKhachHang = tenKhachHang;
            ViewBag.TrangThaiThanhToan = trangThaiThanhToan;

            ViewBag.NgayDatHang = ngayDatHang.HasValue
                ? ngayDatHang.Value.ToString("yyyy-MM-dd")
                : "";

            ViewBag.SapXep = sapXep;

            ViewBag.TrangHienTai = trang;
            ViewBag.TongSoTrang = tongSoTrang;
            ViewBag.TongSoDon = tongSoDon;

            return View(ketQua);
        }

        // GET: Admin/Orders/DetailedOrder/5
        public ActionResult DetailedOrder(int id)
        {
            var donHang = db.Order
                .Include(x => x.Customer)
                .Include(x => x.OrderDetail.Select(ct => ct.Product))
                .FirstOrDefault(x => x.OrderID == id);

            if (donHang == null)
            {
                return HttpNotFound();
            }

            return View(donHang);
        }

        // GET: Admin/Orders/UpdateStatus/5
        public ActionResult UpdateStatus(int id)
        {
            var donHang = db.Order.Find(id);

            if (donHang == null)
            {
                return HttpNotFound();
            }

            ViewBag.DanhSachTrangThai = new SelectList(
                LayDanhSachTrangThai(),
                donHang.OrderStatus
            );

            return View(donHang);
        }

        // POST: Admin/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string trangThaiMoi)
        {
            var donHang = db.Order.Find(id);

            if (donHang == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra trạng thái hợp lệ
            if (!LayDanhSachTrangThai().Contains(trangThaiMoi))
            {
                ModelState.AddModelError(
                    "",
                    "Trạng thái đơn hàng không hợp lệ."
                );

                ViewBag.DanhSachTrangThai = new SelectList(
                    LayDanhSachTrangThai(),
                    donHang.OrderStatus
                );

                return View(donHang);
            }

            // Cập nhật trạng thái
            donHang.OrderStatus = trangThaiMoi;

            db.SaveChanges();

            TempData["ThongBao"] = "Cập nhật trạng thái đơn hàng thành công!";

            return RedirectToAction("DetailedOrder", new { id = id });
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