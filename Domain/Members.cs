namespace ToolShare.Api;

public sealed class Member : Entity
{
    public string Name { get; private set; }

    // WHY: constructor guards the invariant "a member always has a name"
    // so an invalid Member can never exist (not an anemic property bag).
    public Member(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Member name is required.", nameof(name));
        Name = name.Trim();
    }
}