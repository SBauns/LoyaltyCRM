using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoyaltyCRM.Infrastructure.Database
{
    public class ValidityIntervalConfiguration : IEntityTypeConfiguration<ValidityInterval>
    {
        public void Configure(EntityTypeBuilder<ValidityInterval> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.StartDate)
                .HasConversion(
                    v => v!.GetValue(),
                    v => new StartDate(v));

            builder.Property(x => x.EndDate)
                .HasConversion(
                    v => v!.GetValue(),
                    v => new EndDate(v));
        }
    }
}