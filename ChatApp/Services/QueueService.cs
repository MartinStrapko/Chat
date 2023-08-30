using ChatApp;
using ChatApp.Interfaces;
using ChatApp.Models;
using ChatApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;

public class QueueService : IQueueService
{
    private readonly Queue<ChatSession> _queue = new();
    private readonly Queue<ChatSession> _overflowQueue = new();
    private readonly ChatSettings _chatSettings;
    private readonly int _expectedPollsInIntervalSeconds;
    private readonly int _markInactiveAfterNumberOfMissedPolls;
    private readonly IAgentService _agentService;

    public int MaxCapacity { get; set; } = 10;
    public int OverflowCapacity { get; set; } = 10;
    public TimeSpan OfficeStart { get; set; } = new TimeSpan(9, 0, 0);
    public TimeSpan OfficeEnd { get; set; } = new TimeSpan(17, 0, 0);

    public QueueService(IOptions<ChatSettings> mySettings, IAgentService agentService)
    {
        _chatSettings = mySettings.Value;
        _agentService = agentService;
        _expectedPollsInIntervalSeconds = _chatSettings.TimerIntervalSeconds / _chatSettings.PollsPerSecond;
        _markInactiveAfterNumberOfMissedPolls = _chatSettings.MarkInactiveAfterNumberOfMissedPolls;

        agentService.OnChatEnded += AssignWaitingChats;
    }

    public bool IsQueueFull()
    {
        var teamOnShift = _agentService.GetTeamOnShift();
        var maxQueueLength = teamOnShift.MaxQueueLength;

        return _queue.Count >= maxQueueLength;
    }

    private Agent? AssignAgent(ChatSession session)
    {
        if(_agentService.GetAvailableAgent()!=null)
        {
            return _agentService.AssignChatToAgent(session);
        }
        return null;
    }

    public bool TryEnqueue(ChatSession session)
    {
        if (session == null)
            return false;

        var assignedAgent = AssignAgent(session);

        if (assignedAgent != null)
        {
            return true;
        }

        if (_queue.Count < MaxCapacity)
        {
            _queue.Enqueue(session);
        }
        else if (IsOfficeHours() && _overflowQueue.Count < OverflowCapacity)
        {
            _overflowQueue.Enqueue(session);
        }
        else
        {
            return false;
        }

        return true;
    }

    public ChatSession? Dequeue()
    {
        return _queue.Count > 0 ? _queue.Dequeue() :
               _overflowQueue.Count > 0 ? _overflowQueue.Dequeue() :
               null;
    }

    public bool ContainsSession(Guid sessionId)
    {
        return _queue.Any(s => s.SessionId == sessionId);
    }

    public bool PollSession(Guid sessionId)
    {
        var session = _queue.FirstOrDefault(s => s.SessionId == sessionId);

        if (session != null)
        {
            session.LastPolledAt = DateTime.UtcNow;
            session.PollsSiceLastCheck++;
            return true;
        }

        return false;
    }

    public void CheckMissedPolls()
    {
        foreach (var session in _queue)
        {
            var missedPolls = _expectedPollsInIntervalSeconds - session.PollsSiceLastCheck;
            if (missedPolls >= _markInactiveAfterNumberOfMissedPolls)
            {
                MarkSessionInactive(session);
            }
            session.PollsSiceLastCheck = 0;

        }
    }

    private void AssignWaitingChats()
    {
        while (true)
        {
            var agent = _agentService.GetAvailableAgent();
            if (agent == null)
                break;
            var session = Dequeue();

            if (session != null)
            {
                _agentService.AssignChatToAgent(session);
            }
            else
            {
                break;
            }
        }
    }

    private void MarkSessionInactive(ChatSession session)
    {
    }

    private bool IsOfficeHours()
    {
        var now = DateTime.UtcNow.TimeOfDay;
        return now > OfficeStart && now < OfficeEnd;
    }
}