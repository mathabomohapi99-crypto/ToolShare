using FluentValidation;

namespace ToolShare.Api;

public class CreateLoanRequestValidator : AbstractValidator<CreateLoanRequest>
{
    public CreateLoanRequestValidator()
    {
        // WHY NotEmpty on a Guid: it rejects Guid.Empty (the default when the
        // field is missing from the JSON).
        RuleFor(x => x.ToolId).NotEmpty().WithMessage("ToolId is required.");
        RuleFor(x => x.BorrowerId).NotEmpty().WithMessage("BorrowerId is required.");

        // WHY: due date in the past makes no sense. A missing date defaults to
        // 0001-01-01, which also fails this rule, so that's covered too.
        RuleFor(x => x.DueDate)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("DueDate cannot be in the past.");
    }
}