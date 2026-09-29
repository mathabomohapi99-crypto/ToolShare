namespace ToolShare.Api;

// WHY an interface: services depend on the abstraction, so Week 5 can swap
// in EF Core without touching any service or controller.
public interface IRepository<T> where T : Entity
{
    IReadOnlyList<T> GetAll();
    T? GetById(Guid id);
    void Add(T entity);
    void Update(T entity);
    void Remove(Guid id);
}