using ADMS.API.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Services;

/// <summary>
/// Defines the contract for enterprise-grade validation services providing comprehensive validation logic for ADMS API operations.
/// </summary>
/// <remarks>
/// This interface provides a comprehensive contract for validation implementations including:
/// 
/// Core Validation Capabilities:
/// - Object validation using data annotations and custom validation logic
/// - Entity existence validation with proper error responses
/// - Parameter validation with standardized error handling
/// - Business rule validation for complex scenarios
/// - Specialized validation for file operations, email addresses, and data formats
/// 
/// Validation Framework Integration:
/// - ASP.NET Core model binding and validation infrastructure integration
/// - Data annotation validation with detailed error reporting
/// - Custom validation through IValidatableObject implementation
/// - ValidationResult collections for structured error information
/// - Problem Details format compliance for consistent API responses
/// 
/// Security and Input Validation:
/// - Input sanitization through comprehensive validation
/// - SQL injection prevention through parameterized validation
/// - Cross-site scripting (XSS) prevention through format validation
/// - File type validation to prevent malicious uploads
/// - Checksum validation for data integrity verification
/// 
/// Entity and Business Logic Validation:
/// - Database entity existence validation with proper error responses
/// - Business rule enforcement for document management operations
/// - Complex validation scenarios with multiple interdependent rules
/// - Workflow validation for document lifecycle management
/// - Compliance validation for regulatory requirements
/// 
/// Performance and Reliability:
/// - Asynchronous validation operations for database checks
/// - Efficient validation with minimal performance overhead
/// - Thread-safe operations for concurrent validation scenarios
/// - Graceful error handling with detailed diagnostic information
/// - Integration with logging infrastructure for monitoring and debugging
/// 
/// API Integration Features:
/// - Seamless integration with ASP.NET Core controllers and actions
/// - Support for model binding validation scenarios
/// - Consistent error response formatting using Problem Details
/// - Integration with API versioning and content negotiation
/// - Comprehensive audit trail support for validation activities
/// 
/// Implementation Considerations:
/// - Implementations should provide consistent error response formats
/// - Proper integration with logging infrastructure for diagnostic capabilities
/// - Thread-safe operations for use in multi-threaded web applications
/// - Graceful handling of validation errors without service interruption
/// - Configuration support for validation rules and business logic
/// 
/// The interface is designed to support:
/// - High-performance validation operations with minimal overhead
/// - Comprehensive error reporting with actionable information
/// - Extensible validation logic for evolving business requirements
/// - Integration with monitoring and observability frameworks
/// - Compliance with security and data protection standards
/// 
/// Common Implementation Patterns:
/// - Centralized validation logic to eliminate code duplication
/// - Consistent error response formatting across all validation methods
/// - Integration with dependency injection for service composition
/// - Configuration-driven validation rules for flexible business logic
/// - Comprehensive logging and monitoring for operational visibility
/// 
/// Usage Scenarios:
/// - API request validation in controller actions
/// - Business rule enforcement in service layers
/// - Data integrity validation before database operations
/// - File upload security and format validation
/// - User input sanitization and security validation
/// </remarks>
/// <example>
/// <code>
/// // Example implementation usage in a controller
/// [HttpPost]
/// public async Task&lt;IActionResult&gt; CreateDocument(
///     [FromBody] DocumentForCreationDto document)
/// {
///     // Parameter validation
///     var parameterValidation = _validationService.ValidateNotNull(document, nameof(document));
///     if (parameterValidation != null) return parameterValidation;
///     
///     // Object validation
///     var objectValidation = _validationService.ValidateObject(document);
///     if (objectValidation != null) return objectValidation;
///     
///     // Model state validation
///     var modelStateValidation = _validationService.ValidateModelState(ModelState);
///     if (modelStateValidation != null) return modelStateValidation;
///     
///     // Business rule validation
///     var businessValidation = await _validationService.ValidateDocumentForCreationAsync(matterId, document);
///     if (businessValidation.Any())
///     {
///         return BadRequest(CreateValidationResponse(businessValidation));
///     }
///     
///     // Continue with document creation...
///     return Ok();
/// }
/// 
/// // Example service implementation
/// public class ValidationService : IValidationService
/// {
///     public ActionResult? ValidateObject&lt;T&gt;(T obj)
///     {
///         ArgumentNullException.ThrowIfNull(obj);
///         
///         var context = new ValidationContext(obj);
///         var results = new List&lt;ValidationResult&gt;();
///         
///         if (Validator.TryValidateObject(obj, context, results, true))
///             return null;
///             
///         return new BadRequestObjectResult(new ValidationProblemDetails(
///             results.GroupBy(r =&gt; r.MemberNames.FirstOrDefault() ?? string.Empty)
///                    .ToDictionary(g =&gt; g.Key, g =&gt; g.Select(r =&gt; r.ErrorMessage).ToArray())));
///     }
/// }
/// </code>
/// </example>
public interface IValidationService
{
    #region Object and DTO Validation

    /// <summary>
    /// Validates the specified object against its data annotations and custom validation logic with comprehensive error reporting.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object to validate. Must be a reference type or value type with validation attributes.
    /// Common examples include DTO classes, entity classes, or any .NET type with data annotation validation.
    /// </typeparam>
    /// <param name="obj">
    /// The object instance to validate against its validation attributes and IValidatableObject implementation.
    /// Cannot be null - implementations should validate this parameter and provide appropriate error responses.
    /// The object will be validated using its data annotation attributes and any custom validation logic.
    /// </param>
    /// <returns>
    /// A BadRequestObjectResult containing ValidationProblemDetails if validation fails;
    /// null if the object passes all validation rules and is considered valid.
    /// The ValidationProblemDetails follows RFC 7807 format with property-level error grouping.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive object validation including:
    /// 
    /// Validation Types Supported:
    /// - Data annotation validation (Required, Range, MaxLength, RegularExpression, etc.)
    /// - Custom validation through IValidatableObject implementation
    /// - Recursive validation of complex object properties and nested objects
    /// - Property-level validation with detailed error attribution
    /// - Cross-property validation for complex business rules
    /// 
    /// Error Response Format:
    /// - Uses ValidationProblemDetails following RFC 7807 Problem Details specification
    /// - Groups errors by property name for client-side error handling
    /// - Provides detailed error messages for each validation failure
    /// - Includes HTTP status code 400 (Bad Request) for validation errors
    /// - Supports content negotiation for JSON and XML response formats
    /// 
    /// Performance Considerations:
    /// - Efficient validation with minimal performance overhead
    /// - Caches validation metadata where possible for improved performance
    /// - Thread-safe operations suitable for concurrent validation scenarios
    /// - Minimal memory allocation during validation processes
    /// 
    /// The method is commonly used for:
    /// - API request body validation in controller actions
    /// - DTO validation before business logic processing
    /// - Entity validation before database persistence operations
    /// - Complex object validation in service layer operations
    /// - Input validation in data transformation pipelines
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Implementations should handle null objects gracefully by returning appropriate
    /// BadRequestObjectResult rather than throwing exceptions to maintain API consistency.
    /// </exception>
    /// <example>
    /// <code>
    /// // Validate a document creation DTO
    /// var validationResult = _validationService.ValidateObject(documentForCreation);
    /// if (validationResult != null)
    /// {
    ///     // Validation failed - return error response to client
    ///     return validationResult; // Returns BadRequestObjectResult with ValidationProblemDetails
    /// }
    /// 
    /// // Object is valid - continue with processing
    /// var createdDocument = await _documentService.CreateAsync(documentForCreation);
    /// return CreatedAtRoute("GetDocument", new { id = createdDocument.Id }, createdDocument);
    /// </code>
    /// </example>
    ActionResult? ValidateObject<T>(T obj);

