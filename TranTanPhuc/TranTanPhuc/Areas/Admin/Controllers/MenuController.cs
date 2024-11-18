using Com.CloudRail.SI.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TranTanPhuc.Models;

namespace TranTanPhuc.Areas.Admin.Controllers
{
    public class MenuController : Controller
    {
        SachOnlineEntities db = new SachOnlineEntities();
        // GET: Admin/Menu
        public ActionResult Index()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var listMenu = db.MENUs.Where(n => n.ParentId == null).OrderBy(n => n.OrderNumber).ToList();
            int[] a = new int[listMenu.Count];
            for (int i = 0; i < listMenu.Count; i++)
            {
                int id = listMenu[i].Id;
                var l = db.MENUs.Where(n => n.ParentId == id);
                int k = l.Count();
                a[i] = k;
            }
            ViewBag.lst = a;
            List<CHUDE> cd = db.CHUDEs.ToList();
            ViewBag.ChuDe = cd;
            List<TRANGTIN> tt = db.TRANGTINs.ToList();
            ViewBag.TrangTin = tt;
            return View(listMenu);

        }
        [ChildActionOnly]
        public ActionResult ChildMenu(int parentId)
        {
            List<MENU> lst = new List<MENU>();
            lst = db.MENUs.Where(m => m.ParentId == parentId).OrderBy(m => m.OrderNumber).ToList();
            ViewBag.Count = lst.Count();
            int[] a = new int[lst.Count()];
            for (int i = 0; i < lst.Count; i++)
            {
                int id = lst[i].Id;
                var l = db.MENUs.Where(m => m.ParentId == id);
                int k = l.Count();
                a[i] = k;
            }
            ViewBag.lst = a;
            return PartialView("ChildMenu", lst);
        }
        [ChildActionOnly]
        public ActionResult ChildMenu1(int parentId)
        {
            List<MENU> lst = new List<MENU>();
            lst = db.MENUs.Where(o => o.ParentId == parentId).OrderBy(m => m.OrderNumber).ToList();
            ViewBag.Count = lst.Count;

            int[] a = new int[lst.Count];
            for (int i = 0; i < lst.Count; i++)
            {
                int id = lst[i].Id;
                var l = db.MENUs.Where(m => m.ParentId == id);
                int k = l.Count();
                a[i] = k;
            }

            ViewBag.lst = a;
            return PartialView("ChildMenu1", lst);
        }
        [HttpPost]
        public ActionResult AddMenu(FormCollection f)
        {
            if (!string.IsNullOrEmpty(f["ThemChuDe"]))
            {
                MENU m = new MENU();
                if (int.TryParse(f["MaCD"], out int maCD))
                {
                    var cd = db.CHUDEs.SingleOrDefault(c => c.MaCD == maCD);
                    if (cd != null)
                    {
                        m.MenuName = cd.TenChuDe;
                        m.MenuLink = "sach-theo-chu-de-" + maCD;

                        if (int.TryParse(f["ParentID"], out int parentId))
                            m.ParentId = parentId;
                        else
                            m.ParentId = null;

                        if (int.TryParse(f["OrderNumber"], out int orderNumber))
                            m.OrderNumber = orderNumber;

                        // Điều chỉnh thứ tự các menu khác
                        var L = m.ParentId == null
                            ? db.MENUs.Where(k => k.ParentId == null && k.OrderNumber >= m.OrderNumber).ToList()
                            : db.MENUs.Where(k => k.ParentId == m.ParentId && k.OrderNumber >= m.OrderNumber).ToList();

                        foreach (var menu in L)
                            menu.OrderNumber++;

                        db.MENUs.Add(m);
                        db.SaveChanges();
                    }
                }
            }
            else if (!string.IsNullOrEmpty(f["ThemTrang"]))
            {
                MENU m = new MENU();
                if (int.TryParse(f["MaTT"], out int maTT))
                {
                    var trang = db.TRANGTINs.SingleOrDefault(t => t.MaTT == maTT);
                    if (trang != null)
                    {
                        m.MenuName = trang.TenTrang;
                        m.MenuLink = trang.MetaTitle;

                        if (int.TryParse(f["ParentID"], out int parentId))
                            m.ParentId = parentId;
                        else
                            m.ParentId = null;

                        if (int.TryParse(f["OrderNumber"], out int orderNumber))
                            m.OrderNumber = orderNumber;

                        var L = m.ParentId == null
                            ? db.MENUs.Where(k => k.ParentId == null && k.OrderNumber >= m.OrderNumber).ToList()
                            : db.MENUs.Where(k => k.ParentId == m.ParentId && k.OrderNumber >= m.OrderNumber).ToList();

                        foreach (var menu in L)
                            menu.OrderNumber++;

                        db.MENUs.Add(m);
                        db.SaveChanges();
                    }
                }
            }
            else if (!string.IsNullOrEmpty(f["ThemLink"]))
            {
                MENU m = new MENU
                {
                    MenuName = f["TenMenu"],
                    MenuLink = f["Link"]
                };

                if (int.TryParse(f["ParentID"], out int parentId))
                    m.ParentId = parentId;
                else
                    m.ParentId = null;

                if (int.TryParse(f["OrderNumber"], out int orderNumber))
                    m.OrderNumber = orderNumber;

                var L = m.ParentId == null
                    ? db.MENUs.Where(k => k.ParentId == null && k.OrderNumber >= m.OrderNumber).ToList()
                    : db.MENUs.Where(k => k.ParentId == m.ParentId && k.OrderNumber >= m.OrderNumber).ToList();

                foreach (var menu in L) menu.OrderNumber++;

                db.MENUs.Add(m);
                db.SaveChanges();
            }

            return Redirect("/Admin/Menu/Index");
        }
        [HttpPost]
        public JsonResult Delete(int id)
        {
            List<MENU> submn = db.MENUs.Where(m => m.ParentId == id).ToList();
            if (submn.Count > 0)
            {
                return Json(new { code = 500, msg = "Còn menu con, không xóa được." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var mn = db.MENUs.SingleOrDefault(m => m.Id == id);
                List<MENU> l = null;

                if (mn.ParentId == null)
                {
                    l = db.MENUs.Where(k => k.ParentId == null && k.OrderNumber > mn.OrderNumber).ToList();
                }
                else
                {
                    l = db.MENUs.Where(k => k.ParentId == mn.ParentId && k.OrderNumber > mn.OrderNumber).ToList();
                }

                for (int i = 0; i < l.Count; i++)
                {
                    l[i].OrderNumber--;
                }

                db.MENUs.Remove(mn);
                db.SaveChanges();

                return Json(new { code = 200, msg = "Xóa thành công." }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult Update(int id)
        {
            try
            {
                var mn = (from m in db.MENUs
                          where m.Id == id
                          select new
                          {
                              Id = m.Id,
                              MenuName = m.MenuName,
                              MenuLink = m.MenuLink,
                              OrderNumber = m.OrderNumber
                          }).SingleOrDefault();

                return Json(new { code = 200, mn = mn, msg = "Lấy thông tin thành công." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = "Lấy thông tin thất bại. " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Update(int id, string strTenMenu, string strLink, int STT)
        {
            try
            {
                var mn = db.MENUs.SingleOrDefault(m => m.Id == id);
                List<MENU> l = null;

                if (STT < mn.OrderNumber)
                {
                    if (mn.ParentId == null)
                    {
                        l = db.MENUs.Where(m => m.ParentId == null && m.OrderNumber >= STT && m.OrderNumber < mn.OrderNumber).ToList();
                    }
                    else
                    {
                        l = db.MENUs.Where(m => m.ParentId == mn.ParentId && m.OrderNumber >= STT && m.OrderNumber < mn.OrderNumber).ToList();
                    }
                    for (int i = 0; i < l.Count; i++)
                    {
                        l[i].OrderNumber++;
                    }
                }
                else if (STT > mn.OrderNumber)
                {
                    if (mn.ParentId == null)
                    {
                        l = db.MENUs.Where(m => m.ParentId == null && m.OrderNumber > mn.OrderNumber && m.OrderNumber <= STT).ToList();
                    }
                    else
                    {
                        l = db.MENUs.Where(m => m.ParentId == mn.ParentId && m.OrderNumber > mn.OrderNumber && m.OrderNumber <= STT).ToList();
                    }
                    for (int i = 0; i < l.Count; i++)
                    {
                        l[i].OrderNumber--;
                    }
                }

                mn.MenuName = strTenMenu;
                mn.MenuLink = strLink;
                mn.OrderNumber = STT;

                db.SaveChanges();

                return Json(new { code = 200, msg = "Sửa menu thành công." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = "Sửa menu thất bại. " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}