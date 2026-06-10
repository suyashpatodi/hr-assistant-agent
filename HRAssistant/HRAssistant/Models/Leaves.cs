using System.ComponentModel.DataAnnotations.Schema;

namespace HRAssistant.Models
{
    public class Leaves
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public Employee Employee { get; set; } = default!;

        public DateTime From { get; set; } = default!;
        public DateTime To { get; set; } = default!;
        public string Reason { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
