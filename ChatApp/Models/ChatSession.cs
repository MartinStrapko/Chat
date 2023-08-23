namespace ChatApp.Models
{
    public class ChatSession
    {
        // A unique identifier for the chat session
        public Guid SessionId { get; set; } = Guid.NewGuid();
    }
}