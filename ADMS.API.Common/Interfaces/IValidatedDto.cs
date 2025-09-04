using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Common.Interfaces;

/// <summary>
/// Marker interface for DTOs that implement standardized ADMS validation patterns.
/// </summary>
/// <remarks>
/// This interface identifies DTOs that follow the standardized BaseValidationDto pattern,
/// enabling validation framework operations and ensuring consistent validation behavior.
/// </remarks>
public interface IValidatedDto : IValidatableObject
{
    /// <summary>
    /// Gets a value indicating whether the DTO has valid data for operations.
    /// </summary>
    bool IsValid { get; }
}