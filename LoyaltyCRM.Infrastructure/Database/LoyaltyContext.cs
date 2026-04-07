using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PapasCRM_API.Requests;
using PapasCRM_API.Entities;

namespace PapasCRM_API.Context
{
    public class LoyaltyContext : IdentityDbContext<ApplicationUserEntity>
    {
        public LoyaltyContext(DbContextOptions<LoyaltyContext> options)
        : base(options)
        {
        }

        //public DbSet<PhoneEntity> Phones { get; set; } = null!;

        public DbSet<YearcardEntity> Yearcards { get; set; } = null!;

        public DbSet<ValidityIntervalEntity> ValidityIntervalEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUserEntity>()
                .HasIndex(y => y.Email)
                .IsUnique();

            modelBuilder.Entity<ApplicationUserEntity>()
                .HasIndex(y => y.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<YearcardEntity>()
                .HasIndex(y => y.CardId)
                .IsUnique();

            modelBuilder.Entity<ApplicationUserEntity>()
                .HasOne(a => a.Yearcard)
                .WithOne(y => y.User)
                .HasForeignKey<YearcardEntity>(y => y.UserId)
                .IsRequired();

            modelBuilder.Entity<YearcardEntity>()
                .HasMany(y => y.ValidityIntervals)
                .WithOne(v => v.YearcardEntity)
                .HasForeignKey(v => v.YearcardEntityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
