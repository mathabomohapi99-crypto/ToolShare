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

    // WHY private: the only way to make a Loan is CheckOut(), so a loan
    // always starts life in the CheckedOut state. State can't be faked.
    private Loan(Guid toolId, Guid borrowerId, DateOnly dueDate, DateTimeOffset now)
    {
        ToolId = toolId;
        BorrowerId = borrowerId;
        DueDate = dueDate;
        CheckedOutAt = now;
        Status = LoanStatus.CheckedOut;
    }

    // WHY a factory method: it names the business action, not just "new".
    public static Loan CheckOut(Guid toolId, Guid borrowerId, DateOnly dueDate, DateTimeOffset now)
        => new(toolId, borrowerId, dueDate, now);

    // WHY behavior on the entity: the loan owns its own state transitions.
    // Returning an already-returned loan is a state conflict (409).
    public void Return(DateTimeOffset now)
    {
        if (Status == LoanStatus.Returned)
            throw new ConflictException($"Loan {Id} has already been returned.");

        Status = LoanStatus.Returned;
        ReturnedAt = now;
    }

    public bool IsOverdue(DateOnly today) => Status == LoanStatus.CheckedOut && today > DueDate;
}