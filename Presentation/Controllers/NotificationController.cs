using Application.Common; 
using Application.Interface.IService;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: api/notification
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Notification>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Notification>>> GetMyNotifications()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid User Token"));

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);

            // Return empty list instead of null
            return Ok(ApiResponse<IEnumerable<Notification>>.Success(notifications ?? new List<Notification>()));
        }

        // PUT: api/notification/{id}/read
        [HttpPut("{id}/read")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Notification marked as read."));
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId)) return userId;
            return Guid.Empty;
        }
    }
}