    /// <summary>
    /// Validates the provided ASP.NET Core model state dictionary and returns appropriate error responses.
    /// </summary>
    /// <param name="modelState">
    /// The ModelStateDictionary from ASP.NET Core model binding to validate.
    /// Contains model binding errors, validation errors, and any custom errors added during processing.
    /// Cannot be null - implementations should validate this parameter appropriately.
    /// </param>
    /// <returns>
    /// A BadRequestObjectResult containing ValidationProblemDetails if model state is invalid;
    /// null if the model state is valid and contains no errors.
    /// The ValidationProblemDetails includes all model binding and validation errors with proper formatting.
    /// </returns>
    /// <remarks>
    /// This method validates ASP.NET Core model state which captures:
    /// 
    /// Model State Error Types:
    /// - Model binding errors (type conversion failures, format issues)
    /// - Data annotation validation errors from automatic model validation
    /// - Custom validation errors added during model binding process
    /// - Property-level validation errors with specific property attribution
    /// - Model-level validation errors affecting multiple properties
    /// 
    /// Error Response Characteristics:
    /// - Structured error responses with property-level error grouping
    /// - Consistent error formatting using ValidationProblemDetails
    /// - Proper HTTP status codes following REST conventions (400 Bad Request)
    /// - Detailed error messages suitable for client-side error handling
    /// - Integration with ASP.NET Core's built-in validation infrastructure
    /// 
    /// Common Error Scenarios Handled:
    /// - Invalid JSON format in request body causing binding failures
    /// - Type conversion errors (e.g., string to integer conversion failures)
    /// - Missing required properties in JSON request payload
    /// - Format validation failures (e.g., invalid email address format)
    /// - Range validation failures (e.g., negative values where positive required)
    /// - Custom validation errors added by model binding filters
    /// 
    /// Integration Benefits:
    /// - Seamless integration with ASP.NET Core's model binding pipeline
    /// - Consistent error response format across all validation scenarios
    /// - Proper handling of complex validation scenarios and edge cases
    /// - Support for both simple and complex object model validation
    /// - Integration with custom model binding and validation attributes
    /// 
    /// The method is essential for:
    /// - API controller action validation before processing requests
    /// - Comprehensive input validation in web API scenarios
    /// - Integration with ASP.NET Core's automatic model validation
    /// - Consistent error response formatting across API endpoints
    /// - Client-friendly error reporting with actionable information
    /// </remarks>
    /// <example>
    /// <code>
    /// [HttpPost]
    /// public async Task&lt;IActionResult&gt; CreateDocument([FromBody] DocumentForCreationDto document)
    /// {
    ///     // Validate model state from model binding
    ///     var modelStateResult = _validationService.ValidateModelState(ModelState);
    ///     if (modelStateResult != null)
    ///     {
    ///         // Model binding or validation errors occurred
    ///         return modelStateResult; // Returns BadRequestObjectResult with detailed errors
    ///     }
    ///     
    ///     // Model state is valid - continue with business logic
    ///     var result = await _documentService.CreateDocumentAsync(document);
    ///     return CreatedAtRoute("GetDocument", new { id = result.Id }, result);
    /// }
    /// 
    /// // Custom model state error handling
    /// public IActionResult ProcessWithCustomValidation(MyModel model)
    /// {
    ///     // Add custom validation errors to model state
    ///     if (model.StartDate &gt; model.EndDate)
    ///     {
    ///         ModelState.AddModelError("EndDate", "End date must be after start date");
    ///     }
    ///     
    ///     // Validate the updated model state
    ///     var validationResult = _validationService.ValidateModelState(ModelState);
    ///     return validationResult ?? Ok("Validation passed");
    /// }
    /// </code>
    /// </example>
    ActionResult? ValidateModelState(ModelStateDictionary modelState);

    #endregion Object and DTO Validation

    #region Parameter Validation

    /// <summary>
    /// Validates that the provided GUID parameter is not empty and returns appropriate error responses.
    /// </summary>
    /// <param name="id">
    /// The GUID value to validate for emptiness.
    /// Should be a valid GUID that is not equal to Guid.Empty (all zeros).
    /// Commonly used for entity identifiers, route parameters, and foreign key references.
    /// </param>
    /// <param name="parameterName">
    /// The name of the parameter being validated for error reporting and logging.
    /// Used in error messages and diagnostic information to identify which parameter failed validation.
    /// Should match the actual parameter name for consistency and debugging purposes.
    /// </param>
    /// <returns>
    /// A BadRequestObjectResult with Problem Details if the GUID is empty (Guid.Empty);
    /// null if the GUID is valid and not empty.
    /// The error response includes the parameter name and descriptive error message.
    /// </returns>
    /// <remarks>
    /// This method validates GUID parameters which are commonly used as:
    /// 
    /// Common GUID Usage Scenarios:
    /// - Entity identifiers (matterId, documentId, revisionId) in route parameters
    /// - Foreign key references in API request payloads
    /// - Query parameters for resource identification and filtering
    /// - Correlation IDs and tracking identifiers for request tracing
    /// - Session identifiers and security tokens for authentication
    /// 
    /// Validation Characteristics:
    /// - Detects empty GUIDs (Guid.Empty or 00000000-0000-0000-0000-000000000000)
    /// - Provides descriptive error messages with parameter context
    /// - Uses Problem Details format for consistent error responses
    /// - Integrates with structured logging for diagnostic capabilities
    /// - Thread-safe operation suitable for concurrent validation scenarios
    /// 
    /// Error Response Format:
    /// - HTTP 400 Bad Request status code for invalid GUIDs
    /// - Problem Details JSON format following RFC 7807 specification
    /// - Parameter name included in error context for debugging
    /// - Descriptive error message suitable for client-side error handling
    /// - Consistent format with other validation error responses
    /// 
    /// The method is essential for:
    /// - Route parameter validation in RESTful API controllers
    /// - Entity identifier validation before database query operations
    /// - Foreign key validation in entity relationship scenarios
    /// - Request parameter sanitization and security validation
    /// - Preventing downstream errors from empty or invalid identifiers
    /// 
    /// Performance and Security:
    /// - Fast validation with minimal computational overhead
    /// - No database access required for GUID format validation
    /// - Prevents potential security issues from malformed identifiers
    /// - Early validation failure to avoid unnecessary processing
    /// </remarks>
    /// <example>
    /// <code>
    /// [HttpGet("{matterId}/documents/{documentId}")]
    /// public async Task&lt;IActionResult&gt; GetDocument(Guid matterId, Guid documentId)
    /// {
    ///     // Validate route parameters
    ///     var matterIdValidation = _validationService.ValidateGuid(matterId, nameof(matterId));
    ///     if (matterIdValidation != null) return matterIdValidation;
    ///     
    ///     var documentIdValidation = _validationService.ValidateGuid(documentId, nameof(documentId));
    ///     if (documentIdValidation != null) return documentIdValidation;
    ///     
    ///     // Parameters are valid - continue with business logic
    ///     var document = await _documentService.GetAsync(matterId, documentId);
    ///     return Ok(document);
    /// }
    /// 
    /// // Batch parameter validation
    /// public async Task&lt;IActionResult&gt; ProcessMultipleItems(Guid[] itemIds)
    /// {
    ///     for (int i = 0; i &lt; itemIds.Length; i++)
    ///     {
    ///         var validation = _validationService.ValidateGuid(itemIds[i], $"itemIds[{i}]");
    ///         if (validation != null) return validation;
    ///     }
    ///     
    ///     // All IDs are valid
    ///     return await ProcessItems(itemIds);
    /// }
    /// </code>
    /// </example>
    ActionResult? ValidateGuid(Guid id, string parameterName);

