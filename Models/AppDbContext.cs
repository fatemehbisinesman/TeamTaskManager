using Microsoft.EntityFrameworkCore;
using TeamTaskManager.Models;

namespace TeamTaskManager.Data  // پیشنهاد: جدا کردن فضای نام برای لایه دیتا
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet ها
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Project> Projects { get; set; } = default!;
        public DbSet<TaskItem> Tasks { get; set; } = default!;
        public DbSet<Ad> Ads { get; set; } = default!;
        public DbSet<Payment> Payments { get; set; } = default!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = default!;
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<ProjectMember> ProjectMembers { get; set; } = default!;
        public DbSet<UserModule> UserModules { get; set; } = default!;
        public DbSet<Module> Modules { get; set; } = default!;
        public DbSet<Comment> Comments { get; set; } = default!;
        public DbSet<ProjectFile> ProjectFiles { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<DailyPlanItem> DailyPlanItems { get; set; }  
        public DbSet<DailyGratitude> DailyGratitudes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // رابطه بین ProjectMember و Project
            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // رابطه بین ProjectMember و User
            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany()
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
