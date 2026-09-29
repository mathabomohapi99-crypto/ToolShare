using System.Collections.Concurrent;

namespace ToolShare.Api;

public sealed class InMemoryRepository<T> : IRepository<T> where T : Entity
{

    private readonly ConcurrentDictionary<Guid, T> _items = new();

    public IReadOnlyList<T> GetAll() => _items.Values.ToList();

    
    public T? GetById(Guid id) => _items.GetValueOrDefault(id);

    public void Add(T entity) => _items[entity.Id] = entity;

    
    public void Update(T entity) => _items[entity.Id] = entity;

    public void Remove(Guid id) => _items.TryRemove(id, out _);
}