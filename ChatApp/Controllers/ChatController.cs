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
                var assignedAgent = _agentService.AssignChatToAgent(session);
                if (assignedAgent != null)
                {
                    return Ok(new { SessionId = session.SessionId, AgentId = assignedAgent.Id });
                }
                else
                {
                    return Accepted(session.SessionId);
                }
            }
            return BadRequest("Queue is full.");
        }
    }
}
