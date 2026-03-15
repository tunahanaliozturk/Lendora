using Lendora.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lendora.Infrastructure.BackgroundJobs;

/// <summary>
/// Background worker that periodically checks for overdue loan installments and marks them as late.
/// When a loan has late installments, the loan itself is marked as delinquent.
/// Runs on a configurable interval (default: 1 hour).
/// </summary>
public sealed class LatePaymentMonitoringWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LatePaymentMonitoringWorker> _logger;
    private readonly TimeSpan _interval;

    /// <summary>
    /// Default polling interval between late payment checks.
    /// </summary>
    public static readonly TimeSpan DefaultInterval = TimeSpan.FromHours(1);

    public LatePaymentMonitoringWorker(
        IServiceProvider serviceProvider,
        ILogger<LatePaymentMonitoringWorker> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _interval = DefaultInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "LatePaymentMonitoringWorker started. Polling interval: {Interval}",
            _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOverdueInstallmentsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown — no action needed
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred while processing overdue installments. Will retry in {Interval}",
                    _interval);
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("LatePaymentMonitoringWorker stopped.");
    }

    private async Task ProcessOverdueInstallmentsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var today = dateTimeProvider.UtcNow.Date;

        var overdueInstallments = await loanRepository.GetOverdueInstallmentsAsync(
            today, cancellationToken);

        if (overdueInstallments.Count == 0)
        {
            _logger.LogDebug("No overdue installments found as of {Date}", today);
            return;
        }

        // Mark each overdue installment as late
        foreach (var installment in overdueInstallments)
        {
            installment.MarkAsLate();
        }

        // Group by loan and mark each affected loan as delinquent
        var loanIds = overdueInstallments
            .Select(i => i.LoanId)
            .Distinct()
            .ToList();

        foreach (var loanId in loanIds)
        {
            var loan = await loanRepository.GetWithInstallmentsAsync(loanId, cancellationToken);

            if (loan is null)
            {
                _logger.LogWarning(
                    "Loan {LoanId} not found while processing late installments. Skipping.",
                    loanId);
                continue;
            }

            try
            {
                loan.MarkAsDelinquent();
            }
            catch (Exception ex)
            {
                // The loan may already be delinquent or closed — log and continue
                _logger.LogWarning(ex,
                    "Could not mark loan {LoanId} as delinquent. It may already be in a terminal state.",
                    loanId);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Late payment monitoring completed: {Count} installment(s) marked as late across {LoanCount} loan(s)",
            overdueInstallments.Count,
            loanIds.Count);
    }
}
