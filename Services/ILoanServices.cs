namespace ToolShare.Api;

public interface ILoanService
{
    IReadOnlyList<LoanResponse> GetAll();
    LoanResponse GetById(Guid id);

    // WHY the extra parameter: the service owns the idempotency decision,
    // so the controller just passes the header value through.
    LoanResponse CheckOut(CreateLoanRequest request, string? idempotencyKey);

    void Return(Guid id);
}