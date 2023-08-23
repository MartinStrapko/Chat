using ChatApp.Models;

namespace ChatApp.Interfaces
{
    public interface IAgentService
    {
        Agent? AssignChatToAgent(ChatSession chatSession);
        bool EndChat(Guid agentId);
    }
}