    /// <summary>
    /// Validates that the provided string parameter is not null, empty, or whitespace-only.
    /// </summary>
    /// <param name="value">
    /// The string value to validate for null, empty, or whitespace-only content.
    /// Should contain actual meaningful content beyond just whitespace characters.
    /// Common examples include file names, user input, search terms, and configuration values.
    /// </param>
    /// <param name="parameterName">
    /// The name of the parameter being validated for error reporting and logging purposes.
    /// Used in diagnostic logging and error reporting to identify which parameter failed validation.
    /// Should match the actual parameter name for consistency and debugging support.
    /// </param>
    /// <returns>
    /// true if the string contains meaningful content (not null, empty, or whitespace-only);
    /// false if the string is null, empty, or contains only whitespace characters.
    /// Return value can be used directly in conditional logic for validation flow control.
    /// </returns>
    /// <remarks>
    /// This method validates string parameters for various common scenarios:
    /// 
    /// String Validation Scenarios:
    /// - Required text input validation (user names, descriptions, comments)
    /// - File name validation for document upload operations
    /// - Search query validation for non-empty search terms
    /// - Configuration parameter validation for system settings
    /// - User input sanitization for security and data quality
    /// 
    /// Validation Logic:
    /// - Detects null string references
    /// - Identifies empty strings (zero length)
    /// - Recognizes whitespace-only strings (spaces, tabs, newlines)
    /// - Uses String.IsNullOrWhiteSpace() semantics for comprehensive checking
    /// - Provides consistent validation behavior across all string parameters
    /// 
    /// Integration with Logging:
    /// - Logs validation failures with parameter context for debugging
    /// - Provides diagnostic information for troubleshooting validation issues
    /// - Integrates with structured logging frameworks for monitoring
    /// - Supports operational monitoring and alerting for validation patterns
    /// 
    /// Common Usage Patterns:
    /// - Pre-validation of user input before business logic processing
    /// - Required field validation in data entry scenarios
    /// - Configuration validation during application startup
    /// - API parameter validation for non-optional string parameters
    /// - Security validation to prevent injection attacks through empty parameters
    /// 
    /// Performance Characteristics:
    /// - Efficient validation with minimal memory allocation
    /// - Fast execution suitable for high-frequency validation scenarios
    /// - Thread-safe operation for concurrent validation usage
    /// - No external dependencies or database access required
    /// 
    /// The method helps prevent:
    /// - Null reference exceptions in downstream processing
    /// - Business logic errors from empty or meaningless input
    /// - Security vulnerabilities from malformed string parameters
    /// - Data quality issues from whitespace-only content
    /// </remarks>
    /// <example>
    /// <code>
    /// public async Task&lt;IActionResult&gt; SearchDocuments(string searchTerm, string category)
    /// {
    ///     // Validate required string parameters
    ///     if (!_validationService.ValidateStringNotEmpty(searchTerm, nameof(searchTerm)))
    ///     {
    ///         return BadRequest("Search term is required and cannot be empty");
    ///     }
    ///     
    ///     if (!_validationService.ValidateStringNotEmpty(category, nameof(category)))
    ///     {
    ///         return BadRequest("Category is required and cannot be empty");
    ///     }
    ///     
    ///     // Parameters are valid - perform search
    ///     var results = await _searchService.SearchAsync(searchTerm, category);
    ///     return Ok(results);
    /// }
    /// 
    /// // Configuration validation example
    /// public void ValidateConfiguration(AppConfiguration config)
    /// {
    ///     var errors = new List&lt;string&gt;();
    ///     
    ///     if (!_validationService.ValidateStringNotEmpty(config.DatabaseConnectionString, nameof(config.DatabaseConnectionString)))
    ///         errors.Add("Database connection string is required");
    ///         
    ///     if (!_validationService.ValidateStringNotEmpty(config.ApiKey, nameof(config.ApiKey)))
    ///         errors.Add("API key is required");
    ///     
    ///     if (errors.Any())
    ///         throw new InvalidOperationException($"Configuration validation failed: {string.Join(", ", errors)}");
    /// }
    /// </code>
    /// </example>
    bool ValidateStringNotEmpty(string value, string parameterName);

    /// <summary>
    /// Validates that the provided object parameter is not null and returns appropriate error responses.
    /// </summary>
    /// <param name="obj">
    /// The object instance to validate for null reference.
    /// Can be any reference type including DTOs, entities, collections, or custom objects.
    /// Null values will trigger validation failure with appropriate error responses.
    /// </param>
    /// <param name="parameterName">
    /// The name of the parameter being validated for error reporting and diagnostic purposes.
    /// Used in error messages and logging to identify which parameter failed null validation.
    /// Should match the actual parameter name for consistency and debugging support.
    /// </param>
    /// <returns>
    /// A BadRequestObjectResult with Problem Details if the object is null;
    /// null if the object reference is valid and not null.
    /// The error response includes parameter context and descriptive error messaging.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive null validation for various object types:
    /// 
    /// Null Validation Scenarios:
    /// - DTO parameter validation in API controller actions
    /// - Entity validation before database persistence operations
    /// - Service method parameter validation for required objects
    /// - Collection validation before enumeration or processing operations
    /// - Configuration object validation during application initialization
    /// 
    /// Error Response Characteristics:
    /// - Uses Problem Details format following RFC 7807 specification
    /// - HTTP 400 Bad Request status code for null parameter errors
    /// - Parameter name included in error context for debugging support
    /// - Descriptive error messages suitable for client-side error handling
    /// - Consistent formatting with other validation error responses
    /// 
    /// Integration Benefits:
    /// - Seamless integration with ASP.NET Core controller validation patterns
    /// - Consistent error response format across all validation scenarios
    /// - Support for automatic model binding validation workflows
    /// - Integration with logging infrastructure for diagnostic capabilities
    /// - Thread-safe operation suitable for concurrent validation scenarios
    /// 
    /// Common Usage Patterns:
    /// - API request body validation before processing
    /// - Service method parameter validation for defensive programming
    /// - Entity validation before persistence operations
    /// - Configuration object validation during dependency injection
    /// - Collection validation before LINQ operations or enumeration
    /// 
    /// The method helps prevent:
    /// - Null reference exceptions in downstream processing logic
    /// - Unexpected behavior from null parameters in business logic
    /// - Security vulnerabilities from malformed or missing request data
    /// - Data integrity issues from null entity references
    /// - Runtime errors in collection processing and enumeration scenarios
    /// 
    /// Performance and Reliability:
    /// - Fast validation with minimal computational overhead
    /// - No external dependencies or database access required
    /// - Early validation failure to prevent unnecessary processing
    /// - Consistent behavior across all object types and scenarios
    /// </remarks>
    /// <example>
    /// <code>
    /// [HttpPost]
    /// public async Task&lt;IActionResult&gt; CreateDocument([FromBody] DocumentForCreationDto document)
    /// {
    ///     // Validate request body is not null
    ///     var nullValidation = _validationService.ValidateNotNull(document, nameof(document));
    ///     if (nullValidation != null)
    ///     {
    ///         // Returns BadRequestObjectResult with Problem Details
    ///         return nullValidation;
    ///     }
    ///     
    ///     // Object is not null - continue with additional validation
    ///     var objectValidation = _validationService.ValidateObject(document);
    ///     if (objectValidation != null) return objectValidation;
    ///     
    ///     // All validation passed - process the request
    ///     var result = await _documentService.CreateAsync(document);
    ///     return CreatedAtRoute("GetDocument", new { id = result.Id }, result);
    /// }
    /// 
    /// // Service method parameter validation
    /// public async Task&lt;ProcessingResult&gt; ProcessCollection(IEnumerable&lt;Item&gt; items)
    /// {
    ///     var validation = _validationService.ValidateNotNull(items, nameof(items));
    ///     if (validation != null)
    ///     {
    ///         throw new ArgumentException("Items collection cannot be null", nameof(items));
    ///     }
    ///     
    ///     // Collection is not null - safe to enumerate
    ///     return await ProcessItemsAsync(items);
    /// }
    /// </code>
    /// </example>
    ActionResult? ValidateNotNull(object? obj, string parameterName);

    #endregion Parameter Validation

    #region Entity Existence Validation

