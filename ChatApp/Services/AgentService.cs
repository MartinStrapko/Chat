using ChatApp.Interfaces;
using ChatApp.Models;

namespace ChatApp.Services
{
    public class AgentService : IAgentService
    {
        private readonly List<Agent> _agents;
        private readonly List<Team> _teams;
        private readonly List<Shift> _shifts;
        private readonly IQueueService _queueService;

        public AgentService(IQueueService queueService, List<Team> teams, List<Shift> shifts)
        {
            _agents = new List<Agent>();
            _queueService = queueService;
        }

        public Agent? AssignChatToAgent(ChatSession chatSession)
        {
            var availableAgent = _agents.FirstOrDefault(a => a.IsAvailable);

            if (availableAgent != null)
            {
                availableAgent.IsAvailable = false;
                availableAgent.CurrentChatSession = chatSession;
                return availableAgent;
            }

            return null;
        }

        public bool EndChat(Guid agentId)
        {
            var agent = _agents.FirstOrDefault(a => a.Id == agentId);

            if (agent != null)
            {
                agent.IsAvailable = true;
                agent.CurrentChatSession = null;
                return true;
            }

            return false;
        }

        public bool IsAgentAvailable(Guid agentId)
        {
            return _agents.Any(a => a.Id == agentId && a.IsAvailable);
        }

        private Agent? GetAvailableAgent()
        {
            return _agents.FirstOrDefault(a => a.IsAvailable == true);
        }

        public void AddAgent(Agent agent)
        {
            _agents.Add(agent);

            AssignWaitingChats();
        }

        private Shift DetermineShift(TimeSpan currentTime)
        {
            return _shifts.FirstOrDefault(shift => shift.Start <= currentTime && shift.End >= currentTime)
                ?? throw new Exception("No shift is currently active.");
        }

        private void AssignWaitingChats()
        {
            while (true)
            {
                var agent = GetAvailableAgent();
                if (agent == null)
                    break;

                var session = _queueService.Dequeue();
                if (session != null)
                {
                    AssignChatToAgent(session);
                }
                else
                {
                    break;
                }
            }
        }
    }
}
