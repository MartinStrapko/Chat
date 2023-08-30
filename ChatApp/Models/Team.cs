namespace ChatApp.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Agent> Agents { get; set; }
        public Shift Shift { get; set; }
        public int TotalCapacity => (int)Math.Floor(Agents.Sum(a => a.Capacity));
        public double MaxQueueLength => (int)Math.Floor(TotalCapacity * 1.5);
    }
}
