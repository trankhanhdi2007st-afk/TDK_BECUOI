
using System.Linq;
using System.Web.Mvc;
using TKD_BECUOI.Models;

namespace TDK_BECUOI.Controllers
{
    public class MyOrderController : Controller
    {
        private TKD_BEEntities db = new TKD_BEEntities();

        public ActionResult Index()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString();

            var customer = db.Customer.FirstOrDefault(c =>
                c.Username == username);

            if (customer == null)
            {
                ViewBag.Error =
                    "Tài khoản chưa được liên kết với thông tin khách hàng.";
                return View(new System.Collections.Generic.List<Order>());
            }

            var orders = db.Order
                .Where(o => o.CustomerID == customer.CustomerID)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
