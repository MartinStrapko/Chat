using ChatApp.Enums;

namespace ChatApp.Models
{
    public class Agent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public AgentSeniority Seniority { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool CanHandleMoreChats => CurrentChatSessions.Count < Capacity;

        public const int BaseConcurrency = 10;

        public double EfficiencyMultiplier
        {
            get
            {
                return Seniority switch
                {
                    AgentSeniority.Junior => 0.4,
                    AgentSeniority.MidLevel => 0.6,
                    AgentSeniority.Senior => 0.8,
                    AgentSeniority.TeamLead => 0.5,
                    _ => 1.0
                };
            }
        }

        public double Capacity => BaseConcurrency * EfficiencyMultiplier;
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
