using Microsoft.AspNetCore.Mvc;
using ChatApp.Models;
using ChatApp.Interfaces;
using ChatApp.Services;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IQueueService _queueService;
        private readonly IAgentService _agentService;

        public ChatController(IQueueService queueService, IAgentService agentService)
        {
            _queueService = queueService;
            _agentService = agentService;
        }

        [HttpPost("create-session")]
        public IActionResult InitiateChat()
        {
            var session = new ChatSession
            {
                SessionId = Guid.NewGuid(),
            };

            if (_queueService.TryEnqueue(session))
            {
               return Ok(session.SessionId);
            }
            return BadRequest("Queue is full.");
        }

        [HttpPost("poll-session/{sessionId}")]
        public IActionResult PollSession(Guid sessionId)
        {
            if (_queueService.PollSession(sessionId))
            {
                return Ok();
            }
            return NotFound("Session not found or already inactive.");
        }
    }
}
