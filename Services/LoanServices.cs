using System.Text.Json;

namespace ToolShare.Api;

public sealed class LoanService(
    IRepository<Loan> loans,
    IRepository<Tool> tools,
    IRepository<Member> members,
    IdempotencyStore idempotency) : ILoanService
{
    // WHY a lock: "check if free, then create loan" is two steps. Without a
    // lock two simultaneous requests could both pass the check and both
    // check the tool out. The lock makes the pair atomic.
    private readonly object _gate = new();

    public IReadOnlyList<LoanResponse> GetAll()
        => loans.GetAll().Select(l => l.ToResponse()).ToList();

    public LoanResponse GetById(Guid id)
    {
        // WHY throw: the central handler turns this into a 404 problem+json.
        var loan = loans.GetById(id) ?? throw new NotFoundException($"Loan {id} was not found.");
        return loan.ToResponse();
    }

    public LoanResponse CheckOut(CreateLoanRequest request, string? idempotencyKey)
    {
        lock (_gate)
        {
            // ---- Idempotency check FIRST ----
            // WHY first: a replay of a successful checkout must NOT reach the
            // "already checked out" rule, or it would wrongly return 409.
            string? fingerprint = null;
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                // WHY serialize: a simple, reliable way to compare two payloads.
                fingerprint = JsonSerializer.Serialize(request);

                if (idempotency.TryGet(idempotencyKey, out var saved))
                {
                    // Same key + different payload = reject (the case people forget!).
                    if (saved!.Fingerprint != fingerprint)
                        throw new IdempotencyKeyReuseException(
                            "This Idempotency-Key was already used with a different request.");

                    // Same key + same payload = return the ORIGINAL response, do nothing new.
                    return saved.Response;
                }
            }

            // Existence checks: you can't borrow a ghost tool or be a ghost member.
            var tool = tools.GetById(request.ToolId)
                ?? throw new NotFoundException($"Tool {request.ToolId} was not found.");
            var borrower = members.GetById(request.BorrowerId)
                ?? throw new NotFoundException($"Member {request.BorrowerId} was not found.");

            // ===== THE ONE RULE THAT MATTERS =====
            // A perfectly valid request is still refused if the tool already
            // has an active loan. This is a business decision, not a shape
            // check, so it lives here in the service, never in a controller.
            var alreadyOut = loans.GetAll()
                .Any(l => l.ToolId == tool.Id && l.Status == LoanStatus.CheckedOut);
            if (alreadyOut)
                throw new ConflictException($"Tool '{tool.Name}' is already checked out.");
            // =====================================

            var loan = Loan.CheckOut(tool.Id, borrower.Id, request.DueDate, DateTimeOffset.UtcNow);
            loans.Add(loan);
            var response = loan.ToResponse();

            // WHY save only on success: failed attempts must stay retryable.
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
            loan.Return(DateTimeOffset.UtcNow); // entity enforces its own rule (409 if already returned)
            loans.Update(loan);
        }
    }
}