using Lendora.Application.Common.Interfaces;

namespace Lendora.Infrastructure.Services;

/// <summary>
/// Production implementation of <see cref="IDateTimeProvider"/> that returns the actual system UTC time.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
