using Lendora.Domain.Entities;
using Lendora.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lendora.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Loan"/> aggregate root.
/// </summary>
public sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.Status)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<LoanStatus>(v))
            .HasMaxLength(50);

        builder.Property(l => l.ApprovedAmount).HasPrecision(18, 2);
        builder.Property(l => l.InterestRate).HasPrecision(5, 2);
        builder.Property(l => l.MonthlyPayment).HasPrecision(18, 2);

        builder.HasIndex(l => l.CustomerId);
        builder.HasIndex(l => l.Status);

        builder.HasMany(l => l.Installments)
            .WithOne()
            .HasForeignKey(i => i.LoanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Payments)
            .WithOne()
            .HasForeignKey(p => p.LoanId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure EF Core to access the private backing fields for collections
        builder.Navigation(l => l.Installments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(l => l.Payments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Ignore the Stateless state machine field — it is not persisted.
        builder.Ignore("_stateMachine");
    }
}
