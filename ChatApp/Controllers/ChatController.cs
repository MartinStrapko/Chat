using Microsoft.AspNetCore.Mvc;
using ChatApp.Models;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        [HttpPost("create-session")]
        public IActionResult InitiateChat()
        {
            var session = new ChatSession
            {
                SessionId = Guid.NewGuid(),
            };

            return Ok(session.SessionId);
        }
    }
}
