namespace Lendora.Application.Common.Interfaces;

/// <summary>
/// Provides the current UTC time. Abstracted for testability, allowing deterministic
/// control over time-dependent behavior in tests.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}
