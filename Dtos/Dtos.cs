namespace ToolShare.Api;

public record MemberResponse(Guid Id, string Name);
public record ToolResponse(Guid Id, string Name, string Category, Guid OwnerId);

public record LoanResponse(
    Guid Id, Guid ToolId, Guid BorrowerId,
    DateTimeOffset CheckedOutAt, DateOnly DueDate,
    DateTimeOffset? ReturnedAt, string Status);

public record CreateLoanRequest(Guid ToolId, Guid BorrowerId, DateOnly DueDate);