using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ADMS.API.Common;

/// <summary>
/// Provides comprehensive helper methods for validating document-related data within the ADMS system.
/// </summary>
/// <remarks>
/// This static helper class provides robust document validation functionality for the ADMS legal 
/// document management system, supporting all document-related DTOs and ensuring data integrity,
/// business rule compliance, and consistent validation logic across the application.
/// </remarks>
public static class DocumentValidationHelper
{
    #region Constants

    /// <summary>
    /// The minimum allowed date for document operations.
    /// </summary>
    public static readonly DateTime MinAllowedDocumentDate = new(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The tolerance in minutes for future dates in document-related operations.
    /// </summary>
    public const int FutureDateToleranceMinutes = 1;

    #endregion Constants

    #region Core Validation Methods

    /// <summary>
    /// Determines whether the specified document ID is valid.
    /// </summary>
    /// <param name="documentId">The document ID to validate.</param>
    /// <returns>true if the document ID is not empty; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidDocumentId(Guid documentId) => documentId != Guid.Empty;

    /// <summary>
    /// Determines whether the specified date is valid for document-related operations.
    /// </summary>
    /// <param name="date">The date to validate.</param>
    /// <returns>true if the date is valid; otherwise, false.</returns>
    public static bool IsValidDocumentDate(DateTime date)
    {
        var utcDate = date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(date, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(date, DateTimeKind.Utc)
        };

        return utcDate > DateTime.MinValue &&
               utcDate >= MinAllowedDocumentDate &&
               utcDate <= DateTime.UtcNow.AddMinutes(FutureDateToleranceMinutes);
    }

    #endregion Core Validation Methods

    #region Comprehensive Validation Methods

    /// <summary>
    /// Performs comprehensive validation of a document ID.
    /// </summary>
    /// <param name="documentId">The document ID to validate.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <returns>A collection of validation results.</returns>
    public static IEnumerable<ValidationResult> ValidateDocumentId(
        Guid documentId,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (documentId == Guid.Empty)
        {
            yield return new ValidationResult(
                $"{propertyName} must be a valid non-empty GUID.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Performs comprehensive validation of a document date.
    /// </summary>
    /// <param name="date">The date to validate.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <returns>A collection of validation results.</returns>
    public static IEnumerable<ValidationResult> ValidateDocumentDate(
        DateTime date,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (!IsValidDocumentDate(date))
        {
            if (date <= DateTime.MinValue)
            {
                yield return new ValidationResult(
                    $"{propertyName} must be a valid date.",
                    [propertyName]);
            }
            else if (date < MinAllowedDocumentDate)
            {
                yield return new ValidationResult(
                    $"{propertyName} cannot be earlier than {MinAllowedDocumentDate:yyyy-MM-dd}.",
                    [propertyName]);
            }
            else if (date > DateTime.UtcNow.AddMinutes(FutureDateToleranceMinutes))
            {
                yield return new ValidationResult(
                    $"{propertyName} cannot be in the future.",
                    [propertyName]);
            }
        }
    }

    #endregion Comprehensive Validation Methods
}