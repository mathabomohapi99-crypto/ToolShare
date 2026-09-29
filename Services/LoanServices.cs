using System.Text.Json;

namespace ToolShare.Api;

public sealed class LoanService(
    IRepository<Loan> loans,
    IRepository<Tool> tools,
    IRepository<Member> members,
    IdempotencyStore idempotency) : ILoanService
{
    private readonly object _gate = new();

    public IReadOnlyList<LoanResponse> GetAll()
        => loans.GetAll().Select(l => l.ToResponse()).ToList();

    public LoanResponse GetById(Guid id)
    {
        var loan = loans.GetById(id) ?? throw new NotFoundException($"Loan {id} was not found.");
        return loan.ToResponse();
    }

    public LoanResponse CheckOut(CreateLoanRequest request, string? idempotencyKey)
    {
        lock (_gate)
        {
            string? fingerprint = null;
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                fingerprint = JsonSerializer.Serialize(request);

                if (idempotency.TryGet(idempotencyKey, out var saved))
                {
                    if (saved!.Fingerprint != fingerprint)
                        throw new IdempotencyKeyReuseException(
                            "This Idempotency-Key was already used with a different request.");

                    return saved.Response;
                }
            }

            var tool = tools.GetById(request.ToolId)
                ?? throw new NotFoundException($"Tool {request.ToolId} was not found.");
            var borrower = members.GetById(request.BorrowerId)
                ?? throw new NotFoundException($"Member {request.BorrowerId} was not found.");

            var alreadyOut = loans.GetAll()
                .Any(l => l.ToolId == tool.Id && l.Status == LoanStatus.CheckedOut);
            if (alreadyOut)
                throw new ConflictException($"Tool '{tool.Name}' is already checked out.");

            var loan = Loan.CheckOut(tool.Id, borrower.Id, request.DueDate, DateTimeOffset.UtcNow);
            loans.Add(loan);
            var response = loan.ToResponse();

            if (fingerprint is not null)
                idempotency.Save(idempotencyKey!, new IdempotencyEntry(fingerprint, response));

            return response;
        }
    }

    public void Return(Guid id)
    {
        lock (_gate)
        {
            var loan = loans.GetById(id) ?? throw new NotFoundException($"Loan {id} was not found.");
            loan.Return(DateTimeOffset.UtcNow);
            loans.Update(loan);
        }
    }
}