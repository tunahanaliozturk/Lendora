using Lendora.Domain.Entities;
using Lendora.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lendora.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="RepaymentInstallment"/> entity.
/// </summary>
public sealed class RepaymentInstallmentConfiguration : IEntityTypeConfiguration<RepaymentInstallment>
{
    public void Configure(EntityTypeBuilder<RepaymentInstallment> builder)
    {
        builder.HasKey(ri => ri.Id);
        builder.Property(ri => ri.Id).ValueGeneratedNever();

        builder.Property(ri => ri.PrincipalAmount).HasPrecision(18, 2);
        builder.Property(ri => ri.InterestAmount).HasPrecision(18, 2);
        builder.Property(ri => ri.TotalAmount).HasPrecision(18, 2);
        builder.Property(ri => ri.PaidAmount).HasPrecision(18, 2);

        builder.Property(ri => ri.Status)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<InstallmentStatus>(v))
            .HasMaxLength(50);

        // Composite index for efficient loan schedule queries
        builder.HasIndex(ri => new { ri.LoanId, ri.DueDate });

        // Index optimized for the late payment monitoring worker
        builder.HasIndex(ri => new { ri.Status, ri.DueDate });
    }
}
