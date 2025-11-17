using Application.DTOs;
using Application.Interface.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminServic)
        {
            _adminService = adminServic;
        }

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [ProducesResponseType(typeof(AdminDto), StatusCodes.Status200OK)]
        [HttpPost("AddAdmin")]
        public async Task<IActionResult> AddAdmin([FromBody] AdminDto adminDTO)
        {
            if (!ModelState.IsValid)
                return Ok(adminDTO);

            if (adminDTO == null)
            {
                return BadRequest("fill panela");
            }
            var addedAdmin = await _adminService.AddAdminAsync(adminDTO);
            return Ok(addedAdmin);
        }

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [ProducesResponseType(typeof(AdminDto), StatusCodes.Status200OK)]
        [HttpDelete("DeleteAdmin")]
        public async Task<IActionResult> DeleteAdmin(Guid? Id)
        {
            if (Id == null) { return BadRequest("id ela"); }
            await _adminService.DeleteAdminAsync(Id);
            return Ok();
        }

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [HttpGet("1Admin")]
        public async Task<IActionResult> GetAdminById(Guid adminId)
        {
            var admin = await _adminService.GetAdminAsync(adminId);
            if (admin == null) return NotFound();
            return Ok(admin);
        }
        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [HttpGet("AllAdmin")]
        public async Task<IActionResult> GetAllAsync()
        {
            var admins = await _adminService.GetAllAsync();
            if (admins == null) return NotFound();
            return Ok(admins);
        }

    }
}
