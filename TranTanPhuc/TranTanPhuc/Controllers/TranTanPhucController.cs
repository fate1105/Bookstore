using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using PagedList.Mvc;
using TranTanPhuc.Models;
using Com.CloudRail.SI.Types;
using Microsoft.Ajax.Utilities;

namespace TranTanPhuc.Controllers
{
    public class TranTanPhucController : Controller
    {
        SachOnlineEntities db = new SachOnlineEntities();
        private List<SACH> LaySachMoi(int count)
        {
            return db.SACHes.OrderByDescending(a => a.NgayCapNhat).Take(count).ToList();
        }
        private List<SACH> LaySachBanNhieu(int count)
        {
            return db.SACHes.OrderByDescending(a => a.SoLuongBan).Take(count).ToList();
        }
        public ActionResult Index()
        {
            var listSachMoi = LaySachMoi(6);
            return View(listSachMoi);
        }
        public ActionResult ChudePartial()
        {
            var dbChuDe = db.CHUDEs.ToList();   
            return PartialView(dbChuDe);
        }
        public ActionResult NXBPartial()
        {
            var dbNXB = db.NHAXUATBANs.ToList();
            return PartialView(dbNXB);
        }
        public ActionResult SliderPartial()
        {
            return PartialView();
        }
        public ActionResult SachBanNhieuPartial()
        {
            var listSachBanNhieu = LaySachBanNhieu(6);
            return PartialView(listSachBanNhieu);
        }
        [ChildActionOnly]
        public ActionResult NavPartial()
        {
            List<MENU> lst = db.MENUs
                                 .Where(m => m.ParentId == null)
                                 .OrderBy(m => m.OrderNumber)
                                 .ToList();

            int[] a = new int[lst.Count];
            for (int i = 0; i < lst.Count; i++)
            {
                int parentId = lst[i].Id; 
                List<MENU> l = db.MENUs.Where(m => m.ParentId == parentId).ToList();
                int k = l.Count();
                a[i] = k;
            }

            ViewBag.lst = a;
            return PartialView(lst);
        }
        [ChildActionOnly]
        public ActionResult LoadChildMenu(int parentId)
        {
            List<MENU> lst = db.MENUs
                                 .Where(m => m.ParentId == parentId)
                                 .OrderBy(m => m.OrderNumber)
                                 .ToList();
            ViewBag.Count = lst.Count();
            int[] a = new int[lst.Count()];
            for (int i = 0; i < lst.Count; i++)
            {
                int p = lst[i].Id;
                List<MENU> l = db.MENUs.Where(m => m.ParentId == p).ToList();
                int k = l.Count();
                a[i] = k;
            }

            ViewBag.lst = a;
            return PartialView("LoadChildMenu", lst);

        }
        public ActionResult FooterPartial()
        {
            return PartialView();
        }
        public ActionResult SachTheoChuDe(int? id, int? page)
        {
            ViewBag.MaCD = id;
            int iSize = 3;
            int iPageNumber = (page ?? 1);
            var kq = (from s in db.SACHes where s.MaCD == id select s).ToList();
            var chuDe = db.CHUDEs.FirstOrDefault(cd => cd.MaCD == id);
            ViewBag.TenChuDe = chuDe.TenChuDe;
            return View(kq.ToPagedList(iPageNumber, iSize));
        }
        public ActionResult SachTheoNXB(int? id, int? page)
        {
            ViewBag.MaNXB = id;
            int iSize = 3;
            int iPageNumber = (page ?? 1);
            var kq = (from s in db.SACHes where s.MaNXB == id select s).ToList();
            return View(kq.ToPagedList(iPageNumber, iSize));
        }
        public ActionResult ChiTietSach(int? id)
        {
            var sach = db.SACHes.Where(x => x.MaSach == id);
            return View(sach.Single());
        }
        public ActionResult LoginLogout()
        {
            return PartialView("LoginLogoutPartial");
        }
        public ActionResult TrangTin(string metatitle)
        {
            var tt = (from t in db.TRANGTINs where t.MetaTitle == metatitle select t).Single();
            return View(tt);
        }
    }
}