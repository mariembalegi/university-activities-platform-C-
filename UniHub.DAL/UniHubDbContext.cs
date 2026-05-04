using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniHub.BL.Entities;

namespace UniHub.DAL
{
    public class UniHubDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Activity>? Activities { get; set; }
        public DbSet<Inscription>? Inscriptions { get; set; }
        public DbSet<Comment>? Comments { get; set; }

        public UniHubDbContext(DbContextOptions<UniHubDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            // Suppress the pending model changes warning
            optionsBuilder.ConfigureWarnings(warnings => 
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("Identity");

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "User");

                // Configure Department as enum (stored as int)
                entity.Property(u => u.Department)
                    .HasConversion<int?>();
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Role");
            });

            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles");
            });

            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims");
            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins");
            });

            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims");
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens");
            });

            // Configure Activity
            builder.Entity<Activity>(entity =>
            {
                entity.ToTable("Activities");

                // Configure Department as enum (stored as int)
                entity.Property(a => a.Department)
                    .HasConversion<int>();

                entity.HasOne(a => a.CreatedByUser)
                    .WithMany(u => u.CreatedActivities)
                    .HasForeignKey(a => a.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Inscription
            builder.Entity<Inscription>(entity =>
            {
                entity.ToTable("Inscriptions");

                entity.HasOne(i => i.Activity)
                    .WithMany(a => a.Inscriptions)
                    .HasForeignKey(i => i.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.User)
                    .WithMany(u => u.Inscriptions)
                    .HasForeignKey(i => i.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Empêcher les inscriptions en double
                entity.HasIndex(i => new { i.ActivityId, i.UserId })
                    .IsUnique();
            });

            // Configure Comment
            builder.Entity<Comment>(entity =>
            {
                entity.ToTable("Comments");

                entity.HasOne(c => c.Activity)
                    .WithMany(a => a.Comments)
                    .HasForeignKey(c => c.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
