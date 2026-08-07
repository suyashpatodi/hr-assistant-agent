namespace HRAssistant.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public Roles Role { get; set; } = default!;
        public string Email { get; set; } = default!;
        public EmployeeStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Leaves> Leaves { get; set; } = default!;
    }
}
