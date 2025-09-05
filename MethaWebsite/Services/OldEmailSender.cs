using MailKit;
using MethaWebsite.Data;
using MethaWebsite.Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using MimeKit;
using MimeKit.Utils;
namespace MethaWebsite.Services
{
    public class OldEmailSender(ILogger<EmailSender> logger, IConfiguration configuration, IStringLocalizer<SharedResource> Loc) : IEmailSender<ApplicationUser>
    {
        Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        => SendLinkEmailAsync(email, "Confirm your email", confirmationLink);

        Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        => SendEmailAsync(email, "Reset your password",
                $"Looks like you're having trouble accessing your account. " +
                $"Please reset your password using the following code: {resetCode}");

        Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        => SendEmailAsync(email, "Reset your password",
                $"Looks like you're having trouble accessing your account. " +
                $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            IConfiguration EmailSection = configuration.GetSection("GlobalEmailSettings");
            string EmailAuthKey = EmailSection["EmailAuthKey"];
            if (string.IsNullOrEmpty(EmailAuthKey))
            {
                throw new Exception("Null EmailAuthKey");
            }

            await Execute(EmailAuthKey, subject, message, toEmail);
        }
        public async Task SendEmailAsync(string toEmail, string fromEmail, string subject, string message)
        {
            IConfiguration EmailSection = configuration.GetSection("GlobalEmailSettings");
            string EmailAuthKey = EmailSection["EmailAuthKey"];
            if (string.IsNullOrEmpty(EmailAuthKey))
            {
                throw new Exception("Null EmailAuthKey");
            }

            await Execute(EmailAuthKey, subject, message, toEmail, fromEmail);
        }
        public async Task SendLinkEmailAsync(string toEmail, string subject, string message)
        {
            IConfiguration EmailSection = configuration.GetSection("GlobalEmailSettings");
            string EmailAuthKey = EmailSection["EmailAuthKey"];
            if (string.IsNullOrEmpty(EmailAuthKey))
            {
                throw new Exception("Null EmailAuthKey");
            }

            await ExecuteLink(EmailAuthKey, subject, message, toEmail);
        }

