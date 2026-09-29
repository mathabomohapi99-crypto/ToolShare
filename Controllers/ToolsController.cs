using Microsoft.AspNetCore.Mvc;

namespace ToolShare.Api;

[ApiController]
[Route("api/tools")]
public class ToolsController(IRepository<Tool> tools) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<ToolResponse>> GetAll()
        => Ok(tools.GetAll().Select(t => t.ToResponse()));

    [HttpGet("{id:guid}")]
    public ActionResult<ToolResponse> GetById(Guid id)
    {
      
        var tool = tools.GetById(id) ?? throw new NotFoundException($"Tool {id} was not found.");
        return Ok(tool.ToResponse());
    }
}