using Microsoft.AspNetCore.Mvc;
using ChatApp.Models;
using ChatApp.Interfaces;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IQueueService _queueService;

        public ChatController(IQueueService queueService)
        {
            _queueService = queueService;
        }

        [HttpPost("create-session")]
        public IActionResult InitiateChat()
        {
            var session = new ChatSession
            {
                SessionId = Guid.NewGuid(),
            };

            if (!_queueService.TryEnqueue(session))
            {
                return BadRequest("Queue is full.");
            }

            return Ok(session.SessionId);
        }
    }
}
