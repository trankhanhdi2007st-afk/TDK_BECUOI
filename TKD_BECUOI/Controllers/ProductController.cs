using System.Linq;
using System.Web.Mvc;
using TKD_BECUOI.Models;

namespace TKD_BECUOI.Controllers
{
    public class ProductController : Controller
    {


        
        private TKD_BECUOI.Models.TKD_BEEntities db = new TKD_BECUOI.Models.TKD_BEEntities();
        public ActionResult Details(int id)
        {
            var sanPham = db.Product
                .FirstOrDefault(x => x.ProductID == id);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            return View(sanPham);
        }
    }
}