namespace ChatApp.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Agent> Agents { get; set; }
    }
}
