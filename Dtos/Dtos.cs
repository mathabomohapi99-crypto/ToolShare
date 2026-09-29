namespace ToolShare.Api;

// WHY DTOs: the HTTP contract is separate from the domain model, so
// changing one doesn't accidentally break the other.
public record MemberResponse(Guid Id, string Name);
public record ToolResponse(Guid Id, string Name, string Category, Guid OwnerId);

public record LoanResponse(
    Guid Id, Guid ToolId, Guid BorrowerId,
    DateTimeOffset CheckedOutAt, DateOnly DueDate,
    DateTimeOffset? ReturnedAt, string Status);

// WHY a record: value equality. Two identical requests compare as equal,
// which is handy for the idempotency check later.
public record CreateLoanRequest(Guid ToolId, Guid BorrowerId, DateOnly DueDate);