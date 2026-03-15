using Lendora.Application.Common.Interfaces;
using Lendora.Infrastructure.BackgroundJobs;
using Lendora.Infrastructure.Caching;
using Lendora.Infrastructure.Persistence;
using Lendora.Infrastructure.Persistence.Repositories;
using Lendora.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lendora.Infrastructure;

/// <summary>
/// Registers all infrastructure services, persistence, caching, and background workers.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Application configuration for connection strings.</param>
    /// <returns>The configured service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Persistence — PostgreSQL via Npgsql
        services.AddDbContext<LendoraDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(
                    typeof(LendoraDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<LendoraDbContext>());

        // Repositories
        services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();

        // Distributed cache — Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "Lendora:";
        });

        services.AddScoped<ICacheService, RedisCacheService>();

        // Services
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Background workers
        services.AddHostedService<LatePaymentMonitoringWorker>();

        return services;
    }
}
