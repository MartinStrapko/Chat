using ChatApp.Models;

namespace ChatApp.Interfaces
{
    public interface IQueueService
    {
        bool TryEnqueue(ChatSession session);
        ChatSession? Dequeue();
        bool ContainsSession(Guid sessionId);
        bool PollSession(Guid sessionId);
        void CheckMissedPolls();
    }
}
