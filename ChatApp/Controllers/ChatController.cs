using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        [HttpPost("create-session")]
        public IActionResult InitiateChat()
        {
            return Ok();
        }
    }
}
