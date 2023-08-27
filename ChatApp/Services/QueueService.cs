using ChatApp;
using ChatApp.Interfaces;
using ChatApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

public class QueueService : IQueueService
{
    private readonly Queue<ChatSession> _queue = new();
    private readonly Queue<ChatSession> _overflowQueue = new();
    private readonly ChatSettings _chatSettings;
    private readonly int _expectedPollsInIntervalSeconds;
    private readonly int _markInactiveAfterNumberOfMissedPolls;

    public int MaxCapacity { get; set; } = 10;
    public int OverflowCapacity { get; set; } = 10;
    public TimeSpan OfficeStart { get; set; } = new TimeSpan(9, 0, 0);
    public TimeSpan OfficeEnd { get; set; } = new TimeSpan(17, 0, 0);

    public QueueService(IOptions<ChatSettings> mySettings)
    {
        _chatSettings = mySettings.Value;
        _expectedPollsInIntervalSeconds = _chatSettings.TimerIntervalSeconds / _chatSettings.PollsPerSecond;
        _markInactiveAfterNumberOfMissedPolls = _chatSettings.MarkInactiveAfterNumberOfMissedPolls;
    }

    public bool TryEnqueue(ChatSession session)
    {
        if (session == null)
            return false;
        if (_queue.Count < MaxCapacity)
        {
            _queue.Enqueue(session);
            return true;
        }
        else if (IsOfficeHours() && _overflowQueue.Count < OverflowCapacity)
        {
            _overflowQueue.Enqueue(session);
            return true;
        }
        return false;
    }

    public ChatSession? Dequeue()
    {
        return _queue.Count > 0 ? _queue.Dequeue() :
               _overflowQueue.Count > 0 ? _overflowQueue.Dequeue() :
               null;
    }

    public bool ContainsSession(Guid sessionId)
    {
        return _queue.Any(s => s.SessionId == sessionId);
    }

    public bool PollSession(Guid sessionId)
    {
        var session = _queue.FirstOrDefault(s => s.SessionId == sessionId);

        if (session != null)
        {
            session.LastPolledAt = DateTime.UtcNow;
            session.PollsSiceLastCheck++;
            return true;
        }

        return false;
    }

    public void CheckMissedPolls()
    {
        foreach (var session in _queue)
        {
            var missedPolls = _expectedPollsInIntervalSeconds - session.PollsSiceLastCheck;
            if (missedPolls >= _markInactiveAfterNumberOfMissedPolls)
            {
                MarkSessionInactive(session);
            }
            session.PollsSiceLastCheck = 0;

        }
    }

    private void MarkSessionInactive(ChatSession session)
    {
    }

    private bool IsOfficeHours()
    {
        var now = DateTime.UtcNow.TimeOfDay;
        return now > OfficeStart && now < OfficeEnd;
    }
}