    /// <summary>
    /// Validates asynchronously whether the specified matter exists in the database.
    /// </summary>
    /// <param name="matterId">
    /// The unique identifier of the matter to validate for existence.
    /// Must be a valid GUID that corresponds to an existing matter record in the database.
    /// Used as the primary key for matter lookup operations.
    /// </param>
    /// <returns>
    /// A Task that represents the asynchronous validation operation.
    /// Task result is null if the matter exists in the database;
    /// NotFoundObjectResult with Problem Details if the matter does not exist or cannot be accessed.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive matter existence validation including:
    /// 
    /// Database Validation Process:
    /// - Executes asynchronous database query to verify matter existence
    /// - Uses efficient existence checking to minimize database overhead
    /// - Handles database connectivity issues and timeout scenarios gracefully
    /// - Provides detailed error information for troubleshooting database issues
    /// - Integrates with database connection pooling and resource management
    /// 
    /// Error Response Characteristics:
    /// - HTTP 404 Not Found status code for non-existent matters
    /// - Problem Details format following RFC 7807 specification for consistency
    /// - Matter ID included in error context for debugging and logging
    /// - Descriptive error messages suitable for client-side error handling
    /// - Structured logging for audit trails and operational monitoring
    /// 
    /// Performance Considerations:
    /// - Asynchronous operation to prevent blocking of calling threads
    /// - Optimized database queries for fast existence checking
    /// - Connection pooling support for high-concurrency scenarios
    /// - Minimal resource usage with efficient query execution
    /// - Caching integration where appropriate for frequently accessed matters
    /// 
    /// Security and Authorization:
    /// - Validates matter accessibility based on current security context
    /// - Prevents information disclosure about non-existent entities
    /// - Integrates with authorization policies and access control
    /// - Audit logging for matter access attempts and validation results
    /// - Support for tenant isolation and multi-tenancy scenarios
    /// 
    /// Integration Scenarios:
    /// - Route parameter validation in matter-scoped API endpoints
    /// - Foreign key validation before document creation operations
    /// - Authorization prerequisite checking for matter-related operations
    /// - Data integrity validation in business logic workflows
    /// - Cascade validation for dependent entity operations
    /// 
    /// The method is essential for:
    /// - RESTful API parameter validation (e.g., /api/matters/{matterId}/documents)
    /// - Business rule enforcement for matter-scoped operations
    /// - Data integrity validation before related entity operations
    /// - Security validation for matter access control
    /// - Audit trail creation for matter access and validation activities
    /// </remarks>
    /// <example>
    /// <code>
    /// [HttpGet("{matterId}/documents")]
    /// public async Task&lt;IActionResult&gt; GetDocuments(Guid matterId)
    /// {
    ///     // Validate matter exists before retrieving documents
    ///     var matterValidation = await _validationService.ValidateMatterExistsAsync(matterId);
    ///     if (matterValidation != null)
    ///     {
    ///         // Matter not found - return 404 with Problem Details
    ///         return matterValidation;
    ///     }
    ///     
    ///     // Matter exists - retrieve documents
    ///     var documents = await _documentService.GetDocumentsByMatterAsync(matterId);
    ///     return Ok(documents);
    /// }
    /// 
    /// // Business logic validation example
    /// public async Task&lt;ValidationResult&gt; ValidateDocumentCreation(Guid matterId, DocumentForCreationDto document)
    /// {
    ///     // Ensure matter exists before creating document
    ///     var matterExists = await _validationService.ValidateMatterExistsAsync(matterId);
    ///     if (matterExists != null)
    ///     {
    ///         return ValidationResult.Failed("Cannot create document for non-existent matter");
    ///     }
    ///     
    ///     // Continue with document-specific validation
    ///     return await ValidateDocumentBusinessRules(matterId, document);
    /// }
    /// </code>
    /// </example>
    Task<ActionResult?> ValidateMatterExistsAsync(Guid matterId);

    /// <summary>
    /// Validates asynchronously whether the specified document exists in the database.
    /// </summary>
    /// <param name="documentId">
    /// The unique identifier of the document to validate for existence.
    /// Must be a valid GUID that corresponds to an existing document record in the database.
    /// Used as the primary key for document lookup operations.
    /// </param>
    /// <returns>
    /// A Task that represents the asynchronous validation operation.
    /// Task result is null if the document exists in the database;
    /// NotFoundObjectResult with Problem Details if the document does not exist or cannot be accessed.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive document existence validation including:
    /// 
    /// Database Validation Features:
    /// - Asynchronous database query execution for document existence checking
    /// - Efficient query optimization to minimize database resource usage
    /// - Integration with Entity Framework Core for robust data access
    /// - Connection pooling and resource management for scalability
    /// - Transaction support where required for data consistency
    /// 
    /// Error Response Management:
    /// - HTTP 404 Not Found status code for missing documents
    /// - Problem Details format compliance with RFC 7807 specification
    /// - Document ID context included in error responses for debugging
    /// - Structured error information suitable for client-side error handling
    /// - Comprehensive logging for audit trails and troubleshooting
    /// 
    /// Performance Optimization:
    /// - Asynchronous operations to maintain application responsiveness
    /// - Optimized existence queries without unnecessary data retrieval
    /// - Database connection efficiency and connection pooling support
    /// - Minimal memory allocation during validation operations
    /// - Integration with caching strategies for frequently accessed documents
    /// 
    /// Security and Access Control:
    /// - Document accessibility validation based on security context
    /// - Authorization integration for document access permissions
    /// - Tenant isolation support for multi-tenant applications
    /// - Audit logging for document access validation activities
    /// - Prevention of information disclosure through existence checking
    /// 
    /// Common Usage Scenarios:
    /// - Document-scoped API endpoint parameter validation
    /// - Prerequisite checking before document update or deletion operations
    /// - Revision creation validation to ensure parent document exists
    /// - File operation validation to verify document context
    /// - Business rule enforcement for document-dependent operations
    /// 
    /// Integration Benefits:
    /// - Seamless integration with document management workflows
    /// - Support for complex document hierarchy and relationship validation
    /// - Integration with document lifecycle management processes
    /// - Compatibility with document versioning and revision control
    /// - Support for document metadata and classification validation
    /// </remarks>
    /// <example>
    /// <code>
    /// [HttpPut("{matterId}/documents/{documentId}")]
    /// public async Task&lt;IActionResult&gt; UpdateDocument(
    ///     Guid matterId, 
    ///     Guid documentId, 
    ///     [FromBody] DocumentForUpdateDto document)
    /// {
    ///     // Validate document exists before updating
    ///     var documentValidation = await _validationService.ValidateDocumentExistsAsync(documentId);
    ///     if (documentValidation != null)
    ///     {
    ///         return documentValidation; // Returns 404 NotFound with Problem Details
    ///     }
    ///     
    ///     // Document exists - continue with update operation
    ///     var result = await _documentService.UpdateAsync(documentId, document);
    ///     return Ok(result);
    /// }
    /// 
    /// // Cascade validation example
    /// public async Task&lt;IActionResult&gt; CreateRevision(
    ///     Guid matterId, 
    ///     Guid documentId, 
    ///     RevisionForCreationDto revision)
    /// {
    ///     // Validate parent document exists before creating revision
    ///     var documentExists = await _validationService.ValidateDocumentExistsAsync(documentId);
    ///     if (documentExists != null) return documentExists;
    ///     
    ///     // Create revision for existing document
    ///     var result = await _revisionService.CreateAsync(documentId, revision);
    ///     return CreatedAtRoute("GetRevision", new { matterId, documentId, revisionId = result.Id }, result);
    /// }
    /// </code>
    /// </example>
    Task<ActionResult?> ValidateDocumentExistsAsync(Guid documentId);

