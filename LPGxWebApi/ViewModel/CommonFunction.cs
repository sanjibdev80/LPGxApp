using System.Net;
using System.Net.Mail;
using System.Text;

namespace LPGxWebApi.ViewModel
{
    public class CommonFunction
    {
        // send OTP
        public CommonFunction(string OTPMedia, string ToEmailId, string plainTxtOTP, string UserName, ref bool sendemail)
        {
            string ProjectName = EmailSettings.ProjectName;

            // Send OTP if selected medium is email
            if (OTPMedia == "E")
            {
                // Format email body
                string mailBody = EmailTemplate(ToEmailId, ProjectName, plainTxtOTP, UserName);

                // Set up email details
                string to = ToEmailId.Trim().ToLower();
                string from = EmailSettings.FromAddress;

                // Set up the email message
                MailMessage message = new MailMessage(from, to)
                {
                    Subject = $"OTP Code from {ProjectName}",
                    Body = mailBody,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = true
                };

                // Configure the SMTP client
                SmtpClient client = new SmtpClient(EmailSettings.SmtpServer, EmailSettings.port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(EmailSettings.FromAddress, EmailSettings.FromAddressPassword)
                };

                try
                {
                    client.Send(message);
                    sendemail = true; // Successful send
                }
                catch (SmtpException smtpEx)
                {
                    Console.WriteLine($"SMTP error: {smtpEx.StatusCode} - {smtpEx.Message}");
                    sendemail = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email send error: {ex.Message}");
                    sendemail = false; // Send failed
                }

            }

        }

        // Transaction details
        public CommonFunction(string OTPMedia, string ToEmailId, string TxnRefId, string BillType, decimal TxnAmount, string ReceivedBy, string UserName, string Remarks, string AptNo, string BldName, ref bool sendemail)
        {
            string ProjectName = EmailSettings.ProjectName;

            // Send OTP if selected medium is email
            if (OTPMedia == "E")
            {
                // Format email body
                string mailBody = EmailTransactionTemplate(ToEmailId, ProjectName, TxnRefId, BillType, TxnAmount, ReceivedBy, UserName, Remarks, AptNo, BldName);

                // Set up email details
                string to = ToEmailId.Trim().ToLower();
                string from = EmailSettings.FromAddress;

                // Set up the email message
                MailMessage message = new MailMessage(from, to)
                {
                    Subject = $"Transaction details from {ProjectName}",
                    Body = mailBody,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = true
                };

                // Configure the SMTP client
                SmtpClient client = new SmtpClient(EmailSettings.SmtpServer, EmailSettings.port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(EmailSettings.FromAddress, EmailSettings.FromAddressPassword)
                };

                try
                {
                    client.Send(message);
                    sendemail = true; // Successful send
                }
                catch (SmtpException smtpEx)
                {
                    Console.WriteLine($"SMTP error: {smtpEx.StatusCode} - {smtpEx.Message}");
                    sendemail = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email send error: {ex.Message}");
                    sendemail = false; // Send failed
                }

            }

        }

        //New user
        public CommonFunction(string OTPMedia, string ToEmailId, string passKey, string UserName, string contactno, string urlLink, ref bool sendemail)
        {
            string ProjectName = EmailSettings.ProjectName;

            // Send OTP if selected medium is email
            if (OTPMedia == "E")
            {
                // Format email body
                string mailBody = EmailUserCreateTemplate(ToEmailId, ProjectName, passKey, UserName, contactno, urlLink);

                // Set up email details
                string to = ToEmailId.Trim().ToLower();
                string from = EmailSettings.FromAddress;

                // Set up the email message
                MailMessage message = new MailMessage(from, to)
                {
                    Subject = $"New user from {ProjectName}",
                    Body = mailBody,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = true
                };

                // Configure the SMTP client
                SmtpClient client = new SmtpClient(EmailSettings.SmtpServer, EmailSettings.port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(EmailSettings.FromAddress, EmailSettings.FromAddressPassword)
                };

                try
                {
                    client.Send(message);
                    sendemail = true; // Successful send
                }
                catch (SmtpException smtpEx)
                {
                    Console.WriteLine($"SMTP error: {smtpEx.StatusCode} - {smtpEx.Message}");
                    sendemail = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email send error: {ex.Message}");
                    sendemail = false; // Send failed
                }
            }
        }

