using ADMS.Application.Features.Matters.Commands;
using ADMS.Application.Features.Matters.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ADMS.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/matters")]
public class MattersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a new matter with comprehensive validation and business rule enforcement.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateMatter([FromBody] CreateMatterCommand command)
    {
        var result = await mediator.Send(command);
        
        return result.IsSuccess 
            ? CreatedAtRoute(nameof(GetMatter), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    /// <summary>
    /// Retrieves a matter by its identifier with optional document inclusion.
    /// </summary>
    [HttpGet("{id:guid}", Name = nameof(GetMatter))]
    public async Task<ActionResult<MatterDto>> GetMatter(Guid id, [FromQuery] bool includeDocuments = false)
    {
        var query = new GetMatterQuery(id, includeDocuments);
        var result = await mediator.Send(query);
        
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}