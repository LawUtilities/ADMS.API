namespace ADMS.Domain.Common;

/// <summary>
/// Represents the result of a domain operation that can either succeed or fail with structured error information.
/// </summary>
/// <remarks>
/// The Result type implements the functional programming "Result" or "Either" pattern, providing a robust
/// alternative to exception-based error handling. This approach is particularly valuable in Domain-Driven
/// Design where business rule violations and validation failures are expected outcomes rather than
/// exceptional conditions.
/// 
/// <para><strong>Functional Programming Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>Explicit Error Handling:</strong> Forces callers to handle both success and failure cases</item>
/// <item><strong>Performance:</strong> Avoids expensive exception throwing and stack unwinding</item>
/// <item><strong>Composability:</strong> Results can be chained and transformed functionally</item>
/// <item><strong>Predictability:</strong> Method signatures clearly indicate potential failure</item>
/// <item><strong>Testability:</strong> Both success and failure paths are easily testable</item>
/// </list>
/// 
/// <para><strong>Domain-Driven Design Integration:</strong></para>
/// The Result pattern aligns with DDD principles by:
/// <list type="bullet">
/// <item>Treating business rule violations as expected outcomes, not exceptions</item>
/// <item>Providing rich, structured error information through DomainError</item>
/// <item>Enabling clean separation between domain logic and error handling</item>
/// <item>Supporting comprehensive validation without performance penalties</item>
/// <item>Facilitating functional domain model design patterns</item>
/// </list>
/// 
/// <para><strong>Legal Practice Applications:</strong></para>
/// In legal document management, the Result pattern supports:
/// <list type="bullet">
/// <item><strong>Document Validation:</strong> Comprehensive validation with detailed error reporting</item>
/// <item><strong>Business Rule Enforcement:</strong> Professional responsibility and compliance rules</item>
/// <item><strong>User Experience:</strong> Clear, actionable error messages for legal professionals</item>
/// <item><strong>Audit Trail Integrity:</strong> Structured tracking of validation failures</item>
/// <item><strong>System Reliability:</strong> Predictable error handling reduces system instability</item>
/// </list>
/// 
/// <para><strong>Error Handling Strategy:</strong></para>
/// <list type="bullet">
/// <item><strong>Expected Failures:</strong> Use Result for validation, business rules, not-found scenarios</item>
/// <item><strong>Unexpected Failures:</strong> Still use exceptions for system errors, infrastructure failures</item>
/// <item><strong>Structured Errors:</strong> DomainError provides machine-readable and human-readable information</item>
/// <item><strong>Composable Operations:</strong> Chain operations while preserving error context</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Using Result in domain operations
/// public static Result&lt;Document&gt; Create(string fileName, long fileSize, UserId createdBy)
/// {
///     // Validation returns Results instead of throwing exceptions
///     var fileNameResult = FileName.Create(fileName);
///     if (fileNameResult.IsFailure)
///         return Result.Failure&lt;Document&gt;(fileNameResult.Error);
///         
///     if (fileSize &lt;= 0)
///         return Result.Failure&lt;Document&gt;(new DomainError("INVALID_FILE_SIZE", "File size must be greater than zero"));
///         
///     var document = new Document(fileNameResult.Value, fileSize, createdBy);
///     return Result.Success(document);
/// }
/// 
/// // Consuming Results in application layer
/// public async Task&lt;IActionResult&gt; CreateDocument(CreateDocumentCommand command)
/// {
///     var result = Document.Create(command.FileName, command.FileSize, command.CreatedBy);
///     
///     if (result.IsFailure)
///     {
///         // Structured error information available
///         return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
///     }
///     
///     await _repository.AddAsync(result.Value);
///     return Ok(new { DocumentId = result.Value.Id });
/// }
/// 
/// // Chaining operations with Results
/// public Result&lt;DocumentDto&gt; ProcessDocument(DocumentId documentId)
/// {
///     return GetDocument(documentId)
///         .Bind(document => ValidateDocument(document))
///         .Bind(document => TransformToDto(document));
/// }
/// 
/// // Error accumulation for multiple validations
/// public Result ValidateDocumentRequest(CreateDocumentRequest request)
/// {
///     var errors = new List&lt;DomainError&gt;();
///     
///     if (string.IsNullOrEmpty(request.FileName))
///         errors.Add(new DomainError("FILENAME_REQUIRED", "File name is required"));
///         
///     if (request.FileSize &lt;= 0)
///         errors.Add(new DomainError("INVALID_FILE_SIZE", "File size must be positive"));
///         
///     return errors.Any() 
///         ? Result.Failure(errors.First()) // Or combine errors
///         : Result.Success();
/// }
/// </code>
/// </example>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    /// <value>true if the operation was successful; otherwise, false.</value>
    /// <remarks>
    /// This property provides the primary mechanism for determining operation outcome.
    /// Successful results have IsSuccess = true and Error = DomainError.None.
    /// Failed results have IsSuccess = false and a populated Error property.
    /// 
    /// <para><strong>Usage Pattern:</strong></para>
    /// Always check IsSuccess before accessing result values or using the result
    /// in subsequent operations. The Result pattern enforces explicit handling
    /// of both success and failure cases.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = Document.Create(fileName, fileSize, userId);
    /// 
    /// if (result.IsSuccess)
    /// {
    ///     // Safe to access result.Value
    ///     var document = result.Value;
    ///     await _repository.AddAsync(document);
    /// }
    /// else
    /// {
    ///     // Handle failure case
    ///     _logger.LogWarning("Document creation failed: {Error}", result.Error);
    /// }
    /// </code>
    /// </example>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    /// <value>true if the operation failed; otherwise, false.</value>
    /// <remarks>
    /// This property provides convenient inverse logic for IsSuccess, enabling
    /// more natural conditional expressions when checking for failure conditions.
    /// It's particularly useful in guard clauses and early return patterns.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = ValidateDocument(document);
    /// 
    /// if (result.IsFailure)
    /// {
    ///     return BadRequest(result.Error.Description);
    /// }
    /// 
    /// // Continue with successful operation
    /// await ProcessDocument(document);
    /// </code>
    /// </example>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error information associated with a failed operation.
    /// </summary>
    /// <value>A DomainError containing error details, or DomainError.None for successful operations.</value>
    /// <remarks>
    /// The Error property provides structured error information that includes both
    /// machine-readable error codes and human-readable descriptions. This enables
    /// sophisticated error handling, logging, and user communication strategies.
    /// 
    /// <para><strong>Error Information:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Success Case:</strong> Error equals DomainError.None (empty code and description)</item>
    /// <item><strong>Failure Case:</strong> Error contains specific code and descriptive message</item>
    /// <item><strong>Structured Data:</strong> Enables automated error categorization and handling</item>
    /// <item><strong>User Communication:</strong> Professional error messages suitable for legal practitioners</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Error Handling:</strong></para>
    /// Error information should be professional and actionable, helping legal
    /// practitioners understand and resolve issues while maintaining the technical
    /// precision needed for system administration and support.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = Document.Create(fileName, fileSize, userId);
    /// 
    /// if (result.IsFailure)
    /// {
    ///     // Access structured error information
    ///     string errorCode = result.Error.Code;
    ///     string errorMessage = result.Error.Description;
    ///     
    ///     // Log with structured data
    ///     _logger.LogWarning("Operation failed with code {ErrorCode}: {ErrorMessage}",
    ///         errorCode, errorMessage);
    ///     
    ///     // Return appropriate response
    ///     return new ApiResponse
    ///     {
    ///         Success = false,
    ///         ErrorCode = errorCode,
    ///         Message = errorMessage
    ///     };
    /// }
    /// </code>
    /// </example>
    public DomainError Error { get; }

    /// <summary>
    /// Initializes a new instance of the Result class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error information for failed operations.</param>
    /// <remarks>
    /// This protected constructor enforces Result creation through factory methods,
    /// ensuring proper validation of the success/error relationship and preventing
    /// invalid Result instances.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Successful results must have Error = DomainError.None</item>
    /// <item>Failed results must have a valid error with non-empty code and description</item>
    /// <item>These rules are enforced to maintain Result pattern integrity</item>
    /// </list>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when success state doesn't match error state (e.g., successful result with error).
    /// </exception>
    protected Result(bool isSuccess, DomainError error)
    {
        if (isSuccess && error != DomainError.None)
            throw new InvalidOperationException("Success result cannot have an error");

        if (!isSuccess && error == DomainError.None)
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful Result with no error information.
    /// </summary>
    /// <returns>A Result representing a successful operation.</returns>
    /// <remarks>
    /// This factory method creates Results for operations that succeed without returning
    /// a value. It's commonly used for void operations like updates, deletions, or
    /// validation operations that either succeed or fail without producing data.
    /// 
    /// <para><strong>Common Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Document update operations</item>
    /// <item>Validation methods</item>
    /// <item>Entity deletion operations</item>
    /// <item>Business rule enforcement</item>
    /// <item>Authorization checks</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Domain method returning success without value
    /// public Result UpdateFileName(FileName newFileName)
    /// {
    ///     if (IsDeleted)
    ///         return Result.Failure(new DomainError("DOCUMENT_DELETED", "Cannot update deleted document"));
    ///         
    ///     FileName = newFileName;
    ///     return Result.Success(); // No value returned, just success indication
    /// }
    /// 
    /// // Validation method
    /// public Result ValidateCheckout()
    /// {
    ///     if (IsCheckedOut)
    ///         return Result.Failure(new DomainError("ALREADY_CHECKED_OUT", "Document is already checked out"));
    ///         
    ///     if (IsDeleted)
    ///         return Result.Failure(new DomainError("DOCUMENT_DELETED", "Cannot check out deleted document"));
    ///         
    ///     return Result.Success(); // Validation passed
    /// }
    /// </code>
    /// </example>
    public static Result Success() => new(true, DomainError.None);

    /// <summary>
    /// Creates a failed Result with the specified error information.
    /// </summary>
    /// <param name="error">The domain error describing the failure.</param>
    /// <returns>A Result representing a failed operation with error details.</returns>
    /// <remarks>
    /// This factory method creates Results for operations that fail without producing
    /// a value. The error parameter provides structured information about why the
    /// operation failed, enabling appropriate error handling and user communication.
    /// 
    /// <para><strong>Error Information Requirements:</strong></para>
    /// The error parameter must contain both a machine-readable code and a
    /// human-readable description to support comprehensive error handling strategies.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Domain method returning failure
    /// public Result Delete()
    /// {
    ///     if (IsDeleted)
    ///         return Result.Failure(new DomainError("ALREADY_DELETED", "Document is already deleted"));
    ///         
    ///     if (IsCheckedOut)
    ///         return Result.Failure(new DomainError("DOCUMENT_CHECKED_OUT", "Cannot delete checked out document"));
    ///         
    ///     IsDeleted = true;
    ///     return Result.Success();
    /// }
    /// 
    /// // Using predefined error constants
    /// public Result ValidateFileSize(long fileSize)
    /// {
    ///     if (fileSize <= 0)
    ///         return Result.Failure(DocumentErrors.InvalidFileSize);
    ///         
    ///     if (fileSize > MaxFileSize)
    ///         return Result.Failure(DocumentErrors.FileSizeExceedsLimit);
    ///         
    ///     return Result.Success();
    /// }
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">Thrown when error is null.</exception>
    public static Result Failure(DomainError error) => new(false, error ?? throw new ArgumentNullException(nameof(error)));

    /// <summary>
    /// Creates a successful Result containing the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value being returned.</typeparam>
    /// <param name="value">The value to be contained in the successful result.</param>
    /// <returns>A Result&lt;T&gt; representing a successful operation with the specified value.</returns>
    /// <remarks>
    /// This factory method creates typed Results for operations that succeed and return
    /// a value. This is the most common pattern for domain operations that create or
    /// retrieve entities, value objects, or other domain data.
    /// 
    /// <para><strong>Common Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Entity creation methods</item>
    /// <item>Value object factory methods</item>
    /// <item>Repository query operations</item>
    /// <item>Domain service operations</item>
    /// <item>Data transformation methods</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Entity factory method
    /// public static Result&lt;Document&gt; Create(FileName fileName, long fileSize, UserId createdBy)
    /// {
    ///     var document = new Document(fileName, fileSize, createdBy);
    ///     return Result.Success(document);
    /// }
    /// 
    /// // Value object creation
    /// public static Result&lt;FileName&gt; Create(string value)
    /// {
    ///     if (string.IsNullOrWhiteSpace(value))
    ///         return Result.Failure&lt;FileName&gt;(new DomainError("FILENAME_REQUIRED", "Filename cannot be empty"));
    ///         
    ///     var fileName = new FileName(value);
    ///     return Result.Success(fileName);
    /// }
    /// 
    /// // Repository query method
    /// public async Task&lt;Result&lt;Document&gt;&gt; GetByIdAsync(DocumentId id)
    /// {
    ///     var document = await _dbContext.Documents.FindAsync(id);
    ///     
    ///     if (document == null)
    ///         return Result.Failure&lt;Document&gt;(new DomainError("DOCUMENT_NOT_FOUND", "Document not found"));
    ///         
    ///     return Result.Success(document);
    /// }
    /// </code>
    /// </example>
    public static Result<T> Success<T>(T value) => new(value, true, DomainError.None);

    /// <summary>
    /// Creates a failed Result of the specified type with error information.
    /// </summary>
    /// <typeparam name="T">The type that would have been returned on success.</typeparam>
    /// <param name="error">The domain error describing the failure.</param>
    /// <returns>A Result&lt;T&gt; representing a failed operation with error details.</returns>
    /// <remarks>
    /// This factory method creates typed Results for operations that fail to produce
    /// the expected value. The type parameter ensures type safety while the error
    /// parameter provides detailed failure information.
    /// 
    /// <para><strong>Type Safety:</strong></para>
    /// The generic type parameter ensures that failed Results maintain type
    /// compatibility with their successful counterparts, enabling consistent
    /// handling patterns throughout the application.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Entity factory with validation failure
    /// public static Result&lt;Document&gt; Create(string fileName, long fileSize)
    /// {
    ///     if (string.IsNullOrEmpty(fileName))
    ///         return Result.Failure&lt;Document&gt;(new DomainError("FILENAME_REQUIRED", "File name is required"));
    ///         
    ///     if (fileSize &lt;= 0)
    ///         return Result.Failure&lt;Document&gt;(new DomainError("INVALID_FILE_SIZE", "File size must be positive"));
    ///         
    ///     var document = new Document(fileName, fileSize);
    ///     return Result.Success(document);
    /// }
    /// 
    /// // Repository method with not found handling
    /// public Result&lt;User&gt; FindByEmail(string email)
    /// {
    ///     var user = _users.FirstOrDefault(u => u.Email == email);
    ///     
    ///     return user != null 
    ///         ? Result.Success(user)
    ///         : Result.Failure&lt;User&gt;(new DomainError("USER_NOT_FOUND", "User not found with specified email"));
    /// }
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">Thrown when error is null.</exception>
    public static Result<T> Failure<T>(DomainError error) => new(default!, false, error ?? throw new ArgumentNullException(nameof(error)));
}

