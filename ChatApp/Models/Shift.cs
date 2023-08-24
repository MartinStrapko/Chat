namespace ChatApp.Models
{
    public class Shift
    {
        public string Name { get; set; }
        public Guid ShiftId { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }
}
