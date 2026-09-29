namespace ToolShare.Api;

public sealed class Tool : Entity
{
    public string Name { get; private set; }
    public string Category { get; private set; }
    public Guid OwnerId { get; }

    // WHY: same reason as Member. Invalid tools can't be constructed.
    public Tool(string name, string category, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tool name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Tool category is required.", nameof(category));

        Name = name.Trim();
        Category = category.Trim();
        OwnerId = ownerId;
    }
}