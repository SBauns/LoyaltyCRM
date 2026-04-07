using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoyaltyCRM.Infrastructure.Database
{
    public class AddressConfiguration : IEntityTypeConfiguration<Yearcard>
    {
        public void Configure(EntityTypeBuilder<Yearcard> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasConversion(
                    v => v!.GetValue(),
                    v => new Name(v));

            builder.Property(x => x.CardId)
                .HasConversion(
                    v => v!.GetValue(),
                    v => new CardNumber(v));
        }
    }
}