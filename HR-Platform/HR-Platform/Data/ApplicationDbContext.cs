using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<JobPosting> JobPostings { get; set; }
    public DbSet<JobApplication> JobApplications { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
        .Property(u => u.Salary)
        .HasColumnType("decimal(18,2)");

        builder.Entity<Team>()
            .HasOne(t => t.Manager)
            .WithMany()
            .HasForeignKey(t => t.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TeamMember>()
            .HasKey(tm => new { tm.TeamId, tm.UserId });

        builder.Entity<TeamMember>()
            .HasOne(tm => tm.Team)
            .WithMany(t => t.TeamMembers)
            .HasForeignKey(tm => tm.TeamId);

        builder.Entity<TeamMember>()
            .HasOne(tm => tm.User)
            .WithMany(u => u.Teams)
            .HasForeignKey(tm => tm.UserId);

        builder.Entity<JobPosting>()
            .HasOne(jp => jp.Recruiter)
            .WithMany()
            .HasForeignKey(jp => jp.RecruiterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<JobApplication>()
            .HasOne(ja => ja.JobPosting)
            .WithMany(jp => jp.JobApplications)
            .HasForeignKey(ja => ja.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);
    }

}