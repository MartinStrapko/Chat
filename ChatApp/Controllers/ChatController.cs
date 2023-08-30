using Microsoft.AspNetCore.Mvc;
using MediatR;
using ChatApp.Commands;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create-session")]
        public async Task<IActionResult> InitiateChat()
        {
            var command = new InitiateChatCommand();
            bool isSuccess = await _mediator.Send(command);

            if (isSuccess)
            {
                return Ok(command.SessionId);
            }

            return BadRequest("Queue is full.");
        }

        [HttpPost("poll-session/{sessionId}")]
        public async Task<IActionResult> PollSession(Guid sessionId)
        {
            var command = new PollChatSessionCommand(sessionId);
            bool isPolled = await _mediator.Send(command);

            if (isPolled)
            {
                return Ok();
            }

            return NotFound("Session not found or already inactive.");
        }
    }
}
