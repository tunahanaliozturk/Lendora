using System.Text.Json;
using Lendora.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Lendora.Infrastructure.Caching;

/// <summary>
/// Distributed cache implementation backed by Redis via <see cref="IDistributedCache"/>.
/// Serializes values as UTF-8 JSON using System.Text.Json.
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        var bytes = await _cache.GetAsync(key, cancellationToken);

        if (bytes is null || bytes.Length == 0)
            return default;

        return JsonSerializer.Deserialize<T>(bytes, SerializerOptions);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(
        string key, T value, TimeSpan? expiry, CancellationToken cancellationToken)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);

        var options = new DistributedCacheEntryOptions();

        if (expiry.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiry.Value;
        }

        await _cache.SetAsync(key, bytes, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }
}
