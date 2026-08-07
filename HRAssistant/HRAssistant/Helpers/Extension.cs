namespace HRAssistant.Helpers
{
    public static class Extension
    {
        public static void UseMigration(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();

            context.Database.Migrate();
            DataSeeder.Seed(context);
        }
    }

    public class DataSeeder
    {
        public static void Seed(EmployeeDbContext dbContext)
        {
            // Seed Employees if empty
            if (!dbContext.Employees.Any())
            {
                dbContext.Employees.AddRange(Employees);
                dbContext.SaveChanges();
            }

            // Seed Leaves if empty
            if (!dbContext.Leaves.Any())
            {
                dbContext.Leaves.AddRange(Leaves);
                dbContext.SaveChanges();
            }

            // Seed ManagerEmployee Mapping if empty
            if (!dbContext.ManagerEmployee.Any())
            {
                dbContext.ManagerEmployee.AddRange(ManagerEmployee);
                dbContext.SaveChanges();
            }
        }

        public static IEnumerable<Employee> Employees =>
            [
                new Employee {
                    Id = 1,
                    FirstName = "Mukesh",
                    LastName = "Ambani",
                    Role = Roles.Manager,
                    Email = "mukeshambani@gmail.com",
                    Status = EmployeeStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddMonths(-24)
                },
                new Employee {
                    Id = 2,
                    FirstName = "John",
                    LastName="Doe",
                    Role = Roles.Employee,
                    Email = "johndoe@gmail.com",
                    Status = EmployeeStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddMonths(-6)
                },
                new Employee {
                    Id = 3,
                    FirstName="Jane",
                    LastName ="Doe",
                    Role = Roles.Employee,
                    Email = "janedoe@gmail.com",
                    Status = EmployeeStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                }
            ];

        public static IEnumerable<Leaves> Leaves =>
            [
                new Leaves {
                    Id = 1,
                    EmployeeId = 2,
                    From = DateTime.UtcNow.AddDays(-10),
                    To = DateTime.UtcNow.AddDays(-7),
                    Days = (DateTime.UtcNow.AddDays(-7) - DateTime.UtcNow.AddDays(-10)).Days + 2,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    Status = LeaveStatus.Approved,
                    Reason = "Travelling out of station"
                },
                new Leaves {
                    Id = 2,
                    EmployeeId = 3,
                    From = DateTime.UtcNow.AddDays(5),
                    To = DateTime.UtcNow.AddDays(7),
                    Days = (DateTime.UtcNow.AddDays(7) - DateTime.UtcNow.AddDays(5)).Days + 2,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    Status = LeaveStatus.Approved,
                    Reason = "Routine medical procedure and recovery"
                }
            ];

        public static IEnumerable<ManagerEmployee> ManagerEmployee =>
            [
                new ManagerEmployee {
                    Id = 1,
                    EmployeeId = 2,
                    ManagerId = 1,
                    ModifiedAt = DateTime.UtcNow.AddDays(-60),
                    CreatedAt = DateTime.UtcNow.AddDays(-60),
                },
                new ManagerEmployee {
                    Id = 2,
                    EmployeeId = 3,
                    ManagerId = 1,
                    ModifiedAt = DateTime.UtcNow.AddDays(-50),
                    CreatedAt = DateTime.UtcNow.AddDays(-50),
                }
            ];
    }
}
