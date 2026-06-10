using System.ComponentModel;
using System.Text.Json;

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
            var employee = await _dbContext.Employees.Where(x => x.Id.ToString() == search || x.Email == search).FirstOrDefaultAsync();
            if (employee == null) return null;

            return JsonSerializer.Serialize(employee);
        }
    }
}
