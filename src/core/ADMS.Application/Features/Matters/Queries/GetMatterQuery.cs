using ADMS.Application.Common.Interfaces;
using ADMS.Application.DTOs;
using ADMS.Domain.ValueObjects;
using ADMS.Shared.Common;
using Mapster;
using MediatR;

namespace ADMS.Application.Features.Matters.Queries;

/// <summary>
/// Query to retrieve a matter by its identifier.
/// </summary>
public record GetMatterQuery(Guid MatterId, bool IncludeDocuments = false) : IRequest<Result<MatterDto>>;

public class GetMatterQueryHandler(
    IMatterRepository matterRepository) : IRequestHandler<GetMatterQuery, Result<MatterDto>>
{
    public async Task<Result<MatterDto>> Handle(GetMatterQuery request, CancellationToken cancellationToken)
    {
        var matterId = MatterId.From(request.MatterId);
        var matter = await matterRepository.GetByIdAsync(matterId, request.IncludeDocuments, cancellationToken);
        
        if (matter == null)
            return Result.Failure<MatterDto>(DomainError.Create(
                "MATTER_NOT_FOUND", 
                $"Matter with ID {request.MatterId} was not found"));

        var dto = matter.Adapt<MatterDto>();
        return Result.Success(dto);
    }
}