using Humanizer;
using MethaWebsite.Localization;
using Microsoft.Extensions.Localization;
using MimeKit;
using MimeKit.Utils;
using System.Net.Http.Headers;
using System.Text;
namespace MethaWebsite.Services
{
    public class EmailService(ILogger<OldEmailSender> logger, IConfiguration configuration, IStringLocalizer<SharedResource> Loc)
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri($"https://api.mailgun.net/v3/{configuration.GetSection("Mailgun")["Domain"]}/")
        };
        byte[] byteArray = Encoding.ASCII.GetBytes($"api:{configuration.GetSection("Mailgun")["ApiKey"]}");

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var builder = new BodyBuilder();
            builder.HtmlBody = string.Format(@"
            <html lang=""en"">
                <head>
                    <meta charset=""UTF-8"">
                </head>
                <body style=""margin:0; padding:0; background-color:#f4f4f4;"">
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""height:100%; background-color:#f4f4f4;"">
                        <tr>
                            <td align=""center"" valign=""top"">
                                <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#ffffff; border-radius:8px; box-shadow:0 0 10px rgba(0,0,0,0.1); font-family:Arial, sans-serif;"">
                                    <tr>
                                        <td style=""padding:40px; text-align:left;"">
                                            <img alt=""Ivory Stores"" width=""90"" height=""60"" src=""https://res.cloudinary.com/dzmfpxcwu/image/upload/v1754591532/CompanyLogo_z14qli.png"">
                                            <p style=""margin:0 0 20px; padding-top: 20px; font-size:16px; color:#555;"">
                                                {0}
                                            </p>", message);
            builder.HtmlBody += string.Format(@"
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align=""center"" valign=""top"" style="" width: 70%; padding:40px; text-align:left; font-size:12px; color:black; background-color: darkgray;"">
                                            <p class=""""ms-3 mt-3 mb-3"""" style=""""font-family: 'Times New Roman';"""">
                                                &copy;2025 IvoryStores.com. {0}
                                            </p>", Loc["IvoryStoresAndAllRelatedMarks"]);
            builder.HtmlBody += string.Format(@"
                                            <img src=""https://res.cloudinary.com/dzmfpxcwu/image/upload/v1754591012/Ivory_yqbulf.png"" width='80' height='40' alt='Ivory Stores'/>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
            </html>");
            var form = new Dictionary<string, string>
            {
                ["from"] = "Ivory Stores <no-reply@ivorystores.com>",
                ["to"] = toEmail,
                ["subject"] = subject,
                ["html"] = builder.HtmlBody
            };

            var content = new FormUrlEncodedContent(form);
            var response = await _httpClient.PostAsync("messages", content);
            response.EnsureSuccessStatusCode();
        }
        public async Task SendEmailAsync(string toEmail, string fromEmail, string subject, string message)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var builder = new BodyBuilder();
            builder.HtmlBody = string.Format(@"
            <html lang=""en"">
                <head>
                    <meta charset=""UTF-8"">
                </head>
                <body style=""margin:0; padding:0; background-color:#f4f4f4;"">
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""height:100%; background-color:#f4f4f4;"">
                        <tr>
                            <td align=""center"" valign=""top"">
                                <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#ffffff; border-radius:8px; box-shadow:0 0 10px rgba(0,0,0,0.1); font-family:Arial, sans-serif;"">
                                    <tr>
                                        <td style=""padding:40px; text-align:left;"">
                                            <img alt=""Ivory Stores"" width=""90"" height=""60"" src=""https://res.cloudinary.com/dzmfpxcwu/image/upload/v1754591532/CompanyLogo_z14qli.png"">
                                            <p style=""margin:0 0 20px; padding-top: 20px; font-size:16px; color:#555;"">
                                                {0}
                                            </p>", message);
                builder.HtmlBody += string.Format(@"
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align=""center"" valign=""top"" style="" width: 70%; padding:40px; text-align:left; font-size:12px; color:black; background-color: darkgray;"">
                                            <p class=""""ms-3 mt-3 mb-3"""" style=""""font-family: 'Times New Roman';"""">
                                                &copy;2025 IvoryStores.com. {0}
                                            </p>", Loc["IvoryStoresAndAllRelatedMarks"]);
            builder.HtmlBody += string.Format(@"
                                            <img src=""https://res.cloudinary.com/dzmfpxcwu/image/upload/v1754591012/Ivory_yqbulf.png"" width='80' height='40' alt='Ivory Stores'/>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
            </html>");
            var form = new Dictionary<string, string>
            {
                ["from"] = "Ivory Stores <no-reply@ivorystores.com>",
                ["to"] = toEmail,
                ["h:Reply-To"] = fromEmail,
                ["subject"] = subject,
                ["html"] = builder.HtmlBody
            };

            var content = new FormUrlEncodedContent(form);
            var response = await _httpClient.PostAsync("messages", content);
            response.EnsureSuccessStatusCode();
        }

        //    public async Task SendEmailAsync(string toEmail, string subject, string message)
        //    {
        //        IConfiguration EmailSection = configuration.GetSection("GlobalEmailSettings");
        //        string EmailAuthKey = EmailSection["EmailAuthKey"];
        //        if (string.IsNullOrEmpty(EmailAuthKey))
        //        {
        //            throw new Exception("Null EmailAuthKey");
        //        }

        //        await Execute(EmailAuthKey, subject, message, toEmail);
        //    }
        //    public async Task SendEmailAsync(string toEmail, string fromEmail, string subject, string message)
        //    {
        //        IConfiguration EmailSection = configuration.GetSection("GlobalEmailSettings");
        //        string EmailAuthKey = EmailSection["EmailAuthKey"];
        //        if (string.IsNullOrEmpty(EmailAuthKey))
        //        {
        //            throw new Exception("Null EmailAuthKey");
        //        }

        //        await Execute(EmailAuthKey, subject, message, toEmail, fromEmail);
        //    }

        //    public async Task Execute(string apiKey, string subject, string message,
        //        string toEmail)
        //    {
        //        int index = toEmail.IndexOf('@');
        //        string user = toEmail.Substring(0, index);

        //        var msg = new MimeMessage();
        //        var builder = new BodyBuilder();

        //        msg.From.Add(new MailboxAddress("Ivory Stores", "no-reply@ivorystores.com"));
        //        msg.To.Add(InternetAddress.Parse(toEmail));
        //        msg.Subject = subject;
        //        var logo = builder.LinkedResources.Add(@"wwwroot/CompanyLogoEmail.png");
        //        var secondLogo = builder.LinkedResources.Add(@"wwwroot/Ivory.png");
        //        logo.ContentId = MimeUtils.GenerateMessageId();
        //        secondLogo.ContentId = MimeUtils.GenerateMessageId();

        //        //HTML Email Content
        //        builder.HtmlBody = string.Format(
        //    @"<html lang=""en"">
        //    <head>
        //        <meta charset=""UTF-8"">
        //    </head>
        //    <body style=""margin:0; padding:0; background-color:#f4f4f4;"">

        //    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""height:100%; background-color:#f4f4f4;"">
        //        <tr>
        //            <td align=""center"" valign=""top"">
        //                <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#ffffff; border-radius:8px; box-shadow:0 0 10px rgba(0,0,0,0.1); font-family:Arial, sans-serif;"">
        //                    <tr>
        //                        <th style='color:seagreen;'>
        //                            <footer style='font-family: Brush Script MT; padding:0px;font-size:35px;'>
        //                                <img alt=""Ivory Stores Logo"" width=""90"" height=""60"" src=""cid:{0}"">
        //                            </footer>
        //                        </th>
        //                    </tr>", logo.ContentId);
        //        builder.HtmlBody += string.Format(
        //        @"<tr>
        //        <td>
        //            <body style='font-family: Arial, Helvetica, sans-serif;'>
        //                <header style='font-size:20px; padding:25px;'><strong>{0},</strong>", user);
        //        builder.HtmlBody += string.Format(
        //    @"</header>
        //                    <section>
        //                        <article style='font-size:15px; padding:25px;'>
        //                            <p style='text-align:left;'>{0}</p>
        //                        </article>
        //                    </section>", message);
        //        builder.HtmlBody += string.Format(
        //            @"<div style='padding-bottom: 10px; padding-left: 15px;'>
        //            <footer style='font-family: Sans-Serif; font-size:13px; background-color: whitesmoke; color: darkgray; padding: 10px;'>
        //                    <div>
        //                                <p class=""ms-3 mt-3 mb-3"" style=""font-family: 'Times New Roman';"">
        //                                    &copy;2025 IvoryStores.com. {0}
        //                                </p>", Loc["IvoryStoresAndAllRelatedMarks"]);
        //        builder.HtmlBody += string.Format(
        //                                @"<div style=""background-color: whitesmoke; justify-self: left; color: black; width: min-content; padding: 0px;"">
        //                                    <img alt=""Ivory Logo"" src=""cid:{0}"" width=""80"" height=""40""/>
        //                                </div>
        //                            </div> 
        //             </footer>
        //            </div>
        //            </body>
        //            </td>
        //                </tr>", secondLogo.ContentId);
        //        builder.HtmlBody += string.Format(
        //            @"</table>
        //            </td>
        //        </tr>
        //    </table>
        //</html>");

        //        msg.Body = builder.ToMessageBody();
        //        int maxRetries = 3;
        //        int delayMilliseconds = 2000;

        //        for (int attempt = 1; attempt <= maxRetries; attempt++)
        //        {
        //            try
        //            {
        //                using (var client = new MailKit.Net.Smtp.SmtpClient())
        //                {
        //                    client.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

        //                    await client.ConnectAsync("smtpout.secureserver.net", 587, MailKit.Security.SecureSocketOptions.StartTls);
        //                    await client.AuthenticateAsync("no-reply@ivorystores.com", configuration.GetSection("GlobalEmailSettings")["Password"]);

        //                    await client.SendAsync(msg);
        //                    await client.DisconnectAsync(true);
        //                    logger.LogInformation("Email sent to {Recipient} on attempt {Attempt}", toEmail, attempt);
        //                    return;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                logger.LogWarning(ex, "Failed to send email to {Recipient} on attempt {Attempt}", toEmail, attempt);

        //                if (attempt == maxRetries)
        //                {
        //                    logger.LogError("Email sending failed permanently for {Recipient}", toEmail);
        //                    throw;
        //                }

        //                await Task.Delay(delayMilliseconds * attempt); // Exponential backoff
        //            }
        //        }
        //    }
        //    public async Task Execute(string apiKey, string subject, string message,
        //        string toEmail, string fromEmail)
        //    {
        //        int index = toEmail.IndexOf('@');
        //        string user = toEmail.Substring(0, index);

        //        var msg = new MimeMessage();
        //        var builder = new BodyBuilder();

        //        msg.From.Add(new MailboxAddress("Ivory Stores", "no-reply@ivorystores.com"));
        //        msg.To.Add(MailboxAddress.Parse("support@ivorystores.com"));
        //        msg.ReplyTo.Add(MailboxAddress.Parse(fromEmail));
        //        msg.Subject = subject;
        //        var logo = builder.LinkedResources.Add(@"wwwroot/CompanyLogoEmail.png");
        //        var secondLogo = builder.LinkedResources.Add(@"wwwroot/Ivory.png");
        //        logo.ContentId = MimeUtils.GenerateMessageId();
        //        secondLogo.ContentId = MimeUtils.GenerateMessageId();

        //        //HTML Email Content
        //        builder.HtmlBody = string.Format(
        //    @"<html lang=""en"">
        //    <head>
        //        <meta charset=""UTF-8"">
        //    </head>
        //    <body style=""margin:0; padding:0; background-color:#f4f4f4;"">

        //    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""height:100%; background-color:#f4f4f4;"">
        //        <tr>
        //            <td align=""center"" valign=""top"">
        //                <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#ffffff; border-radius:8px; box-shadow:0 0 10px rgba(0,0,0,0.1); font-family:Arial, sans-serif;"">
        //                    <tr>
        //                        <th style='color:seagreen;'>
        //                            <footer style='font-family: Brush Script MT; padding:0px;font-size:35px;'>
        //                                <img alt=""Ivory Stores Logo"" width=""90"" height=""60"" src=""cid:{0}"">
        //                            </footer>
        //                        </th>
        //                    </tr>", logo.ContentId);
        //        builder.HtmlBody += string.Format(
        //        @"<tr>
        //        <td>
        //            <body style='font-family: Arial, Helvetica, sans-serif;'>
        //                <header style='font-size:20px; padding:25px;'><strong>{0},</strong>", user);
        //        builder.HtmlBody += string.Format(
        //    @"</header>
        //                    <section>
        //                        <article style='font-size:15px; padding:25px;'>
        //                            <p style='text-align:left;'>{0}</p>
        //                        </article>
        //                    </section>", message);
        //        builder.HtmlBody += string.Format(
        //            @"<div style='padding-bottom: 10px; padding-left: 15px;'>
        //            <footer style='font-family: Sans-Serif; font-size:13px; background-color: whitesmoke; color: darkgray; padding: 10px;'>
        //                    <div>
        //                                <p class=""ms-3 mt-3 mb-3"" style=""font-family: 'Times New Roman';"">
        //                                    &copy;2025 IvoryStores.com. {0}
        //                                </p>", Loc["IvoryStoresAndAllRelatedMarks"]);
        //        builder.HtmlBody += string.Format(
        //                                @"<div style=""background-color: whitesmoke; justify-self: left; color: black; width: min-content; padding: 0px;"">
        //                                    <img alt=""Ivory Logo"" src=""cid:{0}"" width=""80"" height=""40""/>
        //                                </div>
        //                            </div> 
        //             </footer>
        //            </div>
        //            </body>
        //            </td>
        //                </tr>", secondLogo.ContentId);
        //        builder.HtmlBody += string.Format(
        //            @"</table>
        //            </td>
        //        </tr>
        //    </table>
        //</html>");

        //        msg.Body = builder.ToMessageBody();
        //        int maxRetries = 3;
        //        int delayMilliseconds = 2000;

        //        for (int attempt = 1; attempt <= maxRetries; attempt++)
        //        {
        //            try
        //            {
        //                using (var client = new MailKit.Net.Smtp.SmtpClient())
        //                {
        //                    client.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

        //                    await client.ConnectAsync("smtpout.secureserver.net", 587, MailKit.Security.SecureSocketOptions.StartTls);
        //                    await client.AuthenticateAsync("no-reply@ivorystores.com", configuration.GetSection("GlobalEmailSettings")["Password"]);

        //                    await client.SendAsync(msg);
        //                    await client.DisconnectAsync(true);
        //                    logger.LogInformation("Email sent to {Recipient} on attempt {Attempt}", toEmail, attempt);
        //                    return;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                logger.LogWarning(ex, "Failed to send email to {Recipient} on attempt {Attempt}", toEmail, attempt);

        //                if (attempt == maxRetries)
        //                {
        //                    logger.LogError("Email sending failed permanently for {Recipient}", toEmail);
        //                    throw;
        //                }

        //                await Task.Delay(delayMilliseconds * attempt); // Exponential backoff
        //            }
        //        }
        //    }
    }
}
