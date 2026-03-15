using Lendora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lendora.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Payment"/> entity.
/// </summary>
public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Amount).HasPrecision(18, 2);

        builder.Property(p => p.PaymentReference).HasMaxLength(100);

        // Unique index on PaymentReference, filtered to non-null values only.
        // This prevents duplicate external references while allowing multiple null entries.
        builder.HasIndex(p => p.PaymentReference)
            .IsUnique()
            .HasFilter("\"PaymentReference\" IS NOT NULL");

        builder.HasIndex(p => p.LoanId);
    }
}
