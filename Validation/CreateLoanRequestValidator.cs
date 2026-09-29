using FluentValidation;

namespace ToolShare.Api;

public class CreateLoanRequestValidator : AbstractValidator<CreateLoanRequest>
{
    public CreateLoanRequestValidator()
    {
        
        RuleFor(x => x.ToolId).NotEmpty().WithMessage("ToolId is required.");
        RuleFor(x => x.BorrowerId).NotEmpty().WithMessage("BorrowerId is required.");

      
        RuleFor(x => x.DueDate)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("DueDate cannot be in the past.");
    }
}