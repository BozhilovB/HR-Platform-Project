﻿using Microsoft.AspNetCore.Identity;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.EnsureCreatedAsync();

        var roles = new[] { "Admin", "HR", "Recruiter", "Manager", "Employee", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = role, NormalizedName = role.ToUpper() });
            }
        }

        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                UserName = "admin@hrplatform.com",
                Email = "admin@hrplatform.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                Salary = 100000M,
            },
            new ApplicationUser
            {
                UserName = "hr@hrplatform.com",
                Email = "hr@hrplatform.com",
                FirstName = "Hr",
                LastName = "User",
                EmailConfirmed = true,
                Salary = 100000M,
            },
            new ApplicationUser
            {
                UserName = "recruiter@hrplatform.com",
                Email = "recruiter@hrplatform.com",
                FirstName = "Recruiter",
                LastName = "User",
                EmailConfirmed = true,
                Salary = 4500.50M,
            },
            new ApplicationUser
            {
                UserName = "manager@hrplatform.com",
                Email = "manager@hrplatform.com",
                FirstName = "Manager",
                LastName = "User",
                EmailConfirmed = true,
                Salary = 80000M,
            },
            new ApplicationUser
            {
                UserName = "user1@hrplatform.com",
                Email = "user1@hrplatform.com",
                FirstName = "User1",
                LastName = "Demo",
                EmailConfirmed = true,
            },
            new ApplicationUser
            {
                UserName = "user2@hrplatform.com",
                Email = "user2@hrplatform.com",
                FirstName = "User2",
                LastName = "Demo",
                EmailConfirmed = true,
            },
            new ApplicationUser
            {
                UserName = "user3@hrplatform.com",
                Email = "user3@hrplatform.com",
                FirstName = "User3",
                LastName = "Demo",
                EmailConfirmed = true,
            },
            new ApplicationUser
            {
                UserName = "employee1@hrplatform.com",
                Email = "employee1@hrplatform.com",
                FirstName = "John",
                LastName = "Doe",
                EmailConfirmed = true,
                Salary = 4500.20M,
            },
            new ApplicationUser
            {
                UserName = "employee2@hrplatform.com",
                Email = "employee2@hrplatform.com",
                FirstName = "Jane",
                LastName = "Smith",
                EmailConfirmed = true,
                Salary = 4700.50M,
            }
        };

        foreach (var user in users)
        {
            if (await userManager.FindByEmailAsync(user.Email) == null)
            {
                await userManager.CreateAsync(user, $"{user.FirstName}@123");
                if (user.Email == "admin@hrplatform.com") await userManager.AddToRoleAsync(user, "Admin");
                if (user.Email == "recruiter@hrplatform.com") await userManager.AddToRoleAsync(user, "Recruiter");
                if (user.Email == "manager@hrplatform.com") await userManager.AddToRoleAsync(user, "Manager");
                if (user.Email == "hr@hrplatform.com") await userManager.AddToRoleAsync(user, "HR");
                if (user.Email.StartsWith("user")) await userManager.AddToRoleAsync(user, "User");
                if (user.Email.StartsWith("employee")) await userManager.AddToRoleAsync(user, "Employee");
            }
        }

        if (!context.Teams.Any())
        {
            var manager = context.Users.First(u => u.Email == "manager@hrplatform.com");

            var teams = new[]
            {
                new Team { Name = "Development Team", ManagerId = manager.Id },
                new Team { Name = "HR Team", ManagerId = manager.Id }
            };

            context.Teams.AddRange(teams);
            await context.SaveChangesAsync();

            var team1 = context.Teams.First(t => t.Name == "Development Team");
            var team2 = context.Teams.First(t => t.Name == "HR Team");

            var employee1 = context.Users.First(u => u.Email == "employee1@hrplatform.com");
            var employee2 = context.Users.First(u => u.Email == "employee2@hrplatform.com");

            context.TeamMembers.AddRange(
                new TeamMember { TeamId = team1.Id, UserId = employee1.Id, JoinedAt = DateTime.UtcNow },
                new TeamMember { TeamId = team2.Id, UserId = employee2.Id, JoinedAt = DateTime.UtcNow }
            );

            await context.SaveChangesAsync();
        }

        if (!context.JobPostings.Any())
        {
            var recruiter = context.Users.First(u => u.Email == "recruiter@hrplatform.com");
            context.JobPostings.AddRange(
                new JobPosting
                {
                    Title = "Software Developer",
                    Description = "Develop and maintain web applications.",
                    PostedDate = DateTime.UtcNow,
                    RecruiterId = recruiter.Id
                },
                new JobPosting
                {
                    Title = "HR Specialist",
                    Description = "Handle recruitment and employee relations.",
                    PostedDate = DateTime.UtcNow,
                    RecruiterId = recruiter.Id
                }
            );

            await context.SaveChangesAsync();
        }

        if (!context.JobApplications.Any())
        {
            var job1 = context.JobPostings.First();
            var job2 = context.JobPostings.Skip(1).First();

            context.JobApplications.AddRange(
                new JobApplication
                {
                    ApplicantName = "User1 Demo",
                    ApplicantEmail = "user1@hrplatform.com",
                    ResumeUrl = "https://example.com/resume/user1.pdf",
                    Status = "Pending",
                    JobPostingId = job1.Id
                },
                new JobApplication
                {
                    ApplicantName = "User2 Demo",
                    ApplicantEmail = "user2@hrplatform.com",
                    ResumeUrl = "https://example.com/resume/user2.pdf",
                    Status = "Pending",
                    JobPostingId = job2.Id
                },
                new JobApplication
                {
                    ApplicantName = "User3 Demo",
                    ApplicantEmail = "user3@hrplatform.com",
                    ResumeUrl = "https://example.com/resume/user3.pdf",
                    Status = "Pending",
                    JobPostingId = job1.Id
                }
            );

            await context.SaveChangesAsync();
        }
    }
}