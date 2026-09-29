namespace ToolShare.Api;

public enum LoanStatus { CheckedOut, Returned }

public sealed class Loan : Entity
{
    public Guid ToolId { get; }
    public Guid BorrowerId { get; }
    public DateTimeOffset CheckedOutAt { get; }
    public DateOnly DueDate { get; }
    public DateTimeOffset? ReturnedAt { get; private set; }
    public LoanStatus Status { get; private set; }

   
    private Loan(Guid toolId, Guid borrowerId, DateOnly dueDate, DateTimeOffset now)
    {
        ToolId = toolId;
        BorrowerId = borrowerId;
        DueDate = dueDate;
        CheckedOutAt = now;
        Status = LoanStatus.CheckedOut;
    }

    // Factory method: it names the business action, not just "new".
    public static Loan CheckOut(Guid toolId, Guid borrowerId, DateOnly dueDate, DateTimeOffset now)
        => new(toolId, borrowerId, dueDate, now);

    public void Return(DateTimeOffset now)
    {
        if (Status == LoanStatus.Returned)
            throw new ConflictException($"Loan {Id} has already been returned.");

        Status = LoanStatus.Returned;
        ReturnedAt = now;
    }

    public bool IsOverdue(DateOnly today) => Status == LoanStatus.CheckedOut && today > DueDate;
}