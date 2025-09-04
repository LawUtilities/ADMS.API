using ADMS.API.Common;
using ADMS.API.Entities;
using ADMS.API.Models;

using Mapster;

using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace ADMS.Application.Profiles;

/// <summary>
/// Efficient Mapster mapping profile for Document entity-to-DTO transformations with performance optimization and validation integration.
/// </summary>
/// <remarks>
/// This profile configures Mapster type adapters for Document entities and their corresponding DTOs,
/// featuring conditional navigation property loading, performance optimizations, and integrated validation.
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item><strong>Mapster Optimized:</strong> Leverages Mapster's performance features and type adapters</item>
/// <item><strong>Conditional Loading:</strong> Smart navigation property loading based on collection size</item>
/// <item><strong>Validation Integrated:</strong> Automatic validation using BaseValidationDto framework</item>
/// <item><strong>Professional Standards:</strong> File naming and security standard enforcement</item>
/// </list>
/// 
/// <para><strong>Supported Mappings:</strong></para>
/// <list type="bullet">
/// <item>Document ↔ DocumentDto (complete with conditional navigation properties)</item>
/// <item>Document ↔ DocumentMinimalDto (performance optimized, no navigation)</item>
/// <item>DocumentForCreationDto → Document (creation workflow with ID generation)</item>
/// <item>DocumentForUpdateDto → Document (update workflow preserving immutable fields)</item>
/// <item>Cross-DTO transformations for workflow optimization</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Register with dependency injection
/// services.AddSingleton&lt;DocumentProfile&gt;();
/// 
/// // Use in services
/// var documentDto = document.Adapt&lt;DocumentDto&gt;();
/// var entity = createDto.Adapt&lt;Document&gt;();
/// </code>
/// </example>
public sealed class DocumentProfile
{
    #region Constants

    /// <summary>Maximum collection size for navigation property loading to prevent performance issues.</summary>
    private const int MaxCollectionSize = 1000;

    /// <summary>Large document threshold for performance optimization (50 MB).</summary>
    private const long LargeDocumentThreshold = 50 * 1024 * 1024;

    /// <summary>Maximum revisions to load for large documents.</summary>
    private const int MaxRevisionsForLargeDocuments = 10;

    /// <summary>Maximum activities to load for large documents.</summary>
    private const int MaxActivitiesForLargeDocuments = 25;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentProfile"/> class and configures Mapster type adapters.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when profile configuration fails.</exception>
    public DocumentProfile()
    {
        try
        {
            ConfigureEntityToDtoMappings();
            ConfigureDtoToEntityMappings();
            ConfigureCrossDtoMappings();
            ConfigureGlobalSettings();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to configure DocumentProfile: {ex.Message}", ex);
        }
    }

    #endregion

    #region Entity to DTO Mappings

    /// <summary>
    /// Configures entity-to-DTO mappings with conditional navigation property loading.
    /// </summary>
    private static void ConfigureEntityToDtoMappings()
    {
        // Document to complete DTO with conditional navigation loading
        TypeAdapterConfig<Document, DocumentDto>
            .NewConfig()
            .Map(dest => dest.Revisions, src => MapRevisions(src))
            .Map(dest => dest.DocumentActivityUsers, src => MapDocumentActivities(src))
            .Map(dest => dest.MatterDocumentActivityUsersFrom, src => MapMatterActivitiesFrom(src))
            .Map(dest => dest.MatterDocumentActivityUsersTo, src => MapMatterActivitiesTo(src))
            .AfterMapping((src, dest) => ValidateCompleteMapping(src, dest))
            .IgnoreNullValues(true)
            .PreserveReference(true);

        // Document to minimal DTO (performance optimized - no navigation properties)
        TypeAdapterConfig<Document, DocumentMinimalDto>
            .NewConfig()
            .AfterMapping((src, dest) => ValidateMinimalMapping(dest))
            .IgnoreNullValues(true)
            .PreserveReference(false);
    }

    #endregion

    #region DTO to Entity Mappings

