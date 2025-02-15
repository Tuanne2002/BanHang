using System.Net.Mail;
using System.Net;

namespace BanHangWeb.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("truongdanganhtuan2002@gmail.com", "ymzsznpgkcuvtojl")
            };

            //return client.SendMailAsync(
            //    new MailMessage(from: "truongdanganhtuan2002@gmail.com",
            //                    to: email,
            //                    subject,
            //                    message
            //                    ));
            var mailMessage = new MailMessage
            {
                From = new MailAddress("truongdanganhtuan2002@gmail.com"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true  // Thiết lập cho phép nội dung HTML
            };

            mailMessage.To.Add(email);

            return client.SendMailAsync(mailMessage);
        }
    }
}
