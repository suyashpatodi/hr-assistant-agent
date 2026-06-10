namespace HRAssistant.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public Status Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Leaves> Leaves { get; set; } = default!;
    }

    public enum Status
    {
        Inactive = 0,
        Active = 1
    }
}
