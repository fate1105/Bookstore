using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using TranTanPhuc.Models;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;

namespace TranTanPhuc.Areas.Admin.Controllers
{
    public class FileAndMailController : Controller
    {
        // GET: Admin/FileAndMail
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult SendMail()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SendMail(Mail model)
        {
            var mail = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("fatum61205@gmail.com", "csxv lwha uwpn imng"),
                EnableSsl = true
            };

            var message = new MailMessage();
            message.From = new MailAddress(model.From);
            message.ReplyToList.Add(model.From);
            message.To.Add(new MailAddress(model.To));
            message.Subject = model.Subject;
            message.Body = model.Notes;

            var f = Request.Files["attachment"];
            var path = Path.Combine(Server.MapPath("~/UploadFile"), f.FileName);
            if (!System.IO.File.Exists(path))
            {
                f.SaveAs(path);
            }

            Attachment data = new Attachment(Server.MapPath("~/UploadFile/" + f.FileName), MediaTypeNames.Application.Octet);
            message.Attachments.Add(data);  

            mail.Send(message);

            return View("SendMail");
        }
    }
}