using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TranTanPhuc.Areas.Admin.Controllers;
using TranTanPhuc.Models;

namespace TranTanPhuc.Controllers
{
    public class UserController : Controller
    {
        SachOnlineEntities db = new SachOnlineEntities();
        // GET: User

        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangNhap(FormCollection collection)
        {
            var sTenDN = collection["TenDN"];
            var sMatKhau = collection["MatKhau"];

            if (String.IsNullOrEmpty(sTenDN))
            {
                ViewData["Err1"] = "Bạn chưa nhập tên đăng nhập";
            }
            else if (String.IsNullOrEmpty(sMatKhau))
            {
                ViewData["Err2"] = "Phải nhập mật khẩu";
            }
            else
            {
                KHACHHANG kh = db.KHACHHANGs.SingleOrDefault(n => n.TaiKhoan == sTenDN && n.MatKhau == sMatKhau);
                if (kh != null)
                {
                    ViewBag.ThongBao = "Chúc mừng đăng nhập thành công";
                    Session["TaiKhoan"] = kh;
                    Session["TenKH"] = kh.HoTen;
                    if (collection["remember"].Contains("true"))
                    {
                        Response.Cookies["TenDN"].Value = sTenDN;
                        Response.Cookies["MatKhau"].Value = sMatKhau;
                        Response.Cookies["TenDN"].Expires = DateTime.Now.AddDays(1);
                        Response.Cookies["MatKhau"].Expires = DateTime.Now.AddDays(1);
                    }
                    else
                    {
                        Response.Cookies["TenDN"].Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies["MatKhau"].Expires = DateTime.Now.AddDays(-1);
                    }
                    return RedirectToAction("Index", "TranTanPhuc");
                }
                else
                {
                    ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }

            return View();
        }
        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangKy(FormCollection collection, KHACHHANG kh)
        {
            var hoTen = collection["HoTen"];
            var sTenDN = collection["TenDN"];
            var sMatKhau = collection["MatKhau"];
            var sMatKhauNhapLai = collection["MatKhauNL"];
            var sDiaChi = collection["DiaChi"];
            var sEmail = collection["Email"];
            var sDienThoai = collection["DienThoai"];
            var sNgaySinh = String.Format("{0:MM/dd/yyyy}", collection["NgaySinh"]);

            if (String.IsNullOrEmpty(hoTen))
            {
                ViewData["err1"] = "Họ tên không được rỗng";
            }
            else if (String.IsNullOrEmpty(sTenDN))
            {
                ViewData["err2"] = "Tên đăng nhập không được rỗng";
            }
            else if (String.IsNullOrEmpty(sMatKhau))
            {
                ViewData["err3"] = "Phải nhập mật khẩu";
            }
            else if (String.IsNullOrEmpty(sMatKhauNhapLai))
            {
                ViewData["err4"] = "Phải nhập lại mật khẩu";
            }
            else if (sMatKhau != sMatKhauNhapLai)
            {
                ViewData["err4"] = "Mật khẩu nhập lại không khớp";
            }
            else if (String.IsNullOrEmpty(sEmail))
            {
                ViewData["err5"] = "Email không được rỗng";
            }
            else if (String.IsNullOrEmpty(sDienThoai))
            {
                ViewData["err6"] = "Số điện thoại không được rỗng";
            }
            else if (db.KHACHHANGs.SingleOrDefault(n => n.TaiKhoan == sTenDN) != null)
            {
                ViewBag.ThongBao = "Tên đăng nhập đã tồn tại";
            }
            else if (db.KHACHHANGs.SingleOrDefault(n => n.Email == sEmail) != null)
            {
                ViewBag.ThongBao = "Email đã được sử dụng";
            }
            else
            {
                kh.HoTen = hoTen;
                kh.TaiKhoan = sTenDN;
                kh.MatKhau = sMatKhau;
                kh.Email = sEmail;
                kh.DiaChi = sDiaChi;
                kh.DienThoai = sDienThoai;
                kh.NgaySinh = DateTime.Parse(sNgaySinh);

                db.KHACHHANGs.Add(kh);
                db.SaveChanges();

                SendRegistrationEmail(kh.Email, kh.HoTen);

                return RedirectToAction("DangNhap");
            }

            return this.DangKy();
        }
        public ActionResult DangXuat()
        {
            Session.Abandon();
            return RedirectToAction("Index", "TranTanPhuc");
        }
        private void SendRegistrationEmail(string toEmail, string hoTen)
        {
            var mail = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("fatum61205@gmail.com", "csxv lwha uwpn imng"),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress("fatum61205@gmail.com"),
                Subject = "Đăng Ký Thành Công",
                Body = "Cảm ơn " + hoTen + " đã đăng ký. Chào mừng bạn đến với chúng tôi!",
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(toEmail));

            mail.Send(message);
        }
        [HttpGet]
        public ActionResult DangKyKiemLoi()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangKyKiemLoi(FormCollection f, KHACHHANG kh)
        {
            var matKhauNL = f["MatkhauNL"];
            string sTenDN = f["TaiKhoan"];
            string sEmail = f["Email"];
            if (String.IsNullOrEmpty(matKhauNL))
            {
                ViewData["err4"] = "Phải nhập lại mật khẩu";
            }
            else if (kh.MatKhau != matKhauNL)
            {
                ViewData["err4"] = "Mật khẩu nhập lại không khớp";
            }
            else if (db.KHACHHANGs.SingleOrDefault(n => n.TaiKhoan == sTenDN) != null)
            {
                ViewBag.ThongBao = "Tên đăng nhập đã tồn tại";
            }
            else if (db.KHACHHANGs.SingleOrDefault(n => n.Email == sEmail) != null)
            {
                ViewBag.ThongBao = "Email đã được sử dụng";
            }
            else if (ModelState.IsValid)
            {

                // Thêm khách hàng vào cơ sở dữ liệu và lưu thay đổi
                db.KHACHHANGs.Add(kh);
                db.SaveChanges();
                SendRegistrationEmail(kh.Email, kh.HoTen);
                // Chuyển hướng tới trang đăng nhập
                return Redirect("~/User/DangNhap");
            }


            return View("DangKyKiemLoi");
        }
    }
}