using Microsoft.EntityFrameworkCore;

namespace HRAssistant.Data
{
    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Leaves> Leaves => Set<Leaves>();
    }
}
