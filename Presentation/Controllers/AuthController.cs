using Application.Common;
using Application.DTOs;
using Application.DTOs.Auth;
using Application.DTOs.Forgot;
using Application.DTOs.Google;
using Application.Interface.IAuth;
using Application.Interface.IService;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, IEmailService emailService) : BaseApiController
    {
        private readonly IAuthService _authService = authService;
        private readonly IEmailService _emailService = emailService;

        // --- Customer Routes ---
        [HttpPost("customer/register")]
        public async Task<ActionResult<AuthResponseDto>> RegisterCustomer(RegisterCustomerDto dto)
        {
            var result = await _authService.RegisterCustomerAsync(dto);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return Ok(result);
        }


        [HttpPost("customer/google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> CustomerSignInWithGoogle([FromBody] GoogleAuthRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.IdToken))
                return BadRequest("IdToken is required.");

            try
            {
               var result = await _authService.CustomerSignInWithGoogleAsync(dto.IdToken);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // For debugging - in production, log properly
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }
        [HttpPost("vendor/google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> VendorSignInWithGoogle([FromBody] GoogleAuthRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.IdToken))
                return BadRequest("IdToken is required.");

            try
            {
                var result = await _authService.VendorSignInWithGoogleAsync(dto.IdToken);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // For debugging - in production, log properly
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }


        [HttpPost("customer/login")]
        public async Task<ActionResult<AuthResponseDto>> LoginCustomer(LoginDto dto)
        {
            var result = await _authService.LoginCustomerAsync(dto);
            if (!result.IsSuccess) return Unauthorized(result.Message);
            return Ok(result);
        }
        [HttpGet("customer/profile")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetCustomerProfile()
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized();

            var profile = await _authService.GetCustomerProfileAsync(CurrentUserId);

            if (profile == null) return NotFound("User not found");

            return Ok(ApiResponse<object>.Success(profile));
        }

        // --- Vendor Routes ---
        [HttpPost("vendor/register")]
        public async Task<ActionResult<AuthResponseDto>> RegisterVendor(RegisterVendorDto dto)
        {
            var result = await _authService.RegisterVendorAsync(dto);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpPost("vendor/login")]
        public async Task<ActionResult<AuthResponseDto>> LoginVendor(LoginDto dto)
        {
            var result = await _authService.LoginVendorAsync(dto);
            if (!result.IsSuccess) return Unauthorized(result.Message);
            return Ok(result);
        }

        // --- Admin Route ---
        [HttpPost("admin/login")]
        public async Task<ActionResult<AuthResponseDto>> LoginAdmin(LoginDto dto)
        {
            var result = await _authService.LoginAdminAsync(dto);
            if (!result.IsSuccess) return Unauthorized(result.Message);
            return Ok(result);
        }

        // --- Profile Update Routes
        // ---




        [HttpPut("vendor/profile")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> UpdateVendorProfile(UpdateVendorProfileDto dto)
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized();

            // 1. Update the profile
            var success = await _authService.UpdateVendorProfileAsync(CurrentUserId, dto);

            if (!success) return BadRequest(ApiResponse<object>.Failure("Failed to update profile."));

            var updatedProfile = await _authService.GetVendorProfileAsync(CurrentUserId);

            return Ok(ApiResponse<object>.Success(updatedProfile, "Profile updated successfully."));
        }



        [HttpGet("vendor/profile")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetVendorProfile()
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized();

            var profile = await _authService.GetVendorProfileAsync(CurrentUserId);

            if (profile == null) return NotFound("Vendor not found");

            return Ok(ApiResponse<object>.Success(profile));
        }

    

        [HttpPut("customer/profile")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateCustomerProfile(UpdateCustomerProfileDto dto)
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized();

            // 1. Update the profile
            var success = await _authService.UpdateCustomerProfileAsync(CurrentUserId, dto);

            if (!success) return BadRequest(ApiResponse<object>.Failure("Failed to update profile."));


            var updatedData = new
            {
                displayName = dto.Name,       
                phoneNumber = dto.PhoneNumber,
                location = dto.Location,
                img = dto.ProfilePhotoUrl
            };

            // 3. Return the updated data inside Success
            return Ok(ApiResponse<object>.Success(updatedData, "Profile updated successfully."));
        }

        [HttpPost("customer/forgot-password")]
        public async Task<IActionResult> CustomerForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto { Success = false, Message = "Invalid request data", Data = ModelState });

            var result = await _authService.CustomerForgotPasswordAsync(dto);

            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("customer/verify-otp")]
        public async Task<IActionResult> CustomerVerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto { Success = false, Message = "Invalid request data", Data = ModelState });

            var result = await _authService.CustomerVerifyOtpAsync(dto);

            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("customer/reset-password")]
        public async Task<IActionResult> CustomerResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto { Success = false, Message = "Invalid request data", Data = ModelState });

            var result = await _authService.CustomerResetPasswordAsync(dto);

            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("vendor/forgot-password")]
        public async Task<IActionResult> VendorForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto { Success = false, Message = "Invalid request data", Data = ModelState });

            var result = await _authService.VendorForgotPasswordAsync(dto);

            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("vendor/verify-otp")]
        public async Task<IActionResult> VendorVerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto { Success = false, Message = "Invalid request data", Data = ModelState });

            var result = await _authService.VendorVerifyOtpAsync(dto);

            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("vendor/reset-password")]
        public async Task<IActionResult> VendorResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto { Success = false, Message = "Invalid request data", Data = ModelState });

            var result = await _authService.VendorResetPasswordAsync(dto);

            if (result.Success) return Ok(result);
            return BadRequest(result);
        }
        [HttpGet("verify/customer")]
        public async Task<IActionResult> Verify(string token)
        {
            var success = await _authService.VerifyEmailCustomerAsync(token);

            if (!success)
                return BadRequest("Invalid or expired token");

            return Ok("Email verified successfully");
        }
        [HttpGet("verify/vendor")]
        public async Task<IActionResult> VerifyVendor(string token)
        {
            var success = await _authService.VerifyEmailVendorAsync(token);

            if (!success)
                return BadRequest("Invalid or expired token");

            return Ok("Email verified successfully");
        }
        [AllowAnonymous]
        [HttpPost("send")]
        public async Task<IActionResult> SendToAdmin([FromBody] MessageDto dto)
        {
            string adminEmail = "vvenujan04@gmail.com";

            string subject = "New  Message";
            string body = $@"
            <h3>Customer Message</h3>
            <p><b>Name:</b>{dto.Name}</p>
            <p><b>PhoneNumber:</b>{dto.PhoneNumber}</p>
            <p><b>Email:</b> {dto.Email}</p>
            <p><b>Message:</b><br>{dto.Message.Replace("\n", "<br>")}</p>
        ";

            await _emailService.SendEmailAsync(adminEmail, subject, body);

            return Ok(ApiResponse<object>.Success(null, "Message sent to admin successfully!"));
        }

    }
}