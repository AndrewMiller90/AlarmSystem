using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Net;

namespace Socks
{
    static class EmailHelper
    {
        private static Regex emailRegex = new Regex("^[a-zA-Z].*@[a-zA-Z].*[.][a-zA-Z].*$", RegexOptions.Compiled);

        public static string[] GetEmailList()
        {
            string dir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string emailListPath = Path.Combine(Path.GetDirectoryName(dir), "EmailList.txt");
            string[] recipients = { "Andrew.Miller90@hotmail.com", "Andrew.Miller@Desktoplion.com" };
            if (File.Exists(emailListPath))
            {
                recipients = (from item in File.ReadAllLines(emailListPath) where emailRegex.IsMatch(item) select item).ToArray();
            }
            return recipients;
        }

        public static void SendAlarmEmail() {
            Console.WriteLine("Attempting to send email");
            try {
                string[] recipients = EmailHelper.GetEmailList();
                Console.WriteLine("Sending email to " + String.Join(", ", recipients));
                string senderAddress = "ciabubblesalarm@gmail.com";
                using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587)) {
                    client.Credentials = new System.Net.NetworkCredential(senderAddress, "@1@rm@c71v3");
                    client.EnableSsl = true;

                    using (MailMessage message = new MailMessage()) {
                        message.Subject = "System Triggered";
                        message.From = new MailAddress(senderAddress, "Alarm");
                        foreach (string item in recipients) {
                            message.To.Add(new MailAddress(item));
                        }
                        message.Body = string.Format("Alarm Triggered at {0}", DateTime.Now);

                        client.Send(message);
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Email Failed To Send");
                Console.WriteLine(ex.ToString());
            }
            
        }
    }
}
