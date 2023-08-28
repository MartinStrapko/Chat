using ChatApp.Enums;

namespace ChatApp.Models
{
    public class Agent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public AgentSeniority Seniority { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int CurrentChatsCount { get; set; } = 0;
        public const int MaxConcurrentChats = 10;
        public bool CanHandleMoreChats => CurrentChatsCount < MaxConcurrentChats;
        public List<ChatSession> CurrentChatSessions { get; set; } = new List<ChatSession>();
        public void AssignChatSession(ChatSession session)
        {
            CurrentChatSessions.Add(session);
        }

        public void RemoveChatSession(ChatSession session)
        {
            CurrentChatSessions.Remove(session);
        }

    }
}
