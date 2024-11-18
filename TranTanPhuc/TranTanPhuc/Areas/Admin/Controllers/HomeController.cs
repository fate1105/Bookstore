using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TranTanPhuc.Models;

namespace TranTanPhuc.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        SachOnlineEntities db = new SachOnlineEntities();
        // GET: Admin/Home
        public ActionResult Index()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection f)
        {
            var sTenDN = f["UserName"];
            var sMatKhau = f["Password"];
            ADMIN ad = db.ADMINs.SingleOrDefault(n => n.TenDN == sTenDN && n.MatKhau == sMatKhau);
            if (ad != null)
            {
                Session["Admin"] = ad;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
            }
            return View();
        }
        public ActionResult LoginLogout()
        {
            return PartialView("LoginLogout");
        }
        public ActionResult DangXuat()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
        public ActionResult Search(string strSearch)
        {
            ViewBag.Search = strSearch;
            if (!string.IsNullOrEmpty(strSearch))
            {
                var kq = from s in db.SACHes
                         join cd in db.CHUDEs on s.MaCD equals cd.MaCD
                         join nxb in db.NHAXUATBANs on s.MaNXB equals nxb.MaNXB
                         where s.TenSach.Contains(strSearch)
                            || s.MoTa.Contains(strSearch)
                            || cd.TenChuDe.Contains(strSearch)
                            || nxb.TenNXB.Contains(strSearch)
                         orderby s.NgayCapNhat descending
                         select s;
                return View(kq);
            }
            return View();
        }
    }
}