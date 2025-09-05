using MethaWebsite.Data;
using MethaWebsite.Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using MimeKit;
using System.Collections;
using System.Net.Http.Headers;
using System.Text;

namespace MethaWebsite.Services
{
    public class EmailSender(ILogger<EmailSender> logger, IConfiguration configuration, IStringLocalizer<SharedResource> Loc) : IEmailSender<ApplicationUser>
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri($"https://api.mailgun.net/v3/{configuration.GetSection("Mailgun")["Domain"]}/")
        };
        byte[] byteArray = Encoding.ASCII.GetBytes($"api:{configuration.GetSection("Mailgun")["ApiKey"]}");

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


        public async Task SendLinkEmailAsync(string toEmail, string subject, string message)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            byte[] logo_image_Bytes = File.ReadAllBytes("wwwroot/CompanyLogoEmail.png");
            string base64Image = Convert.ToBase64String(logo_image_Bytes);
            string logoImageSrc = $"data:image/png;base64,{base64Image}";

            byte[] second_logo_image_Bytes = File.ReadAllBytes("wwwroot/Ivory.png");
            string base64Image2 = Convert.ToBase64String(second_logo_image_Bytes);
            string secondLogoImageSrc = $"data:image/png;base64,{base64Image2}";

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
                                        <td style=""padding:40px; padding-bottom: 20px; text-align:left;"">
                                            <img alt=""Ivory Stores"" width=""90"" height=""60"" src=""https://res.cloudinary.com/dzmfpxcwu/image/upload/v1754591532/CompanyLogo_z14qli.png"">
                                            <h2 style=""margin:0 0 20px; color:#333;"">Welcome to Ivory Stores</h2>
                                            <p style="" padding-top: 10px; margin:0 0 20px; font-size:16px; color:#555;"">
                                                Please confirm your account by <a style=""text-decoration: none;"" href=""{0}"">clicking here</a>.
                                            </p>
                                        </td>
                                    </tr>", message);
            builder.HtmlBody += string.Format(@"
                                    <tr>
                                        <td align=""center"" valign=""top"" style="" width: 70%; padding:40px; text-align:left; font-size:12px; color:black; background-color: darkgray;"">
                                            <p class=""ms-3 mt-3 mb-3"" style=""font-family: 'Times New Roman';"">
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