    /// <summary>
    /// Validates asynchronously whether the specified revision exists in the database.
    /// </summary>
    /// <param name="revisionId">
    /// The unique identifier of the revision to validate for existence.
    /// Must be a valid GUID that corresponds to an existing revision record in the database.
    /// Used as the primary key for revision lookup operations in document version control.
    /// </param>
    /// <returns>
    /// A Task that represents the asynchronous validation operation.
    /// Task result is null if the revision exists in the database;
    /// NotFoundObjectResult with Problem Details if the revision does not exist or cannot be accessed.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive revision existence validation including:
    /// 
    /// Revision-Specific Validation:
    /// - Asynchronous database query for revision existence verification
    /// - Integration with document version control and revision management systems
    /// - Support for revision history and audit trail validation
    /// - Efficient query execution optimized for revision table structure
    /// - Integration with document hierarchy and relationship validation
    /// 
    /// Version Control Integration:
    /// - Validates revision existence within document version control context
    /// - Supports document revision history and branching scenarios
    /// - Integration with document lifecycle and approval workflows
    /// - Validation of revision accessibility and permissions
    /// - Support for revision-specific metadata and classification
    /// 
    /// Database Performance:
    /// - Optimized queries for revision table structure and indexing
    /// - Efficient existence checking without unnecessary data retrieval
    /// - Connection pooling and resource management for scalability
    /// - Transaction support where required for consistency
    /// - Integration with caching strategies for active revisions
    /// 
    /// Error Handling and Responses:
    /// - HTTP 404 Not Found for non-existent revisions
    /// - Problem Details format with revision context information
    /// - Structured error responses for client-side error handling
    /// - Comprehensive logging for revision access validation
    /// - Integration with audit trail and compliance reporting
    /// 
    /// Security and Access Control:
    /// - Revision accessibility validation based on security context
    /// - Integration with document-level access control policies
    /// - Support for revision-specific permissions and authorization
    /// - Audit logging for revision access and validation activities
    /// - Prevention of unauthorized revision access through validation
    /// 
    /// Common Usage Scenarios:
    /// - Revision-specific API endpoint parameter validation
    /// - File download validation for specific document revisions
    /// - Revision update and deletion prerequisite checking
    /// - Audit trail and history access validation
    /// - Business rule enforcement for revision-dependent operations
    /// 
    /// The method is crucial for:
    /// - Document version control and revision management operations
    /// - File access control and download authorization
    /// - Revision history and audit trail functionality
    /// - Integration with document approval and workflow systems
    /// - Compliance and regulatory reporting for document changes
    /// </remarks>
    /// <example>
    /// <code>
    /// [HttpGet("{matterId}/documents/{documentId}/revisions/{revisionId}")]
    /// public async Task&lt;IActionResult&gt; GetRevision(
    ///     Guid matterId, 
    ///     Guid documentId, 
    ///     Guid revisionId)
    /// {
    ///     // Validate revision exists before retrieval
    ///     var revisionValidation = await _validationService.ValidateRevisionExistsAsync(revisionId);
    ///     if (revisionValidation != null)
    ///     {
    ///         return revisionValidation; // Returns 404 NotFound with Problem Details
    ///     }
    ///     
    ///     // Revision exists - retrieve and return
    ///     var revision = await _revisionService.GetAsync(revisionId);
    ///     return Ok(revision);
    /// }
    /// 
    /// // File download validation example
    /// [HttpGet("{matterId}/documents/{documentId}/revisions/{revisionId}/download")]
    /// public async Task&lt;IActionResult&gt; DownloadRevisionFile(
    ///     Guid matterId, 
    ///     Guid documentId, 
    ///     Guid revisionId)
    /// {
    ///     // Validate revision exists before file download
    ///     var revisionExists = await _validationService.ValidateRevisionExistsAsync(revisionId);
    ///     if (revisionExists != null) return revisionExists;
    ///     
    ///     // Revision exists - proceed with file download
    ///     var fileStream = await _fileService.GetRevisionFileAsync(revisionId);
    ///     var fileName = await _revisionService.GetFileNameAsync(revisionId);
    ///     
    ///     return File(fileStream, "application/octet-stream", fileName);
    /// }
    /// </code>
    /// </example>
    Task<ActionResult?> ValidateRevisionExistsAsync(Guid revisionId);

    #endregion Entity Existence Validation

    #region Business Rule Validation

    /// <summary>
    /// Validates a document for creation operations with comprehensive business rule checking and security validation.
    /// </summary>
    /// <param name="matterId">
    /// The unique identifier of the matter where the document will be created.
    /// Must be a valid GUID corresponding to an existing matter in the database.
    /// Used for matter-specific business rule validation and document placement.
    /// </param>
    /// <param name="document">
    /// The document creation data transfer object containing all information needed to create a new document.
    /// Cannot be null and must contain valid document metadata including file information.
    /// Will be validated against business rules, security policies, and data integrity requirements.
    /// </param>
    /// <returns>
    /// A Task that represents the asynchronous validation operation.
    /// Task result contains an enumerable collection of ValidationResult objects.
    /// An empty collection indicates successful validation; non-empty collection contains validation errors.
    /// Each ValidationResult includes error messages and affected property names for client-side handling.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive document creation validation including:
    /// 
    /// File and Content Validation:
    /// - File name presence, format, and length validation within system limits
    /// - File extension validation against allowed types and security policies
    /// - MIME type validation and consistency checking with file extension
    /// - File size validation within acceptable limits and storage constraints
    /// - Checksum validation for data integrity and file verification
    /// - Content validation to prevent malicious file uploads and security threats
    /// 
    /// Business Rule Validation:
    /// - Duplicate file name detection within matter scope to prevent conflicts
    /// - File type restrictions based on matter type and security policies
    /// - Storage quota validation and capacity management
    /// - Document classification and metadata requirements
    /// - Workflow integration and approval process requirements
    /// - Compliance validation for regulatory and legal requirements
    /// 
    /// Security Validation:
    /// - File extension allow-list enforcement for security protection
    /// - MIME type validation against known safe types and attack vectors
    /// - File name sanitization and security checking for injection prevention
    /// - Content-based file type verification and malicious content detection
    /// - Virus scanning integration where required for file security
    /// - Access control validation for document creation permissions
    /// 
    /// Data Integrity Validation:
    /// - Required field validation for essential document metadata
    /// - Data format validation for structured information
    /// - Referential integrity checking for related entities
    /// - Consistency validation across multiple document properties
    /// - Business rule enforcement for document lifecycle management
    /// - Audit trail and compliance information validation
    /// 
    /// Performance Considerations:
    /// - Asynchronous validation for database operations and external service calls
    /// - Efficient validation sequencing to minimize resource usage
    /// - Early validation failure to prevent unnecessary processing overhead
    /// - Integration with caching strategies for validation rule lookup
    /// - Optimized database queries for business rule verification
    /// 
    /// The validation process follows a comprehensive approach:
    /// 1. Parameter validation (matter ID, document DTO null checking)
    /// 2. Basic format validation (file name, extension, MIME type)
    /// 3. Security validation (file type restrictions, content scanning)
    /// 4. Business rule validation (duplicates, quotas, workflows)
    /// 5. Data integrity validation (consistency, referential integrity)
    /// 6. Compliance validation (regulatory requirements, policies)
    /// 
    /// Integration with validation infrastructure:
    /// - Uses ValidationResult objects for structured error reporting
    /// - Integrates with logging infrastructure for audit and debugging
    /// - Supports localization for multi-language error messages
    /// - Compatible with client-side validation frameworks
    /// - Integration with monitoring and alerting for validation patterns
    /// </remarks>
    /// <example>
    /// <code>
    /// [HttpPost("{matterId}/documents")]
    /// public async Task&lt;IActionResult&gt; CreateDocument(
    ///     Guid matterId, 
    ///     [FromBody] DocumentForCreationDto document)
    /// {
    ///     // Comprehensive document creation validation
    ///     var validationResults = await _validationService.ValidateDocumentForCreationAsync(matterId, document);
    ///     var errors = validationResults.ToList();
    ///     
    ///     if (errors.Any())
    ///     {
    ///         // Create structured error response
    ///         var problemDetails = new ValidationProblemDetails();
    ///         
    ///         foreach (var error in errors)
    ///         {
    ///             var propertyName = error.MemberNames.FirstOrDefault() ?? "General";
    ///             if (!problemDetails.Errors.ContainsKey(propertyName))
    ///                 problemDetails.Errors[propertyName] = new string[0];
    ///                 
    ///             problemDetails.Errors[propertyName] = problemDetails.Errors[propertyName]
    ///                 .Concat(new[] { error.ErrorMessage }).ToArray();
    ///         }
    ///         
    ///         return BadRequest(problemDetails);
    ///     }
    ///     
    ///     // Validation passed - create document
    ///     var createdDocument = await _documentService.CreateAsync(matterId, document);
    ///     return CreatedAtRoute("GetDocument", 
    ///         new { matterId, documentId = createdDocument.Id }, 
    ///         createdDocument);
    /// }
    /// 
    /// // Business service integration example
    /// public async Task&lt;BusinessResult&gt; ProcessDocumentUpload(
    ///     Guid matterId, 
    ///     DocumentForCreationDto document, 
    ///     Stream fileContent)
    /// {
    ///     // Validate document before processing file upload
    ///     var validationResults = await _validationService.ValidateDocumentForCreationAsync(matterId, document);
    ///     
    ///     if (validationResults.Any())
    ///     {
    ///         return BusinessResult.ValidationFailed(validationResults);
    ///     }
    ///     
    ///     // Validation passed - proceed with secure file processing
    ///     return await ProcessSecureFileUpload(matterId, document, fileContent);
    /// }
    /// </code>
    /// </example>
    Task<IEnumerable<ValidationResult>> ValidateDocumentForCreationAsync(Guid matterId, DocumentForCreationDto? document);

