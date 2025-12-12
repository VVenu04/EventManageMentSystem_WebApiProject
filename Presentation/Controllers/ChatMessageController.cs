using Application.Common;
using Application.DTOs.Chat;
using infrastucure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatMessageController : BaseApiController
    {
        private readonly ApplicationDbContext _context;

        public ChatMessageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // குறிப்பிட்ட நபருடனான Chat History-ஐ எடு
        [HttpGet("history/{otherUserId}")]
        public async Task<ActionResult<IEnumerable<ChatHistoryDto>>> GetChatHistory(Guid otherUserId)
        {
            var messages = await _context.Set<Domain.Entities.ChatMessage>()
                .Where(m => (m.SenderID == CurrentUserId && m.ReceiverID == otherUserId) ||
                            (m.SenderID == otherUserId && m.ReceiverID == CurrentUserId))
                .OrderBy(m => m.SentAt)
                .Select(m => new ChatHistoryDto
                {
                    MessageID = m.MessageID,
                    SenderID = m.SenderID,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsMyMessage = m.SenderID == CurrentUserId
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<ChatHistoryDto>>.Success(messages));
        }
    }
}
