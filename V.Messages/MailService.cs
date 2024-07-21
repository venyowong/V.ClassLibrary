using System;
using System.Net.Mail;
using System.Net;

namespace V.Messages
{
    public class MailService
    {
        private string host;
        private int port;
        private string userName;
        private string password;

        public MailService(string host, int port, string userName, string password)
        {
            this.host = host;
            this.port = port;
            this.userName = userName;
            this.password = password;
        }

        public bool SendMail(string subject, string body, params string[] toMails)
        {
            using (var client = new SmtpClient(this.host))
            {
                if (this.port > 0)
                {
                    client.Port = this.port;
                }
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(this.userName, this.password);
                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(this.userName);
                foreach (var item in toMails)
                {
                    mailMessage.To.Add(item);
                }
                mailMessage.Body = body;
                mailMessage.Subject = subject;
                client.Send(mailMessage);
                return true;
            }
        }
    }
}
