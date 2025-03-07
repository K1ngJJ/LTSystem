using Microsoft.EntityFrameworkCore;
using LTS_app.Models;

namespace LTS_app.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Bill> Bills { get; set; }
        public DbSet<Legislator> Legislators { get; set; }
        public DbSet<Committee> Committees { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Amendment> Amendments { get; set; }
        public DbSet<BillHistory> BillHistories { get; set; }
        public DbSet<CitizenFeedback> CitizenFeedbacks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                entity.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Bill -> Legislator (One-to-Many)
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Legislator)
                .WithMany(l => l.Bills)  // Ensure Legislator has Bills collection in Legislator.cs
                .HasForeignKey(b => b.LegislatorId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascading delete

            // Bill -> Committee (One-to-Many)
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Committee)
                .WithMany(c => c.Bills)  // Ensure Committee has Bills collection in Committee.cs
                .HasForeignKey(b => b.CommitteeId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascading delete

            // Bill - Amendments (One-to-Many)
            modelBuilder.Entity<Amendment>()
                .HasOne(a => a.Bill)
                .WithMany(b => b.Amendments)
                .HasForeignKey(a => a.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bill - BillHistory (One-to-Many)
            modelBuilder.Entity<BillHistory>()
                .HasOne(bh => bh.Bill)
                .WithMany(b => b.BillHistories)
                .HasForeignKey(bh => bh.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bill - Votes (One-to-Many)
            modelBuilder.Entity<Vote>()
                .HasOne(v => v.Bill)
                .WithMany(b => b.Votes)
                .HasForeignKey(v => v.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vote - Legislator (One-to-Many)
            modelBuilder.Entity<Vote>()
                .HasOne(v => v.Legislator)
                .WithMany(l => l.Votes)
                .HasForeignKey(v => v.LegislatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Citizen Feedback - Bill (One-to-Many)
            modelBuilder.Entity<CitizenFeedback>()
                .HasOne(cf => cf.Bill)
                .WithMany(b => b.CitizenFeedbacks)
                .HasForeignKey(cf => cf.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification - User (One-to-Many)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure Enum is stored as a string
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();
        }

    }
}
