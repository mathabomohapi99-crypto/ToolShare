namespace ToolShare.Api;

// WHY hand-written mapping: the rules say no mapping library.
public static class Mappings
{
    public static MemberResponse ToResponse(this Member m) => new(m.Id, m.Name);

    public static ToolResponse ToResponse(this Tool t) => new(t.Id, t.Name, t.Category, t.OwnerId);

    public static LoanResponse ToResponse(this Loan l) => new(
        l.Id, l.ToolId, l.BorrowerId, l.CheckedOutAt, l.DueDate,
        l.ReturnedAt, l.Status.ToString());
}