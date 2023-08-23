using ChatApp.Interfaces;
using ChatApp.Models;

public class QueueService : IQueueService
{
    private readonly Queue<ChatSession> _queue = new();

    public bool TryEnqueue(ChatSession session)
    {
        if (session == null)
            return false;

        _queue.Enqueue(session);
        return true;
    }

    public ChatSession? Dequeue()
    {
        if (_queue.Count == 0)
            return null;

        return _queue.Dequeue();
    }

    public bool ContainsSession(Guid sessionId)
    {
        return _queue.Any(s => s.SessionId == sessionId);
    }
}