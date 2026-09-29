namespace ToolShare.Api;

// WHY: every entity needs an Id, and the generic repository needs a
// common type to constrain on. One base class avoids repeating it.
public abstract class Entity
{
    public Guid Id { get; } = Guid.NewGuid();
}