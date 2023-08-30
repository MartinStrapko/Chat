using ChatApp.Interfaces;
using ChatApp.Models;
using MediatR;

namespace ChatApp.Commands
{
    public class InitiateChatCommandHandler : IRequestHandler<InitiateChatCommand, bool>
    {
        private readonly IQueueService _queueService;

        public InitiateChatCommandHandler(IQueueService queueService)
        {
            _queueService = queueService;
        }

        public async Task<bool> Handle(InitiateChatCommand request, CancellationToken cancellationToken)
        {
            var session = new ChatSession
            {
                SessionId = Guid.NewGuid(),
            };

            request.AssignSessionId(session.SessionId);

            return _queueService.TryEnqueue(session);
        }
    }

}
