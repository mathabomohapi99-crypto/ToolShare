using Microsoft.AspNetCore.Mvc;

namespace ToolShare.Api;

[ApiController]
[Route("api/members")]
public class MembersController(IRepository<Member> members) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<MemberResponse>> GetAll()
        => Ok(members.GetAll().Select(m => m.ToResponse()));

    [HttpGet("{id:guid}")]
    public ActionResult<MemberResponse> GetById(Guid id)
    {
        var member = members.GetById(id) ?? throw new NotFoundException($"Member {id} was not found.");
        return Ok(member.ToResponse());
    }
}