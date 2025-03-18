﻿using Microsoft.EntityFrameworkCore;
using LTS_app.Models;

namespace LTS_app.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Legislator> Legislators { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Committee> Committees { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Amendment> Amendments { get; set; }
        public DbSet<BillHistory> BillHistories { get; set; }
        public DbSet<UserFeedback> UserFeedbacks { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserLog> UserLogs { get; set; }
        public DbSet<CommitteeLegislator> CommitteeLegislators { get; set; }

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
                else
                {
                    Entry(entity).Property(x => x.CreatedAt).IsModified = false; // Prevent modification of CreatedAt
                }
                entity.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🏛 Legislator - User (One-to-One)
            modelBuilder.Entity<Legislator>()
                .HasOne(l => l.User)
                .WithOne(u => u.Legislator)
                .HasForeignKey<Legislator>(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 📜 Bill -> User (One-to-Many) - Each Bill has a Creator
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bills)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 📜 Bill -> Committee (One-to-Many, Optional)
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Committee)
                .WithMany(c => c.Bills)
                .HasForeignKey(b => b.CommitteeId)
                .OnDelete(DeleteBehavior.SetNull);

            // 📜 Bill -> Session (One-to-Many)
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Session)
                .WithMany(s => s.Bills)
                .HasForeignKey(b => b.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✏️ Amendment -> Bill (One-to-Many)
            modelBuilder.Entity<Amendment>()
                .HasOne(a => a.Bill)
                .WithMany(b => b.Amendments)
                .HasForeignKey(a => a.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🏛 Committee -> Legislators (Many-to-Many)
            modelBuilder.Entity<CommitteeLegislator>()
                .HasKey(cl => new { cl.CommitteeId, cl.LegislatorId });

            modelBuilder.Entity<CommitteeLegislator>()
                .HasOne(cl => cl.Committee)
                .WithMany(c => c.CommitteeLegislators)
                .HasForeignKey(cl => cl.CommitteeId);

            modelBuilder.Entity<CommitteeLegislator>()
                .HasOne(cl => cl.Legislator)
                .WithMany(l => l.CommitteeLegislators)
                .HasForeignKey(cl => cl.LegislatorId);

            // 📜 Bill History -> Bill (One-to-Many)
            modelBuilder.Entity<BillHistory>()
                .HasOne(bh => bh.Bill)
                .WithMany(b => b.BillHistories)
                .HasForeignKey(bh => bh.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🗳 Vote -> Bill (One-to-Many)
            modelBuilder.Entity<Vote>()
                .HasOne(v => v.Bill)
                .WithMany(b => b.Votes)
                .HasForeignKey(v => v.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🗳 Vote -> Legislator (One-to-Many)
            modelBuilder.Entity<Vote>()
                .HasOne(v => v.Legislator)
                .WithMany(l => l.Votes)
                .HasForeignKey(v => v.LegislatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // 💬 Citizen Feedback -> Bill (One-to-Many)
            modelBuilder.Entity<UserFeedback>()
                .HasOne(cf => cf.Bill)
                .WithMany(b => b.CitizenFeedbacks)
                .HasForeignKey(cf => cf.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔔 Notification -> User (One-to-Many)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 💾 Enum Conversions for Better Storage
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<BillHistory>()
                .Property(bh => bh.Status)
                .HasConversion<string>();
        }
    }
}