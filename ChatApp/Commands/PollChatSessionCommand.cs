using MediatR;

namespace ChatApp.Commands
{

    public class PollChatSessionCommand : IRequest<bool>
    {
        public Guid SessionId { get; set; }

        public PollChatSessionCommand(Guid sessionId)
        {
            SessionId = sessionId;
        }
    }
}
