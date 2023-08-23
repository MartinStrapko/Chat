using ChatApp.Enums;

namespace ChatApp.Models
{
    public class Agent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public AgentSeniority Seniority { get; set; }
        public ChatSession? CurrentChatSession { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
