using ADMS.Application.Common;
using ADMS.Application.Constants;
using ADMS.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using DomainError = ADMS.Domain.Common.DomainError;
using Result = ADMS.Domain.Common.Result;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing a matter with complete document collection and activity relationships.
/// </summary>
/// <remarks>
/// Serves as the complete representation of a matter within the ADMS legal document management system,
/// providing all properties and relationships from the domain Matter entity including document collection 
/// and activity audit trails.
/// </remarks>
public record MatterDto : IValidatableObject
{
    #region Core Properties

    /// <summary>
    /// Gets the unique identifier for the matter.
    /// </summary>
    [Required(ErrorMessage = "Matter ID is required.")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the matter description.
    /// </summary>
    [Required(ErrorMessage = "Matter description is required.")]
    [StringLength(
        MatterConstants.DescriptionMaxLength,
        MinimumLength = MatterConstants.DescriptionMinLength,
        ErrorMessage = "Matter description must be between 3 and 128 characters.")]
    public required string Description { get; init; }

    /// <summary>
    /// Gets a value indicating whether the matter is archived.
    /// </summary>
    public bool IsArchived { get; init; }

    /// <summary>
    /// Gets a value indicating whether the matter is deleted.
    /// </summary>
    public bool IsDeleted { get; init; }

    /// <summary>
    /// Gets the UTC creation date of the matter.
    /// </summary>
    [Required(ErrorMessage = "Creation date is required.")]
    public required DateTime CreationDate { get; init; }

    #endregion

    #region Navigation Collections

    /// <summary>
    /// Gets the collection of documents associated with this matter.
    /// </summary>
    public ICollection<DocumentWithoutRevisionsDto> Documents { get; init; } = [];

    /// <summary>
    /// Gets the collection of matter activity users associated with this matter.
    /// </summary>
    public ICollection<MatterActivityUserDto> MatterActivityUsers { get; init; } = [];

    /// <summary>
    /// Gets the collection of "from" matter document activity users.
    /// </summary>
    public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; init; } = [];

    /// <summary>
    /// Gets the collection of "to" matter document activity users.
    /// </summary>
    public ICollection<MatterDocumentActivityUserToDto> MatterDocumentActivityUsersTo { get; init; } = [];

    #endregion

    #region Computed Properties - Optimized for Performance

    /// <summary>
    /// Gets the normalized description for consistent comparison.
    /// </summary>
    [NotMapped]
    public string NormalizedDescription =>
        _normalizedDescription ??= MatterValidationHelper.NormalizeDescription(Description) ?? string.Empty;
    private string? _normalizedDescription;

    /// <summary>
    /// Gets the creation date formatted as a localized string.
    /// </summary>
    [NotMapped]
    public string LocalCreationDateString =>
        _localCreationDateString ??= CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");
    private string? _localCreationDateString;

    /// <summary>
    /// Gets the current status of the matter.
    /// </summary>
    [NotMapped]
    public string Status => _status ??= ComputeStatus();
    private string? _status;

    /// <summary>
    /// Gets the age of this matter in days since creation.
    /// </summary>
    [NotMapped]
    public double AgeDays => (DateTime.UtcNow - CreationDate).TotalDays;

    /// <summary>
    /// Gets a value indicating whether this matter is currently active.
    /// </summary>
    [NotMapped]
    public bool IsActive => !IsArchived && !IsDeleted;

    /// <summary>
    /// Gets a value indicating whether this matter contains any documents.
    /// </summary>
    [NotMapped]
    public bool HasDocuments => DocumentCount > 0;

    /// <summary>
    /// Gets the total number of documents in this matter.
    /// </summary>
    [NotMapped]
    public int DocumentCount => _documentCount ??= Documents.Count;
    private int? _documentCount;

    /// <summary>
    /// Gets the number of active (non-deleted) documents.
    /// </summary>
    [NotMapped]
    public int ActiveDocumentCount => _activeDocumentCount ??= Documents.Count(d => !d.IsDeleted);
    private int? _activeDocumentCount;

    /// <summary>
    /// Gets the number of documents that are currently checked out.
    /// </summary>
    [NotMapped]
    public int CheckedOutDocumentCount => _checkedOutDocumentCount ??= Documents.Count(d => d.IsCheckedOut);
    private int? _checkedOutDocumentCount;

    /// <summary>
    /// Gets the total file size of all documents.
    /// </summary>
    [NotMapped]
    public long TotalDocumentSize => _totalDocumentSize ??= Documents.Sum(d => d.FileSize);
    private long? _totalDocumentSize;

    /// <summary>
    /// Gets the total count of all activities.
    /// </summary>
    [NotMapped]
    public int TotalActivityCount => _totalActivityCount ??= ComputeTotalActivityCount();
    private int? _totalActivityCount;

    /// <summary>
    /// Gets a value indicating whether this matter has any activity history.
    /// </summary>
    [NotMapped]
    public bool HasActivityHistory => MatterActivityUsers.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this DTO has valid data.
    /// </summary>
    [NotMapped]
    public bool IsValid => _isValid ??= ComputeIsValid();
    private bool? _isValid;

