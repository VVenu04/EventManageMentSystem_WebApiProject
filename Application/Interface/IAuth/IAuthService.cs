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
        Task<ApiResponseDto> CustomerForgotPasswordAsync(ForgotPasswordDto dto);
        Task<ApiResponseDto> CustomerVerifyOtpAsync(VerifyOtpDto dto);
        Task<ApiResponseDto> CustomerResetPasswordAsync(ResetPasswordDto dto);
        Task<AuthResponseDto> CustomerSignInWithGoogleAsync(string idToken);
        Task<ApiResponseDto> VendorForgotPasswordAsync(ForgotPasswordDto dto);
        Task<ApiResponseDto> VendorVerifyOtpAsync(VerifyOtpDto dto);
        Task<ApiResponseDto> VendorResetPasswordAsync(ResetPasswordDto dto);
        Task<AuthResponseDto> VendorSignInWithGoogleAsync(string idToken);
    }
}
