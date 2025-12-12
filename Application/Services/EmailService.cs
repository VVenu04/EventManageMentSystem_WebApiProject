using Application.Interface.IService;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly SmtpClient _smtpClient;

        public EmailService(IConfiguration configuration, SmtpClient smtpClient)
        {
            _configuration = configuration;
            _smtpClient = smtpClient;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];
            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);
            await _smtpClient.SendMailAsync(mailMessage);
        }
        public async Task SendOtpEmailAsync(string toEmail, string otp, string userName)
        {
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = "Password Reset OTP - Action Required",
                Body = GetOtpEmailTemplate(otp, userName),
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await _smtpClient.SendMailAsync(mailMessage);
        }

        private string GetOtpEmailTemplate(string otp, string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
        }}
        .container {{
            max-width: 600px;
            margin: 30px auto;
            background-color: #ffffff;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #ffffff;
            padding: 30px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 28px;
            font-weight: 600;
        }}
        .content {{
            padding: 40px 30px;
        }}
        .greeting {{
            font-size: 18px;
            margin-bottom: 20px;
            color: #333;
        }}
        .otp-container {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 10px;
            padding: 30px;
            text-align: center;
            margin: 30px 0;
        }}
        .otp-label {{
            color: #ffffff;
            font-size: 14px;
            font-weight: 500;
            margin-bottom: 10px;
            text-transform: uppercase;
            letter-spacing: 1px;
        }}
        .otp-code {{
            background-color: #ffffff;
            color: #667eea;
            font-size: 36px;
            font-weight: bold;
            letter-spacing: 8px;
            padding: 15px 30px;
            border-radius: 8px;
            display: inline-block;
            margin: 10px 0;
            font-family: 'Courier New', monospace;
        }}
        .expiry-notice {{
            background-color: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .expiry-notice strong {{
            color: #856404;
        }}
        .security-tips {{
            background-color: #f8f9fa;
            border-radius: 8px;
            padding: 20px;
            margin: 20px 0;
        }}
        .security-tips h3 {{
            color: #667eea;
            font-size: 16px;
            margin-top: 0;
            margin-bottom: 15px;
        }}
        .security-tips ul {{
            margin: 0;
            padding-left: 20px;
        }}
        .security-tips li {{
            margin-bottom: 8px;
            color: #555;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px 30px;
            text-align: center;
            font-size: 12px;
            color: #777;
            border-top: 1px solid #e9ecef;
        }}
        .footer p {{
            margin: 5px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #667eea;
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
            font-weight: 500;
        }}
        .warning-icon {{
            color: #dc3545;
            font-size: 20px;
            margin-right: 5px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Password Reset Request</h1>
        </div>
        
        <div class='content'>
            <p class='greeting'>Hello <strong>{userName}</strong>,</p>
            
            <p>We received a request to reset your password. Use the One-Time Password (OTP) below to complete the password reset process.</p>
            
            <div class='otp-container'>
                <div class='otp-label'>Your OTP Code</div>
                <div class='otp-code'>{otp}</div>
            </div>
            
            <div class='expiry-notice'>
                <strong>⏰ Important:</strong> This OTP will expire in <strong>10 minutes</strong>.
            </div>
            
            <div class='security-tips'>
                <h3>🛡️ Security Tips:</h3>
                <ul>
                    <li><strong>Never share</strong> your OTP with anyone, including our support team</li>
                    <li>We will <strong>never ask</strong> for your OTP via phone or email</li>
                    <li>If you didn't request this reset, please <strong>ignore this email</strong> and your password will remain unchanged</li>
                    <li>For your security, change your password immediately if you suspect unauthorized access</li>
                </ul>
            </div>
            
            <p style='margin-top: 30px; color: #555;'>If you didn't request a password reset, no action is needed. Your account is secure.</p>
        </div>
        
        <div class='footer'>
            <p><strong>This is an automated message, please do not reply to this email.</strong></p>
            <p>© 2024 Your Company Name. All rights reserved.</p>
            <p>If you have any questions, contact our support team.</p>
        </div>
    </div>
</body>
</html>
            ";
        }
         
    }
}
