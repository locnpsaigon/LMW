using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;

namespace LMW.Libs
{
    public class Emailer
    {
        const string sender = "info@letmewash.vn";
        const string senderName = "L.M.W";
        const string senderPassword = "doi8bxhe3s";

        public static bool sendMail(string toEmail, string toName, string subject, string body)
        {
            try
            {
                var fromAddress = new MailAddress(sender, senderName);
                var toAddress = new MailAddress(toEmail, toName);

                var smtp = new SmtpClient
                {
                    Host = "smtp.zoho.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, senderPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Write error logs
                EventWriter.WriteEventLog("Emailer - sendMail: " + ex.ToString());
            }

            return false;
        }

    }
}