        public string EmailTemplate(string ToEmailId, string ProjectName, string plainTxtOTP, string UserName)
        {
            string to = ToEmailId.Trim(); //To address    
            string from = EmailSettings.FromAddress; //From address    
            MailMessage message = new MailMessage(from, to);

            #region OTP Confirmation Email Template

            StringBuilder sb = new StringBuilder();
            sb.Append(@"<!doctype html>
                <html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
                <head>");
            sb.Append(@"<title>" + ProjectName + "</title>");
            sb.Append(@"
                <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                <!--<![endif]-->
                <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
                <style type=""text/css"">
                    #outlook a {
                padding: 0;
                }

                .ReadMsgBody {
                  width: 100%;
                }

                .ExternalClass {
                  width: 100%;
                }

                .ExternalClass * {
                  line-height: 100%;
                }

                body {
                  margin: 0;
                  padding: 0;
                  -webkit-text-size-adjust: 100%;
                  -ms-text-size-adjust: 100%;
                }

                table,
                td {
                  border-collapse: collapse;
                  mso-table-lspace: 0pt;
                  mso-table-rspace: 0pt;
                }

                img {
                  border: 0;
                  height: auto;
                  line-height: 100%;
                  outline: none;
                  text-decoration: none;
                  -ms-interpolation-mode: bicubic;
                }

                p {
                  display: block;
                  margin: 13px 0;
                }
                </style>
                <style type=""text/css"">
                @media only screen and (max-width:480px) {
                  @-ms-viewport {
                    width: 320px;
                }
                @viewport {
                    width: 320px;
                }
                }
                </style>
                <link href=""https://fonts.googleapis.com/css?family=Ubuntu:300,400,500,700"" rel=""stylesheet"" type=""text/css"">
                <style type=""text/css"">
                @import url(https://fonts.googleapis.com/css?family=Ubuntu:300,400,500,700);
                </style>
                <style type=""text/css"">
                @media only screen and (min-width:480px) {
                  .mj-column-per-100 {
                    width: 100% !important;
                    max-width: 100%;
                }
                .mj-column-per-50 {
                    width: 50% !important;
                    max-width: 50%;
                }
                }
                </style>
                <style type=""text/css"">
                @media only screen and (max-width:480px) {
                  table.full-width-mobile {
                    width: 100% !important;
                }
                td.full-width-mobile {
                    width: auto !important;
                }
                }
                </style>
                </head>
                <body style=""background-color:#F4F4F4; padding-top: 50px;"">
                <div style=""background-color:#F4F4F4;"">
                <div style=""background:#131519;background-color:#131519;Margin:0px auto;max-width:600px;"">
                <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#131519;background-color:#131519;width:100%;"">
                <tbody>
                <tr>
                <td style=""direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
                <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"">
                <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top;"" width=""100%"">
                <tr>
                <td align=""center"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">
                <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:collapse;border-spacing:0px;"">
                <tbody>
                <tr>
                ");
            sb.Append(@"<td style=""width:428px;""> <div style=""font-family:Arial, sans-serif;font-size:30px;line-height:22px;text-align:center;color:#ffffff;"">" + ProjectName + "</div> </td>");
            sb.Append(@"
                </tr>
                </tbody>
                </table>
                </td>
                </tr>
                </table>
                </div>
                </td>
                </tr>
                </tbody>
                </table>
                </div>
                <div style=""background:#f1f5f9;background-color:#f1f5f9;Margin:0px auto;max-width:600px;"">
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#f1f5f9;background-color:#f1f5f9;width:100%;"">
                        <tbody>
                        <tr>
                        <td style=""direction:ltr;font-size:0px;padding:30px 0 15px;text-align:center;vertical-align:top;border-bottom: 1px solid #ccc;"">
                        <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"">
                        <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top;"" width=""100%"">
                        <tr>
                        <td align=""left"" style=""font-size:0px;padding:0px 25px 0px 25px;word-break:break-word;"">");
            sb.Append(@"<div style=""font-family:Arial, sans-serif;font-size:20px;font-weight:700;line-height:28px;text-align:left;color:#f9b707;"">Dear, " + UserName + "</div>");
            sb.Append(@"
                        </td>
                        </tr>
                        </table>
                        </div>
                        </td>
                        </tr>
                        </tbody>
                        </table>
                        </div>
                        <div style=""background:#f1f5f9;background-color:#f1f5f9;Margin:0px auto;max-width:600px;"">
                          <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#f1f5f9;background-color:#f1f5f9;width:100%;"">
                            <tbody>
                              <tr>
                                <td style=""direction:ltr;font-size:0px;padding:0px 0px 15px 0px;text-align:center;vertical-align:top;border-bottom: 1px solid #ccc;"">
                                  <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"">
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top;"" width=""100%"">
                                      <tr>
                                        <td align=""left"" style=""font-size:0px;padding:15px 25px 25px 25px;word-break:break-word;"">
                                          <div style=""font-family:Arial, sans-serif;font-size:18px;line-height:28px;text-align:left;color:#55575d;"">Your OTP Code is below.  Do not share your OTP code to anyone.</div>
                                        </td>
                                      </tr>
                                    </table>
                                  </div>
                                </td>
                              </tr>
                            </tbody>
                          </table>
                        </div>
                         <div style=""Margin:0px auto;max-width:600px;background:#f1f5f9; "">
                          <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""width:100%;"">
                            <tbody>
                              <tr>
                                <td style=""direction:ltr;font-size:0px;padding:10px 30px 15px;text-align:center;vertical-align:top;"">
                                  <div class=""mj-column-per-50 outlook-group-fix"" style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"">
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top;"" width=""100%"">
                                      <tr>
                                        <td align=""center"" vertical-align=""middle"" style=""font-size:0px;padding:5px 2px;word-break:break-word;"">
                                          <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:separate;width:100%;line-height:100%;"">
                                            <tr>
                                              <td align=""center"" bgcolor=""#414141"" role=""presentation"" style=""border:none;border-radius:0px;cursor:auto;padding:10px 25px;background:#414141;"" valign=""middle"">
                                                <p style=""background:#414141;color:#ffffff;font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:13px;font-weight:normal;line-height:120%;Margin:0;text-decoration:none;text-transform:none;"">");
            sb.Append(@"<span style=""letter-spacing: 2px;"">OTP Code :</span> <span>" + plainTxtOTP + "</span> </p>");
            sb.Append(@"
                                              </td>
                                            </tr>
                                          </table>
                                        </td>
                                      </tr>
                                    </table>
                                  </div>
                                </td>
                              </tr>
                            </tbody>
                          </table>
                        </div>
                <div style=""background:#f9b707;background-color:#f9b707;Margin:0px auto;max-width:600px;"">
                <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#f9b707;background-color:#f9b707;width:100%;"">
                <tbody>
                <tr>
                <td style=""direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
                <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"">
                <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top;"" width=""100%"">
                <tr>
                <td align=""center"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">");
            sb.Append(@"<div style=""font-family:Arial, sans-serif;font-size:16px;line-height:22px;text-align:center;color:#000000;""> Regards <b>" + ProjectName + " Team</b> </div>");
            sb.Append(@"</td>
                </tr>
                </table>
                </div>
                </td>
                </tr>
                </tbody>
                </table>
                </div>
                </div>
                </body>
                </html>
                ");
            #endregion OTP Confirmation Email Template

            return sb.ToString();
        }

        public string EmailTransactionTemplate(string ToEmailId, string ProjectName, string TxnRefId, string BillType, decimal TxnAmount, string ReceivedBy, string UserName, string Remarks, string AptNo, string BldName)
        {
            string to = ToEmailId.Trim(); //To address    
            string from = EmailSettings.FromAddress; //From address    
            MailMessage message = new MailMessage(from, to);

            #region Email Template
            StringBuilder sb = new StringBuilder();

            sb.Append(@"<!doctype html>
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <title>" + ProjectName + @"</title>
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <style type=""text/css"">
        #outlook a { padding: 0; }
        .ReadMsgBody { width: 100%; }
        .ExternalClass { width: 100%; }
        .ExternalClass * { line-height: 100%; }
        body { margin: 0; padding: 0; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; }
        table, td { border-collapse: collapse; mso-table-lspace: 0pt; mso-table-rspace: 0pt; }
        img { border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; }
        p { display: block; margin: 13px 0; }
    </style>
</head>
<body style=""background-color:#F4F4F4; padding-top: 50px;"">
    <div style=""background-color:#F4F4F4;"">
        <div style=""background:#131519; background-color:#131519; Margin:0px auto; max-width:600px;"">
            <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#131519; width:100%;"">
                <tbody>
                    <tr>
                        <td style=""direction:ltr; font-size:0px; padding:20px 0; text-align:center; vertical-align:top;"">
                            <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px; text-align:left; direction:ltr; display:inline-block; vertical-align:top; width:100%;"">
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top; width:100%;"">
                                    <tr>
                                        <td align=""center"" style=""font-size:0px; padding:10px 25px; word-break:break-word;"">
                                            <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:collapse;"">
                                                <tbody>
                                                    <tr>
                                                        <td style=""width:428px;"">
                                                            <div style=""font-family:Arial, sans-serif; font-size:30px; line-height:22px; text-align:center; color:#ffffff;"">
                                                                " + ProjectName + @"
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div style=""background:#f1f5f9; background-color:#f1f5f9; Margin:0px auto; max-width:600px;"">
            <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#f1f5f9; width:100%;"">
                <tbody>
                    <tr>
                        <td style=""direction:ltr; font-size:0px; padding:30px 0 15px; text-align:center; vertical-align:top; border-bottom: 1px solid #ccc;"">
                            <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px; text-align:left; direction:ltr; display:inline-block; vertical-align:top; width:100%;"">
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top; width:100%;"">
                                    <tr>
                                        <td align=""left"" style=""font-size:0px; padding:0px 25px; word-break:break-word;"">
                                            <div style=""font-family:Arial, sans-serif; font-size:20px; font-weight:700; line-height:28px; text-align:left; color:#f9b707;"">
                                                Dear, " + UserName + @"
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div style=""background:#f1f5f9; background-color:#f1f5f9; Margin:0px auto; max-width:600px;"">
            <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#f1f5f9; width:100%;"">
                <tbody>
                    <tr>
                        <td style=""direction:ltr; font-size:0px; padding:0px 0px 15px 0px; text-align:center; vertical-align:top; border-bottom: 1px solid #ccc;"">
                            <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px; text-align:left; direction:ltr; display:inline-block; vertical-align:top; width:100%;"">
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top; width:100%;"">
                                    <tr>
                                        <td align=""left"" style=""font-size:0px; padding:15px 25px 25px 25px; word-break:break-word;"">
                                            <div style=""font-family:Arial, sans-serif; font-size:18px; line-height:28px; text-align:left; color:#55575d;"">
                                                Your transaction details as follows:
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <!-- Table data section -->
        <div style=""Margin:0px auto; max-width:600px; background:#f1f5f9;"">
            <table align=""center"" border=""0"" cellpadding=""10"" cellspacing=""0"" role=""presentation"" style=""width:100%; border: 1px solid #ccc; background-color:#fff;"">
                <tbody>
                    <tr style=""background-color:#f9b707;"">
                        <td style=""padding:10px; font-weight:bold; text-align:left; border: 1px solid #ccc;"">Building Name</td>
                        <td style=""padding:10px; font-weight:bold; text-align:left; border: 1px solid #ccc;"">" + BldName + @"</td>
                    </tr>
                    <tr>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">Apartment No</td>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">" + AptNo + @"</td>
                    </tr>
                    <tr style=""background-color:#f9b707;"">
                        <td style=""padding:10px; font-weight:bold; text-align:left; border: 1px solid #ccc;"">Transaction Reference ID</td>
                        <td style=""padding:10px; font-weight:bold; text-align:left; border: 1px solid #ccc;"">" + TxnRefId + @"</td>
                    </tr>
                    <tr>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">Bill Type</td>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">" + BillType + @"</td>
                    </tr>
                    <tr style=""background-color:#f9b707;"">
                        <td style=""padding:10px; font-weight:bold; text-align:left; border: 1px solid #ccc;"">Transaction Amount</td>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">" + TxnAmount.ToString("0.00") + @"</td>
                    </tr>
                    <tr>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">Received By</td>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">" + ReceivedBy + @"</td>
                    </tr>
                    <tr style=""background-color:#f9b707;"">
                        <td style=""padding:10px; font-weight:bold; text-align:left; border: 1px solid #ccc;"">Received Time</td>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + @"</td>
                    </tr>
                    <tr>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">Remarks</td>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">" + Remarks + @"</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div style=""background:#f9b707; background-color:#f9b707; Margin:0px auto; max-width:600px;"">
            <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#f9b707; width:100%;"">
                <tbody>
                    <tr>
                        <td style=""direction:ltr; font-size:0px; padding:20px 0; text-align:center; vertical-align:top;"">
                            <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px; text-align:left; direction:ltr; display:inline-block; vertical-align:top; width:100%;"">
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top; width:100%;"">
                                    <tr>
                                        <td align=""center"" style=""font-size:0px; padding:10px 25px; word-break:break-word;"">
                                            <div style=""font-family:Arial, sans-serif; font-size:16px; line-height:22px; text-align:center; color:#000000;"">
                                                Regards, <b>" + ProjectName + @" Team
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</body>
</html>");

            #endregion Email Template

            return sb.ToString();
        }

        public string EmailUserCreateTemplate(string ToEmailId, string ProjectName, string passKey, string UserName, string contactno, string urlLink)
        {
            string to = ToEmailId.Trim(); //To address    
            string from = EmailSettings.FromAddress; //From address    
            MailMessage message = new MailMessage(from, to);

            #region Email Template
            StringBuilder sb = new StringBuilder();

            sb.Append(@"<!doctype html>
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <title>" + ProjectName + @"</title>
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <style type=""text/css"">
        #outlook a { padding: 0; }
        .ReadMsgBody { width: 100%; }
        .ExternalClass { width: 100%; }
        .ExternalClass * { line-height: 100%; }
        body { margin: 0; padding: 0; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; }
        table, td { border-collapse: collapse; mso-table-lspace: 0pt; mso-table-rspace: 0pt; }
        img { border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; }
        p { display: block; margin: 13px 0; }
    </style>
</head>
<body style=""background-color:#F4F4F4; padding-top: 50px;"">
    <div style=""background-color:#F4F4F4;"">
        <div style=""background:#131519; background-color:#131519; Margin:0px auto; max-width:600px;"">
            <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#131519; width:100%;"">
                <tbody>
                    <tr>
                        <td style=""direction:ltr; font-size:0px; padding:20px 0; text-align:center; vertical-align:top;"">
                            <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px; text-align:left; direction:ltr; display:inline-block; vertical-align:top; width:100%;"">
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top; width:100%;"">
                                    <tr>
                                        <td align=""center"" style=""font-size:0px; padding:10px 25px; word-break:break-word;"">
                                            <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:collapse;"">
                                                <tbody>
                                                    <tr>
                                                        <td style=""width:428px;"">
                                                            <div style=""font-family:Arial, sans-serif; font-size:30px; line-height:22px; text-align:center; color:#ffffff;"">
                                                                " + ProjectName + @"
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div style=""background:#f1f5f9; background-color:#f1f5f9; Margin:0px auto; max-width:600px;"">
            <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#f1f5f9; width:100%;"">
                <tbody>
                    <tr>
                        <td style=""direction:ltr; font-size:0px; padding:30px 0 15px; text-align:center; vertical-align:top; border-bottom: 1px solid #ccc;"">
                            <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px; text-align:left; direction:ltr; display:inline-block; vertical-align:top; width:100%;"">
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top; width:100%;"">
                                    <tr>
                                        <td align=""left"" style=""font-size:0px; padding:0px 25px; word-break:break-word;"">
                                            <div style=""font-family:Arial, sans-serif; font-size:20px; font-weight:700; line-height:28px; text-align:left; color:#f9b707;"">
                                                Dear, " + UserName + @"
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div style=""background:#f1f5f9; background-color:#f1f5f9; Margin:0px auto; max-width:600px;"">
            <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#f1f5f9; width:100%;"">
                <tbody>
                    <tr>
                        <td style=""direction:ltr; font-size:0px; padding:0px 0px 15px 0px; text-align:center; vertical-align:top; border-bottom: 1px solid #ccc;"">
                            <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px; text-align:left; direction:ltr; display:inline-block; vertical-align:top; width:100%;"">
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top; width:100%;"">
                                    <tr>
                                        <td align=""left"" style=""font-size:0px; padding:15px 25px 25px 25px; word-break:break-word;"">
                                            <div style=""font-family:Arial, sans-serif; font-size:18px; line-height:28px; text-align:left; color:#55575d;"">
                                                Your New user details as follows:
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <!-- Table data section -->
        <div style=""Margin:0px auto; max-width:600px; background:#f1f5f9;"">
            <table align=""center"" border=""0"" cellpadding=""10"" cellspacing=""0"" role=""presentation"" style=""width:100%; border: 1px solid #ccc; background-color:#fff;"">
                <tbody>
                    <tr style=""background-color:#f9b707;"">
                        <td style=""padding:10px; font-weight:bold; text-align:left; border: 1px solid #ccc;"">LPGx url</td>
                        <td style=""padding:10px; font-weight:bold; text-align:left; border: 1px solid #ccc;"">" + urlLink + @"</td>
                    </tr>
                    <tr>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">Login Id</td>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">" + contactno + @"</td>
                    </tr>
                    <tr style=""background-color:#f9b707;"">
                        <td style=""padding:10px; font-weight:bold; text-align:left; border: 1px solid #ccc;"">Password</td>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">" + passKey + @"</td>
                    </tr>
                    <tr>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">Create Time</td>
                        <td style=""padding:10px; text-align:left; border: 1px solid #ccc;"">" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + @"</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div style=""background:#f9b707; background-color:#f9b707; Margin:0px auto; max-width:600px;"">
            <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#f9b707; width:100%;"">
                <tbody>
                    <tr>
                        <td style=""direction:ltr; font-size:0px; padding:20px 0; text-align:center; vertical-align:top;"">
                            <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px; text-align:left; direction:ltr; display:inline-block; vertical-align:top; width:100%;"">
                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top; width:100%;"">
                                    <tr>
                                        <td align=""center"" style=""font-size:0px; padding:10px 25px; word-break:break-word;"">
                                            <div style=""font-family:Arial, sans-serif; font-size:16px; line-height:22px; text-align:center; color:#000000;"">
                                                Regards, <b>" + ProjectName + @" Team
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</body>
</html>");


            #endregion Email Template

            return sb.ToString();
        }

    }
}
