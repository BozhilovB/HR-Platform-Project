using Microsoft.AspNetCore.Identity;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Roles
        var roles = new[] { "Admin", "HR", "Recruiter", "Manager", "Employee" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = role, NormalizedName = role.ToUpper() });
            }
        }

        // Seed Users
        var adminEmail = "admin@hrplatform.com";
        var managerEmail = "manager@hrplatform.com";
        var applicantEmail = "applicant@hrplatform.com";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "Admin@123");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        if (await userManager.FindByEmailAsync(managerEmail) == null)
        {
            var managerUser = new ApplicationUser
            {
                UserName = managerEmail,
                Email = managerEmail,
                FirstName = "Manager",
                LastName = "User",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(managerUser, "Manager@123");
            await userManager.AddToRoleAsync(managerUser, "Manager");
        }

        if (await userManager.FindByEmailAsync(applicantEmail) == null)
        {
            var applicantUser = new ApplicationUser
            {
                UserName = applicantEmail,
                Email = applicantEmail,
                FirstName = "Applicant",
                LastName = "User",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(applicantUser, "Applicant@123");
            await userManager.AddToRoleAsync(applicantUser, "Employee");
        }

        // Seed Teams
        if (!context.Teams.Any())
        {
            context.Teams.AddRange(
                new Team { Name = "Development Team", ManagerId = context.Users.First(u => u.Email == managerEmail).Id },
                new Team { Name = "HR Team", ManagerId = context.Users.First(u => u.Email == managerEmail).Id }
            );
        }

        // Seed Job Postings
        if (!context.JobPostings.Any())
        {
            context.JobPostings.AddRange(
                new JobPosting
                {
                    Title = "Software Developer",
                    Description = "Develop and maintain web applications.",
                    PostedDate = DateTime.UtcNow,
                    RecruiterId = context.Users.First(u => u.Email == managerEmail).Id
                },
                new JobPosting
                {
                    Title = "HR Specialist",
                    Description = "Handle recruitment and employee relations.",
                    PostedDate = DateTime.UtcNow,
                    RecruiterId = context.Users.First(u => u.Email == managerEmail).Id
                }
            );

            await context.SaveChangesAsync();
        }

        // Seed Job Applications
        if (!context.JobApplications.Any())
        {
            context.JobApplications.AddRange(
                new JobApplication
                {
                    ApplicantName = "John Doe",
                    ApplicantEmail = "johndoe@example.com",
                    ResumeUrl = "https://example.com/resume/johndoe.pdf",
                    Status = "Pending",
                    JobPostingId = context.JobPostings.First().Id
                },
                new JobApplication
                {
                    ApplicantName = "Jane Smith",
                    ApplicantEmail = "janesmith@example.com",
                    ResumeUrl = "https://example.com/resume/janesmith.pdf",
                    Status = "Approved",
                    JobPostingId = context.JobPostings.Skip(1).First().Id
                }
            );
        }

        //Seed Employees to Teams
        var employee1 = new ApplicationUser
        {
            Id = "user-employee-1",
            UserName = "employee1@hrplatform.com",
            NormalizedUserName = "EMPLOYEE1@HRPLATFORM.COM",
            Email = "employee1@hrplatform.com",
            NormalizedEmail = "EMPLOYEE1@HRPLATFORM.COM",
            EmailConfirmed = true,
            PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(null, "Employee@123"),
            FirstName = "John",
            LastName = "Doe"
        };

        var employee2 = new ApplicationUser
        {
            Id = "user-employee-2",
            UserName = "employee2@hrplatform.com",
            NormalizedUserName = "EMPLOYEE2@HRPLATFORM.COM",
            Email = "employee2@hrplatform.com",
            NormalizedEmail = "EMPLOYEE2@HRPLATFORM.COM",
            EmailConfirmed = true,
            PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(null, "Employee@123"),
            FirstName = "Jane",
            LastName = "Smith"
        };

        if (!context.Users.Any(u => u.Id == employee1.Id))
        {
            context.Users.Add(employee1);
        }

        if (!context.Users.Any(u => u.Id == employee2.Id))
        {
            context.Users.Add(employee2);
        }

        await context.SaveChangesAsync();

        var employeeRoleId = context.Roles.FirstOrDefault(r => r.Name == "Employee")?.Id;
        if (employeeRoleId != null)
        {
            if (!context.UserRoles.Any(ur => ur.UserId == employee1.Id && ur.RoleId == employeeRoleId))
            {
                context.UserRoles.Add(new IdentityUserRole<string>
                {
                    UserId = employee1.Id,
                    RoleId = employeeRoleId
                });
            }

            if (!context.UserRoles.Any(ur => ur.UserId == employee2.Id && ur.RoleId == employeeRoleId))
            {
                context.UserRoles.Add(new IdentityUserRole<string>
                {
                    UserId = employee2.Id,
                    RoleId = employeeRoleId
                });
            }
        }

        if (!context.TeamMembers.Any(tm => tm.TeamId == 2 && tm.UserId == employee1.Id))
        {
            context.TeamMembers.Add(new TeamMember
            {
                TeamId = 2,
                UserId = employee1.Id,
                JoinedAt = DateTime.UtcNow.AddDays(-7)
            });
        }

        if (!context.TeamMembers.Any(tm => tm.TeamId == 2 && tm.UserId == employee2.Id))
        {
            context.TeamMembers.Add(new TeamMember
            {
                TeamId = 2,
                UserId = employee2.Id,
                JoinedAt = DateTime.UtcNow.AddDays(-4)
            });
        }


        // Save changes
        await context.SaveChangesAsync();
    }
}