    /// <summary>
    /// Validates a document for update operations with comprehensive business rule checking and state validation.
    /// </summary>
    /// <param name="document">
    /// The document update data transfer object containing the updated document information to validate.
    /// Cannot be null and must contain all required fields for document update operations.
    /// Will be validated against business rules, data consistency requirements, and state transition rules.
    /// </param>
    /// <returns>
    /// An enumerable collection of ValidationResult objects containing any validation errors found.
    /// An empty collection indicates successful validation and the document can be safely updated.
    /// Each ValidationResult includes detailed error messages and affected property names for error handling.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive document update validation including:
    /// 
    /// Content and Format Validation:
    /// - File name presence, format requirements, and length constraints
    /// - File extension format validation and security checking
    /// - MIME type format validation and consistency verification
    /// - File size validation within acceptable ranges and system limits
    /// - Checksum integrity validation for data consistency verification
    /// - Content format validation for structured document information
    /// 
    /// Business Rule Validation:
    /// - Document state consistency checking (deleted vs checked out scenarios)
    /// - Update permission validation based on current document state
    /// - File content consistency validation and version control integration
    /// - Workflow state validation for document approval processes
    /// - Retention policy validation and compliance requirements
    /// - Document classification and metadata consistency checking
    /// 
    /// State Transition Validation:
    /// - Prevents documents from being both deleted and checked out simultaneously
    /// - Validates state transitions for document lifecycle management
    /// - Ensures consistent document status across all operations
    /// - Validates user permissions for specific state changes
    /// - Integration with approval workflows and business process validation
    /// - Compliance validation for document state change requirements
    /// 
    /// Data Integrity Validation:
    /// - Required field validation for essential document metadata
    /// - Cross-field validation for interdependent properties
    /// - Referential integrity checking for related entities and relationships
    /// - Consistency validation across document versions and revisions
    /// - Business rule enforcement for document management policies
    /// - Audit trail and compliance information validation
    /// 
    /// Security and Permission Validation:
    /// - Update permission validation based on security context
    /// - Document access control and authorization checking
    /// - Content modification validation for sensitive documents
    /// - Classification level validation for security requirements
    /// - Compliance validation for regulatory and legal constraints
    /// - Integration with audit logging for security monitoring
    /// 
    /// Performance and Efficiency:
    /// - Efficient validation logic optimized for frequent update operations
    /// - Early validation failure to prevent unnecessary processing
    /// - Minimal database access for validation rule verification
    /// - Integration with caching for frequently validated business rules
    /// - Optimized error collection and reporting for user experience
    /// 
    /// The validation ensures:
    /// - Data integrity during document update operations
    /// - Business rule compliance for document management workflows
    /// - Security validation for file content and metadata changes
    /// - Consistent document state management across the system
    /// - Compliance with regulatory and organizational policies
    /// 
    /// This method is critical for maintaining:
    /// - Document integrity and consistency in the management system
    /// - Business rule enforcement during document lifecycle operations
    /// - Security compliance for document modification and access control
    /// - Data quality and consistency across document management workflows
    /// - Integration with enterprise document management and compliance systems
    /// </remarks>
    /// <example>
    /// <code>
    /// [HttpPut("{matterId}/documents/{documentId}")]
    /// public async Task&lt;IActionResult&gt; UpdateDocument(
    ///     Guid matterId, 
    ///     Guid documentId, 
    ///     [FromBody] DocumentForUpdateDto document)
    /// {
    ///     // Comprehensive document update validation
    ///     var validationResults = _validationService.ValidateDocumentForUpdate(document);
    ///     var errors = validationResults.ToList();
    ///     
    ///     if (errors.Any())
    ///     {
    ///         // Create detailed validation error response
    ///         var errorResponse = new ValidationProblemDetails();
    ///         
    ///         foreach (var error in errors)
    ///         {
    ///             var propertyName = error.MemberNames.FirstOrDefault() ?? "General";
    ///             
    ///             if (!errorResponse.Errors.ContainsKey(propertyName))
    ///                 errorResponse.Errors[propertyName] = new List&lt;string&gt;();
    ///                 
    ///             ((List&lt;string&gt;)errorResponse.Errors[propertyName]).Add(error.ErrorMessage ?? "Validation error");
    ///         }
    ///         
    ///         _logger.LogWarning("Document update validation failed for document {DocumentId}: {ErrorCount} errors",
    ///             documentId, errors.Count);
    ///             
    ///         return BadRequest(errorResponse);
    ///     }
    ///     
    ///     // Validation passed - proceed with document update
    ///     var updatedDocument = await _documentService.UpdateAsync(documentId, document);
    ///     
    ///     _logger.LogInformation("Document {DocumentId} updated successfully", documentId);
    ///     return Ok(updatedDocument);
    /// }
    /// 
    /// // Batch update validation example
    /// public async Task&lt;BatchOperationResult&gt; UpdateMultipleDocuments(
    ///     Dictionary&lt;Guid, DocumentForUpdateDto&gt; documentUpdates)
    /// {
    ///     var results = new BatchOperationResult();
    ///     
    ///     foreach (var kvp in documentUpdates)
    ///     {
    ///         var documentId = kvp.Key;
    ///         var document = kvp.Value;
    ///         
    ///         // Validate each document update
    ///         var validationResults = _validationService.ValidateDocumentForUpdate(document);
    ///         var errors = validationResults.ToList();
    ///         
    ///         if (errors.Any())
    ///         {
    ///             results.AddFailure(documentId, "Validation failed", errors);
    ///         }
    ///         else
    ///         {
    ///             // Validation passed - add to successful updates
    ///             results.AddSuccess(documentId);
    ///         }
    ///     }
    ///     
    ///     return results;
    /// }
    /// </code>
    /// </example>
    IEnumerable<ValidationResult> ValidateDocumentForUpdate(DocumentForUpdateDto document);

    #endregion Business Rule Validation

    #region Format Validation

