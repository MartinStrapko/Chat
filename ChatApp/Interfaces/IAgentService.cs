using ChatApp.Models;

namespace ChatApp.Interfaces
{
    public interface IAgentService
    {
        Agent? AssignChatToAgent();
        bool EndChat(Guid sessionId);
    }
}
