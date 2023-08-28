using ChatApp.Models;

namespace ChatApp.Interfaces
{
    public interface IAgentService
    {
        Agent? AssignChatToAgent(ChatSession chatSession);
        Agent? GetAvailableAgent();
        bool EndChat(Guid sessionId);
        public Team GetTeamOnShift();
    }
}
