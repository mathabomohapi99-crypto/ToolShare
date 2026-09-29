namespace ToolShare.Api;


public abstract class Entity
{
    public Guid Id { get; } = Guid.NewGuid();
}