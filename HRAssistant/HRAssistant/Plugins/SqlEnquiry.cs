using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HRAssistant.Plugins
{
    public class SqlEnquiry
    {
        private readonly EmployeeDbContext _dbContext;
        public SqlEnquiry(EmployeeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [KernelFunction("get_employee_info_by_id")]
        [Description("Get Employee data from id or email")]
        public async Task<string?> GetEmployeeInfoById([Description("Contains value for either employee's id or email")] string search)
        {
            var employee = await _dbContext.Employees.Where(x => (x.Id.ToString() == search || x.Email == search)).FirstOrDefaultAsync();
            if (employee == null)
            {
                return $"Termination Status: Employee matching identity '{search}' was not found in the database.";
            }
            return JsonSerializer.Serialize(employee);
        }

        [KernelFunction("get_leave_detail_for_employee")]
        [Description("Get only leave details for an employee from id")]
        public async Task<string?> GetLeaveDetailForEmployee([Description("Value for employee id")] int id)
        {
            var leaves = await _dbContext.Leaves.Where(x => x.EmployeeId == id).ToListAsync();
            if (leaves == null)
            {
                return $"Termination Status: Leave details for matching employee '{id}' was not found in the database.";
            }

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = false // Keeps token usage smaller for the LLM
            };


            return JsonSerializer.Serialize(leaves, options);
        }
    }
}
