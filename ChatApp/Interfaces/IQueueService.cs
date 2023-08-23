using ChatApp.Models;

namespace ChatApp.Interfaces
{
    public interface IQueueService
    {
        // Try to add a chat session to the queue
        bool TryEnqueue(ChatSession session);

        // Remove a chat session from the queue
        ChatSession? Dequeue();

        // Checks if a session is already in the queue
        bool ContainsSession(Guid sessionId);
    }
}
