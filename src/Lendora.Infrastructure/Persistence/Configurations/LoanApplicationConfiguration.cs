using Lendora.Domain.Entities;
using Lendora.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lendora.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="LoanApplication"/> aggregate root.
/// </summary>
public sealed class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
{
    public void Configure(EntityTypeBuilder<LoanApplication> builder)
    {
        builder.HasKey(la => la.Id);
        builder.Property(la => la.Id).ValueGeneratedNever();

        builder.Property(la => la.CustomerId);
        builder.HasIndex(la => la.CustomerId);

        builder.Property(la => la.Status)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<LoanApplicationStatus>(v))
            .HasMaxLength(50);
        builder.HasIndex(la => la.Status);

        builder.Property(la => la.RequestedAmount).HasPrecision(18, 2);
        builder.Property(la => la.InterestRate).HasPrecision(5, 2);

        builder.Property(la => la.RejectionReason).HasMaxLength(500);
        builder.Property(la => la.ApprovedBy).HasMaxLength(500);

        builder.HasMany<RiskAssessment>()
            .WithOne()
            .HasForeignKey(ra => ra.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore the Stateless state machine field — it is not persisted.
        builder.Ignore("_stateMachine");
    }
}
