using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TranTanPhuc.Controllers
{
    public class PhucDemoController : Controller
    {
        // GET: TranTanPhuc
        public ActionResult DanhSach()
        {
            return View();
        }
        public ActionResult PartialNav()
        {
            return PartialView();
        }
    }
}