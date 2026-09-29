using System.Collections.Concurrent;

namespace ToolShare.Api;

//the fingerprint detects "same key, different payload",
//store both and the saved response lets us replay the exact original result.
public record IdempotencyEntry(string Fingerprint, LoanResponse Response);

public sealed class IdempotencyStore
{
    /

    private readonly ConcurrentDictionary<string, IdempotencyEntry> _entries = new();

    public bool TryGet(string key, out IdempotencyEntry? entry) => _entries.TryGetValue(key, out entry);

    public void Save(string key, IdempotencyEntry entry) => _entries[key] = entry;
}