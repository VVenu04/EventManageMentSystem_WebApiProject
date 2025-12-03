using Application.DTOs.Auth;
using Application.DTOs.Forgot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IAuth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterCustomerAsync(RegisterCustomerDto dto);
        Task<AuthResponseDto> RegisterVendorAsync(RegisterVendorDto dto);

        Task<AuthResponseDto> LoginCustomerAsync(LoginDto dto);
        Task<AuthResponseDto> LoginVendorAsync(LoginDto dto);
        Task<AuthResponseDto> LoginAdminAsync(LoginDto dto);
        Task<bool> UpdateVendorProfileAsync(Guid vendorId, UpdateVendorProfileDto dto);
        Task<bool> UpdateCustomerProfileAsync(Guid customerId, UpdateCustomerProfileDto dto);
        Task<ApiResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<ApiResponseDto> VerifyOtpAsync(VerifyOtpDto dto);
        Task<ApiResponseDto> ResetPasswordAsync(ResetPasswordDto dto);
        Task<AuthResponseDto> SignInWithGoogleAsync(string idToken);
    }
}
