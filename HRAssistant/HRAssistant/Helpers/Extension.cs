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
        }

        public static IEnumerable<Employee> Employees =>
            [
                new Employee {
                    Id = 1,
                    Name = "Suyash Patodi",
                    Email = "suyashpatodi@gmail.com",
                    Status = Status.Active,
                    CreatedAt = DateTime.UtcNow.AddMonths(-12)
                },
                new Employee {
                    Id = 2,
                    Name = "Sarah Jenkins",
                    Email = "sarah.j@hrassistant.com",
                    Status = Status.Active,
                    CreatedAt = DateTime.UtcNow.AddMonths(-6)
                },
                new Employee {
                    Id = 3,
                    Name = "Arjun Mehta",
                    Email = "arjun.mehta@hrassistant.com",
                    Status = Status.Active,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                },
                new Employee {
                    Id = 4,
                    Name = "Emma Watson",
                    Email = "emma.w@hrassistant.com",
                    Status = Status.Inactive,
                    CreatedAt = DateTime.UtcNow.AddMonths(-18)
                }
            ];

        public static IEnumerable<Leaves> Leaves =>
            [
                // Suyash's Leaves (Past trip)
                new Leaves {
                    Id = 1,
                    EmployeeId = 1,
                    From = DateTime.UtcNow.AddDays(-10),
                    To = DateTime.UtcNow.AddDays(-7),
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    Reason = "Travelling out of station"
                },
                // Sarah's Leaves (Upcoming Medical Leave)
                new Leaves {
                    Id = 2,
                    EmployeeId = 2,
                    From = DateTime.UtcNow.AddDays(5),
                    To = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    Reason = "Routine medical procedure and recovery"
                },
                // Arjun's Leaves (Short personal day)
                new Leaves {
                    Id = 3,
                    EmployeeId = 3,
                    From = DateTime.UtcNow.AddDays(12),
                    To = DateTime.UtcNow.AddDays(13),
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    Reason = "Family event / attending wedding"
                },
                // Emma's Leaves (Currently Active Extended Leave)
                new Leaves {
                    Id = 4,
                    EmployeeId = 4,
                    From = DateTime.UtcNow.AddDays(-3),
                    To = DateTime.UtcNow.AddDays(4),
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    Reason = "Moving to a new apartment & settling in"
                },
                // Suyash's second leaf request (Future casual leave)
                new Leaves {
                    Id = 5,
                    EmployeeId = 1,
                    From = DateTime.UtcNow.AddDays(20),
                    To = DateTime.UtcNow.AddDays(22),
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    Reason = "Long weekend family trip"
                }
            ];
    }
}
