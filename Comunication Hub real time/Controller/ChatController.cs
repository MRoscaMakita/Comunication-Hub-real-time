using Comunication_Hub_real_time.Service;
using Microsoft.AspNetCore.Mvc;

namespace Comunication_Hub_real_time.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly RenderChatService _chatService;
        public ChatController(RenderChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] MessageDto dto)
        {
            await _chatService.SendMessage(dto.Message);
            return Ok();
        }
    }

    public record MessageDto(string Message);
}
