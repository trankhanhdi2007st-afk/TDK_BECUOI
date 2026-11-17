
using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using TKD_BECUOI.Models;

namespace TKD_BECUOI.Controllers
{
    public class HomeController : Controller
    {
        // Trang chủ và danh sách sản phẩm
        public ActionResult Index(string tuKhoa, int? categoryId)
        {
            using (var db = new TKD_BEEntities())
            {
                // Lấy danh sách sản phẩm kèm danh mục
                var products = db.Product
                    .Include(p => p.Category)
                    .AsQueryable();

                // Tìm kiếm theo tên sản phẩm
                if (!String.IsNullOrEmpty(tuKhoa))
                {
                    products = products.Where(p =>
                        p.ProductName.Contains(tuKhoa));
                }

                // Lọc theo danh mục
                if (categoryId.HasValue)
                {
                    products = products.Where(p =>
                        p.CategoryID == categoryId.Value);
                }

                // Lấy danh mục từ database
                ViewBag.Categories = db.Category
                    .OrderBy(c => c.CategoryID)
                    .ToList();

                ViewBag.TuKhoa = tuKhoa;
                ViewBag.CategoryId = categoryId;

                // Sản phẩm mới nhất hiển thị trước
                var danhSachSanPham = products
                    .OrderByDescending(p => p.ProductID)
                    .ToList();

                return View(danhSachSanPham);
            }
        }

        // Trang chi tiết sản phẩm
        public ActionResult Details(int id)
        {
            using (var db = new TKD_BEEntities())
            {
                var product = db.Product
                    .Include(p => p.Category)
                    .FirstOrDefault(p => p.ProductID == id);

                if (product == null)
                {
                    return HttpNotFound();
                }

                return View(product);
            }
        }
    }
}
