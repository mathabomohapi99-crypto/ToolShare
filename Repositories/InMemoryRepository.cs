using System.Collections.Concurrent;

namespace ToolShare.Api;

public sealed class InMemoryRepository<T> : IRepository<T> where T : Entity
{
    // ConcurrentDictionary: the API handles requests on many threads
    // at once, and a plain Dictionary is not thread-safe.
    private readonly ConcurrentDictionary<Guid, T> _items = new();

    public IReadOnlyList<T> GetAll() => _items.Values.ToList();

    // nullable return: "not found" isn't an error at this layer.
    // The caller decides whether to throw NotFoundException.
    public T? GetById(Guid id) => _items.GetValueOrDefault(id);

    public void Add(T entity) => _items[entity.Id] = entity;

    // Update exists: in-memory the object is already changed by reference,
    // but a real database needs an explicit save, so we keep the same shape.
    public void Update(T entity) => _items[entity.Id] = entity;

    public void Remove(Guid id) => _items.TryRemove(id, out _);
}