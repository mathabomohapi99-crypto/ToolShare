namespace ToolShare.Api;

public interface ILoanService
{
    IReadOnlyList<LoanResponse> GetAll();
    LoanResponse GetById(Guid id);

  
    LoanResponse CheckOut(CreateLoanRequest request, string? idempotencyKey);

    void Return(Guid id);
}