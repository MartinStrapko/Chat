using ChatApp.Interfaces;
using ChatApp.Models;

public class QueueService : IQueueService
{
    private readonly Queue<ChatSession> _queue = new();
    private readonly Queue<ChatSession> _overflowQueue = new();
    public int MaxCapacity { get; set; } = 10;
    public int OverflowCapacity { get; set; } = 10;
    public TimeSpan OfficeStart { get; set; } = new TimeSpan(9, 0, 0);
    public TimeSpan OfficeEnd { get; set; } = new TimeSpan(17, 0, 0);

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

    private bool IsOfficeHours()
    {
        var now = DateTime.UtcNow.TimeOfDay;
        return now > OfficeStart && now < OfficeEnd;
    }
}