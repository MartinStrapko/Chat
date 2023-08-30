using ChatApp.Interfaces;
using ChatApp.Models;

namespace ChatApp.Services
{
    public class AgentService : IAgentService
    {
        private readonly List<Team> _teams;
        private readonly List<Shift> _shifts;

        public event Action OnChatEnded;

        public AgentService( List<Team> teams)
        {
            _teams = teams;
        }

        public Team GetTeamOnShift()
        {
            var currentTime = DateTime.UtcNow.TimeOfDay;

            Team currentTeam = _teams.FirstOrDefault(team => currentTime >= team.Shift.Start && currentTime <= team.Shift.End);
            return currentTeam;
        }

        public Agent? AssignChatToAgent(ChatSession sessionToAssign)
        {
            Agent? availableAgent = GetAvailableAgent();

            if (availableAgent == null)
            {
                return null;
            }

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
                    OnChatEnded?.Invoke();
                    return true;
                }
            }

            return false;
        }

        public Agent? GetAvailableAgent()
        {
            Team currentTeam = GetTeamOnShift();
            return currentTeam.Agents.FirstOrDefault(a => a.CanHandleMoreChats);
        }
    }
}