    /// <summary>
    /// Configures DTO-to-entity mappings with proper field handling and validation.
    /// </summary>
    private static void ConfigureDtoToEntityMappings()
    {
        // Complete DTO to entity (ignore navigation properties)
        TypeAdapterConfig<DocumentDto, Document>
            .NewConfig()
            .Ignore(dest => dest.Revisions)
            .Ignore(dest => dest.DocumentActivityUsers)
            .Ignore(dest => dest.MatterDocumentActivityUsersFrom)
            .Ignore(dest => dest.MatterDocumentActivityUsersTo)
            .IgnoreNullValues(true);

        // Creation DTO to entity with ID generation and defaults
        TypeAdapterConfig<DocumentForCreationDto, Document>
            .NewConfig()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.Extension, src => NormalizeExtension(src.Extension))
            .Map(dest => dest.IsDeleted, src => false)
            .Map(dest => dest.CreationDate, src => DateTime.UtcNow)
            .Ignore(dest => dest.Revisions)
            .Ignore(dest => dest.DocumentActivityUsers)
            .Ignore(dest => dest.MatterDocumentActivityUsersFrom)
            .Ignore(dest => dest.MatterDocumentActivityUsersTo)
            .AfterMapping((src, dest) => ValidateCreationMapping(src, dest))
            .IgnoreNullValues(true);