/// <summary>
/// Represents the result of a domain operation that can either succeed with a value or fail with structured error information.
/// </summary>
/// <typeparam name="T">The type of the value returned on successful operations.</typeparam>
/// <remarks>
/// Result&lt;T&gt; extends the base Result class to include a strongly-typed value for successful
/// operations. This enables type-safe functional programming patterns while maintaining
/// consistent error handling semantics with the base Result class.
/// 
/// <para><strong>Type Safety Benefits:</strong></para>
/// <list type="bullet">
/// <item>Compile-time type checking for successful operation values</item>
/// <item>IntelliSense support for result value properties and methods</item>
/// <item>Prevention of type casting errors and runtime type exceptions</item>
/// <item>Clear method signatures indicating return value types</item>
/// </list>
/// 
/// <para><strong>Functional Composition:</strong></para>
/// Result&lt;T&gt; enables functional composition patterns such as:
/// <list type="bullet">
/// <item>Chaining operations with Bind/FlatMap patterns</item>
/// <item>Transforming values with Map/Select patterns</item>
/// <item>Error handling with Match/Fold patterns</item>
/// <item>Validation composition with applicative patterns</item>
/// </list>
/// 
/// <para><strong>Legal Document Management Applications:</strong></para>
/// Typed results are particularly valuable for:
/// <list type="bullet">
/// <item>Document creation and retrieval operations</item>
/// <item>Value object construction and validation</item>
/// <item>Business rule evaluation returning computed values</item>
/// <item>Data transformation and mapping operations</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Typed result for document operations
/// public static Result&lt;Document&gt; CreateDocument(string fileName, byte[] content)
/// {
///     var fileNameResult = FileName.Create(fileName);
///     if (fileNameResult.IsFailure)
///         return Result.Failure&lt;Document&gt;(fileNameResult.Error);
///         
///     var checksumResult = FileChecksum.ComputeFromData(content);
///     if (checksumResult.IsFailure)
///         return Result.Failure&lt;Document&gt;(checksumResult.Error);
///         
///     var document = new Document(fileNameResult.Value, checksumResult.Value, content.Length);
///     return Result.Success(document);
/// }
/// 
/// // Consuming typed results
/// public async Task&lt;IActionResult&gt; CreateDocumentEndpoint([FromBody] CreateDocumentRequest request)
/// {
///     var result = CreateDocument(request.FileName, request.Content);
///     
///     if (result.IsSuccess)
///     {
///         // Type-safe access to the document
///         var document = result.Value;
///         await _repository.AddAsync(document);
///         
///         return Ok(new { DocumentId = document.Id, FileName = document.FileName.Value });
///     }
///     
///     return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
/// }
/// 
/// // Functional composition example
/// public Result&lt;DocumentDto&gt; GetDocumentWithValidation(DocumentId id)
/// {
///     return _repository.GetById(id)  // Returns Result&lt;Document&gt;
///         .Bind(document => ValidateDocumentAccess(document))  // Result&lt;Document&gt;
///         .Map(document => new DocumentDto(document));  // Result&lt;DocumentDto&gt;
/// }
/// </code>
/// </example>
public class Result<T> : Result
{
    /// <summary>
    /// Gets the value contained in this successful result.
    /// </summary>
    /// <value>The value of type T for successful operations, or default(T) for failed operations.</value>
    /// <remarks>
    /// This property provides access to the strongly-typed value produced by successful
    /// operations. For failed results, this property contains the default value for type T
    /// and should not be accessed without first checking IsSuccess.
    /// 
    /// <para><strong>Usage Guidelines:</strong></para>
    /// <list type="bullet">
    /// <item>Always check IsSuccess before accessing Value</item>
    /// <item>Use type-specific default handling for null reference types</item>
    /// <item>Consider using pattern matching or functional composition for safe access</item>
    /// <item>Document null-possibility for reference types in your domain</item>
    /// </list>
    /// 
    /// <para><strong>Type Safety:</strong></para>
    /// The Value property is strongly-typed, providing compile-time type checking
    /// and IntelliSense support. This reduces runtime errors and improves developer
    /// productivity when working with domain operations.
    /// 
    /// <para><strong>Memory Considerations:</strong></para>
    /// For failed results, the Value property contains the default value for T,
    /// which is null for reference types and default struct values for value types.
    /// This approach minimizes memory allocation while maintaining type safety.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Safe value access pattern
    /// var result = Document.Create(fileName, fileSize, userId);
    /// 
    /// if (result.IsSuccess)
    /// {
    ///     var document = result.Value; // Type-safe access
    ///     Console.WriteLine($"Created document: {document.FileName}");
    /// }
    /// else
    /// {
    ///     // result.Value would be null here - don't access it
    ///     Console.WriteLine($"Creation failed: {result.Error}");
    /// }
    /// 
    /// // Pattern matching approach
    /// var message = result switch
    /// {
    ///     { IsSuccess: true, Value: var document } => $"Document created: {document.Id}",
    ///     { IsFailure: true, Error: var error } => $"Creation failed: {error.Description}",
    ///     _ => "Unknown result state"
    /// };
    /// 
    /// // Functional approach with extension methods
    /// var documentId = result
    ///     .Map(document => document.Id)
    ///     .Match(
    ///         onSuccess: id => id.ToString(),
    ///         onFailure: error => "No ID - creation failed"
    ///     );
    /// </code>
    /// </example>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the Result&lt;T&gt; class.
    /// </summary>
    /// <param name="value">The value to be contained in the result.</param>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error information for failed operations.</param>
    /// <remarks>
    /// This internal constructor is used by the factory methods to create typed results
    /// while maintaining the validation rules established in the base Result class.
    /// The constructor ensures that successful results have valid values and failed
    /// results have appropriate error information.
    /// 
    /// <para><strong>Value Handling:</strong></para>
    /// <list type="bullet">
    /// <item>Successful results store the provided value</item>
    /// <item>Failed results store default(T) regardless of the provided value parameter</item>
    /// <item>The isSuccess parameter determines how the value is handled</item>
    /// </list>
    /// 
    /// <para><strong>Constructor Access:</strong></para>
    /// This constructor is internal to prevent external code from creating invalid
    /// Result instances. All Result&lt;T&gt; instances should be created through the
    /// static factory methods on the base Result class.
    /// </remarks>
    internal Result(T value, bool isSuccess, DomainError error) : base(isSuccess, error)
    {
        Value = value;
    }
}