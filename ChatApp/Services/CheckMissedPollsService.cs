using ChatApp.Interfaces;
using Microsoft.Extensions.Options;

namespace ChatApp.Services
{
    public class CheckMissedPollsService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _timer;
        private readonly ChatSettings _chatSettings;

        public CheckMissedPollsService(IServiceScopeFactory scopeFactory, IOptions<ChatSettings> mySettings)
        {
            _scopeFactory = scopeFactory;
            _chatSettings = mySettings.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CheckMissedPolls, null, TimeSpan.Zero, TimeSpan.FromSeconds(_chatSettings.TimerIntervalSeconds));
            return Task.CompletedTask;
        }

        private void CheckMissedPolls(object state)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var queueService = scope.ServiceProvider.GetRequiredService<IQueueService>();
                queueService.CheckMissedPolls();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