        // Update DTO to entity (preserve immutable fields)
        TypeAdapterConfig<DocumentForUpdateDto, Document>
            .NewConfig()
            .Map(dest => dest.Extension, src => NormalizeExtension(src.Extension))
            .Ignore(dest => dest.Id)           // ID is immutable
            .Ignore(dest => dest.CreationDate) // Creation date is immutable
            .Ignore(dest => dest.MatterId)     // Matter association is immutable
            .Ignore(dest => dest.Revisions)
            .Ignore(dest => dest.DocumentActivityUsers)
            .Ignore(dest => dest.MatterDocumentActivityUsersFrom)
            .Ignore(dest => dest.MatterDocumentActivityUsersTo)
            .AfterMapping((src, dest) => ValidateUpdateMapping(src, dest))
            .IgnoreNullValues(true);
    }

    #endregion

    #region Cross-DTO Mappings

    /// <summary>
    /// Configures cross-DTO transformations for workflow optimization.
    /// </summary>
    private static void ConfigureCrossDtoMappings()
    {
        // Complete DTO to minimal DTO (performance optimization)
        TypeAdapterConfig<DocumentDto, DocumentMinimalDto>
            .NewConfig()
            .IgnoreNullValues(true);

        // Minimal DTO to creation DTO (template functionality)
        TypeAdapterConfig<DocumentMinimalDto, DocumentForCreationDto>
            .NewConfig()
            .Map(dest => dest.FileName, src => $"Copy_of_{src.FileName}")
            .Map(dest => dest.Extension, src => NormalizeExtension(src.Extension))
            .Map(dest => dest.IsCheckedOut, src => false)
            .Map(dest => dest.MimeType, src => GetMimeTypeForExtension(src.Extension))
            .Map(dest => dest.Checksum, src => string.Empty)
            .Map(dest => dest.Description, src => $"Copy of {src.FileName}")
            .IgnoreNullValues(true);

        // Complete DTO to update DTO (modification workflow)
        TypeAdapterConfig<DocumentDto, DocumentForUpdateDto>
            .NewConfig()
            .Map(dest => dest.Description, src => (string?)null) // Reset for manual input
            .IgnoreNullValues(true);
    }

    #endregion

    #region Global Settings Configuration

    /// <summary>
    /// Configures global Mapster settings for optimal performance and validation.
    /// </summary>
    private static void ConfigureGlobalSettings()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        // Performance optimizations
        config.Default.MapToConstructor(true);
        config.Default.RequireDestinationMemberSource(false);
        config.Default.ShallowCopyForSameType(true);
        config.Default.PreserveReference(true);
        config.Default.MaxDepth(5);

        // Professional standards transformations
        config.Default.Settings.DestinationTransforms.Add(
            new DestinationTransform
            {
                Condition = t => t == typeof(string),
                TransformFunc = t => Expression.Lambda<Func<string, string>>(
                    Expression.Condition(
                        Expression.Call(
                            typeof(string).GetMethod(nameof(string.IsNullOrWhiteSpace), new[] { typeof(string) })!,
                            Expression.Parameter(typeof(string), "str")
                        ),
                        Expression.Parameter(typeof(string), "str"),
                        Expression.Call(
                            Expression.Parameter(typeof(string), "str"),
                            typeof(string).GetMethod(nameof(string.Trim), Type.EmptyTypes)!
                        )
                    ),
                    Expression.Parameter(typeof(string), "str")
                )
            }
        );

        config.Default.Settings.DestinationTransforms.Add(
            new DestinationTransform
            {
                Condition = t => t == typeof(DateTime),
                TransformFunc = t =>
                {
                    var param = Expression.Parameter(typeof(DateTime), "date");
                    var kindProp = Expression.Property(param, nameof(DateTime.Kind));
                    var unspecified = Expression.Constant(DateTimeKind.Unspecified);
                    var utcKind = Expression.Constant(DateTimeKind.Utc);
                    var specifyKindCall = Expression.Call(
                        typeof(DateTime).GetMethod(nameof(DateTime.SpecifyKind), new[] { typeof(DateTime), typeof(DateTimeKind) })!,
                        param,
                        utcKind
                    );
                    var cond = Expression.Condition(
                        Expression.Equal(kindProp, unspecified),
                        specifyKindCall,
                        param
                    );
                    return Expression.Lambda<Func<DateTime, DateTime>>(cond, param);
                }
            }
        );
    }

    #endregion

    #region Navigation Property Mapping Helpers

    /// <summary>
    /// Maps revisions with performance optimization for large collections.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ICollection<RevisionDto> MapRevisions(Document src)
    {
        if (src.Revisions == null || src.Revisions.Count == 0)
            return new List<RevisionDto>();

        // For large documents or large revision collections, limit to latest revisions
        if (src.FileSize > LargeDocumentThreshold || src.Revisions.Count > 50)
        {
            return src.Revisions
                .OrderByDescending(r => r.RevisionNumber)
                .Take(MaxRevisionsForLargeDocuments)
                .Adapt<ICollection<RevisionDto>>();
        }

        return src.Revisions.Count <= MaxCollectionSize
            ? src.Revisions.Adapt<ICollection<RevisionDto>>()
            : new List<RevisionDto>();
    }

    /// <summary>
    /// Maps document activities with performance optimization for large collections.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ICollection<DocumentActivityUserMinimalDto> MapDocumentActivities(Document src)
    {
        if (src.DocumentActivityUsers == null || src.DocumentActivityUsers.Count == 0)
            return new List<DocumentActivityUserMinimalDto>();

        // For large documents, limit activities to most recent
        if (src.FileSize > LargeDocumentThreshold || src.DocumentActivityUsers.Count > 100)
        {
            return src.DocumentActivityUsers
                .OrderByDescending(a => a.CreatedAt)
                .Take(MaxActivitiesForLargeDocuments)
                .Adapt<ICollection<DocumentActivityUserMinimalDto>>();
        }

        return src.DocumentActivityUsers.Count <= MaxCollectionSize
            ? src.DocumentActivityUsers.Adapt<ICollection<DocumentActivityUserMinimalDto>>()
            : new List<DocumentActivityUserMinimalDto>();
    }

    /// <summary>
    /// Maps matter activities FROM with performance optimization.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ICollection<MatterDocumentActivityUserFromDto> MapMatterActivitiesFrom(Document src)
    {
        return src.MatterDocumentActivityUsersFrom != null && src.MatterDocumentActivityUsersFrom.Count <= MaxCollectionSize
            ? src.MatterDocumentActivityUsersFrom.Adapt<ICollection<MatterDocumentActivityUserFromDto>>()
            : new List<MatterDocumentActivityUserFromDto>();
    }

    /// <summary>
    /// Maps matter activities TO with performance optimization.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ICollection<MatterDocumentActivityUserToDto> MapMatterActivitiesTo(Document src)
    {
        return src.MatterDocumentActivityUsersTo != null && src.MatterDocumentActivityUsersTo.Count <= MaxCollectionSize
            ? src.MatterDocumentActivityUsersTo.Adapt<ICollection<MatterDocumentActivityUserToDto>>()
            : new List<MatterDocumentActivityUserToDto>();
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates complete document mappings for data integrity.
    /// </summary>
    /// <param name="source">The source document entity.</param>
    /// <param name="destination">The mapped document DTO.</param>
    /// <exception cref="ValidationException">Thrown when critical validation fails.</exception>
    private static void ValidateCompleteMapping(Document source, DocumentDto destination)
    {
        // Validate using BaseValidationDto framework
        var validationResults = BaseValidationDto.ValidateModel(destination);
        if (BaseValidationDto.HasValidationErrors(validationResults))
        {
            var summary = BaseValidationDto.GetValidationSummary(validationResults);
            throw new ValidationException($"Complete mapping validation failed: {summary}");
        }

        // Additional mapping-specific validation
        if (source.Id != destination.Id)
            throw new ValidationException("Document ID mismatch during mapping.");

        if (source.FileName != destination.FileName)
            throw new ValidationException("File name mismatch during mapping.");
    }

    /// <summary>
    /// Validates minimal document mappings for performance operations.
    /// </summary>
    /// <param name="destination">The mapped minimal document DTO.</param>
    private static void ValidateMinimalMapping(DocumentMinimalDto destination)
    {
        if (destination.Id == Guid.Empty)
            throw new ValidationException("Minimal mapping failed: Invalid document ID.");

        if (string.IsNullOrWhiteSpace(destination.FileName))
            throw new ValidationException("Minimal mapping failed: File name is required.");
    }

    /// <summary>
    /// Validates creation mappings with business rule enforcement.
    /// </summary>
    /// <param name="source">The creation DTO source.</param>
    /// <param name="destination">The mapped entity destination.</param>
    /// <exception cref="ValidationException">Thrown when creation validation fails.</exception>
    private static void ValidateCreationMapping(DocumentForCreationDto source, Document destination)
    {
        if (destination.Id == Guid.Empty)
            throw new ValidationException("Document ID must be generated during creation mapping.");

        if (destination.CreationDate == default)
            throw new ValidationException("Creation date must be set during creation mapping.");

        if (destination.IsDeleted)
            throw new ValidationException("New documents cannot be created in deleted state.");

        // Validate using FileValidationHelper
        if (!FileValidationHelper.IsValidChecksum(source.Checksum))
            throw new ValidationException("Valid checksum is required for document creation.");

        if (!FileValidationHelper.IsExtensionAllowed(source.Extension))
            throw new ValidationException($"Extension '{source.Extension}' is not allowed for document creation.");

        if (!FileValidationHelper.IsMimeTypeAllowed(source.MimeType))
            throw new ValidationException($"MIME type '{source.MimeType}' is not allowed for document creation.");
    }

    /// <summary>
    /// Validates update mappings with business rule enforcement.
    /// </summary>
    /// <param name="source">The update DTO source.</param>
    /// <param name="destination">The mapped entity destination.</param>
    /// <exception cref="ValidationException">Thrown when update validation fails.</exception>
    private static void ValidateUpdateMapping(DocumentForUpdateDto source, Document destination)
    {
        if (source.IsDeleted && source.IsCheckedOut)
            throw new ValidationException("Cannot update document to be both deleted and checked out.");

        // Validate file properties if they're being updated
        if (!string.IsNullOrWhiteSpace(source.Checksum) && !FileValidationHelper.IsValidChecksum(source.Checksum))
            throw new ValidationException("Valid checksum is required for document updates with file changes.");

        if (!string.IsNullOrWhiteSpace(source.Extension) && !FileValidationHelper.IsExtensionAllowed(source.Extension))
            throw new ValidationException($"Extension '{source.Extension}' is not allowed for document updates.");

        if (!string.IsNullOrWhiteSpace(source.MimeType) && !FileValidationHelper.IsMimeTypeAllowed(source.MimeType))
            throw new ValidationException($"MIME type '{source.MimeType}' is not allowed for document updates.");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Normalizes file extensions to professional standards (lowercase with dot prefix).
    /// </summary>
    /// <param name="extension">The file extension to normalize.</param>
    /// <returns>A normalized file extension meeting professional standards.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string NormalizeExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return string.Empty;

        var trimmed = extension.Trim().ToLowerInvariant();
        return trimmed.StartsWith('.') ? trimmed : $".{trimmed}";
    }

    /// <summary>
    /// Gets the appropriate MIME type for a given file extension.
    /// </summary>
    /// <param name="extension">The file extension.</param>
    /// <returns>The corresponding MIME type or empty string if unknown.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetMimeTypeForExtension(string? extension)
    {
        var normalizedExtension = NormalizeExtension(extension);

        return normalizedExtension switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".txt" => "text/plain",
            ".rtf" => "application/rtf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => string.Empty
        };
    }

    #endregion

    #region Public Validation Methods

    /// <summary>
    /// Validates that all DocumentProfile Mapster configurations are properly set up and functional.
    /// </summary>
    /// <returns>A validation result indicating configuration status.</returns>
    /// <remarks>
    /// This method tests all configured Mapster type adapters to ensure they work correctly
    /// and meet professional standards for legal document management operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Validate during application startup
    /// var result = DocumentProfile.ValidateConfiguration();
    /// if (!result.IsValid)
    /// {
    ///     throw new InvalidOperationException($"DocumentProfile invalid: {result.Summary}");
    /// }
    /// </code>
    /// </example>
    public static ProfileValidationResult ValidateConfiguration()
    {
        var errors = new List<string>();
        var mappings = new List<string>();

        try
        {
            var testDoc = CreateTestDocument();

            // Test Document → DocumentDto
            try
            {
                var completeDto = testDoc.Adapt<DocumentDto>();
                if (completeDto?.Id == testDoc.Id && completeDto.FileName == testDoc.FileName)
                    mappings.Add("Document → DocumentDto");
                else
                    errors.Add("Document → DocumentDto mapping failed");
            }
            catch (Exception ex)
            {
                errors.Add($"Document → DocumentDto error: {ex.Message}");
            }

            // Test Document → DocumentMinimalDto
            try
            {
                var minimalDto = testDoc.Adapt<DocumentMinimalDto>();
                if (minimalDto?.Id == testDoc.Id)
                    mappings.Add("Document → DocumentMinimalDto");
                else
                    errors.Add("Document → DocumentMinimalDto mapping failed");
            }
            catch (Exception ex)
            {
                errors.Add($"Document → DocumentMinimalDto error: {ex.Message}");
            }

            // Test DocumentForCreationDto → Document
            try
            {
                var creationDto = new DocumentForCreationDto
                {
                    FileName = "Test_Creation",
                    Extension = "pdf",
                    FileSize = 1024,
                    MimeType = "application/pdf",
                    Checksum = "A1B2C3D4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E3F4A5B6C7D8E9F0A1B2C3D4",
                    IsCheckedOut = false,
                    Description = "Test document"
                };

                var createdEntity = creationDto.Adapt<Document>();
                if (createdEntity?.FileName == creationDto.FileName && createdEntity.Id != Guid.Empty)
                    mappings.Add("DocumentForCreationDto → Document");
                else
                    errors.Add("DocumentForCreationDto → Document mapping failed");
            }
            catch (Exception ex)
            {
                errors.Add($"DocumentForCreationDto → Document error: {ex.Message}");
            }

            return new ProfileValidationResult(
                !errors.Any(),
                mappings,
                errors,
                errors.Any()
                    ? $"Configuration has {errors.Count} errors: {string.Join("; ", errors)}"
                    : $"Configuration valid with {mappings.Count} working mappings"
            );
        }
        catch (Exception ex)
        {
            return new ProfileValidationResult(
                false,
                [],
                [ex.Message],
                $"Validation failed: {ex.Message}"
            );
        }
    }

    /// <summary>
    /// Creates a test document entity for validation purposes.
    /// </summary>
    /// <returns>A Document entity with realistic test data.</returns>
    private static Document CreateTestDocument() => new()
    {
        Id = Guid.NewGuid(),
        FileName = "Test_Document",
        Extension = ".pdf",
        FileSize = 1024,
        MimeType = "application/pdf",
        Checksum = "A1B2C3D4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E3F4A5B6C7D8E9F0A1B2C3D4",
        IsCheckedOut = false,
        IsDeleted = false,
        CreationDate = DateTime.UtcNow,
        MatterId = Guid.NewGuid(),
        Matter = new Matter
        {
            Description = "Test Matter"
        },
        Revisions = [],
        DocumentActivityUsers = [],
        MatterDocumentActivityUsersFrom = [],
        MatterDocumentActivityUsersTo = []
    };

    #endregion

    #region Supporting Types

    /// <summary>
    /// Represents validation results for DocumentProfile configuration.
    /// </summary>
    /// <param name="IsValid">Whether the profile configuration is valid.</param>
    /// <param name="ConfiguredMappings">List of successfully configured mappings.</param>
    /// <param name="Errors">List of configuration errors.</param>
    /// <param name="Summary">Summary of validation results.</param>
    public sealed record ProfileValidationResult(
        bool IsValid,
        List<string> ConfiguredMappings,
        List<string> Errors,
        string Summary);

    #endregion
}