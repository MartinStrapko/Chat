namespace ChatApp.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Agent> Agents { get; set; }
        public Shift Shift { get; set; }
        public double TotalCapacity => Agents.Sum(a => a.Capacity);
        public double MaxQueueLength => TotalCapacity * 1.5;
    }
}
