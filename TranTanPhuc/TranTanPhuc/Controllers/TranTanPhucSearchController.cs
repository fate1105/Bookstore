using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TranTanPhuc.Models;
using PagedList;
using PagedList.Mvc;
using System.Linq.Dynamic;
using System.Linq.Expressions;
namespace TranTanPhuc.Controllers
{
    public class TranTanPhucSearchController : Controller
    {
        // GET: TranTanPhucSearch
        SachOnlineEntities db = new SachOnlineEntities();
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
        public ActionResult SearchTheoDanhMuc(string strSearch = null, int maCD = 0)
        {
            ViewBag.Search = strSearch;
            var kq = db.SACHes.Select(b => b);
            if (!String.IsNullOrEmpty(strSearch))
                kq = kq.Where(b => b.TenSach.Contains(strSearch));
            {
                kq = kq.Where(b => b.CHUDE.MaCD == maCD);
            }
            ViewBag.MaCD = new SelectList(db.CHUDEs, "MaCD", "TenChuDe");
            return View(kq.ToList());
        }
        public ActionResult Group()
        {
            var kq = db.SACHes.GroupBy(s => s.MaCD);
            return View(kq);
        }
        public ActionResult ThongKe()
        {
            var rawResult = from s in db.SACHes
                            join cd in db.CHUDEs on s.MaCD equals cd.MaCD
                            group s by new { cd.MaCD, cd.TenChuDe } into g
                            select new
                            {
                                MaCD = g.Key.MaCD,
                                TenChuDe = g.Key.TenChuDe,
                                Count = g.Count(),
                                Sum = g.Sum(n => n.SoLuongBan),
                                Max = g.Max(n => n.SoLuongBan),
                                Avg = g.Average(n => n.SoLuongBan)
                            };
            var kq = rawResult.ToList().Select(g => new ReportInfo
            {
                ID = g.MaCD.ToString(),
                Name = g.TenChuDe,
                Count = g.Count,
                Sum = g.Sum,
                Max = g.Max,
                Avg = Convert.ToDecimal(g.Avg)
            });

            return View(kq);
        }
        public ActionResult SearchPhanTrang(int? page, string strSearch = null)
        {
            ViewBag.Search = strSearch;
            if (!string.IsNullOrEmpty(strSearch))
            {
                int iSize = 3;
                int iPageNumber = (page ?? 1);
                var kq = (from s in db.SACHes
                          where s.TenSach.Contains(strSearch) || s.MoTa.Contains(strSearch)
                          select s).ToList();
                return View(kq.ToPagedList(iPageNumber, iSize));
            }
            return View();
        }
        public ActionResult SearchPhanTrangTuyChon(int? size, int? page, string strSearch)
        {

            List<SelectListItem> items = new List<SelectListItem>()
            {
                new SelectListItem { Text = "3", Value = "3" },
                new SelectListItem { Text = "5", Value = "5" },
                new SelectListItem { Text = "10", Value = "10" },
                new SelectListItem { Text = "20", Value = "20" },
                new SelectListItem { Text = "25", Value = "25" },
                new SelectListItem { Text = "50", Value = "50" },
            };
            foreach (var item in items)
            {
                if (item.Value == size.ToString())
                {
                    item.Selected = true;
                }
            }
            ViewBag.Size = items;
            ViewBag.CurrentSize = size;
            ViewBag.Search = strSearch;
            int iSize = (size ?? 3);
            int iPageNumber = (page ?? 1);
            if (!string.IsNullOrEmpty(strSearch))
            {
                var kq = (from s in db.SACHes
                          where s.TenSach.Contains(strSearch) || s.MoTa.Contains(strSearch)
                          select s).ToList();

                return View(kq.ToPagedList(iPageNumber, iSize));
            }
            return View();
        }
        public ActionResult SearchPhanTrangSapXep(int? page, string sortProperty, string sortOrder = "", string strSearch = null)
        {
            ViewBag.Search = strSearch;

            if (!string.IsNullOrEmpty(strSearch))
            {
                int iSize = 3;
                int iPageNumber = (page ?? 1);

                //Gán giá trị cho biến sortOrder
                if (sortOrder == "") ViewBag.SortOrder = "desc";
                if (sortOrder == "desc") ViewBag.SortOrder = "";
                if (sortOrder == "") ViewBag.SortOrder = "asc";

                // Tạo thuộc tính sắp xếp mặc định là "Tên Sách"
                if (String.IsNullOrEmpty(sortProperty))
                {
                    sortProperty = "TenSach";
                }

                // Gán giá trị cho biến sortProperty
                ViewBag.SortProperty = sortProperty;

                // Truy vấn
                var kq = from s in db.SACHes
                         where s.TenSach.Contains(strSearch) || s.MoTa.Contains(strSearch)
                         select s;

                // Sắp xếp tăng/giảm bằng phương thức OrderBy sử dụng trong thư viện Dynamic LINQ
                kq = kq.OrderBy(sortProperty + " " + sortOrder);
                return View(kq.ToPagedList(iPageNumber, iSize));
            }
            return View();
        }
    }
}