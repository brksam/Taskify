using Microsoft.EntityFrameworkCore;
using Taskify.Domain;
public class TaskifyDbContext : DbContext {
    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<UserTeam> UserTeams => Set<UserTeam>();

    public TaskifyDbContext(DbContextOptions<TaskifyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<UserTeam>().HasKey(ut => new { ut.UserId, ut.TeamId });
    }
}