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

            ChatSession? sessionToAssign = _queueService.Dequeue();

            if (sessionToAssign == null)
            {
                return null;
            }

            Agent? availableAgent = currentTeam.Agents.FirstOrDefault(agent => agent.IsAvailable);

            if (availableAgent == null)
            {
                return null;
            }

            availableAgent.IsAvailable = false;
            availableAgent.CurrentChatSession = sessionToAssign;

            return availableAgent;
        }

        public bool EndChat(Guid agentId)
        {
            Team currentTeam = GetTeamOnShift();
            var agent = currentTeam.Agents.FirstOrDefault(a => a.Id == agentId);

            if (agent != null)
            {
                agent.IsAvailable = true;
                agent.CurrentChatSession = null;
                return true;
            }

            return false;
        }

        private Agent? GetAvailableAgent()
        {
            Team currentTeam = GetTeamOnShift();
            return currentTeam.Agents.FirstOrDefault(a => a.IsAvailable == true);
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
