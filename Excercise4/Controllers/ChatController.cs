using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

namespace Excercise4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] string question)
        {
            var result = await _chatService.Ask(question);

            return Ok(result);
        }

        [HttpPost("summary")]
        public async Task<IActionResult> GetChatSummary([FromBody] string chatContent)
        {
            var result = await _chatService.GetChatSummaryResponseAsync(chatContent);

            return Ok(result);
        }
    }
}
