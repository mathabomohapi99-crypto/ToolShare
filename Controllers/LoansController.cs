using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ToolShare.Api;

[ApiController]
[Route("api/loans")]
public class LoansController(ILoanService service, IValidator<CreateLoanRequest> validator) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<LoanResponse>> GetAll() => Ok(service.GetAll());

    [HttpGet("{id:guid}")]
    public ActionResult<LoanResponse> GetById(Guid id) => Ok(service.GetById(id));

    /// <summary>Checks a tool out to a member.</summary>
    /// <param name="request">Tool, borrower and due date.</param>
    /// <param name="idempotencyKey">Optional. Repeating the same key with the same body returns the original result.</param>
    /// <response code="201">Loan created; Location header points to it.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Tool or member not found.</response>
    /// <response code="409">Tool is already checked out.</response>
    /// <response code="422">Idempotency-Key reused with a different request.</response>
    [HttpPost]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<LoanResponse>> CheckOut(
        [FromBody] CreateLoanRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        // WHY here: validation = "is it shaped correctly?" and it runs BEFORE
        // the service. It throws, and the central handler makes it a 400.
        await validator.ValidateAndThrowAsync(request, ct);

        // WHY no if-statement: the controller only translates HTTP <-> service.
        var loan = service.CheckOut(request, idempotencyKey);

        // 201 + Location header pointing at the new loan.
        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }

    // WHY no PUT: a loan has exactly one legal change (checked out -> returned),
    // so a named action is clearer than a generic "replace the whole loan".
    [HttpPost("{id:guid}/return")]
    public IActionResult Return(Guid id)
    {
        service.Return(id);
        return NoContent(); // 204
    }
}