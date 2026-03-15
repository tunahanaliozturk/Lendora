using Lendora.Application.Common.Interfaces;
using Lendora.Domain.Common;
using Lendora.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lendora.Infrastructure.Persistence;

/// <summary>
/// The application's Entity Framework Core database context. Manages entity persistence,
/// audit timestamp population, and domain event dispatch after successful saves.
/// </summary>
public sealed class LendoraDbContext : DbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;

    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<RepaymentInstallment> RepaymentInstallments => Set<RepaymentInstallment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();

    public LendoraDbContext(DbContextOptions<LendoraDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LendoraDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditTimestamps();

        // Collect domain events before saving. We clear them from the aggregates
        // to prevent double-dispatch if SaveChangesAsync is called again.
        var domainEvents = CollectAndClearDomainEvents();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events after the transaction commits successfully.
        // This ensures events are only published for persisted state changes.
        await DispatchDomainEventsAsync(domainEvents, cancellationToken);

        return result;
    }

    private void SetAuditTimestamps()
    {
        var utcNow = DateTime.UtcNow;
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(BaseEntity.CreatedAt)).CurrentValue = utcNow;
                    break;

                case EntityState.Modified:
                    entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = utcNow;
                    // Prevent overwriting CreatedAt on updates
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    break;
            }
        }
    }

    private List<IDomainEvent> CollectAndClearDomainEvents()
    {
        var aggregateRoots = ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(ar => ar.DomainEvents)
            .ToList();

        foreach (var aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }

        return domainEvents;
    }

    private async Task DispatchDomainEventsAsync(
        List<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            // Create DomainEventNotification<TDomainEvent> using reflection since
            // we only know the concrete event type at runtime.
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent)
                ?? throw new InvalidOperationException(
                    $"Failed to create DomainEventNotification for {domainEvent.GetType().Name}.");

            await _publisher.Publish(notification, cancellationToken);
        }
    }
}