    /// <summary>
    /// Gets the display text suitable for UI controls.
    /// </summary>
    [NotMapped]
    public string DisplayText => Description;

    #endregion

    #region Private Helper Methods

    private string ComputeStatus()
    {
        return (IsDeleted, IsArchived) switch
        {
            (true, true) => "Archived and Deleted",
            (true, false) => "Deleted",
            (false, true) => "Archived",
            _ => "Active"
        };
    }

    private int ComputeTotalActivityCount()
    {
        return MatterActivityUsers.Count +
               MatterDocumentActivityUsersFrom.Count +
               MatterDocumentActivityUsersTo.Count;
    }

    private bool ComputeIsValid()
    {
        return Id != Guid.Empty &&
               MatterValidationHelper.IsValidDescription(Description) &&
               MatterValidationHelper.IsValidDate(CreationDate) &&
               MatterValidationHelper.IsValidArchiveState(IsArchived, IsDeleted);
    }

    #endregion

    #region Validation Implementation

    /// <summary>
    /// Validates the MatterDto for data integrity and business rules compliance.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate matter ID
        if (Id == Guid.Empty)
            yield return new ValidationResult("Matter ID cannot be empty.", [nameof(Id)]);

        // Validate description using centralized helper
        foreach (var result in MatterValidationHelper.ValidateDescription(Description, nameof(Description)))
            yield return result;

        // Validate creation date
        foreach (var result in MatterValidationHelper.ValidateDate(CreationDate, nameof(CreationDate)))
            yield return result;

        // Validate state consistency
        foreach (var result in MatterValidationHelper.ValidateStates(IsArchived, IsDeleted,
            nameof(IsArchived), nameof(IsDeleted)))
            yield return result;

        // Validate collections using centralized helper
        foreach (var result in DtoValidationHelper.ValidateCollection(Documents, nameof(Documents)))
            yield return result;

        foreach (var result in DtoValidationHelper.ValidateCollection(MatterActivityUsers, nameof(MatterActivityUsers)))
            yield return result;
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Validates a MatterDto instance and returns detailed validation results.
    /// </summary>
    public static IList<ValidationResult> ValidateModel(MatterDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
        return results;
    }

    /// <summary>
    /// Creates a MatterDto from a Domain Matter entity with validation.
    /// </summary>
    public static Domain.Common.Result<MatterDto> FromEntity(Matter matter, bool includeActivities = false)
    {
        try
        {
            var dto = new MatterDto
            {
                Id = matter.Id,
                Description = matter.Description,
                IsArchived = matter.IsArchived,
                IsDeleted = matter.IsDeleted,
                CreationDate = matter.CreationDate
                // Collections would be mapped using AutoMapper in practice
            };

            var validationResults = ValidateModel(dto);
            if (validationResults.Count <= 0) return Result.Success(dto);
            var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
            return Result.Failure<MatterDto>(DomainError.Create(
                "MATTER_DTO_VALIDATION_FAILED",
                $"Failed to create valid MatterDto: {errorMessages}"));

        }
        catch (Exception ex)
        {
            return Result.Failure<MatterDto>(DomainError.Create(
                "MATTER_DTO_CREATION_FAILED",
                $"Failed to create MatterDto: {ex.Message}"));
        }
    }

    #endregion

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this matter can be archived based on business rules.
    /// </summary>
    public bool CanBeArchived() => !IsArchived && !IsDeleted;

    /// <summary>
    /// Determines whether this matter can be restored from deleted state.
    /// </summary>
    public bool CanBeRestored() => IsDeleted;

    /// <summary>
    /// Determines whether this matter can be safely deleted.
    /// </summary>
    public bool CanBeDeleted() => !IsDeleted && Documents.All(d => d.IsDeleted && !d.IsCheckedOut);

