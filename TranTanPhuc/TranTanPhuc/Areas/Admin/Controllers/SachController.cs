using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using System.IO;

using TranTanPhuc.Models;
using System.Web.UI.WebControls;

namespace TranTanPhuc.Areas.Admin.Controllers
{
    public class SachController : Controller
    {
        // GET: Admin/Sach
        SachOnlineEntities db = new SachOnlineEntities();
        public ActionResult Index(int? page)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            int iPageNum = (page ?? 1);
            int iPageSize = 7;

            return View(db.SACHes.ToList().OrderBy(n => n.MaSach).ToPagedList(iPageNum, iPageSize));
        }
        public ActionResult Create(int? page)
        {
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");
            return PartialView();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(SACH sach, FormCollection f, HttpPostedFileBase FileUpload)
        {
            // Đưa dữ liệu vào DropDown
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");

            // Kiểm tra xem FileUpload có rỗng không
            if (FileUpload == null || FileUpload.ContentLength == 0)
            {
                ViewBag.ThongBao = "Hãy chọn ảnh bìa!";
                ViewBag.TenSach = f["sTenSach"];
                ViewBag.MoTa = f["sMoTa"];
                ViewBag.SoLuong = int.Parse(f["iSoLuong"]);
                ViewBag.GiaBan = decimal.Parse(f["mGiaBan"]);
                ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe", int.Parse(f["MaCD"]));
                ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB", int.Parse(f["MaNXB"]));
                return View(sach); // Trả về mô hình đã nhập
            }

            if (!ModelState.IsValid)
            {
                // Kiểm tra lỗi ModelState
                return View(sach);
            }

            // Khai báo thư viện: System.IO để xử lý file
            var sFileName = Path.GetFileName(FileUpload.FileName);
            var path = Path.Combine(Server.MapPath("~/Images"), sFileName);

            // Kiểm tra ảnh bìa đã tồn tại chưa để lưu lên thư mục
            if (!System.IO.File.Exists(path))
            {
                FileUpload.SaveAs(path);
            }

            // Lưu thông tin sách vào CSDL
            sach.TenSach = f["sTenSach"];
            sach.MoTa = f["sMoTa"];
            sach.SoLuongBan = int.Parse(f["iSoLuong"]);
            sach.GiaBan = decimal.Parse(f["mGiaBan"]);
            sach.AnhBia = sFileName;
            sach.NgayCapNhat = Convert.ToDateTime(f["dNgayCapNhat"]);
            sach.MaCD = int.Parse(f["MaCD"]);
            sach.MaNXB = int.Parse(f["MaNXB"]);

            db.SACHes.Add(sach);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Details(int id)
        {
            var sach = db.SACHes.SingleOrDefault(n => n.MaSach == id);
            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return PartialView(sach);
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var sach = db.SACHes.SingleOrDefault(n => n.MaSach == id);
            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return PartialView(sach);
        }
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirm(int id, FormCollection f)
        {
            var sach = db.SACHes.SingleOrDefault(n => n.MaSach == id);

            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var ctdh = db.CHITIETDATHANGs.Where(ct => ct.MaSach == id);
            if (ctdh.Count() > 0)
            {
                //Nội dung sẽ hiển thị khi sách cần xóa đã có trong table ChiTietDonHang
                ViewBag.ThongBao = "Sách này đang có trong bảng Chi tiết đặt hàng <br>" + "Nếu muốn xóa thì phải xóa hết mã sách này trong bảng Chi tiết đặt hàng";
                return View(sach);
            }

            //Xóa hết thông tin của cuốn sách trong table VietSach trước khi xóa sách này
            var vietSach = db.VIETSACHes.Where(vi => vi.MaSach == id).ToList();
            if (vietSach != null)
            {
                db.VIETSACHes.RemoveRange(vietSach);
                db.SaveChanges();
            }
            //Xóa sách
            db.SACHes.Remove(sach);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var sach = db.SACHes.SingleOrDefault(n => n.MaSach == id);
            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            // Hiến thị danh sách chủ đề và nhà xuất bán đông thời chọn chủ đề và nhà xuất bản của cuốn hiện tại
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe", sach.MaCD);
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB", sach.MaNXB);
            return PartialView(sach);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f, HttpPostedFileBase fFileUpload)
        {
            int id = int.Parse(f["iMaSach"]);
            var sach = db.SACHes.SingleOrDefault(n => n.MaSach == id);
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe", sach.MaCD);
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB", sach.MaNXB);
            if (ModelState.IsValid)
            {
                if (fFileUpload != null)// Kiếm tra để xác nhận cho thay đối ảnh bìa
                {
                    // Lấy tên file(Khai bao thư vien system.o)
                    var sFileName = Path.GetFileName(fFileUpload.FileName);
                    // Lẩy đường dẫn lưu file
                    var path = Path.Combine(Server.MapPath("~/Images"), sFileName);
                    // Kiếm tra file đã tôn tại chưa
                    if (!System.IO.File.Exists(path))
                        fFileUpload.SaveAs(path);
                    sach.AnhBia = sFileName;
                }
                // Lưu Sach vào CSDL
                sach.TenSach = f["sTenSach"];
                sach.MoTa = f["sMoTa"];
                sach.NgayCapNhat = Convert.ToDateTime(f["dNgayCapNhat"]);
                sach.SoLuongBan = int.Parse(f["iSoLuong"]);
                sach.GiaBan = decimal.Parse(f["mGiaBan"]);
                sach.MaCD = int.Parse(f["MaCD"]);
                sach.MaNXB = int.Parse(f["MaNXB"]);
                db.SaveChanges();
                //Về lại trang Quản lý sách 
                return RedirectToAction("Index");
            }
            return View(sach);
        }
    }
}
