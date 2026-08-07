using System.ComponentModel;

namespace HRAssistant.Plugins
{
    public class SqlEnquiry
    {
        private readonly EmployeeDbContext _dbContext;
        public SqlEnquiry(EmployeeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [KernelFunction("get_leave_detail_for_employee")]
        [Description("Get only leave details for an employee from email")]
        public async Task<List<Leaves>> GetLeaveDetailForEmployee(string email)
        {
            var employee = await _dbContext.Employees.Where(x => x.Email == email).AsNoTracking().FirstOrDefaultAsync();
            if (employee == null) return null;
            var leaves = await _dbContext.Leaves.Where(x => x.EmployeeId == employee!.Id).ToListAsync();
            //var options = new JsonSerializerOptions
            //{
            //    ReferenceHandler = ReferenceHandler.IgnoreCycles,
            //    WriteIndented = false // Keeps token usage smaller for the LLM
            //};
            return leaves;
        }
    }
}
