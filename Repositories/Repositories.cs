namespace ToolShare.Api;


public interface IRepository<T> where T : Entity
{
    IReadOnlyList<T> GetAll();
    T? GetById(Guid id);
    void Add(T entity);
    void Update(T entity);
    void Remove(Guid id);
}