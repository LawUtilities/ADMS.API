using ADMS.Application.Common.Interfaces;
using ADMS.Domain.Entities;
using ADMS.Shared.Common;
using MediatR;

namespace ADMS.Application.Features.Matters.Commands;

/// <summary>
/// Command to create a new matter in the system.
/// </summary>
/// <param name="Description">The matter description</param>
/// <param name="CreatedBy">User creating the matter</param>
public record CreateMatterCommand(string Description, string CreatedBy) : IRequest<Result<Guid>>;

/// <summary>
/// Handler for creating new matters with comprehensive validation and business rule enforcement.
/// </summary>
public class CreateMatterCommandHandler(
    IMatterRepository matterRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateMatterCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMatterCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate matter descriptions
        if (await matterRepository.ExistsByDescriptionAsync(request.Description, cancellationToken))
        {
            return Result.Failure<Guid>(DomainError.Create(
                "MATTER_DESCRIPTION_DUPLICATE",
                $"A matter with description '{request.Description}' already exists"));
        }

        // Create matter using domain factory method
        var matterResult = Matter.Create(request.Description, request.CreatedBy);
        if (matterResult.IsFailure)
            return Result.Failure<Guid>(matterResult.Error);

        // Persist the matter
        await matterRepository.AddAsync(matterResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(matterResult.Value.Id.Value);
    }
}