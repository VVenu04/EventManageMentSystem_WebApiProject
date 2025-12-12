using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otp, string userName);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