    /// <summary>
    /// Gets activities of a specific type performed on this matter.
    /// </summary>
    public IEnumerable<MatterActivityUserDto> GetActivitiesByType(string activityType)
    {
        if (string.IsNullOrWhiteSpace(activityType))
            yield break;

        foreach (var activity in MatterActivityUsers
            .Where(a => string.Equals(a.MatterActivity?.Activity, activityType, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(a => a.CreatedAt))
        {
            yield return activity;
        }
    }

    /// <summary>
    /// Gets the most recent activity performed on this matter.
    /// </summary>
    public MatterActivityUserDto? GetMostRecentActivity() =>
        MatterActivityUsers.OrderByDescending(a => a.CreatedAt).FirstOrDefault();

    /// <summary>
    /// Gets comprehensive document statistics for this matter.
    /// </summary>
    public DocumentStatistics GetDocumentStatistics()
    {
        var documentsByType = Documents
            .Where(d => !d.IsDeleted)
            .GroupBy(d => d.Extension)
            .ToDictionary(g => g.Key, g => g.Count());

        return new DocumentStatistics
        {
            TotalDocuments = DocumentCount,
            ActiveDocuments = ActiveDocumentCount,
            DeletedDocuments = DocumentCount - ActiveDocumentCount,
            CheckedOutDocuments = CheckedOutDocumentCount,
            TotalSizeBytes = TotalDocumentSize,
            TotalSizeMb = TotalDocumentSize / (1024.0 * 1024.0),
            DocumentsByType = documentsByType,
            AverageFileSize = Documents.Any() ? Documents.Average(d => d.FileSize) : 0,
            LargestDocument = Documents.Any() ? Documents.Max(d => d.FileSize) : 0,
            HasDocuments = HasDocuments
        };
    }

    #endregion

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified <see cref="MatterDto"/> is equal to the current instance.
    /// </summary>
    /// <remarks>Two instances are considered equal if their <see cref="Id"/> properties are equal and not
    /// empty.</remarks>
    /// <param name="other">The <see cref="MatterDto"/> to compare with the current instance, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the specified <see cref="MatterDto"/> is equal to the current instance; otherwise,
    /// <see langword="false"/>.</returns>
    public virtual bool Equals(MatterDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Returns a hash code for the current object.
    /// </summary>
    /// <remarks>The hash code is derived from the <see cref="Id"/> property.  This ensures that objects with
    /// the same <see cref="Id"/> produce the same hash code.</remarks>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the matter, including its description, ID, status, and document count.
    /// </summary>
    /// <returns>A string that contains the description, ID, status, and document count of the matter in the format: "Matter:
    /// {Description} ({Id}) - {Status} ({DocumentCount} documents)".</returns>
    public override string ToString() =>
        $"Matter: {Description} ({Id}) - {Status} ({DocumentCount} documents)";

    #endregion
}

/// <summary>
/// Represents statistical information about a collection of documents, including counts, sizes, and other metrics.
/// </summary>
/// <remarks>This type provides a snapshot of various document-related statistics, such as the total number of
/// documents, their sizes, and their distribution by type. It is designed to be immutable and is typically used for
/// reporting or analysis purposes.</remarks>
public sealed record DocumentStatistics
{
    /// <summary>
    /// Gets the total number of documents.
    /// </summary>
    public int TotalDocuments { get; init; }

    /// <summary>
    /// Gets the number of currently active documents.
    /// </summary>
    public int ActiveDocuments { get; init; }

    /// <summary>
    /// Gets the number of documents that have been marked as deleted.
    /// </summary>
    public int DeletedDocuments { get; init; }

    /// <summary>
    /// Gets the number of documents currently checked out.
    /// </summary>
    public int CheckedOutDocuments { get; init; }

    /// <summary>
    /// Gets the total size, in bytes, of the associated data or resource.
    /// </summary>
    public long TotalSizeBytes { get; init; }

    /// <summary>
    /// Gets the total size, in megabytes, of the associated resource.
    /// </summary>
    public double TotalSizeMb { get; init; }

    /// <summary>
    /// Gets a dictionary mapping document types (e.g., file extensions) to their respective counts.
    /// </summary>
    public Dictionary<string, int> DocumentsByType { get; init; } = [];

    /// <summary>
    /// Gets the average size of files, in bytes, calculated from the associated data set.
    /// </summary>
    public double AverageFileSize { get; init; }

    /// <summary>
    /// Gets the size, in bytes, of the largest document processed.
    /// </summary>
    public long LargestDocument { get; init; }

    /// <summary>
    /// Gets a value indicating whether the current instance contains any documents.
    /// </summary>
    public bool HasDocuments { get; init; }
}

/// <summary>
/// Provides utility methods for validating data and generating validation results.
/// </summary>
/// <remarks>This class includes methods for common validation scenarios, such as checking required GUIDs and
/// validating string lengths. Each method returns validation results that can be used to indicate errors in the input
/// data. The methods are designed to simplify validation logic in applications and ensure consistent error
/// reporting.</remarks>
public static class ValidationHelper
{
    /// <summary>
    /// Creates a validation result with multiple member names.
    /// </summary>
    public static ValidationResult CreateResult(string errorMessage, params string[] memberNames) =>
        new(errorMessage, memberNames);

    /// <summary>
    /// Validates a required GUID and returns appropriate validation results.
    /// </summary>
    public static IEnumerable<ValidationResult> ValidateRequiredGuid(Guid value, string propertyName)
    {
        if (value == Guid.Empty)
            yield return CreateResult($"{propertyName} is required and cannot be empty.", propertyName);
    }

    /// <summary>
    /// Validates a string length with custom error message.
    /// </summary>
    public static IEnumerable<ValidationResult> ValidateStringLength(
        string? value,
        string propertyName,
        int minLength,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield return CreateResult($"{propertyName} is required.", propertyName);
            yield break;
        }

        if (value.Length < minLength || value.Length > maxLength)
        {
            yield return CreateResult(
                $"{propertyName} must be between {minLength} and {maxLength} characters.",
                propertyName);
        }
    }
}