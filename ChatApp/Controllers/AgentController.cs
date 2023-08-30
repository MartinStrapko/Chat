using ChatApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentService _agentService;

        public AgentController(IAgentService agentService)
        {
            _agentService = agentService;
        }


        [HttpPost("end-session/{sessionId}")]
        public IActionResult EndChat(Guid sessionId)
        {
            var chats = _agentService.EndChat(sessionId);

            if(chats)
            {
                return Ok();
            }
            return BadRequest("Chat does not exist.");
        }
    }
}