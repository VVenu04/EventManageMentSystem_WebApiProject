using Application.DTOs.Auth;
using Application.Interface.IAuth;
using Application.Interface.IRepo;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AuthService: IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly ITokenService _tokenService;

        public AuthService(IAuthRepository authRepo, ITokenService tokenService)
        {
            _authRepo = authRepo;
            _tokenService = tokenService;
        }

        // --- CUSTOMER ---
        public async Task<AuthResponseDto> RegisterCustomerAsync(RegisterCustomerDto dto)
        {

            if (await _authRepo.CustomerEmailExistsAsync(dto.Email))
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Email already exists" };
            }

            var customer = new Customer
            {
                CustomerID = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _authRepo.AddCustomerAsync(customer);

            return CreateAuthResponse(customer.CustomerID, customer.Name, customer.Email, "Customer", "Registration Successful");
        }

        public async Task<AuthResponseDto> LoginCustomerAsync(LoginDto dto)
        {
            var customer = await _authRepo.GetCustomerByEmailAsync(dto.Email);

            if (customer == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid Email" };
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, customer.PasswordHash))
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid Password" };
            }

            return CreateAuthResponse(customer.CustomerID, customer.Name, customer.Email, "Customer", "Login Successful");
        }

        // --- VENDOR ---
        public async Task<AuthResponseDto> RegisterVendorAsync(RegisterVendorDto dto)
        {
            if (await _authRepo.VendorEmailExistsAsync(dto.Email))
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Email already exists" };
            }

            var vendor = new Vendor
            {
                VendorID = Guid.NewGuid(),
                Name = dto.Name,
                CompanyName = dto.CompanyName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _authRepo.AddVendorAsync(vendor);

            return CreateAuthResponse(vendor.VendorID, vendor.Name, vendor.Email, "Vendor", "Registration Successful");
        }

        public async Task<AuthResponseDto> LoginVendorAsync(LoginDto dto)
        {
            var vendor = await _authRepo.GetVendorByEmailAsync(dto.Email);

            if (vendor == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid Email" };
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, vendor.PasswordHash))
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid Password" };
            }

            return CreateAuthResponse(vendor.VendorID, vendor.Name, vendor.Email, "Vendor", "Login Successful");
        }

        // --- ADMIN ---
        public async Task<AuthResponseDto> LoginAdminAsync(LoginDto dto)
        {
            var admin = await _authRepo.GetAdminByEmailAsync(dto.Email);

            if (admin == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid Email" };
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash))
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid Password" };
            }

            return CreateAuthResponse(admin.AdminID, admin.AdminName, admin.AdminEmail, "Admin", "Login Successful");
        }



        private AuthResponseDto CreateAuthResponse(Guid userId, string name, string email, string role, string message)
        {
            var token = _tokenService.CreateToken(userId, email, role);
            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = message,
                Token = token,
                UserId = userId,
                Name = name,
                Role = role
            };
        }
    }
}

