using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace V.User.Services
{
    public class MailService
    {
        private Configuration config;

        public MailService(Configuration config)
        {
            this.config = config;
        }

        public bool SendMail(string subject, string body, params string[] toMails)
        {
            using (var client = new SmtpClient(this.config.SmtpServer))
            {
                if (this.config.SmtpPort > 0)
                {
                    client.Port = this.config.SmtpPort;
                }
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(this.config.AdmMailAccount, this.config.AdmMailPwd);
                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(this.config.AdmMailAccount);
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