        public async Task ExecuteLink(string apiKey, string subject, string message,
            string toEmail)
        {
            int index = toEmail.IndexOf('@');
            string user = toEmail.Substring(0, index);

            var msg = new MimeMessage();
            var builder = new BodyBuilder();

            msg.From.Add(new MailboxAddress("Ivory Stores", "no-reply@ivorystores.com"));
            msg.To.Add(InternetAddress.Parse(toEmail));
            msg.Subject = subject;
            var logo = builder.LinkedResources.Add(@"wwwroot/CompanyLogoEmail.png");
            var secondLogo = builder.LinkedResources.Add(@"wwwroot/Ivory.png");
            logo.ContentId = MimeUtils.GenerateMessageId();
            secondLogo.ContentId = MimeUtils.GenerateMessageId();

            //HTML Email Content
            builder.HtmlBody = string.Format(
        @"<html lang=""en"">
        <head>
            <meta charset=""UTF-8"">
        </head>
        <body style=""margin:0; padding:0; background-color:#f4f4f4;"">

        <table width='100%' cellpadding='0' cellspacing='0' border='0' style='height:100%; background-color:#f4f4f4;'>
            <tr>
                <td align='center' valign='top'>
                    <table width='600' cellpadding='0' cellspacing='0' border='0' style='background-color:#ffffff; border-radius:8px; box-shadow:0 0 10px rgba(0,0,0,0.1); font-family:Arial, sans-serif;'>
                        <tr>
                            <th style='color:seagreen;'>
                                <footer style='font-family: Brush Script MT; padding:0px;font-size:35px;'>
                                    <img alt=""Ivory Stores Logo"" width=""90"" height=""60"" src=""cid:{0}"">
                                </footer>
                            </th>
                        </tr>", logo.ContentId);
            builder.HtmlBody += string.Format(
            @"<tr>
            <td>
                <body style='font-family: Arial, Helvetica, sans-serif;'>
                    <header style='font-size:20px; padding:10px;'><strong style='padding-left: 15px;'>{0},</strong>", user);
            builder.HtmlBody += string.Format(
        @"</header>
                        <section>
                            <article style='font-size:15px; padding:10px;'>
                                <p style='text-align:left; padding-left: 15px;'> Please confirm your account by <a style='color:darkgray;' href={0}> clicking here.</a></p>
                            </article>
                        </section>", message);
            builder.HtmlBody += string.Format(
                @"<div style='padding-bottom: 10px; padding-left: 15px;'>
                <footer style='font-family: Sans-Serif; font-size:16px; background-color: whitesmoke; color: darkgray; padding: 10px;'>
                        <div>
                                    <p class=""ms-3 mt-3 mb-3"" style=""font-family: 'Times New Roman';"">
                                        &copy;2025 IvoryStores.com. {0}
                                    </p>", Loc["IvoryStoresAndAllRelatedMarks"]);
            builder.HtmlBody += string.Format(
                                    @"<div style=""background-color: whitesmoke; justify-self: left; color: black; width: min-content; padding: 0px;"">
                                        <img alt=""Ivory Logo"" src=""cid:{0}"" width=""80"" height=""40""/>
                                    </div>
                                </div> 
                 </footer>
                </div>
                </body>
                </td>
                    </tr>", secondLogo.ContentId);
            builder.HtmlBody += string.Format(
                @"</table>
                </td>
            </tr>
        </table>
    </html>");

            msg.Body = builder.ToMessageBody();
            int maxRetries = 3;
            int delayMilliseconds = 2000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using (var client = new MailKit.Net.Smtp.SmtpClient())
                    {
                        client.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

                        await client.ConnectAsync("smtpout.secureserver.net", 587, MailKit.Security.SecureSocketOptions.StartTls);
                        var password = configuration.GetSection("GlobalEmailSettings")["Password"];
                        await client.AuthenticateAsync("no-reply@ivorystores.com", password);

                        await client.SendAsync(msg);
                        await client.DisconnectAsync(true);
                        logger.LogInformation("Email sent to {Recipient} on attempt {Attempt}", toEmail, attempt);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to send email to {Recipient} on attempt {Attempt}", toEmail, attempt);

                    if (attempt == maxRetries)
                    {
                        logger.LogError("Email sending failed permanently for {Recipient}", toEmail);
                        throw;
                    }

                    await Task.Delay(delayMilliseconds * attempt); // Exponential backoff
                }
            }
        }
        public async Task Execute(string apiKey, string subject, string message,
            string toEmail)
        {
            int index = toEmail.IndexOf('@');
            string user = toEmail.Substring(0, index);

            var msg = new MimeMessage();
            var builder = new BodyBuilder();

            msg.From.Add(new MailboxAddress("Ivory Stores", "no-reply@ivorystores.com"));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = subject;
            var logo = builder.LinkedResources.Add(@"wwwroot/CompanyLogoEmail.png");
            var secondLogo = builder.LinkedResources.Add(@"wwwroot/Ivory.png");
            logo.ContentId = MimeUtils.GenerateMessageId();
            secondLogo.ContentId = MimeUtils.GenerateMessageId();

            //HTML Email Content
            builder.HtmlBody = string.Format(
        @"<html lang=""en"">
        <head>
            <meta charset=""UTF-8"">
        </head>
        <body style=""margin:0; padding:0; background-color:#f4f4f4;"">

        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""height:100%; background-color:#f4f4f4;"">
            <tr>
                <td align=""center"" valign=""top"">
                    <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#ffffff; border-radius:8px; box-shadow:0 0 10px rgba(0,0,0,0.1); font-family:Arial, sans-serif;"">
                        <tr>
                            <th style='color:seagreen;'>
                                <footer style='font-family: Brush Script MT; padding:0px;font-size:35px;'>
                                    <img alt=""Ivory Stores Logo"" width=""90"" height=""60"" src=""cid:{0}"">
                                </footer>
                            </th>
                        </tr>", logo.ContentId);
            builder.HtmlBody += string.Format(
            @"<tr>
            <td>
                <body style='font-family: Arial, Helvetica, sans-serif;'>
                    <header style='font-size:20px; padding:25px;'><strong>{0},</strong>", user);
            builder.HtmlBody += string.Format(
        @"</header>
                        <section>
                            <article style='font-size:15px; padding:25px;'>
                                <p style='text-align:left;'>{0}</p>
                            </article>
                        </section>", message);
            builder.HtmlBody += string.Format(
                @"<div style='padding-bottom: 10px; padding-left: 15px;'>
                <footer style='font-family: Sans-Serif; font-size:13px; background-color: whitesmoke; color: darkgray; padding: 10px;'>
                        <div>
                                    <p class=""ms-3 mt-3 mb-3"" style=""font-family: 'Times New Roman';"">
                                        &copy;2025 IvoryStores.com. {0}
                                    </p>", Loc["IvoryStoresAndAllRelatedMarks"]);
            builder.HtmlBody += string.Format(
                                    @"<div style=""background-color: whitesmoke; justify-self: left; color: black; width: min-content; padding: 0px;"">
                                        <img alt=""Ivory Logo"" src=""cid:{0}"" width=""80"" height=""40""/>
                                    </div>
                                </div> 
                 </footer>
                </div>
                </body>
                </td>
                    </tr>", secondLogo.ContentId);
            builder.HtmlBody += string.Format(
                @"</table>
                </td>
            </tr>
        </table>
    </html>");

            msg.Body = builder.ToMessageBody();
            int maxRetries = 3;
            int delayMilliseconds = 2000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using (var client = new MailKit.Net.Smtp.SmtpClient())
                    {
                        client.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

                        await client.ConnectAsync("smtpout.secureserver.net", 587, MailKit.Security.SecureSocketOptions.StartTls);
                        await client.AuthenticateAsync("no-reply@ivorystores.com", configuration.GetSection("GlobalEmailSettings")["Password"]);

                        await client.SendAsync(msg);
                        await client.DisconnectAsync(true);
                        logger.LogInformation("Email sent to {Recipient} on attempt {Attempt}", toEmail, attempt);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to send email to {Recipient} on attempt {Attempt}", toEmail, attempt);

                    if (attempt == maxRetries)
                    {
                        logger.LogError("Email sending failed permanently for {Recipient}", toEmail);
                        throw;
                    }

                    await Task.Delay(delayMilliseconds * attempt); // Exponential backoff
                }
            }
        }
        public async Task Execute(string apiKey, string subject, string message,
            string toEmail, string fromEmail)
        {
            int index = toEmail.IndexOf('@');
            string user = toEmail.Substring(0, index);

            var msg = new MimeMessage();
            var builder = new BodyBuilder();

            msg.From.Add(new MailboxAddress("Ivory Stores", "no-reply@ivorystores.com"));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.ReplyTo.Add(MailboxAddress.Parse(fromEmail));
            msg.Subject = subject;
            var logo = builder.LinkedResources.Add(@"wwwroot/CompanyLogoEmail.png");
            var secondLogo = builder.LinkedResources.Add(@"wwwroot/Ivory.png");
            logo.ContentId = MimeUtils.GenerateMessageId();
            secondLogo.ContentId = MimeUtils.GenerateMessageId();

            //HTML Email Content
            builder.HtmlBody = string.Format(
        @"<html lang=""en"">
        <head>
            <meta charset=""UTF-8"">
        </head>
        <body style=""margin:0; padding:0; background-color:#f4f4f4;"">

        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""height:100%; background-color:#f4f4f4;"">
            <tr>
                <td align=""center"" valign=""top"">
                    <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#ffffff; border-radius:8px; box-shadow:0 0 10px rgba(0,0,0,0.1); font-family:Arial, sans-serif;"">
                        <tr>
                            <th style='color:seagreen;'>
                                <footer style='font-family: Brush Script MT; padding:0px;font-size:35px;'>
                                    <img alt=""Ivory Stores Logo"" width=""90"" height=""60"" src=""cid:{0}"">
                                </footer>
                            </th>
                        </tr>", logo.ContentId);
            builder.HtmlBody += string.Format(
            @"<tr>
            <td>
                <body style='font-family: Arial, Helvetica, sans-serif;'>
                    <header style='font-size:20px; padding:25px;'><strong>{0},</strong>", user);
            builder.HtmlBody += string.Format(
        @"</header>
                        <section>
                            <article style='font-size:15px; padding:25px;'>
                                <p style='text-align:left;'>{0}</p>
                            </article>
                        </section>", message);
            builder.HtmlBody += string.Format(
                @"<div style='padding-bottom: 10px; padding-left: 15px;'>
                <footer style='font-family: Sans-Serif; font-size:13px; background-color: whitesmoke; color: darkgray; padding: 10px;'>
                        <div>
                                    <p class=""ms-3 mt-3 mb-3"" style=""font-family: 'Times New Roman';"">
                                        &copy;2025 IvoryStores.com. {0}
                                    </p>", Loc["IvoryStoresAndAllRelatedMarks"]);
            builder.HtmlBody += string.Format(
                                    @"<div style=""background-color: whitesmoke; justify-self: left; color: black; width: min-content; padding: 0px;"">
                                        <img alt=""Ivory Logo"" src=""cid:{0}"" width=""80"" height=""40""/>
                                    </div>
                                </div> 
                 </footer>
                </div>
                </body>
                </td>
                    </tr>", secondLogo.ContentId);
            builder.HtmlBody += string.Format(
                @"</table>
                </td>
            </tr>
        </table>
    </html>");

            msg.Body = builder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

                await client.ConnectAsync("smtpout.secureserver.net", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("no-reply@ivorystores.com", configuration.GetSection("GlobalEmailSettings")["Password"]);

                await client.SendAsync(msg);
                await client.DisconnectAsync(true);
                logger.LogInformation("Email to {EmailAddress} sent!", toEmail);
            }
        }

        Task IEmailSender<ApplicationUser>.SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            return SendConfirmationLinkAsync(user, email, confirmationLink);
        }

        Task IEmailSender<ApplicationUser>.SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            return SendPasswordResetLinkAsync(user, email, resetLink);
        }

        Task IEmailSender<ApplicationUser>.SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            return SendPasswordResetCodeAsync(user, email, resetCode);
        }
    }
}
