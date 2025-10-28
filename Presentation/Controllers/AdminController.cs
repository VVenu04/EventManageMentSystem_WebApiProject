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
            if (adminDTO == null)
            {
                return BadRequest("fill panela");
            }
            var addedAdmin = await _adminService.AddAdminAsync(adminDTO);
            return Ok(addedAdmin);
        }


    }
}
