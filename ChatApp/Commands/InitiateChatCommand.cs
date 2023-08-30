using MediatR;

namespace ChatApp.Commands
{
    public class InitiateChatCommand : IRequest<bool>
    {
        public Guid SessionId { get; private set; }

        public void AssignSessionId(Guid sessionId)
        {
            SessionId = sessionId;
        }
    }
}