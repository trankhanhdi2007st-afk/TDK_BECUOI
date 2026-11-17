
using System.Linq;
using System.Web.Mvc;
using TKD_BECUOI.Models;

namespace TKD_BECUOI.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private TKD_BEEntities db = new TKD_BEEntities();

        // Danh sách sản phẩm
        public ActionResult Index()
        {
            var sanPham = db.Product.ToList();
            return View(sanPham);
        }

        // Thêm sản phẩm

        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(
                db.Category.ToList(),
                "CategoryID",
                "CategoryName"
            );

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                db.Product.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(product);
        }

        // Sửa sản phẩm
        public ActionResult Edit(int id)
        {
            var product = db.Product.Find(id);

            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State =
                    System.Data.Entity.EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(product);
        }

        // Xóa sản phẩm
        public ActionResult Delete(int id)
        {
            var product = db.Product.Find(id);

            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var product = db.Product.Find(id);

            if (product != null)
            {
                db.Product.Remove(product);
                db.SaveChanges();
            }

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
