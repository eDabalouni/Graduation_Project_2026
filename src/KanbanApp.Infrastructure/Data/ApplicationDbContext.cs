using KanbanApp.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KanbanApp.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); 
        builder.Entity<Project>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Description).HasMaxLength(1000);

            entity.HasOne(p => p.Owner)
                  .WithMany(u => u.OwnedProjects)
                  .HasForeignKey(p => p.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict); 
        });

        builder.Entity<ProjectMember>(entity =>
        {
            entity.HasIndex(pm => new { pm.ProjectId, pm.UserId }).IsUnique();

            entity.HasOne(pm => pm.Project)
                  .WithMany(p => p.Members)
                  .HasForeignKey(pm => pm.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pm => pm.User)
                  .WithMany(u => u.ProjectMemberships)
                  .HasForeignKey(pm => pm.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TaskItem>(entity =>
        {
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Description).HasMaxLength(2000);

            entity.HasOne(t => t.Project)
                  .WithMany(p => p.Tasks)
                  .HasForeignKey(t => t.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade); 

            entity.HasOne(t => t.AssignedUser)
                  .WithMany(u => u.AssignedTasks)
                  .HasForeignKey(t => t.AssignedUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
