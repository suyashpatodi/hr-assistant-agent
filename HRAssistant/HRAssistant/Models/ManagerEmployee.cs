using System.ComponentModel.DataAnnotations.Schema;

namespace HRAssistant.Models
{
    public class ManagerEmployee
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int ManagerId { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public Employee Employee { get; set; } = default!;

        [ForeignKey(nameof(ManagerId))]
        public Employee Manager { get; set; } = default!;

    }
}
