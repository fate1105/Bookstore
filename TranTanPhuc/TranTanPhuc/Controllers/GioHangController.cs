using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TranTanPhuc.Models;
using System.Text;
using System.Data.Entity;
using PayPal.Api;

namespace TranTanPhuc.Controllers
{
    public class GioHangController : Controller
    {
        // GET: GioHang
        SachOnlineEntities db = new SachOnlineEntities();
        public List<GioHang> LayGioHang()
        {
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang == null)
            {
                lstGioHang = new List<GioHang>();
                Session["GioHang"] = lstGioHang;
            }
            return lstGioHang;
        }
        public ActionResult ThemGioHang(int id, string url)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sp = lstGioHang.Find(n => n.iMaSach == id);
            if (sp == null)
            {
                sp = new GioHang(id);
                lstGioHang.Add(sp);
            }
            else
            {
                sp.iSoLuong++;
            }
            return Redirect(url);

        }
        private int TongSoLuong()
        {
            int iTongSoLuong = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                iTongSoLuong = lstGioHang.Sum(n => n.iSoLuong);
            }
            return iTongSoLuong;
        }
        private double TongTien()
        {
            double iTongTien = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                iTongTien = lstGioHang.Sum(n => n.dThanhTien);
            }
            return iTongTien;
        }
        private double TongTienUSD()
        {
            double iTongTien = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                iTongTien = lstGioHang.Sum(n => n.dThanhTien);
            }
            return Math.Round(iTongTien /24000, 2);
        }
        public ActionResult XoaSPGioHang(int iMaSach)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sp = lstGioHang.SingleOrDefault(n => n.iMaSach == iMaSach);
            if (sp != null)
            {
                lstGioHang.RemoveAll(n => n.iMaSach == iMaSach);
                if (lstGioHang.Count == 0)
                {
                    return RedirectToAction("Index", "TranTanPhuc");
                }
            }
            return RedirectToAction("GioHang");
        }
        public ActionResult CapNhatGioHang(int iMaSach, FormCollection f)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sp = lstGioHang.SingleOrDefault(n => n.iMaSach == iMaSach);
            if (sp != null)
            {
                sp.iSoLuong = int.Parse(f["txtSoLuong"].ToString());
            }
            return RedirectToAction("GioHang");
        }
        public ActionResult XoaGioHang()
        {
            List<GioHang> lstGioHang = LayGioHang();
            lstGioHang.Clear();
            return RedirectToAction("Index", "TranTanPhuc");
        }
        public ActionResult GioHang()
        {
            List<GioHang> lstGioHang = LayGioHang();
            if (lstGioHang.Count == 0)
            {
                return RedirectToAction("Index", "TranTanPhuc");
            }
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return View(lstGioHang);
        }
        public ActionResult GioHangPartial()
        {
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return PartialView();
        }

        [HttpGet]
        public ActionResult DatHang()
        {
            if (Session["TaiKhoan"] == null || Session["TaiKhoan"].ToString() == "")
            {
                return RedirectToAction("DangNhap", "User");
            }
            if (Session["GioHang"] == null)
            {
                return RedirectToAction("Index", "LeHoaiNam");
            }
            List<GioHang> lstGioHang = LayGioHang();
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return View(lstGioHang);
        }
        [HttpPost]
        public ActionResult DatHang(FormCollection f)
        {
            DONDATHANG ddh = new DONDATHANG();
            KHACHHANG kh = (KHACHHANG)Session["TaiKhoan"];
            List<GioHang> lstgioHangs = LayGioHang();
            if (string.IsNullOrEmpty(f["NgayGiao"]))
            {
                ModelState.AddModelError("NgayGiao", "Bạn phải nhập ngày giao.");
                ViewBag.TongSoLuong = TongSoLuong();
                ViewBag.TongTien = TongTien();
                return View(lstgioHangs);
            }
            ddh.MaKH = kh.MaKH;
            ddh.NgayDat = DateTime.Now;
            var NgayGiao = String.Format("{0:MM/DD/yyyy}", f["NgayGiao"]);

            ddh.NgayGiao = DateTime.Parse(NgayGiao);
            if (ddh.NgayGiao < ddh.NgayDat)
            {
                ModelState.AddModelError("NgayGiao", "Ngày giao phải sau ngày đặt hàng.");
                ViewBag.TongSoLuong = TongSoLuong();
                ViewBag.TongTien = TongTien();
                return View(lstgioHangs);
            }
            ddh.TinhTrangGiaoHang = 1;
            ddh.DaThanhToan = false;
            db.DONDATHANGs.Add(ddh);

            db.SaveChanges();
            foreach (var item in lstgioHangs)
            {
                CHITIETDATHANG ctdh = new CHITIETDATHANG();
                ctdh.MaDonHang = ddh.MaDonHang;
                ctdh.MaSach = item.iMaSach;
                ctdh.SoLuong = item.iSoLuong;
                ctdh.DonGia = (decimal)item.dDonGia;
                db.CHITIETDATHANGs.Add(ctdh);

            }
            db.SaveChanges();
            decimal tongTien = (decimal)TongTien();
            GuiMailXacNhan(kh.Email, kh.HoTen, ddh, tongTien);
            Session["GioHang"] = null;
            return RedirectToAction("XacNhanDonHang", "GioHang");
        }
        private void GuiMailXacNhan(string toEmail, string hoTen, DONDATHANG donHang, decimal tongTien)
        {
            var mail = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("fatum61205@gmail.com", "csxv lwha uwpn imng"),
                EnableSsl = true
            };
            var message = new MailMessage
            {
                From = new MailAddress("fatum61205@gmail.com"),
                Subject = "Xác nhận đơn hàng - " + donHang.MaDonHang,
                IsBodyHtml = true // Đánh dấu nội dung là HTML
            };

            message.To.Add(new MailAddress(toEmail));
            var body = new StringBuilder();
            body.AppendLine("<h3>Cảm ơn " + hoTen + " đã đặt hàng tại cửa hàng của chúng tôi!</h3>");
            body.AppendLine("<p>Đơn hàng của bạn đã được xác nhận và đang được xử lý.</p>");
            body.AppendLine("<p><b>Thông tin đơn hàng:</b></p>");
            body.AppendLine("<ul>");
            body.AppendLine("<li>Mã đơn hàng: " + donHang.MaDonHang + "</li>");
            body.AppendLine("<li>Ngày đặt hàng: " + donHang.NgayDat + "</li>");
            body.AppendLine("<li><b>Chi tiết sản phẩm:</b></li>");
            body.AppendLine("<table border='1'>");
            body.AppendLine("<tr><th>Sản phẩm</th><th>Số lượng</th><th>Đơn giá</th><th>Thành tiền</th></tr>");
            foreach (var item in donHang.CHITIETDATHANGs)
            {
                body.AppendLine("<tr>");
                var tenSach = db.SACHes.FirstOrDefault(s => s.MaSach == item.MaSach)?.TenSach;
                body.AppendLine("<td>" + tenSach + "</td>");
                body.AppendLine("<td>" + item.SoLuong + "</td>");
                body.AppendLine("<td>" + item.DonGia + " VNĐ (" + (Math.Round((double)(item.DonGia) / 24000, 2) + "$)</td>"));
                body.AppendLine("<td>" + (item.SoLuong * item.DonGia) + " VNĐ (" +(Math.Round((double)(item.SoLuong * item.DonGia) / 24000, 2) + "$)</td>"));
                body.AppendLine("</tr>");
            }
            body.AppendLine("</table>");
            body.AppendLine("<li><b>Tổng tiền:</b> " + tongTien + " VNĐ</li>");
            body.AppendLine("<li><b>Tổng tiền:</b> " + Math.Round(tongTien / 24000, 2) + " $</li>");
            body.AppendLine("</ul>");
            body.AppendLine("<p>Bạn có thể theo dõi tình trạng đơn hàng của mình bằng cách truy cập vào trang web của chúng tôi.</p>");
            body.AppendLine("<p>Mọi thắc mắc xin liên hệ với chúng tôi qua email: ttp@.com hoặc điện thoại: 0xxx.</p>");
            body.AppendLine("<p>Trân trọng,</p>");
            body.AppendLine("<p>Cửa hàng của chúng tôi</p>");

            message.Body = body.ToString();

            // Gửi email
            mail.Send(message);
        }
        public ActionResult XacNhanDonHang()
        {
            return View();
        }
        public ActionResult FailureView()
        {
            // You can pass a message to the view if needed
            ViewBag.Message = "Payment Failed. Please try again.";
            return View();
        }
        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            //getting the apiContext  
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/GioHang/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guid = Convert.ToString((new Random()).Next(100000));
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }
                }
            }
            catch (Exception ex)
            {
                return View("FailureView");
            }
            //on successful payment, show success page to user.  
            return View("XacNhanDonHang");
        }
        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            List<GioHang> lstGioHang = LayGioHang();
            //create itemlist and add item objects to it  
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            var tongTien =TongTienUSD();

            //Adding Item Details like name, currency, price etc  
            foreach (var item in lstGioHang)
            {
                // Lấy tên sách từ database
                var tenSach = db.SACHes.FirstOrDefault(s => s.MaSach == item.iMaSach)?.TenSach;


                // Thêm sản phẩm vào danh sách item của PayPal
                itemList.items.Add(new Item()
                {
                    name = tenSach,                // Tên sản phẩm
                    currency = "USD",              // Đơn vị tiền tệ là USD
                    price = Math.Round(item.dDonGia / 24000, 2).ToString(),  // Giá sản phẩm (chuyển đổi sang USD)
                    quantity = item.iSoLuong.ToString(), // Số lượng sản phẩm
                    sku = item.iMaSach.ToString()   // Mã sản phẩm (SKU)
                });
            }
            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = tongTien.ToString()
            };
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "USD",
                total = tongTien.ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            var paypalOrderId = DateTime.Now.Ticks;
            transactionList.Add(new Transaction()
            {
                description = $"Invoice #{paypalOrderId}",
                invoice_number = paypalOrderId.ToString(), //Generate an Invoice No    
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }

    }
}