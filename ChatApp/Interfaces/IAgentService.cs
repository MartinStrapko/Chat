using ChatApp.Models;

namespace ChatApp.Interfaces
{
    public interface IAgentService
    {
        event Action? OnChatEnded;
        Agent? AssignChatToAgent(ChatSession chatSession);
        Agent? GetAvailableAgent();
        bool EndChat(Guid sessionId);
        public Team? GetTeamOnShift();
        public Team? GetOverflowTeam();
    }
}
