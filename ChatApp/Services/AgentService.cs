using ChatApp.Interfaces;
using ChatApp.Models;

namespace ChatApp.Services
{
    public class AgentService : IAgentService
    {
        private readonly List<Team> _teams;
        private readonly List<Shift> _shifts;
        private readonly IQueueService _queueService;

        public AgentService(IQueueService queueService, List<Team> teams, List<Shift> shifts)
        {
            _teams = teams;
            _queueService = queueService;
        }
        private Team GetTeamOnShift()
        {
            var currentTime = DateTime.UtcNow.TimeOfDay;

            Team currentTeam = _teams.FirstOrDefault(team => currentTime >= team.Shift.Start && currentTime <= team.Shift.End);
            return currentTeam;
        }

        public Agent? AssignChatToAgent()
        {
            var currentTime = DateTime.UtcNow.TimeOfDay;

            Team currentTeam = GetTeamOnShift();

            Agent? availableAgent = GetAvailableAgent();

            if (availableAgent == null)
            {
                return null;
            }

            ChatSession? sessionToAssign = _queueService.Dequeue();

            if (sessionToAssign == null)
            {
                return null;
            }

            availableAgent.CurrentChatsCount++;

            availableAgent.AssignChatSession(sessionToAssign);

            return availableAgent;
        }

        public bool EndChat(Guid sessionId)
        {
            var agentHandlingSession = _teams
                .SelectMany(t => t.Agents)
                .FirstOrDefault(a => a.CurrentChatSessions.Any(s => s.SessionId == sessionId));

            if (agentHandlingSession != null)
            {
                var sessionToBeEnded = agentHandlingSession.CurrentChatSessions.FirstOrDefault(s => s.SessionId == sessionId);

                if (sessionToBeEnded != null)
                {
                    agentHandlingSession.RemoveChatSession(sessionToBeEnded);
                    return true;
                }
            }

            return false;
        }

        private Agent? GetAvailableAgent()
        {
            Team currentTeam = GetTeamOnShift();
            return currentTeam.Agents.FirstOrDefault(a => a.CanHandleMoreChats);
        }

        private void AssignWaitingChats()
        {
            while (true)
            {
                var agent = GetAvailableAgent();
                if (agent == null)
                    break;

                AssignChatToAgent();
            }
        }
    }
}
