using ChatApp.Interfaces;
using ChatApp.Models;

namespace ChatApp.Services
{
    public class AgentService : IAgentService
    {
        private readonly List<Team> _teams;
        private readonly List<Shift> _shifts;

        public event Action OnChatEnded;

        public AgentService(List<Team> teams)
        {
            _teams = teams;
        }

        public Team? GetTeamOnShift()
        {
            var currentTime = DateTime.UtcNow.TimeOfDay;
            return _teams.FirstOrDefault(team => currentTime >= team.Shift.Start && currentTime <= team.Shift.End);
        }

        public Team? GetOverflowTeam()
        {
            return _teams.Last();
        }

        public Agent? AssignChatToAgent(ChatSession sessionToAssign)
        {
            Agent? agent =
            GetTeamOnShift()?.Agents
                            .Where(a => a.CanHandleMoreChats)
                            .GroupBy(a => a.Seniority)
                            .OrderBy(g => g.Key)
                            .SelectMany(g => g.OrderBy(a => a.CurrentChatSessions.Count))
                            .FirstOrDefault();

            if (agent != null)
            {
                agent.AssignChatSession(sessionToAssign);
                return agent;
            }
            return null;
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
            return currentTeam?.Agents.FirstOrDefault(a => a.CanHandleMoreChats);
        }
    }
}
