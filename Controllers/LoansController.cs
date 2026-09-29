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
       
        await validator.ValidateAndThrowAsync(request, ct);

       
        var loan = service.CheckOut(request, idempotencyKey);

        // 201 + Location header pointing at the new loan.
        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }

 
    [HttpPost("{id:guid}/return")]
    public IActionResult Return(Guid id)
    {
        service.Return(id);
        return NoContent(); // 204
    }
}