    /// <summary>
    /// Validates whether the provided string represents a valid email address format.
    /// </summary>
    /// <param name="email">
    /// The email address string to validate for proper format and structure.
    /// Should conform to standard email address format (local-part@domain.tld).
    /// Can be null or empty - implementations should handle these cases appropriately.
    /// </param>
    /// <param name="parameterName">
    /// The name of the parameter being validated for error reporting and logging purposes.
    /// Used in diagnostic logging and error reporting to identify which parameter failed validation.
    /// Should match the actual parameter name for consistency and debugging support.
    /// </param>
    /// <returns>
    /// true if the email address format is valid according to standard email validation rules;
    /// false if the email format is invalid, null, empty, or fails validation checks.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive email validation including:
    /// 
    /// Email Format Validation:
    /// - Basic RFC 822 format compliance checking for standard email structure
    /// - Local part validation (portion before @) for acceptable characters and length
    /// - Domain part validation (portion after @) for proper domain format
    /// - Top-level domain (TLD) validation for acceptable domain extensions
    /// - Special character handling and escape sequence validation where applicable
    /// 
    /// Security Validation Features:
    /// - Prevention of email header injection attacks through format validation
    /// - Validation against malicious email format strings and attack patterns
    /// - Protection against SMTP injection attempts through input sanitization
    /// - Ensures proper email format for system notifications and communications
    /// - Integration with anti-spam and email security best practices
    /// 
    /// Format Support and Compatibility:
    /// - Support for international domain names and Unicode characters
    /// - Validation of common email address formats used in business scenarios
    /// - Handling of plus addressing and sub-addressing formats where appropriate
    /// - Compatibility with modern email systems and address formats
    /// - Support for quoted local parts and special character scenarios
    /// 
    /// Performance and Efficiency:
    /// - Efficient regex-based validation for fast email format checking
    /// - Compiled regular expressions for optimal performance in high-frequency scenarios
    /// - Minimal memory allocation during validation operations
    /// - Thread-safe validation suitable for concurrent email validation
    /// - Integration with caching strategies for frequently validated patterns
    /// 
    /// Error Handling and Logging:
    /// - Comprehensive logging of email validation failures for debugging
    /// - Parameter context information for troubleshooting validation issues
    /// - Integration with structured logging frameworks for monitoring
    /// - Diagnostic information for email format validation patterns
    /// - Support for validation metrics collection and analysis
    /// 
    /// The validation covers common scenarios including:
    /// - Standard business email addresses (user@company.com)
    /// - Personal email addresses with various providers
    /// - Educational and government email formats
    /// - International email addresses with Unicode domains
    /// - Subaddressing and alias formats where supported
    /// 
    /// Note: While comprehensive, this validation focuses on format correctness
    /// rather than email deliverability or existence verification. For production
    /// email validation, consider additional verification steps such as:
    /// - SMTP verification for email deliverability
    /// - Domain MX record validation for mail server existence
    /// - Email verification services for comprehensive validation
    /// - Integration with email reputation and blacklist services
    /// </remarks>
    /// <example>
    /// <code>
    /// // Basic email validation in user registration
    /// public async Task&lt;IActionResult&gt; RegisterUser(UserRegistrationDto registration)
    /// {
    ///     if (!_validationService.ValidateEmail(registration.Email, nameof(registration.Email)))
    ///     {
    ///         return BadRequest("Please provide a valid email address");
    ///     }
    ///     
    ///     // Email format is valid - continue with registration
    ///     var user = await _userService.CreateAsync(registration);
    ///     return Ok(user);
    /// }
    /// 
    /// // Bulk email validation for mailing list
    /// public ValidationResult ValidateEmailList(List&lt;string&gt; emailAddresses)
    /// {
    ///     var invalidEmails = new List&lt;string&gt;();
    ///     
    ///     foreach (var email in emailAddresses)
    ///     {
    ///         if (!_validationService.ValidateEmail(email, "EmailAddress"))
    ///         {
    ///             invalidEmails.Add(email);
    ///         }
    ///     }
    ///     
    ///     if (invalidEmails.Any())
    ///     {
    ///         return ValidationResult.Failed(
    ///             $"Invalid email addresses: {string.Join(", ", invalidEmails)}");
    ///     }
    ///     
    ///     return ValidationResult.Success();
    /// }
    /// </code>
    /// </example>
    bool ValidateEmail(string email, string parameterName);

    /// <summary>
    /// Validates whether the provided string represents a valid file extension format.
    /// </summary>
    /// <param name="extension">
    /// The file extension string to validate for proper format and security compliance.
    /// Should be a standard file extension format (with or without leading dot).
    /// Common examples include "txt", ".pdf", "docx", ".jpg", etc.
    /// </param>
    /// <param name="parameterName">
    /// The name of the parameter being validated for error reporting and logging purposes.
    /// Used in diagnostic logging and error reporting to identify which parameter failed validation.
    /// Should match the actual parameter name for consistency and debugging support.
    /// </param>
    /// <returns>
    /// true if the file extension format is valid and meets security requirements;
    /// false if the extension format is invalid, potentially unsafe, or fails validation checks.
    /// </returns>
    /// <remarks>
    /// This method validates file extensions for multiple important purposes:
    /// 
    /// Security Validation:
    /// - Prevention of malicious file uploads through extension validation
    /// - Blocking of executable file extensions that could pose security risks
    /// - Validation against known dangerous file types and attack vectors
    /// - Protection against file type spoofing and masquerading attacks
    /// - Integration with file type allow-lists and security policies
    /// 
    /// Format Consistency:
    /// - Ensures consistent file extension format across the application
    /// - Validates extension length within reasonable limits (typically 2-5 characters)
    /// - Supports both dotted (.txt) and non-dotted (txt) extension formats
    /// - Alphanumeric character validation for standard file extensions
    /// - Prevention of special characters that could cause file system issues
    /// 
    /// File Type Management:
    /// - Supports file type identification and handling based on extension
    /// - Integration with MIME type validation and content type detection
    /// - Database storage requirements and field length constraints
    /// - File processing pipeline integration for type-specific handling
    /// - Content delivery and download functionality support
    /// 
    /// Business Rule Integration:
    /// - Validation against organization-specific file type policies
    /// - Integration with document management classification systems
    /// - Support for workflow-specific file type requirements
    /// - Compliance validation for regulatory file type restrictions
    /// - Integration with storage and archival policies based on file types
    /// 
    /// Performance Features:
    /// - Efficient regex-based validation for fast extension checking
    /// - Compiled regular expressions for optimal performance
    /// - Thread-safe validation suitable for concurrent file processing
    /// - Minimal memory allocation during validation operations
    /// - Integration with caching strategies for validation rule lookup
    /// 
    /// The validation supports common file extension scenarios:
    /// - Standard document formats (.pdf, .docx, .xlsx, .pptx)
    /// - Image formats (.jpg, .png, .gif, .bmp, .tiff)
    /// - Text formats (.txt, .rtf, .csv, .xml, .json)
    /// - Archive formats (.zip, .rar, .7z, .tar, .gz)
    /// - Multimedia formats (.mp3, .mp4, .avi, .wav)
    /// 
    /// Security considerations include:
    /// - Blocking of executable extensions (.exe, .bat, .cmd, .scr)
    /// - Prevention of script file uploads (.js, .vbs, .ps1, .sh)
    /// - Protection against double extensions and file system attacks
    /// - Integration with antivirus and malware scanning systems
    /// - Compliance with organizational security policies
    /// 
    /// The method provides robust file extension validation while maintaining
    /// flexibility for legitimate business file types and use cases.
    /// </remarks>
    /// <example>
    /// <code>
    /// // File upload validation with extension checking
    /// [HttpPost("upload")]
    /// public async Task&lt;IActionResult&gt; UploadFile(IFormFile file)
    /// {
    ///     var extension = Path.GetExtension(file.FileName);
    ///     
    ///     if (!_validationService.ValidateFileExtension(extension, nameof(extension)))
    ///     {
    ///         return BadRequest($"File extension '{extension}' is not allowed or invalid format");
    ///     }
    ///     
    ///     // Extension is valid - proceed with file processing
    ///     var result = await _fileService.ProcessUploadAsync(file);
    ///     return Ok(result);
    /// }
    /// 
    /// // Document creation with extension validation
    /// public async Task&lt;ValidationResult&gt; ValidateDocumentCreation(DocumentForCreationDto document)
    /// {
    ///     var errors = new List&lt;string&gt;();
    ///     
    ///     // Validate file extension format and security
    ///     if (!_validationService.ValidateFileExtension(document.Extension, nameof(document.Extension)))
    ///     {
    ///         errors.Add($"File extension '{document.Extension}' is invalid or not allowed");
    ///     }
    ///     
    ///     // Additional business rule validation...
    ///     return errors.Any() ? ValidationResult.Failed(errors) : ValidationResult.Success();
    /// }
    /// </code>
    /// </example>
    bool ValidateFileExtension(string extension, string parameterName);

