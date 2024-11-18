using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TranTanPhuc.Models;

namespace TranTanPhuc.Areas.Admin.Controllers
{
    public class NhaXuatBanController : Controller
    {
        SachOnlineEntities db = new SachOnlineEntities();
        // GET: Admin/NhaXuatBan
        public ActionResult Index()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var data = db.NHAXUATBANs.ToList();
            return View(data);
        }
        //Chi tiet NXB
        public ActionResult Detail(int id)
        {
            //int maNXB =int.Parse(Request.QueryString["id"]);
            var kq = db.NHAXUATBANs.Where(nxb => nxb.MaNXB == id).SingleOrDefault();
            return View(kq);
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Luu(FormCollection f)
        {
            NHAXUATBAN nxb = new NHAXUATBAN();
            nxb.TenNXB = f["TenNXB"];
            nxb.DiaChi = f["DiaChi"];
            nxb.DienThoai = f["DienThoai"];
            db.NHAXUATBANs.Add(nxb);
            db.SaveChanges();

            return View("Create");
        }
        [HttpPost]
        public ActionResult Create(NHAXUATBAN nxb)
        {
            db.NHAXUATBANs.Add(nxb);
            db.SaveChanges();

            return RedirectToAction("Index", "NhaXuatBan");
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            //int maNXB =int.Parse(Request.QueryString["id"]);
            var kq = db.NHAXUATBANs.Where(nxb => nxb.MaNXB == id).SingleOrDefault();
            return View(kq);
        }
        [HttpPost]
        public ActionResult Edit(FormCollection f)
        {
            int maNXB = int.Parse(f["MaNXB"]);
            NHAXUATBAN nxb = db.NHAXUATBANs.Where(n => n.MaNXB == maNXB).SingleOrDefault();
            nxb.TenNXB = f["TenNXB"];
            nxb.DiaChi = f["DiaChi"];
            nxb.DienThoai = f["DienThoai"];
            db.SaveChanges();

            return RedirectToAction("Index", "NhaXuatBan");
        }
    }

}