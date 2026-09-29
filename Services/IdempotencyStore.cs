using System.Collections.Concurrent;

namespace ToolShare.Api;

public record IdempotencyEntry(string Fingerprint, LoanResponse Response);

public sealed class IdempotencyStore
{
    private readonly ConcurrentDictionary<string, IdempotencyEntry> _entries = new();

    public bool TryGet(string key, out IdempotencyEntry? entry) => _entries.TryGetValue(key, out entry);

    public void Save(string key, IdempotencyEntry entry) => _entries[key] = entry;
}