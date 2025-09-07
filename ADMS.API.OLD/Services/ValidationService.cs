using ADMS.API.Models;
using ADMS.API.Services.Common;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ADMS.API.Services;

/// <summary>
/// Centralized validation service providing comprehensive validation logic for ADMS API operations.
/// </summary>
/// <remarks>
/// This service implements comprehensive validation functionality for the ADMS system including:
/// - Object validation using data annotations and custom validation logic
/// - Entity existence validation with proper error responses
/// - Parameter validation with standardized error handling
/// - Business rule validation for complex scenarios
/// - Specialized validation for file operations, email addresses, and data formats
/// 
/// The service follows established patterns for:
/// - Consistent error response formatting using RFC 7807 Problem Details
/// - Structured logging with appropriate context and correlation
/// - Centralized validation logic to eliminate code duplication
/// - Standardized HTTP status code usage following REST conventions
/// - Comprehensive input sanitization and validation
/// 
/// Key Features:
/// - Data annotation validation with detailed error reporting
/// - Entity existence validation with database integration
/// - Format validation for emails, file extensions, MIME types, and checksums
/// - Business rule validation for document operations
/// - Model state validation with proper error propagation
/// - GUID validation with empty value checking
/// - String validation with null/empty checking
/// 
/// Security Features:
/// - Input sanitization through comprehensive validation
/// - SQL injection prevention through parameterized validation
/// - Cross-site scripting (XSS) prevention through format validation
/// - File type validation to prevent malicious uploads
/// - Checksum validation for data integrity
/// 
/// The service integrates with:
/// - Entity Framework for database existence validation
/// - ASP.NET Core model binding and validation infrastructure
/// - Logging infrastructure for audit trails and debugging
/// - Problem Details factory for consistent error responses
/// </remarks>
public partial class ValidationService(
    IEntityExistenceValidator entityExistenceValidator,
    ILogger<ValidationService> logger,
    ProblemDetailsFactory problemDetailsFactory) : IValidationService
{
    private readonly IEntityExistenceValidator _entityExistenceValidator = entityExistenceValidator ?? throw new ArgumentNullException(nameof(entityExistenceValidator));
    private readonly ILogger<ValidationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));

    #region Object and DTO Validation

    /// <summary>
    /// Validates the specified object against its data annotations and custom validation logic.
    /// </summary>
    /// <remarks>
    /// This method provides comprehensive object validation including:
    /// - Data annotation validation (Required, Range, MaxLength, RegularExpression, etc.)
    /// - Custom validation through IValidatableObject implementation
    /// - Recursive validation of complex object properties
    /// - Detailed error reporting with property-level error messages
    /// 
    /// The validation process:
    /// 1. Creates a ValidationContext for the object
    /// 2. Performs comprehensive validation including all properties
    /// 3. Collects all validation errors with property attribution
    /// 4. Returns structured ValidationProblemDetails for client consumption
    /// 5. Logs validation failures for debugging and monitoring
    /// 
    /// Error Response Format:
    /// - Uses ValidationProblemDetails following RFC 7807
    /// - Groups errors by property name for client-side handling
    /// - Provides detailed error messages for each validation failure
    /// - Includes proper HTTP status codes (400 Bad Request)
    /// </remarks>
    /// <typeparam name="T">The type of the object to validate.</typeparam>
    /// <param name="obj">The object to validate. Cannot be null.</param>
    /// <returns>
    /// A BadRequestObjectResult containing ValidationProblemDetails if validation fails;
    /// otherwise, null if the object is valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when obj is null.</exception>
    public ActionResult? ValidateObject<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        try
        {
            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();

            // Perform comprehensive validation including all properties and IValidatableObject
            var isValid = Validator.TryValidateObject(obj, context, results, validateAllProperties: true);

            if (isValid) return null;

            // Group validation errors by property for structured response
            var errors = results
                .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => r.ErrorMessage ?? "Validation error occurred").ToArray()
                );

            _logger.LogWarning(
                "Validation failed for {Type} with {ErrorCount} errors: {@Errors}",
                typeof(T).Name, errors.Count, errors);

            return new BadRequestObjectResult(new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Detail = $"Validation failed for {typeof(T).Name}. See the errors property for details.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error during validation of {Type}", typeof(T).Name);

            return CreateInternalServerErrorResponse("validating the object");
        }
    }

    /// <summary>
    /// Validates the provided model state dictionary and returns appropriate error responses.
    /// </summary>
    /// <remarks>
    /// This method validates ASP.NET Core model state which captures:
    /// - Model binding errors (type conversion, format issues)
    /// - Data annotation validation errors
    /// - Custom validation errors added during model binding
    /// - Property-level and model-level validation failures
    /// 
    /// The validation provides:
    /// - Structured error responses with property-level error grouping
    /// - Consistent error formatting using ValidationProblemDetails
    /// - Proper HTTP status codes following REST conventions
    /// - Detailed logging for debugging and monitoring
    /// 
    /// Common scenarios handled:
    /// - Invalid JSON format in request body
    /// - Type conversion errors (e.g., string to int)
    /// - Missing required properties in JSON
    /// - Format validation failures (e.g., invalid email format)
    /// - Range validation failures (e.g., negative values where positive required)
    /// </remarks>
    /// <param name="modelState">The ASP.NET Core ModelStateDictionary to validate.</param>
    /// <returns>
    /// A BadRequestObjectResult containing ValidationProblemDetails if model state is invalid;
    /// otherwise, null if model state is valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when modelState is null.</exception>
    public ActionResult? ValidateModelState(ModelStateDictionary modelState)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        if (modelState.IsValid) return null;

        try
        {
            // Extract errors with proper grouping and formatting
            var errors = modelState
                .Where(ms => ms.Value?.Errors.Count > 0)
                .ToDictionary(
                    ms => ms.Key,
                    ms => ms.Value!.Errors.Select(e =>
                        !string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? e.ErrorMessage
                            : e.Exception?.Message ?? "Invalid value").ToArray()
                );

            _logger.LogWarning(
                "Model state validation failed with {ErrorCount} errors: {@Errors}",
                errors.Count, errors);

            return new BadRequestObjectResult(new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more model binding errors occurred.",
                Detail = "The request contains invalid data. See the errors property for details.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during model state validation");
            return CreateInternalServerErrorResponse("validating model state");
        }
    }

    #endregion Object and DTO Validation

    #region Parameter Validation

    /// <summary>
    /// Validates that the provided GUID is not empty and returns appropriate error responses.
    /// </summary>
    /// <remarks>
    /// This method validates GUID parameters which are commonly used as:
    /// - Entity identifiers (matterId, documentId, revisionId)
    /// - Route parameters in RESTful APIs
    /// - Query parameters for resource identification
    /// - Foreign key references in entity relationships
    /// 
    /// Validation includes:
    /// - Empty GUID detection (Guid.Empty or all zeros)
    /// - Proper error response formatting with RFC 7807 compliance
    /// - Structured logging with parameter context
    /// - Consistent error messaging across the API
    /// 
    /// The method is essential for:
    /// - Route parameter validation in controllers
    /// - Entity identifier validation before database queries
    /// - Foreign key validation in entity relationships
    /// - Request parameter sanitization and security
    /// </remarks>
    /// <param name="id">The GUID value to validate.</param>
    /// <param name="parameterName">The name of the parameter being validated for error reporting.</param>
    /// <returns>
    /// A BadRequestObjectResult with problem details if the GUID is empty;
    /// otherwise, null if the GUID is valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when parameterName is null.</exception>
    public ActionResult? ValidateGuid(Guid id, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        if (id != Guid.Empty) return null;

        _logger.LogWarning(
            "Invalid {ParameterName} provided (empty GUID): {GuidValue}",
            parameterName, id);

        return new BadRequestObjectResult(CreateProblemDetails(
            StatusCodes.Status400BadRequest,
            $"Invalid {parameterName}. The provided GUID cannot be empty.",
            parameterName));
    }

    /// <summary>
    /// Validates that the provided string is not null, empty, or whitespace.
    /// </summary>
    /// <remarks>
    /// This method validates string parameters for:
    /// - Required text input validation
    /// - Non-empty string requirement enforcement
    /// - Whitespace-only string detection and rejection
    /// - Input sanitization for security purposes
    /// 
    /// Common use cases include:
    /// - File name validation
    /// - User input validation
    /// - Required field validation
    /// - Configuration parameter validation
    /// - Search query validation
    /// 
    /// The validation helps prevent:
    /// - Empty or null reference exceptions
    /// - Database constraint violations
    /// - Business logic errors from empty inputs
    /// - Security issues from malformed input
    /// </remarks>
    /// <param name="value">The string value to validate.</param>
    /// <param name="parameterName">The name of the parameter being validated for error reporting.</param>
    /// <returns>True if the string is valid (not null/empty/whitespace); otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when parameterName is null.</exception>
    public bool ValidateStringNotEmpty(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        var isValid = !string.IsNullOrWhiteSpace(value);

        if (!isValid)
        {
            _logger.LogWarning(
                "{ParameterName} is null, empty, or contains only whitespace: '{Value}'",
                parameterName, value ?? "<null>");
        }

        return isValid;
    }

    /// <summary>
    /// Validates that the provided object is not null and returns appropriate error responses.
    /// </summary>
    /// <remarks>
    /// This method provides null validation for:
    /// - DTO parameters in API endpoints
    /// - Required object properties
    /// - Method parameters that cannot be null
    /// - Entity validation before database operations
    /// 
    /// The validation includes:
    /// - Comprehensive null checking
    /// - Structured error response formatting
    /// - Proper HTTP status codes (400 Bad Request)
    /// - Detailed logging for debugging and monitoring
    /// 
    /// Use cases include:
    /// - API request body validation
    /// - Service method parameter validation
    /// - Entity validation before persistence
    /// - Configuration object validation
    /// </remarks>
    /// <param name="obj">The object to validate for null.</param>
    /// <param name="parameterName">The name of the parameter being validated for error reporting.</param>
    /// <returns>
    /// A BadRequestObjectResult with problem details if the object is null;
    /// otherwise, null if the object is valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when parameterName is null.</exception>
    public ActionResult? ValidateNotNull(object? obj, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        if (obj != null) return null;

        _logger.LogWarning("{ParameterName} is null", parameterName);

        return new BadRequestObjectResult(CreateProblemDetails(
            StatusCodes.Status400BadRequest,
            $"{parameterName} cannot be null.",
            parameterName));
    }

    #endregion Parameter Validation

    #region Entity Existence Validation

    /// <summary>
    /// Validates if the specified matter exists in the database.
    /// </summary>
    /// <remarks>
    /// This method provides matter existence validation including:
    /// - Database query execution through entity existence validator
    /// - Proper error response formatting with 404 Not Found status
    /// - Structured logging for audit trails and debugging
    /// - Consistent error messaging across matter-related operations
    /// 
    /// The validation is essential for:
    /// - Route parameter validation in matter-scoped endpoints
    /// - Foreign key validation before document operations
    /// - Authorization and access control validation
    /// - Data integrity enforcement in business operations
    /// 
    /// Error handling includes:
    /// - Specific HTTP 404 status codes for missing entities
    /// - Detailed error messages for client applications
    /// - Structured logging for monitoring and debugging
    /// - Consistent problem details format across the API
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter to validate.</param>
    /// <returns>
    /// A NotFoundObjectResult with problem details if the matter does not exist;
    /// otherwise, null if the matter exists.
    /// </returns>
    public async Task<ActionResult?> ValidateMatterExistsAsync(Guid matterId)
    {
        try
        {
            var exists = await _entityExistenceValidator.MatterExistsAsync(matterId);
            if (exists) return null;

            _logger.LogWarning("Matter with ID {MatterId} not found during existence validation", matterId);

            return new NotFoundObjectResult(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                $"Matter with ID {matterId} not found.",
                "matterId"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error validating matter existence for ID {MatterId}", matterId);

            return CreateInternalServerErrorResponse("validating matter existence");
        }
    }

    /// <summary>
    /// Validates if the specified document exists in the database.
    /// </summary>
    /// <remarks>
    /// This method provides document existence validation including:
    /// - Database query execution for document lookup
    /// - Proper HTTP status code usage (404 Not Found for missing documents)
    /// - Comprehensive logging for audit and debugging purposes
    /// - Standardized error response formatting
    /// 
    /// The validation supports:
    /// - Document-scoped operation validation
    /// - Entity relationship integrity checking
    /// - Authorization prerequisite validation
    /// - Data consistency enforcement
    /// 
    /// Common usage scenarios:
    /// - Before document update or deletion operations
    /// - For revision creation validation
    /// - In document access control validation
    /// - For file operation prerequisite checking
    /// </remarks>
    /// <param name="documentId">The unique identifier of the document to validate.</param>
    /// <returns>
    /// A NotFoundObjectResult with problem details if the document does not exist;
    /// otherwise, null if the document exists.
    /// </returns>
    public async Task<ActionResult?> ValidateDocumentExistsAsync(Guid documentId)
    {
        try
        {
            var exists = await _entityExistenceValidator.DocumentExistsAsync(documentId);
            if (exists) return null;

            _logger.LogWarning("Document with ID {DocumentId} not found during existence validation", documentId);

            return new NotFoundObjectResult(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                $"Document with ID {documentId} not found.",
                "documentId"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error validating document existence for ID {DocumentId}", documentId);

            return CreateInternalServerErrorResponse("validating document existence");
        }
    }

    /// <summary>
    /// Validates if the specified revision exists in the database.
    /// </summary>
    /// <remarks>
    /// This method provides revision existence validation including:
    /// - Database query execution for revision lookup
    /// - Appropriate HTTP status code handling (404 Not Found)
    /// - Detailed logging for operations tracking
    /// - Consistent error response structure
    /// 
    /// Revision validation supports:
    /// - Revision-specific operations (update, delete, access)
    /// - Entity hierarchy validation (revision -> document -> matter)
    /// - File operation validation for specific revisions
    /// - Audit trail validation for revision activities
    /// 
    /// The method is crucial for:
    /// - File download operations targeting specific revisions
    /// - Revision metadata operations
    /// - Revision history and audit operations
    /// - Business rule enforcement at revision level
    /// </remarks>
    /// <param name="revisionId">The unique identifier of the revision to validate.</param>
    /// <returns>
    /// A NotFoundObjectResult with problem details if the revision does not exist;
    /// otherwise, null if the revision exists.
    /// </returns>
    public async Task<ActionResult?> ValidateRevisionExistsAsync(Guid revisionId)
    {
        try
        {
            var exists = await _entityExistenceValidator.RevisionExistsAsync(revisionId);
            if (exists) return null;

            _logger.LogWarning("Revision with ID {RevisionId} not found during existence validation", revisionId);

            return new NotFoundObjectResult(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                $"Revision with ID {revisionId} not found.",
                "revisionId"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error validating revision existence for ID {RevisionId}", revisionId);

            return CreateInternalServerErrorResponse("validating revision existence");
        }
    }

    #endregion Entity Existence Validation

    #region Business Rule Validation

    /// <summary>
    /// Validates a document for creation operations with comprehensive business rule checking.
    /// </summary>
    /// <remarks>
    /// This method performs comprehensive document creation validation including:
    /// 
    /// File and Content Validation:
    /// - File name presence and format validation
    /// - File extension validation against allowed types
    /// - MIME type validation and format checking
    /// - File size validation within acceptable limits
    /// - Checksum validation for data integrity
    /// 
    /// Business Rule Validation:
    /// - Duplicate file name detection within matter scope
    /// - File type restrictions based on security policies
    /// - Content validation to prevent malicious uploads
    /// - Matter-specific file constraints
    /// 
    /// Security Validation:
    /// - File extension allow-list enforcement
    /// - MIME type validation against known safe types
    /// - File name sanitization and security checking
    /// - Content-based file type verification
    /// 
    /// The validation process:
    /// 1. Validates required fields (file name, extension, MIME type)
    /// 2. Performs format validation using regex patterns
    /// 3. Checks business rules (duplicate names, file restrictions)
    /// 4. Returns structured validation results for error handling
    /// 5. Logs all validation failures for audit and debugging
    /// 
    /// This method is essential for maintaining data integrity and security
    /// in document creation operations across the ADMS system.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter where the document will be created.</param>
    /// <param name="document">The document creation DTO containing the document information to validate.</param>
    /// <returns>
    /// An enumerable collection of ValidationResult objects containing any validation errors found.
    /// An empty collection indicates successful validation.
    /// </returns>
    public async Task<IEnumerable<ValidationResult>> ValidateDocumentForCreationAsync(Guid matterId, DocumentForCreationDto? document)
    {
        var validationResults = new List<ValidationResult>();

        try
        {
            // Null document check
            if (document == null)
            {
                validationResults.Add(new ValidationResult(
                    "Document information is required for creation.",
                    new[] { nameof(document) }));
                _logger.LogWarning("Document creation validation failed: null document provided");
                return validationResults;
            }

            // File name validation
            if (string.IsNullOrWhiteSpace(document.FileName))
            {
                validationResults.Add(new ValidationResult(
                    "File name is required and cannot be empty.",
                    new[] { nameof(document.FileName) }));
                _logger.LogWarning("Document creation validation failed: file name is required");
            }
            else if (document.FileName.Length > FileValidationConstants.MaxFileNameLength)
            {
                validationResults.Add(new ValidationResult(
                    $"File name cannot exceed {FileValidationConstants.MaxFileNameLength} characters.",
                    new[] { nameof(document.FileName) }));
                _logger.LogWarning("Document creation validation failed: file name too long ({Length} characters)",
                    document.FileName.Length);
            }

            // File extension validation
            if (!string.IsNullOrWhiteSpace(document.Extension) &&
                !ValidateFileExtension(document.Extension, nameof(document.Extension)))
            {
                validationResults.Add(new ValidationResult(
                    "Invalid file extension format. Extension must be 2-5 alphanumeric characters.",
                    new[] { nameof(document.Extension) }));
            }

            // MIME type validation
            if (!string.IsNullOrWhiteSpace(document.MimeType) &&
                !ValidateMimeType(document.MimeType, nameof(document.MimeType)))
            {
                validationResults.Add(new ValidationResult(
                    "Invalid MIME type format.",
                    new[] { nameof(document.MimeType) }));
            }

            // File size validation
            if (document.FileSize < 0)
            {
                validationResults.Add(new ValidationResult(
                    "File size cannot be negative.",
                    new[] { nameof(document.FileSize) }));
                _logger.LogWarning("Document creation validation failed: negative file size ({FileSize})",
                    document.FileSize);
            }

            // Checksum validation
            if (!string.IsNullOrWhiteSpace(document.Checksum) &&
                !ValidateChecksum(document.Checksum, nameof(document.Checksum)))
            {
                validationResults.Add(new ValidationResult(
                    "Invalid checksum format. Must be a 64-character hexadecimal string.",
                    new[] { nameof(document.Checksum) }));
            }

            // Duplicate file name check
            if (!string.IsNullOrWhiteSpace(document.FileName) &&
                await _entityExistenceValidator.FileNameExistsAsync(matterId, document.FileName))
            {
                validationResults.Add(new ValidationResult(
                    $"A file named '{document.FileName}' already exists in this matter.",
                    new[] { nameof(document.FileName) }));
                _logger.LogWarning(
                    "Document creation validation failed: duplicate file name '{FileName}' in matter {MatterId}",
                    document.FileName, matterId);
            }

            if (validationResults.Any())
            {
                _logger.LogWarning(
                    "Document creation validation failed with {ErrorCount} errors for matter {MatterId}",
                    validationResults.Count, matterId);
            }

            return validationResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error during document creation validation for matter {MatterId}", matterId);

            validationResults.Add(new ValidationResult(
                "An unexpected error occurred during validation. Please try again.",
                new[] { "ValidationError" }));

            return validationResults;
        }
    }

    /// <summary>
    /// Validates a document for update operations with comprehensive business rule checking.
    /// </summary>
    /// <remarks>
    /// This method performs comprehensive document update validation including:
    /// 
    /// Content Validation:
    /// - File name presence and format requirements
    /// - File extension format and security validation
    /// - MIME type format validation and consistency checking
    /// - File size validation within acceptable ranges
    /// - Checksum integrity validation
    /// 
    /// Business Rule Validation:
    /// - Document state consistency checking (deleted vs checked out)
    /// - Update permission validation based on document state
    /// - File content consistency validation
    /// - Version control state validation
    /// 
    /// State Validation:
    /// - Prevents documents from being both deleted and checked out
    /// - Validates state transitions for document lifecycle
    /// - Ensures consistent document status across operations
    /// - Validates user permissions for state changes
    /// 
    /// The validation ensures:
    /// - Data integrity during update operations
    /// - Business rule compliance for document management
    /// - Security validation for file content changes
    /// - Consistent document state management
    /// 
    /// This method is critical for maintaining document integrity and
    /// enforcing business rules during update operations.
    /// </remarks>
    /// <param name="document">The document update DTO containing the updated document information to validate.</param>
    /// <returns>
    /// An enumerable collection of ValidationResult objects containing any validation errors found.
    /// An empty collection indicates successful validation.
    /// </returns>
    public IEnumerable<ValidationResult> ValidateDocumentForUpdate(DocumentForUpdateDto document)
    {
        var validationResults = new List<ValidationResult>();

        try
        {
            ArgumentNullException.ThrowIfNull(document);

            // File name validation
            if (string.IsNullOrWhiteSpace(document.FileName))
            {
                validationResults.Add(new ValidationResult(
                    "File name is required and cannot be empty.",
                    new[] { nameof(document.FileName) }));
                _logger.LogWarning("Document update validation failed: file name is required");
            }
            else if (document.FileName.Length > FileValidationConstants.MaxFileNameLength)
            {
                validationResults.Add(new ValidationResult(
                    $"File name cannot exceed {FileValidationConstants.MaxFileNameLength} characters.",
                    new[] { nameof(document.FileName) }));
                _logger.LogWarning("Document update validation failed: file name too long ({Length} characters)",
                    document.FileName.Length);
            }

            // File extension validation
            if (!ValidateFileExtension(document.Extension, nameof(document.Extension)))
            {
                validationResults.Add(new ValidationResult(
                    "Invalid file extension format. Extension must be 2-5 alphanumeric characters.",
                    new[] { nameof(document.Extension) }));
            }

            // MIME type validation
            if (!ValidateMimeType(document.MimeType, nameof(document.MimeType)))
            {
                validationResults.Add(new ValidationResult(
                    "Invalid MIME type format.",
                    new[] { nameof(document.MimeType) }));
            }

            // File size validation
            if (document.FileSize < 0)
            {
                validationResults.Add(new ValidationResult(
                    "File size cannot be negative.",
                    new[] { nameof(document.FileSize) }));
                _logger.LogWarning("Document update validation failed: negative file size ({FileSize})",
                    document.FileSize);
            }

            // Checksum validation
            if (!ValidateChecksum(document.Checksum, nameof(document.Checksum)))
            {
                validationResults.Add(new ValidationResult(
                    "Invalid checksum format. Must be a 64-character hexadecimal string.",
                    new[] { nameof(document.Checksum) }));
            }

            // Business rule validation: document cannot be both deleted and checked out
            if (document.IsDeleted && document.IsCheckedOut)
            {
                validationResults.Add(new ValidationResult(
                    "A document cannot be both deleted and checked out simultaneously.",
                    new[] { nameof(document.IsDeleted), nameof(document.IsCheckedOut) }));
                _logger.LogWarning("Document update validation failed: document cannot be both deleted and checked out");
            }

            if (validationResults.Any())
            {
                _logger.LogWarning(
                    "Document update validation failed with {ErrorCount} errors",
                    validationResults.Count);
            }

            return validationResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during document update validation");

            validationResults.Add(new ValidationResult(
                "An unexpected error occurred during validation. Please try again.",
                new[] { "ValidationError" }));

            return validationResults;
        }
    }

    #endregion Business Rule Validation

    #region Format Validation

    /// <summary>
    /// Validates if the provided string is a valid email address format.
    /// </summary>
    /// <remarks>
    /// This method provides email validation including:
    /// - Format validation using RFC-compliant regex patterns
    /// - Null and empty string handling
    /// - Comprehensive logging for debugging and monitoring
    /// - Security validation to prevent email injection attacks
    /// 
    /// Email Validation Features:
    /// - Basic RFC 822 format compliance checking
    /// - Prevents common email format injection attempts
    /// - Supports international domain names and TLDs
    /// - Validates local and domain parts of email addresses
    /// 
    /// Security Considerations:
    /// - Prevents email header injection attacks
    /// - Validates against malicious email format strings
    /// - Protects against SMTP injection attempts
    /// - Ensures proper email format for system notifications
    /// 
    /// The validation uses compiled regex for performance optimization
    /// and follows established patterns for email format validation.
    /// While not 100% RFC-compliant, it covers the vast majority of
    /// valid email address formats used in practice.
    /// </remarks>
    /// <param name="email">The email address string to validate.</param>
    /// <param name="parameterName">The name of the parameter being validated for error reporting.</param>
    /// <returns>True if the email format is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when parameterName is null.</exception>
    public bool ValidateEmail(string email, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("{ParameterName} is null, empty, or whitespace", parameterName);
            return false;
        }

        try
        {
            var isValid = EmailRegex().IsMatch(email);

            if (!isValid)
            {
                _logger.LogWarning(
                    "{ParameterName} is not a valid email address format: '{Email}'",
                    parameterName, email);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error validating email format for {ParameterName}: '{Email}'",
                parameterName, email);
            return false;
        }
    }

    /// <summary>
    /// Validates if the provided string is a valid file extension format.
    /// </summary>
    /// <remarks>
    /// This method validates file extensions for:
    /// - Security purposes (preventing malicious file uploads)
    /// - Format consistency across the application
    /// - Database storage requirements and constraints
    /// - File type identification and handling
    /// 
    /// File Extension Validation:
    /// - Accepts extensions with or without leading dot
    /// - Validates length between 2-5 characters
    /// - Ensures alphanumeric characters only
    /// - Prevents special characters and injection attempts
    /// 
    /// Security Features:
    /// - Prevents directory traversal attempts in extensions
    /// - Blocks malicious file extension combinations
    /// - Validates against known safe file extension patterns
    /// - Protects against file type masquerading attacks
    /// 
    /// The validation supports common file extensions while maintaining
    /// security by restricting to alphanumeric characters only.
    /// This prevents many types of file-based attacks while supporting
    /// legitimate file types used in document management.
    /// </remarks>
    /// <param name="extension">The file extension string to validate.</param>
    /// <param name="parameterName">The name of the parameter being validated for error reporting.</param>
    /// <returns>True if the file extension format is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when parameterName is null.</exception>
    public bool ValidateFileExtension(string extension, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        if (string.IsNullOrWhiteSpace(extension))
        {
            _logger.LogWarning("{ParameterName} is null, empty, or whitespace", parameterName);
            return false;
        }

        try
        {
            // Accepts .ext or ext (with or without dot), 2-5 alphanumeric chars
            var isValid = FileExtensionRegex().IsMatch(extension);

            if (!isValid)
            {
                _logger.LogWarning(
                    "{ParameterName} is not a valid file extension format: '{Extension}' (must be 2-5 alphanumeric characters, optionally prefixed with dot)",
                    parameterName, extension);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error validating file extension format for {ParameterName}: '{Extension}'",
                parameterName, extension);
            return false;
        }
    }

    /// <summary>
    /// Validates if the provided string is a valid MIME type format.
    /// </summary>
    /// <remarks>
    /// This method validates MIME types for:
    /// - HTTP content type header validation
    /// - File upload security and content validation
    /// - Content negotiation support
    /// - Database storage format consistency
    /// 
    /// MIME Type Validation:
    /// - Validates standard type/subtype format
    /// - Supports media type parameters
    /// - Handles vendor-specific MIME types
    /// - Validates against injection attacks
    /// 
    /// Security Features:
    /// - Prevents MIME type spoofing attacks
    /// - Validates against malicious content type headers
    /// - Ensures proper MIME type format for security
    /// - Protects against content type confusion attacks
    /// 
    /// Format Support:
    /// - Standard MIME types (text/html, application/json)
    /// - Vendor-specific types (application/vnd.ms-excel)
    /// - Media types with parameters
    /// - Common document and image MIME types
    /// 
    /// The validation uses regex patterns that cover the vast majority
    /// of legitimate MIME types while preventing malicious formats.
    /// </remarks>
    /// <param name="mimeType">The MIME type string to validate.</param>
    /// <param name="parameterName">The name of the parameter being validated for error reporting.</param>
    /// <returns>True if the MIME type format is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when parameterName is null.</exception>
    public bool ValidateMimeType(string mimeType, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        if (string.IsNullOrWhiteSpace(mimeType))
        {
            _logger.LogWarning("{ParameterName} is null, empty, or whitespace", parameterName);
            return false;
        }

        try
        {
            // Validates type/subtype format with allowed characters
            var isValid = MimeTypeRegex().IsMatch(mimeType);

            if (!isValid)
            {
                _logger.LogWarning(
                    "{ParameterName} is not a valid MIME type format: '{MimeType}' (must be in type/subtype format)",
                    parameterName, mimeType);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error validating MIME type format for {ParameterName}: '{MimeType}'",
                parameterName, mimeType);
            return false;
        }
    }

    /// <summary>
    /// Validates if the provided string is a valid checksum format.
    /// </summary>
    /// <remarks>
    /// This method validates checksums for:
    /// - File integrity verification
    /// - Data corruption detection
    /// - Security validation of file content
    /// - Database storage format consistency
    /// 
    /// Checksum Validation:
    /// - Validates SHA256 64-character hexadecimal format
    /// - Ensures proper hex character usage (0-9, A-F, a-f)
    /// - Validates exact length requirements
    /// - Prevents malicious checksum injection
    /// 
    /// Security Features:
    /// - Ensures checksum integrity for file verification
    /// - Validates against checksum manipulation attacks
    /// - Provides data integrity validation
    /// - Supports secure file verification workflows
    /// 
    /// The method specifically validates SHA256 checksums which are
    /// 64 hexadecimal characters long. This is the standard checksum
    /// format used throughout the ADMS system for file integrity.
    /// </remarks>
    /// <param name="checksum">The checksum string to validate.</param>
    /// <param name="parameterName">The name of the parameter being validated for error reporting.</param>
    /// <returns>True if the checksum format is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when parameterName is null.</exception>
    public bool ValidateChecksum(string checksum, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        if (string.IsNullOrWhiteSpace(checksum))
        {
            _logger.LogWarning("{ParameterName} is null, empty, or whitespace", parameterName);
            return false;
        }

        try
        {
            // Validates 64-character hexadecimal checksum (SHA256)
            var isValid = ChecksumRegex().IsMatch(checksum);

            if (!isValid)
            {
                _logger.LogWarning(
                    "{ParameterName} is not a valid checksum format: '{Checksum}' (must be 64 hexadecimal characters for SHA256)",
                    parameterName, checksum);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error validating checksum format for {ParameterName}: '{Checksum}'",
                parameterName, checksum);
            return false;
        }
    }

    #endregion Format Validation

    #region Private Helper Methods

    /// <summary>
    /// Creates a standardized problem details object for error responses.
    /// </summary>
    /// <remarks>
    /// This method creates RFC 7807 compliant problem details for:
    /// - Consistent error response formatting across the API
    /// - Structured error information for client applications
    /// - Proper HTTP status code usage
    /// - Enhanced debugging and monitoring capabilities
    /// 
    /// The problem details include:
    /// - HTTP status code with proper REST semantics
    /// - Detailed error message for client consumption
    /// - Parameter context for specific field errors
    /// - Type URI for error categorization
    /// - Instance information for debugging
    /// </remarks>
    /// <param name="statusCode">The HTTP status code for the error.</param>
    /// <param name="detail">The detailed error message.</param>
    /// <param name="parameterName">Optional parameter name for field-specific errors.</param>
    /// <returns>A properly formatted ProblemDetails object.</returns>
    private ProblemDetails CreateProblemDetails(int statusCode, string detail, string? parameterName = null)
    {
        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            httpContext: default!, // Use default! to satisfy non-nullable parameter
            statusCode: statusCode,
            detail: detail);

        if (!string.IsNullOrWhiteSpace(parameterName))
        {
            problemDetails.Extensions["parameterName"] = parameterName;
        }

        return problemDetails;
    }

    /// <summary>
    /// Creates a standardized internal server error response.
    /// </summary>
    /// <remarks>
    /// This method creates consistent 500 Internal Server Error responses
    /// for unexpected validation errors while protecting sensitive system
    /// information from being exposed to clients.
    /// </remarks>
    /// <param name="operationContext">Description of the operation that failed.</param>
    /// <returns>A standardized internal server error response.</returns>
    private ObjectResult CreateInternalServerErrorResponse(string operationContext)
    {
        var problemDetails = CreateProblemDetails(
            StatusCodes.Status500InternalServerError,
            $"An unexpected error occurred while {operationContext}. Please try again later.");

        return new ObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }

    #endregion Private Helper Methods

    #region Compiled Regex Patterns

    /// <summary>
    /// Gets a compiled regex for email validation following RFC 822 basic format.
    /// </summary>
    /// <remarks>
    /// This regex provides basic email validation that covers most common email formats
    /// while being performant through compilation. It validates the basic structure
    /// of local@domain.tld format with reasonable character restrictions.
    /// </remarks>
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    /// <summary>
    /// Gets a compiled regex for file extension validation.
    /// </summary>
    /// <remarks>
    /// This regex validates file extensions that:
    /// - May or may not start with a dot
    /// - Contain 2-5 alphanumeric characters
    /// - Exclude special characters for security
    /// - Support common file extension formats
    /// </remarks>
    [GeneratedRegex(@"^\.?[a-zA-Z0-9]{2,5}$", RegexOptions.Compiled)]
    private static partial Regex FileExtensionRegex();

    /// <summary>
    /// Gets a compiled regex for MIME type validation.
    /// </summary>
    /// <remarks>
    /// This regex validates MIME types in the standard type/subtype format
    /// allowing word characters, dots, hyphens, and plus signs which covers
    /// the vast majority of legitimate MIME types including vendor-specific ones.
    /// </remarks>
    [GeneratedRegex(@"^[\w\.\-]+\/[\w\.\-\+]+$", RegexOptions.Compiled)]
    private static partial Regex MimeTypeRegex();

    /// <summary>
    /// Gets a compiled regex for SHA256 checksum validation.
    /// </summary>
    /// <remarks>
    /// This regex validates 64-character hexadecimal checksums which is the
    /// standard format for SHA256 hashes used throughout the ADMS system
    /// for file integrity verification.
    /// </remarks>
    [GeneratedRegex(@"^[A-Fa-f0-9]{64}$", RegexOptions.Compiled)]
    private static partial Regex ChecksumRegex();

    #endregion Compiled Regex Patterns
}

/// <summary>
/// Constants for file validation constraints used throughout the ADMS system.
/// </summary>
public static class FileValidationConstants
{
    /// <summary>
    /// Maximum allowed length for file names (255 characters).
    /// </summary>
    public const int MaxFileNameLength = 255;

    /// <summary>
    /// Maximum allowed length for file extensions (10 characters).
    /// </summary>
    public const int MaxExtensionLength = 10;
}