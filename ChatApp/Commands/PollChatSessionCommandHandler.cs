namespace ChatApp.Commands
{
    using ChatApp.Interfaces;
    using MediatR;
    using System.Threading;
    using System.Threading.Tasks;

    public class PollChatSessionCommandHandler : IRequestHandler<PollChatSessionCommand, bool>
    {
        private readonly IQueueService _queueService;

        public PollChatSessionCommandHandler(IQueueService queueService)
        {
            _queueService = queueService;
        }

        public Task<bool> Handle(PollChatSessionCommand request, CancellationToken cancellationToken)
        {
            bool isPolled = _queueService.PollSession(request.SessionId);
            return Task.FromResult(isPolled);
        }
    }

}
