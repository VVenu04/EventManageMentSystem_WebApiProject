using Application.DTOs.Auth;
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
    }
}
