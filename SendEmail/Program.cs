using System;
using System.IO;
using System.Net.Mail;

namespace SendEmail
{
    public class SendEmailService
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
        public static bool EmailSend(string email, MemoryStream memoryStream, string fileName)
        {
            try
            {
                memoryStream.Position = 0; // ilk satırdan itibaren okumaya başla
                System.Net.Mime.ContentType ct = new System.Net.Mime.ContentType(System.Net.Mime.MediaTypeNames.Application.Pdf);

                Attachment attach = new Attachment(memoryStream, ct);
                attach.ContentDisposition.FileName = $"{fileName}.pdf";
                Console.WriteLine(attach.ContentDisposition.FileName);
                MailMessage mailMessage = new MailMessage();
                SmtpClient smtpClient = new SmtpClient();

                mailMessage.From = new MailAddress("info@hueed.com");
                mailMessage.To.Add(new MailAddress(email));
                mailMessage.CC.Add(new MailAddress("mbarisugur@gmail.com"));
                mailMessage.Subject = "Pdf Dosyası - Microservice Base deneme maili";
                mailMessage.Body = @"Pdf dosyası ektedir.";
                mailMessage.IsBodyHtml = true;
                mailMessage.Attachments.Add(attach);

                smtpClient.Host = "mail.hueed.com";
                smtpClient.Port = 587;
                smtpClient.Credentials = new System.Net.NetworkCredential("info@hueed.com", "Your Password");
                smtpClient.EnableSsl = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Send(mailMessage);

                memoryStream.Close();
                memoryStream.Dispose();
                Console.WriteLine("Email gönderilmiştir.");
                return true;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Hata : {ex.InnerException}");
                throw;
            }

        }
    }
}