    /// <summary>
    /// Validates whether the provided string represents a valid MIME type format.
    /// </summary>
    /// <param name="mimeType">
    /// The MIME type string to validate for proper format and structure.
    /// Should conform to standard MIME type format (type/subtype).
    /// Common examples include "text/plain", "application/pdf", "image/jpeg", etc.
    /// </param>
    /// <param name="parameterName">
    /// The name of the parameter being validated for error reporting and logging purposes.
    /// Used in diagnostic logging and error reporting to identify which parameter failed validation.
    /// Should match the actual parameter name for consistency and debugging support.
    /// </param>
    /// <returns>
    /// true if the MIME type format is valid and conforms to standard specifications;
    /// false if the MIME type format is invalid, malformed, or fails validation checks.
    /// </returns>
    /// <remarks>
    /// This method validates MIME types for comprehensive content type management:
    /// 
    /// MIME Type Format Validation:
    /// - Standard type/subtype format validation (e.g., "text/plain", "application/json")
    /// - Support for vendor-specific MIME types (e.g., "application/vnd.ms-excel")
    /// - Parameter validation for parameterized MIME types (e.g., "text/html; charset=utf-8")
    /// - Special character validation for acceptable MIME type characters
    /// - Length validation within reasonable limits for storage and processing
    /// 
    /// Security and Content Type Management:
    /// - Prevention of MIME type spoofing attacks through format validation
    /// - Validation against malicious content type headers and injection attempts
    /// - Ensures proper MIME type format for secure content negotiation
    /// - Protection against content type confusion attacks
    /// - Integration with file upload security and content validation
    /// 
    /// Format Support and Standards Compliance:
    /// - RFC-compliant MIME type format validation
    /// - Support for standard MIME types (text, application, image, audio, video)
    /// - Vendor-specific and experimental MIME type format support
    /// - Multi-part MIME type validation for complex content scenarios
    /// - Integration with HTTP content negotiation and API response formatting
    /// 
    /// Integration Features:
    /// - HTTP content type header validation for API requests and responses
    /// - File upload security validation to ensure content type consistency
    /// - Content negotiation support for RESTful API operations
    /// - Database storage format consistency and validation
    /// - Integration with content delivery and caching systems
    /// 
    /// Performance and Efficiency:
    /// - Efficient regex-based validation for fast MIME type format checking
    /// - Compiled regular expressions for optimal performance in high-frequency scenarios
    /// - Minimal memory allocation during validation operations
    /// - Thread-safe validation suitable for concurrent content processing
    /// - Integration with caching strategies for frequently validated MIME types
    /// 
    /// Common MIME Type Categories Supported:
    /// - Text types: text/plain, text/html, text/css, text/javascript
    /// - Application types: application/json, application/pdf, application/zip
    /// - Image types: image/jpeg, image/png, image/gif, image/svg+xml
    /// - Audio types: audio/mpeg, audio/wav, audio/ogg, audio/mp4
    /// - Video types: video/mp4, video/mpeg, video/quicktime, video/webm
    /// - Multipart types: multipart/form-data, multipart/mixed
    /// 
    /// Security Considerations:
    /// - Validation against known malicious MIME types and attack patterns
    /// - Prevention of MIME type confusion and content type spoofing
    /// - Integration with content security policies and file upload restrictions
    /// - Compliance with web security standards and best practices
    /// - Support for content type validation in security-sensitive applications
    /// 
    /// The method provides robust MIME type validation suitable for:
    /// - Web API content negotiation and response formatting
    /// - File upload validation and security checking
    /// - Content management system type validation
    /// - Email attachment processing and validation
    /// - Media processing and content delivery systems
    /// </remarks>
    /// <example>
    /// <code>
    /// // API content type validation
    /// [HttpPost]
    /// [Consumes("application/json", "application/xml")]
    /// public async Task&lt;IActionResult&gt; ProcessContent([FromBody] object content)
    /// {
    ///     var contentType = Request.ContentType;
    ///     
    ///     if (!_validationService.ValidateMimeType(contentType, nameof(contentType)))
    ///     {
    ///         return BadRequest($"Invalid or unsupported content type: {contentType}");
    ///     }
    ///     
    ///     // MIME type is valid - continue processing
    ///     var result = await _contentService.ProcessAsync(content);
    ///     return Ok(result);
    /// }
    /// 
    /// // File upload MIME type validation
    /// public async Task&lt;ValidationResult&gt; ValidateFileUpload(IFormFile file)
    /// {
    ///     var errors = new List&lt;string&gt;();
    ///     
    ///     // Validate MIME type format and security
    ///     if (!_validationService.ValidateMimeType(file.ContentType, "ContentType"))
    ///     {
    ///         errors.Add($"Invalid MIME type format: {file.ContentType}");
    ///     }
    ///     
    ///     // Additional file validation logic...
    ///     return errors.Any() ? ValidationResult.Failed(errors) : ValidationResult.Success();
    /// }
    /// 
    /// // Document metadata validation
    /// public IEnumerable&lt;ValidationResult&gt; ValidateDocumentMetadata(DocumentForCreationDto document)
    /// {
    ///     var results = new List&lt;ValidationResult&gt;();
    ///     
    ///     if (!_validationService.ValidateMimeType(document.MimeType, nameof(document.MimeType)))
    ///     {
    ///         results.Add(new ValidationResult(
    ///             "Document MIME type format is invalid",
    ///             new[] { nameof(document.MimeType) }));
    ///     }
    ///     
    ///     return results;
    /// }
    /// </code>
    /// </example>
    bool ValidateMimeType(string mimeType, string parameterName);

    #endregion Format Validation

    #region Enhanced Validation Methods

    /// <summary>
    /// Validates whether the provided string represents a valid checksum format for data integrity verification.
    /// </summary>
    /// <param name="checksum">
    /// The checksum string to validate for proper format and structure.
    /// Should be a hexadecimal string representing a hash value (typically SHA256).
    /// Common formats include 64-character SHA256 hashes or 32-character MD5 hashes.
    /// </param>
    /// <param name="parameterName">
    /// The name of the parameter being validated for error reporting and logging purposes.
    /// Used in diagnostic logging and error reporting to identify which parameter failed validation.
    /// Should match the actual parameter name for consistency and debugging support.
    /// </param>
    /// <returns>
    /// true if the checksum format is valid and represents a proper hash value;
    /// false if the checksum format is invalid, malformed, or fails validation checks.
    /// </returns>
    /// <remarks>
    /// This method validates checksums for comprehensive data integrity management:
    /// 
    /// Checksum Format Validation:
    /// - SHA256 hash validation (64-character hexadecimal strings)
    /// - MD5 hash validation (32-character hexadecimal strings) where supported
    /// - Hexadecimal character validation (0-9, A-F, a-f) for proper hash format
    /// - Length validation for specific hash algorithm requirements
    /// - Case-insensitive validation supporting both uppercase and lowercase hex
    /// 
    /// Security and Data Integrity:
    /// - Ensures checksum integrity for file verification and data validation
    /// - Validates against checksum manipulation attacks and tampering
    /// - Provides data integrity validation for secure file operations
    /// - Supports secure file verification workflows and audit trails
    /// - Integration with cryptographic hash validation and verification systems
    /// 
    /// The method specifically validates checksums used throughout the ADMS system
    /// for file integrity verification and ensures consistent checksum format
    /// across all document management operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // File integrity validation
    /// public async Task&lt;ValidationResult&gt; ValidateFileIntegrity(DocumentForCreationDto document)
    /// {
    ///     if (!_validationService.ValidateChecksum(document.Checksum, nameof(document.Checksum)))
    ///     {
    ///         return ValidationResult.Failed("Invalid checksum format for file integrity verification");
    ///     }
    ///     
    ///     return ValidationResult.Success();
    /// }
    /// </code>
    /// </example>
    bool ValidateChecksum(string checksum, string parameterName);

    #endregion Enhanced Validation Methods
}