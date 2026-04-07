using LoyaltyCRM.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyCRM.Infrastructure.Context
{
    public class LoyaltyContext : IdentityDbContext<ApplicationUser>
    {
        public LoyaltyContext(DbContextOptions<LoyaltyContext> options)
        : base(options)
        {
        }

        //public DbSet<PhoneEntity> Phones { get; set; } = null!;

        public DbSet<Yearcard> Yearcards { get; set; } = null!;

        public DbSet<ValidityInterval> ValidityInterval { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoyaltyContext).Assembly);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(y => y.Email)
                .IsUnique();

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(y => y.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(a => a.Yearcard)
                .WithOne(y => y.User)
                .HasForeignKey<Yearcard>(y => y.UserId)
                .IsRequired();

            modelBuilder.Entity<Yearcard>()
                .HasIndex(y => y.CardId)
                .IsUnique();

            modelBuilder.Entity<Yearcard>()
                .HasMany(y => y.ValidityIntervals)
                .WithOne(v => v.Yearcard)
                .HasForeignKey(v => v.YearcardEntityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
