using Application.DTOs;
using Application.DTOs.Auth;
using Application.DTOs.Forgot;
using Application.DTOs.Google;
using Application.Interface.IAuth;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Domain.Entities;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
 


namespace Application.Services
{


    public class AuthService : IAuthService

    {
        private readonly ICustomerRepo _customerRepo;
        private readonly IEmailService _emailService;
        private readonly IAuthRepository _authRepo;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;

        public AuthService(IConfiguration config, IAuthRepository authRepo, ITokenService tokenService, ICustomerRepo customerRepo , IEmailService emailService)
        {
            _authRepo = authRepo;
            _tokenService = tokenService;
            _customerRepo = customerRepo;
            _emailService = emailService;
            _config = config;
        }

        // --- CUSTOMER ---
        public async Task<AuthResponseDto> SignInWithGoogleAsync(string idToken)
        {
            // 1) Validate Google ID token
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { _config["Google:ClientId"] } // must match your client id
            };

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Invalid Google ID token.", ex);
            }

            // 2) Find or create user
            var googleId = payload.Subject; // "sub"
            var email = payload.Email!;
            var name = payload.Name ?? payload.Email!;
            var picture = payload.Picture;

            var user = await _customerRepo.GetByGoogleIdAsync(googleId)
                       ?? await _customerRepo.GetByEmailAsync(email);

            if (user == null)
            {
                user = new Domain.Entities.Customer
                {
                    GoogleId = googleId,
                    Email = email,
                    Name = name,
                    ProfilePhoto = picture
                };
                await _customerRepo.AddAsync(user);
                await _customerRepo.SaveChangesAsync();
            }
            else
            {
                // If existing user had no GoogleId, set it
                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = googleId;
                    await _customerRepo.SaveChangesAsync();
                }
            }

            return CreateAuthResponse(user.CustomerID, user.Name, user.Email, "Customer", "Login Successful");

            // // 3) Create JWT
            // var token = GenerateJwtToken(user, out DateTime expires);

            // // 4) Build response
            //// var userDto = _mapper.Map<UserDto>(user);
            //var userDto = new CustomerDto
            // {
            //     CustomerID = user.CustomerID,
            //     Name = user.Name,
            //     Email = user.Email,
            //     ProfilePhoto = user.ProfilePhoto
            // };

            // return new  GoogleAuthResponseDto
            // {
            //     Token = token,
            //     ExpiresAt = expires,
            //     Customer = userDto
            // };
        }
    

        //private string GenerateJwtToken(Customer user, out DateTime expires)
        //{
        //    var jwtSection = _config.GetSection("Jwt");
        //    var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
        //    var issuer = jwtSection["Issuer"];
        //    var audience = jwtSection["Audience"];
        //    var expireMinutes = int.Parse(jwtSection["ExpireMinutes"] ?? "60");

        //    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        //    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        //    var claims = new[]
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, user.CustomerID.ToString()),
        //        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        //        new Claim("name", user.Name),
        //        new Claim("googleId", user.GoogleId),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //    };

        //    expires = DateTime.UtcNow.AddMinutes(expireMinutes);

        //    var token = new JwtSecurityToken(
        //        issuer: issuer,
        //        audience: audience,
        //        claims: claims,
        //        expires: expires,
        //        signingCredentials: credentials
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        public async Task<AuthResponseDto> RegisterCustomerAsync(RegisterCustomerDto dto)
        {

            if (await _authRepo.CustomerEmailExistsAsync(dto.Email))
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Email already exists" };
            }

            var customer = new Domain.Entities.Customer
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
        public async Task<ApiResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            try
            {
                var user = await _customerRepo.GetByEmailAsync(dto.Email);

                if (user == null)
                {
                    // Don't reveal that user doesn't exist (security best practice)
                    return new ApiResponseDto
                    {
                        Success = true,
                        Message = "If the email exists, an OTP has been sent to your email address."
                    };
                }

                // Generate 6-digit OTP
                var otp = GenerateOtp();

                // Save OTP and expiry time (10 minutes validity)
                user.PasswordResetOtp = otp;
                user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(10);

                await _customerRepo.UpdateAsync(user);

                // Send OTP via email
                await _emailService.SendOtpEmailAsync(user.Email, otp, user.Name);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "If the email exists, an OTP has been sent to your email address."
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto> VerifyOtpAsync(VerifyOtpDto dto)
        {
            try
            {
                var user = await _customerRepo.GetByEmailAsync(dto.Email);

                if (user == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Invalid email or OTP."
                    };
                }

                // Check if OTP matches
                if (user.PasswordResetOtp != dto.Otp)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Invalid OTP. Please check and try again."
                    };
                }

                // Check if OTP has expired
                if (user.PasswordResetOtpExpiry == null ||
                    user.PasswordResetOtpExpiry < DateTime.UtcNow)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "OTP has expired. Please request a new one."
                    };
                }

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "OTP verified successfully. You can now reset your password."
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
        {
            try
            {
                var user = await _customerRepo.GetByEmailAsync(dto.Email);

                if (user == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Invalid email or OTP."
                    };
                }

                // Verify OTP
                if (user.PasswordResetOtp != dto.Otp)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Invalid OTP. Please check and try again."
                    };
                }

                // Check OTP expiry
                if (user.PasswordResetOtpExpiry == null ||
                    user.PasswordResetOtpExpiry < DateTime.UtcNow)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "OTP has expired. Please request a new one."
                    };
                }

                // Hash the new password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

                // Clear OTP fields
                user.PasswordResetOtp = null;
                user.PasswordResetOtpExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _customerRepo.UpdateAsync(user);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Password has been reset successfully. You can now login with your new password."
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // Generates 6-digit OTP
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
        // ...
        public async Task<bool> UpdateVendorProfileAsync(Guid vendorId, UpdateVendorProfileDto dto)
        {
            var vendor = await _authRepo.GetVendorByIdAsync(vendorId); // (Repo-ல் இந்த method தேவை)
            if (vendor == null) return false;

            // உள்ளீடு இருந்தால் மட்டும் Update செய் (Null Coalescing)
            if (!string.IsNullOrEmpty(dto.CompanyName)) vendor.CompanyName = dto.CompanyName;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) vendor.PhoneNumber = dto.PhoneNumber;
            if (!string.IsNullOrEmpty(dto.Location)) vendor.Location = dto.Location;
            if (!string.IsNullOrEmpty(dto.Description)) vendor.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.LogoUrl)) vendor.Logo = dto.LogoUrl;

            if (dto.EventPerDayLimit > 0)
            {
                vendor.EventPerDayLimit = (int)dto.EventPerDayLimit;
            }
            await _authRepo.UpdateVendorAsync(vendor);
            return true;
        }

        public async Task<bool> UpdateCustomerProfileAsync(Guid customerId, UpdateCustomerProfileDto dto)
        {
            var customer = await _authRepo.GetCustomerByIdAsync(customerId);
            if (customer == null) return false;

            if (!string.IsNullOrEmpty(dto.Name)) customer.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) customer.PhoneNumber = dto.PhoneNumber;
            if (!string.IsNullOrEmpty(dto.Location)) customer.Location = dto.Location;
            if (!string.IsNullOrEmpty(dto.ProfilePhotoUrl)) customer.ProfilePhoto = dto.ProfilePhotoUrl;

            await _authRepo.UpdateCustomerAsync(customer);
            return true;
        }

         
    }
}

