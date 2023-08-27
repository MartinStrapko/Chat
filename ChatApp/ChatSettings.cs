namespace ChatApp
{
    public class ChatSettings
    {
        public int TimerIntervalSeconds { get; set; }
        public int PollsPerSecond { get; set; }
        public int MarkInactiveAfterNumberOfMissedPolls { get; set; }
    }
}
