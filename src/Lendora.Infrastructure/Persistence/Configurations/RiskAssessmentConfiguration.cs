using Lendora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lendora.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="RiskAssessment"/> entity.
/// </summary>
public sealed class RiskAssessmentConfiguration : IEntityTypeConfiguration<RiskAssessment>
{
    public void Configure(EntityTypeBuilder<RiskAssessment> builder)
    {
        builder.HasKey(ra => ra.Id);
        builder.Property(ra => ra.Id).ValueGeneratedNever();

        builder.Property(ra => ra.Decision).HasMaxLength(50);

        builder.HasIndex(ra => ra.ApplicationId);
    }
}
