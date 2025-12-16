using Application.DTOs.AI;
using Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : BaseApiController
    {
        private readonly IAIService _aiService;

        public ChatController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("ask")]
        [Authorize]
        public async Task<IActionResult> AskBot([FromBody] ChatRequestDto dto)
        {
            // Validation
            if (dto == null || string.IsNullOrWhiteSpace(dto.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Customer";

            var response = await _aiService.GetChatResponseAsync(dto.Message, role);
            return Ok(new { response });
        }
        [HttpPost("budget-planner")]
        [AllowAnonymous] 
        public async Task<IActionResult> BudgetPlanner([FromBody] BudgetRequestDto dto)
        {
            if (dto == null) return BadRequest("Invalid data.");

            var response = await _aiService.GenerateBudgetPlanAsync(dto.EventType, dto.GuestCount, dto.TotalBudget);

            return Ok(new { response });
        }
        [HttpPost("generate-description")]
        [Authorize(Roles = "Vendor")] // Vendor only
        public async Task<IActionResult> GenerateDescription([FromBody] DescriptionRequestDto dto)
        {
            var result = await _aiService.GenerateServiceDescriptionAsync(dto.ServiceName, dto.Category);
            return Ok(new { description = result });
        }

    }

}