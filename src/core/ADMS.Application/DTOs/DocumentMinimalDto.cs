using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ADMS.Application.DTOs;

/// <summary>
/// Lightweight Document Data Transfer Object providing essential document information optimized for performance-critical scenarios and UI display operations.
/// </summary>
/// <remarks>
/// This DTO serves as a high-performance, minimal representation of documents within the ADMS legal document management system,
/// containing only essential properties from <see cref="ADMS.API.Entities.Document"/> required for document identification,
/// selection, and basic metadata display. It excludes navigation properties and audit trail collections for optimal
/// performance in scenarios requiring rapid document enumeration, search results, and selection interfaces.
/// 
/// <para><strong>Enhanced with Standardized Validation (.NET 9):</strong></para>
/// <list type="bullet">
/// <item><strong>BaseValidationDto Integration:</strong> Inherits standardized ADMS validation patterns for consistency</item>
/// <item><strong>File-Specific Validation:</strong> Essential file metadata validation using FileValidationHelper</item>
/// <item><strong>Performance Optimized:</strong> Uses yield return for lazy validation evaluation and early termination</item>
/// <item><strong>Validation Hierarchy:</strong> Follows standardized core → business → cross-property pattern</item>
/// <item><strong>Professional Standards:</strong> Maintains professional document management validation standards</item>
/// </list>
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Entity Representation:</strong> Contains only essential properties from ADMS.API.Entities.Document</item>
/// <item><strong>Performance Optimized:</strong> Excludes collections and relationships for fast document enumeration</item>
/// <item><strong>Standardized Validation:</strong> Uses BaseValidationDto for consistent validation patterns</item>
/// <item><strong>Immutable Design:</strong> Record type with init-only properties for thread-safe operations</item>
/// <item><strong>Display Ready:</strong> Pre-validated for immediate use in UI controls and document lists</item>
/// <item><strong>Thread Safe:</strong> Immutable record design supports concurrent access scenarios</item>
/// </list>
/// 
/// <para><strong>Entity Alignment:</strong></para>
/// This DTO mirrors essential properties from ADMS.API.Entities.Document for lightweight operations:
/// <list type="bullet">
/// <item><strong>Id:</strong> Unique document identifier for precise document referencing</item>
/// <item><strong>FileName:</strong> Human-readable document name for identification and display</item>
/// <item><strong>Extension:</strong> File format identifier for content type recognition</item>
/// <item><strong>FileSize:</strong> File size metadata for storage and transfer planning</item>
/// <item><strong>IsCheckedOut:</strong> Version control status for edit availability determination</item>
/// <item><strong>IsDeleted:</strong> Soft deletion status for proper document filtering</item>
/// <item><strong>CreationDate:</strong> Document creation timestamp for chronological ordering</item>
/// </list>
/// 
/// <para><strong>Validation Hierarchy:</strong></para>
/// Following BaseValidationDto standardized validation pattern for essential validation:
/// <list type="number">
/// <item><strong>Core Properties:</strong> Essential document properties using FileValidationHelper and BaseValidationDto helpers</item>
/// <item><strong>Business Rules:</strong> Document state consistency, professional standards compliance</item>
/// <item><strong>Cross-Property:</strong> Basic property relationship validation for minimal scenarios</item>
/// </list>
/// 
/// <para><strong>Performance Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>Fast Enumeration:</strong> Lightweight structure for rapid document list generation</item>
/// <item><strong>Memory Efficient:</strong> Minimal memory footprint for large document collections</item>
/// <item><strong>Quick Validation:</strong> Essential validation only, no complex collection processing</item>
/// <item><strong>Thread Safe Operations:</strong> Immutable design enables safe concurrent access</item>
/// <item><strong>UI Responsive:</strong> Optimized for responsive user interface document displays</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Selection Lists:</strong> Dropdown controls, selection dialogs, and document pickers</item>
/// <item><strong>Search Results:</strong> Document search result displays with essential metadata</item>
/// <item><strong>Performance-Critical APIs:</strong> Lightweight document data for high-throughput endpoints</item>
/// <item><strong>Dashboard Displays:</strong> Document summary information for administrative dashboards</item>
/// <item><strong>Reference Operations:</strong> Document reference lookup and basic metadata retrieval</item>
/// <item><strong>Mobile Applications:</strong> Optimized document data for mobile and bandwidth-constrained environments</item>
/// </list>
/// 
/// <para><strong>When to Use vs Other Document DTOs:</strong></para>
/// <list type="bullet">
/// <item><strong>Use DocumentMinimalDto:</strong> For lists, selections, search results, and performance-critical scenarios</item>
/// <item><strong>Use DocumentDto:</strong> For complete document operations requiring full context, audit trails, and revision history</item>
/// <item><strong>Use DocumentWithoutRevisionsDto:</strong> For document operations needing partial context without revision history</item>
/// <item><strong>Use DocumentForCreationDto:</strong> For document creation operations with creation-specific validation</item>
/// <item><strong>Use DocumentForUpdateDto:</strong> For document update operations with update-specific validation</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>Rapid Document Identification:</strong> Fast document lookup and identification for professional workflows</item>
/// <item><strong>Efficient Selection Interfaces:</strong> Optimized for document selection and reference operations</item>
/// <item><strong>Performance-Critical Operations:</strong> Enables responsive document management interfaces</item>
/// <item><strong>Professional Display Standards:</strong> Validated display text and formatting for professional presentation</item>
/// <item><strong>Resource Optimization:</strong> Minimal resource usage for large-scale document management operations</item>
/// </list>
/// 
/// <para><strong>Data Integrity and Professional Standards:</strong></para>
/// <list type="bullet">
/// <item><strong>Essential Validation:</strong> Core document validation using FileValidationHelper for professional standards</item>
/// <item><strong>Status Consistency:</strong> Validates document status combinations for professional document management</item>
/// <item><strong>Professional Display:</strong> Ensures display properties meet professional presentation standards</item>
/// <item><strong>Immutable Safety:</strong> Record design ensures data consistency for concurrent operations</item>
/// </list>
/// 
/// <para><strong>Integration with ADMS Ecosystem:</strong></para>
/// <list type="bullet">
/// <item><strong>Entity Conversion:</strong> Seamless conversion from ADMS.API.Entities.Document with validation</item>
/// <item><strong>Validation Helper Integration:</strong> Uses FileValidationHelper for consistent file validation</item>
/// <item><strong>Professional Standards:</strong> Aligns with ADMS professional document management patterns</item>
/// <item><strong>API Integration:</strong> Optimized for REST API responses requiring lightweight document data</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating lightweight document list for UI display
/// var minimalDocuments = await context.Documents
///     .Where(d => !d.IsDeleted && !d.IsArchived)
///     .Select(d => new DocumentMinimalDto
///     {
///         Id = d.Id,
///         FileName = d.FileName,
///         Extension = d.Extension,
///         FileSize = d.FileSize,
///         IsCheckedOut = d.IsCheckedOut,
///         IsDeleted = d.IsDeleted,
///         CreationDate = d.CreationDate
///     })
///     .OrderBy(d => d.DisplayText)
///     .ToListAsync();
/// 
/// // Professional document selection interface
/// foreach (var doc in minimalDocuments)
/// {
///     Console.WriteLine($"Document: {doc.DisplayText} - Status: {doc.Status} - Size: {doc.FormattedFileSize}");
/// }
/// 
/// // Comprehensive validation for data quality assurance
/// var validationAnalysis = DocumentMinimalDto.ValidateDocumentCollection(minimalDocuments);
/// if (validationAnalysis.InvalidDocuments > 0)
/// {
///     _logger.LogWarning("Document validation issues: {ErrorSummary}", validationAnalysis.ErrorSummary);
/// }
/// 
/// // Professional UI integration
/// documentSelectionComboBox.DataSource = minimalDocuments;
/// documentSelectionComboBox.DisplayMember = nameof(DocumentMinimalDto.DisplayText);
/// documentSelectionComboBox.ValueMember = nameof(DocumentMinimalDto.Id);
/// </code>
/// </example>
public class DocumentMinimalDto : BaseValidationDto, IEquatable<DocumentMinimalDto>, IComparable<DocumentMinimalDto>
{
    #region Core Essential Properties

    /// <summary>
    /// Gets or sets the unique identifier for the document.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the document within the ADMS legal 
    /// document management system, corresponding to <see cref="ADMS.API.Entities.Document.Id"/>.
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required Property:</strong> Must be provided for all document identification operations</item>
    /// <item><strong>Unique Identification:</strong> Serves as the definitive document identifier for minimal operations</item>
    /// <item><strong>Reference Operations:</strong> Primary identifier for document lookup and selection operations</item>
    /// <item><strong>API Integration:</strong> Core identifier for REST API operations requiring minimal document data</item>
    /// <item><strong>Performance Critical:</strong> Optimized for high-speed document enumeration and selection</item>
    /// </list>
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateGuid() with allowEmpty=false
    /// to ensure proper document identification and system integrity for minimal operations.
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Document.Id"/> exactly, ensuring complete consistency
    /// between entity and DTO representations for reliable document identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard minimal document identification
    /// var documentId = Guid.NewGuid();
    /// var minimalDoc = new DocumentMinimalDto { Id = documentId, /* other essential properties */ };
    /// 
    /// // Using ID for document selection operations
    /// var selectedDocument = documentList.FirstOrDefault(d => d.Id == selectedId);
    /// 
    /// // Performance-optimized document lookup
    /// var documentExists = minimalDocuments.Any(d => d.Id == searchId);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Document ID is required for document identification and selection operations.")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the file name for the document (without extension).
    /// </summary>
    /// <remarks>
    /// The file name serves as the primary human-readable identifier for the document and must conform 
    /// to professional file naming conventions and security standards for legal document management.
    /// 
    /// <para><strong>Professional Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Professional Naming:</strong> Must follow professional legal document naming conventions</item>
    /// <item><strong>Display Ready:</strong> Optimized for immediate display in user interfaces</item>
    /// <item><strong>Selection Friendly:</strong> Formatted for document selection and identification operations</item>
    /// <item><strong>Performance Optimized:</strong> Essential validation only for minimal overhead</item>
    /// <item><strong>Cross-Platform Safe:</strong> Validated for cross-platform compatibility</item>
    /// </list>
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using FileValidationHelper.ValidateFileName() for essential
    /// file name validation including security, compatibility, and professional standards compliance.
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Document.FileName"/> exactly, maintaining
    /// database constraint consistency and ensuring reliable display operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional legal document naming for minimal display
    /// var minimalDoc = new DocumentMinimalDto 
    /// { 
    ///     FileName = "Contract_Amendment_Smith_2024",
    ///     Extension = ".pdf",
    ///     // other essential properties...
    /// };
    /// 
    /// // Display text for UI controls
    /// var displayName = minimalDoc.DisplayText; // "Contract_Amendment_Smith_2024.pdf"
    /// 
    /// // Professional selection interface
    /// documentListBox.Items.Add(new { Text = minimalDoc.DisplayText, Value = minimalDoc.Id });
    /// </code>
    /// </example>
    [Required(ErrorMessage = "File name is required and cannot be empty for document identification.")]
    [MaxLength(FileValidationHelper.MaxFileNameLength,
        ErrorMessage = "File name cannot exceed {1} characters for system compatibility.")]
    public required string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the file extension for the document (including the dot).
    /// </summary>
    /// <remarks>
    /// The file extension identifies the document format and must be one of the allowed extensions 
    /// for the ADMS system, ensuring security compliance and proper document handling for minimal operations.
    /// 
    /// <para><strong>Security and Compliance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Allowed Extensions:</strong> Must be from the approved list of safe file extensions</item>
    /// <item><strong>Security Validation:</strong> Essential security checks for file type safety</item>
    /// <item><strong>Format Recognition:</strong> Enables proper document type identification in UI</item>
    /// <item><strong>Professional Standards:</strong> Supports common legal document formats</item>
    /// <item><strong>Display Optimization:</strong> Formatted for professional document presentation</item>
    /// </list>
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using FileValidationHelper.ValidateExtension() for essential
    /// extension validation including security checks and format consistency verification.
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Document.Extension"/> exactly, maintaining
    /// database constraint consistency and ensuring reliable file type identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Supported legal document extensions for minimal operations
    /// var pdfDoc = new DocumentMinimalDto { Extension = ".pdf", /* other properties */ };
    /// var wordDoc = new DocumentMinimalDto { Extension = ".docx", /* other properties */ };
    /// 
    /// // Extension-based UI operations
    /// var icon = GetDocumentIcon(minimalDoc.Extension);
    /// var canPreview = CanPreviewExtension(minimalDoc.Extension);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "File extension is required and cannot be empty for document format identification.")]
    [MaxLength(FileValidationHelper.MaxExtensionLength,
        ErrorMessage = "File extension cannot exceed {1} characters for system compatibility.")]
    public required string Extension { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the size of the file in bytes.
    /// </summary>
    /// <remarks>
    /// The file size provides essential metadata for storage management, transfer operations, and 
    /// UI display while supporting professional document selection and management operations.
    /// 
    /// <para><strong>Professional Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Storage Information:</strong> Essential for storage planning and capacity display</item>
    /// <item><strong>Transfer Planning:</strong> Important for download time estimation and network planning</item>
    /// <item><strong>User Experience:</strong> Provides file size information for professional document selection</item>
    /// <item><strong>Resource Awareness:</strong> Enables resource-aware document management decisions</item>
    /// <item><strong>Professional Display:</strong> Formatted display for professional document listings</item>
    /// </list>
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using range validation to ensure file sizes are within
    /// acceptable ranges for professional document management and system performance.
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Document.FileSize"/> exactly, ensuring
    /// accurate file size tracking and consistent storage management.
    /// </remarks>
    /// <example>
    /// <code>
    /// // File size analysis and display for minimal operations
    /// var minimalDoc = new DocumentMinimalDto { FileSize = 2547832, /* other properties */ };
    /// 
    /// // Professional file size display
    /// Console.WriteLine($"Document size: {minimalDoc.FormattedFileSize}"); // "2.43 MB"
    /// 
    /// // Size-based UI operations
    /// var isLargeFile = minimalDoc.IsLargeFile;
    /// var downloadTimeEstimate = EstimateDownloadTime(minimalDoc.FileSize);
    /// </code>
    /// </example>
    [Range(0, long.MaxValue, ErrorMessage = "File size must be non-negative for accurate storage tracking.")]
    public long FileSize { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the document is currently checked out for exclusive editing.
    /// </summary>
    /// <remarks>
    /// The check-out status indicates whether the document is currently under exclusive editing control,
    /// essential for professional version control and document availability determination in minimal scenarios.
    /// 
    /// <para><strong>Version Control Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Edit Availability:</strong> Quick determination of document edit availability</item>
    /// <item><strong>Status Display:</strong> Professional status indication for document lists</item>
    /// <item><strong>Selection Logic:</strong> Enables smart document selection based on availability</item>
    /// <item><strong>Professional Workflow:</strong> Supports professional document workflow status indication</item>
    /// <item><strong>Conflict Prevention:</strong> Visual indication of document editing conflicts</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Mutual Exclusivity:</strong> Document cannot be both checked out and deleted simultaneously</item>
    /// <item><strong>Edit Availability:</strong> Checked out documents are not available for editing by others</item>
    /// <item><strong>Status Indication:</strong> Provides clear status indication for professional interfaces</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Document.IsCheckedOut"/> exactly, ensuring
    /// consistent version control state tracking and reliable availability determination.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Document check-out status analysis for minimal operations
    /// var minimalDoc = new DocumentMinimalDto { IsCheckedOut = true, /* other properties */ };
    /// 
    /// // Professional availability checking
    /// var isAvailable = minimalDoc.IsAvailableForEdit; // false when checked out
    /// var statusIcon = GetStatusIcon(minimalDoc.Status);
    /// 
    /// // UI status indication
    /// if (minimalDoc.IsCheckedOut)
    /// {
    ///     documentListItem.BackColor = Color.LightYellow; // Visual indication
    ///     documentListItem.ToolTipText = "Document is checked out and unavailable for editing";
    /// }
    /// </code>
    /// </example>
    public bool IsCheckedOut { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the document is soft-deleted while preserving minimal metadata.
    /// </summary>
    /// <remarks>
    /// The deletion flag supports soft deletion functionality, preserving essential document metadata
    /// while marking documents as deleted for professional document lifecycle management and filtering.
    /// 
    /// <para><strong>Soft Deletion Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Metadata Preservation:</strong> Maintains essential document metadata for identification</item>
    /// <item><strong>Professional Recovery:</strong> Enables document identification for recovery operations</item>
    /// <item><strong>Filtering Support:</strong> Enables proper document filtering in selection interfaces</item>
    /// <item><strong>Status Indication:</strong> Provides clear deletion status for professional interfaces</item>
    /// <item><strong>Administrative Oversight:</strong> Supports administrative document management operations</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Mutual Exclusivity:</strong> Document cannot be both deleted and checked out simultaneously</item>
    /// <item><strong>Edit Restriction:</strong> Deleted documents are not available for editing or selection</item>
    /// <item><strong>Professional Display:</strong> Deleted documents typically filtered from normal operations</item>
    /// <item><strong>Recovery Support:</strong> Deleted status enables restoration workflow identification</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Document.IsDeleted"/> exactly, ensuring
    /// consistent soft deletion state tracking and reliable document filtering.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Document deletion status analysis for minimal operations
    /// var minimalDoc = new DocumentMinimalDto { IsDeleted = true, /* other properties */ };
    /// 
    /// // Professional filtering and display
    /// var activeDocuments = documentList.Where(d => !d.IsDeleted).ToList();
    /// var deletedDocuments = documentList.Where(d => d.IsDeleted).ToList();
    /// 
    /// // Professional status indication
    /// if (minimalDoc.IsDeleted)
    /// {
    ///     documentListItem.ForeColor = Color.Gray; // Visual indication
    ///     documentListItem.Text = $"[DELETED] {minimalDoc.DisplayText}";
    /// }
    /// </code>
    /// </example>
    public bool IsDeleted { get; init; }

    /// <summary>
    /// Gets or sets the creation date and time of the document in UTC for temporal tracking and ordering.
    /// </summary>
    /// <remarks>
    /// The creation date establishes the temporal foundation for the document and is essential for 
    /// chronological ordering, professional document management, and basic audit compliance.
    /// 
    /// <para><strong>Temporal Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Chronological Ordering:</strong> Essential for professional document list ordering and display</item>
    /// <item><strong>Professional Standards:</strong> Supports professional practice standards for document creation tracking</item>
    /// <item><strong>Age Analysis:</strong> Enables basic document age analysis and lifecycle management</item>
    /// <item><strong>UTC Standardization:</strong> Uses UTC for consistent global temporal tracking</item>
    /// <item><strong>Selection Support:</strong> Temporal data for intelligent document selection operations</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Immutable Foundation:</strong> Creation date should not change after initial document creation</item>
    /// <item><strong>Reasonable Bounds:</strong> Must be within reasonable temporal bounds for system validity</item>
    /// <item><strong>Ordering Foundation:</strong> Serves as primary temporal ordering criterion for document lists</item>
    /// <item><strong>Professional Display:</strong> Provides creation timestamp for professional document information</item>
    /// </list>
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateRequiredDate() for essential
    /// temporal validation suitable for minimal operations.
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Document.CreationDate"/> exactly, ensuring
    /// consistent temporal tracking and reliable document ordering.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Document creation with temporal tracking for minimal operations
    /// var minimalDoc = new DocumentMinimalDto 
    /// { 
    ///     CreationDate = DateTime.UtcNow,
    ///     /* other essential properties */
    /// };
    /// 
    /// // Professional temporal analysis for minimal scenarios
    /// var documentAge = minimalDoc.DocumentAge; // TimeSpan representing age
    /// var isRecentDocument = minimalDoc.IsRecentDocument; // Created within last 7 days
    /// 
    /// // Professional chronological display
    /// var orderedDocuments = documentList.OrderByDescending(d => d.CreationDate).ToList();
    /// Console.WriteLine($"Document created: {minimalDoc.CreationDate:yyyy-MM-dd HH:mm:ss} UTC");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Creation date is required for temporal tracking and document ordering.")]
    public required DateTime CreationDate { get; init; }

    #endregion Core Essential Properties

    #region Professional Computed Properties

    /// <summary>
    /// Gets the display text for professional document presentation combining filename and extension.
    /// </summary>
    /// <remarks>
    /// This computed property provides professional document display text, combining filename and extension
    /// for consistent document presentation across the system, optimized for UI display operations.
    /// 
    /// <para><strong>Professional Display Features:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Consistent Formatting:</strong> Standardized display format for professional presentation</item>
    /// <item><strong>UI Optimized:</strong> Ready for immediate use in UI controls and document lists</item>
    /// <item><strong>Professional Standards:</strong> Meets professional document display standards</item>
    /// <item><strong>Selection Friendly:</strong> Formatted for document selection and identification</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional document display in UI
    /// Console.WriteLine($"Document: {minimalDoc.DisplayText}"); // "Contract_Amendment.pdf"
    /// 
    /// // UI control binding
    /// documentComboBox.DisplayMember = nameof(DocumentMinimalDto.DisplayText);
    /// listBoxItem.Text = minimalDoc.DisplayText;
    /// </code>
    /// </example>
    public string DisplayText => $"{FileName}{Extension}";

    /// <summary>
    /// Gets the formatted file size for professional display and user communication.
    /// </summary>
    /// <remarks>
    /// This computed property provides human-readable file size formatting using FileValidationHelper,
    /// essential for professional document presentation and user interface display.
    /// 
    /// <para><strong>Professional Display Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Friendly:</strong> Converts bytes to readable format (KB, MB, GB)</item>
    /// <item><strong>Professional Presentation:</strong> Formatted for professional document listings</item>
    /// <item><strong>Selection Support:</strong> Helps users make informed document selection decisions</item>
    /// <item><strong>Performance Information:</strong> Provides size context for download and transfer operations</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional file size display for minimal document lists
    /// Console.WriteLine($"Document size: {minimalDoc.FormattedFileSize}"); // "2.43 MB"
    /// 
    /// // UI integration
    /// documentGridView.Columns["Size"].Value = minimalDoc.FormattedFileSize;
    /// toolTip.SetToolTip(documentListItem, $"Size: {minimalDoc.FormattedFileSize}");
    /// </code>
    /// </example>
    public string FormattedFileSize => FileValidationHelper.FormatFileSize(FileSize);

    /// <summary>
    /// Gets the professional document status for user interface display and workflow management.
    /// </summary>
    /// <remarks>
    /// This computed property provides human-readable document status based on current state flags,
    /// essential for professional document management and user interface status indication.
    /// 
    /// <para><strong>Status Classifications:</strong></para>
    /// <list type="bullet">
    /// <item><strong>"Deleted":</strong> Document is soft-deleted and not available for operations</item>
    /// <item><strong>"Checked Out":</strong> Document is under exclusive editing control</item>
    /// <item><strong>"Available":</strong> Document is available for normal operations</item>
    /// </list>
    /// 
    /// <para><strong>Professional UI Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Status Indicators:</strong> Provides clear status for professional interfaces</item>
    /// <item><strong>Color Coding:</strong> Supports status-based visual indicators</item>
    /// <item><strong>Selection Logic:</strong> Enables status-based document selection filtering</item>
    /// <item><strong>Professional Communication:</strong> Clear status for client and user communication</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional status display in UI
    /// Console.WriteLine($"Document status: {minimalDoc.Status}");
    /// 
    /// // Status-based UI styling
    /// switch (minimalDoc.Status)
    /// {
    ///     case "Available":
    ///         documentListItem.BackColor = Color.White;
    ///         break;
    ///     case "Checked Out":
    ///         documentListItem.BackColor = Color.LightYellow;
    ///         break;
    ///     case "Deleted":
    ///         documentListItem.BackColor = Color.LightGray;
    ///         break;
    /// }
    /// </code>
    /// </example>
    public string Status =>
        IsDeleted ? "Deleted" :
        IsCheckedOut ? "Checked Out" :
        "Available";

    /// <summary>
    /// Gets a value indicating whether the document is available for editing based on professional document management rules.
    /// </summary>
    /// <remarks>
    /// This computed property determines edit availability based on document status, essential for 
    /// professional document selection and UI control enabling/disabling logic.
    /// 
    /// <para><strong>Availability Criteria:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Not Checked Out:</strong> Document must not be under exclusive editing control</item>
    /// <item><strong>Not Deleted:</strong> Document must not be in soft-deleted state</item>
    /// <item><strong>Professional Standards:</strong> Aligns with professional document management practices</item>
    /// </list>
    /// 
    /// <para><strong>UI Integration Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Control State Management:</strong> Enables/disables UI controls based on availability</item>
    /// <item><strong>Selection Filtering:</strong> Filters available documents for edit operations</item>
    /// <item><strong>Professional Workflow:</strong> Supports professional document workflow logic</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional edit availability for UI operations
    /// editButton.Enabled = minimalDoc.IsAvailableForEdit;
    /// 
    /// // Document selection filtering
    /// var editableDocuments = documentList.Where(d => d.IsAvailableForEdit).ToList();
    /// 
    /// // Professional workflow logic
    /// if (minimalDoc.IsAvailableForEdit)
    /// {
    ///     EnableDocumentEditingOptions(minimalDoc);
    /// }
    /// else
    /// {
    ///     ShowDocumentUnavailableMessage(minimalDoc.Status);
    /// }
    /// </code>
    /// </example>
    public bool IsAvailableForEdit => !IsCheckedOut && !IsDeleted;

    /// <summary>
    /// Gets the complete file name including extension for file system operations.
    /// </summary>
    /// <remarks>
    /// This computed property provides the complete file name for file system operations,
    /// maintaining consistency with DisplayText for professional document identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// // File system operations with minimal data
    /// var fullName = minimalDoc.FullFileName; // "Contract_Amendment.pdf"
    /// var downloadFileName = minimalDoc.FullFileName;
    /// </code>
    /// </example>
    public string FullFileName => $"{FileName}{Extension}";

    /// <summary>
    /// Gets the document age since creation for professional lifecycle analysis.
    /// </summary>
    /// <remarks>
    /// This computed property calculates document age from creation date, useful for professional
    /// document lifecycle analysis and basic retention management in minimal scenarios.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional document age analysis for minimal operations
    /// var age = minimalDoc.DocumentAge;
    /// var ageInDays = age.TotalDays;
    /// 
    /// // Basic lifecycle indicators
    /// var isNewDocument = minimalDoc.IsRecentDocument;
    /// var ageDisplay = $"{ageInDays:F0} days old";
    /// </code>
    /// </example>
    public TimeSpan DocumentAge => DateTime.UtcNow - CreationDate;

    /// <summary>
    /// Gets a value indicating whether the document was created recently (within the last 7 days).
    /// </summary>
    /// <remarks>
    /// This computed property provides quick identification of recent documents, useful for
    /// highlighting new documents in professional document management interfaces.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Highlight recent documents in UI
    /// if (minimalDoc.IsRecentDocument)
    /// {
    ///     documentListItem.Font = new Font(documentListItem.Font, FontStyle.Bold);
    ///     documentListItem.ForeColor = Color.DarkGreen;
    /// }
    /// </code>
    /// </example>
    public bool IsRecentDocument => DocumentAge.TotalDays <= 7;

    /// <summary>
    /// Gets a value indicating whether the document is considered large based on professional standards.
    /// </summary>
    /// <remarks>
    /// This computed property identifies large documents based on FileValidationHelper thresholds,
    /// useful for UI indicators and professional workflow decisions.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Large document handling in UI
    /// if (minimalDoc.IsLargeFile)
    /// {
    ///     documentListItem.ToolTipText = $"Large file ({minimalDoc.FormattedFileSize}) - may take time to download";
    /// }
    /// </code>
    /// </example>
    public bool IsLargeFile => FileSize > FileValidationHelper.LargeFileSizeThreshold;

    /// <summary>
    /// Gets a value indicating whether this minimal document DTO is valid for professional operations.
    /// </summary>
    /// <remarks>
    /// This computed property provides quick validation status checking without performing full validation,
    /// useful for performance-critical scenarios and professional workflow optimization.
    /// 
    /// <para><strong>Basic Validity Criteria:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Valid ID:</strong> Non-empty GUID for proper identification</item>
    /// <item><strong>File Name Present:</strong> Non-empty file name for document identification</item>
    /// <item><strong>Extension Present:</strong> Valid file extension for format identification</item>
    /// <item><strong>Professional Standards:</strong> Meets basic professional document standards</item>
    /// </list>
    /// 
    /// <para><strong>Performance Note:</strong></para>
    /// This is a quick validation check. For comprehensive validation, use ValidateModel() method.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Quick validation for performance-critical scenarios
    /// if (minimalDoc.IsValid)
    /// {
    ///     ProcessValidMinimalDocument(minimalDoc);
    /// }
    /// else
    /// {
    ///     // Perform comprehensive validation for detailed errors
    ///     var validationResults = DocumentMinimalDto.ValidateModel(minimalDoc);
    ///     HandleValidationErrors(validationResults);
    /// }
    /// </code>
    /// </example>
    public bool IsValid =>
        Id != Guid.Empty &&
        !string.IsNullOrWhiteSpace(FileName) &&
        !string.IsNullOrWhiteSpace(Extension) &&
        FileSize >= 0;

    #endregion Professional Computed Properties

    #region Standardized Validation Implementation

    /// <summary>
    /// Validates core document properties including essential file metadata and document identification data.
    /// </summary>
    /// <returns>A collection of validation results for core property validation.</returns>
    /// <remarks>
    /// This method implements the first step of the BaseValidationDto validation hierarchy,
    /// validating essential document properties using standardized ADMS validation helpers for
    /// minimal yet comprehensive validation suitable for lightweight operations.
    /// 
    /// <para><strong>Core Property Validation Steps:</strong></para>
    /// <list type="number">
    /// <item><strong>Document ID validation:</strong> Using BaseValidationDto.ValidateGuid() with allowEmpty=false</item>
    /// <item><strong>File name validation:</strong> Using FileValidationHelper.ValidateFileName() for essential validation</item>
    /// <item><strong>Extension validation:</strong> Using FileValidationHelper.ValidateExtension() for format and security</item>
    /// <item><strong>Creation date validation:</strong> Using BaseValidationDto.ValidateRequiredDate() for temporal consistency</item>
    /// </list>
    /// 
    /// <para><strong>Professional Standards Integration:</strong></para>
    /// Essential validation uses centralized ADMS validation helpers to ensure consistency
    /// while maintaining minimal overhead for performance-critical operations.
    /// 
    /// <para><strong>Performance Optimization:</strong></para>
    /// Uses yield return for lazy evaluation with minimal validation overhead,
    /// optimized for high-throughput document enumeration and selection operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Core property validation is automatically called during minimal DTO validation
    /// var minimalDoc = new DocumentMinimalDto { /* essential properties */ };
    /// var validationResults = DocumentMinimalDto.ValidateModel(minimalDoc);
    /// 
    /// // Validation includes essential properties only:
    /// // - Document ID (required, non-empty GUID)
    /// // - File name (professional naming standards)
    /// // - Extension (security validation)
    /// // - Creation date (temporal consistency)
    /// // - File size (basic range validation)
    /// </code>
    /// </example>
    protected override IEnumerable<ValidationResult> ValidateCoreProperties()
    {
        // Validate document ID using standardized GUID validation (do not allow empty)
        foreach (var result in ValidateGuid(Id, nameof(Id), allowEmpty: false))
            yield return result;

        // Validate file name using FileValidationHelper for essential validation
        foreach (var result in FileValidationHelper.ValidateFileName(FileName, nameof(FileName)))
            yield return result;

        // Validate file extension using FileValidationHelper for security and format validation
        foreach (var result in FileValidationHelper.ValidateExtension(Extension, nameof(Extension)))
            yield return result;

        // Validate creation date using standardized date validation
        foreach (var result in ValidateRequiredDate(CreationDate, nameof(CreationDate)))
            yield return result;

        // Additional essential validation for minimal operations
        if (FileSize < 0)
        {
            yield return CreateValidationResult(
                "File size cannot be negative for accurate document representation.",
                nameof(FileSize));
        }

        // Essential file name validation for professional standards
        if (!string.IsNullOrWhiteSpace(FileName) && FileName.Trim() != FileName)
        {
            yield return CreateValidationResult(
                "File name cannot have leading or trailing whitespace for professional compatibility.",
                nameof(FileName));
        }

        // Essential extension validation for professional standards
        if (!string.IsNullOrWhiteSpace(Extension) && !Extension.StartsWith('.'))
        {
            yield return CreateValidationResult(
                "File extension must start with a dot (.) for proper file identification.",
                nameof(Extension));
        }
    }

    /// <summary>
    /// Validates business rules specific to document status consistency and professional standards for minimal operations.
    /// </summary>
    /// <returns>A collection of validation results for business rule validation.</returns>
    /// <remarks>
    /// This method implements the second step of the BaseValidationDto validation hierarchy,
    /// validating essential business rules for professional document management in minimal scenarios.
    /// 
    /// <para><strong>Business Rules Validated:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document State Logic:</strong> Cannot be both deleted and checked out simultaneously</item>
    /// <item><strong>Professional Standards:</strong> Essential professional document management compliance</item>
    /// <item><strong>Status Consistency:</strong> Document status flags must be logically consistent</item>
    /// <item><strong>Professional Display:</strong> Status combinations must be professionally valid</item>
    /// </list>
    /// 
    /// <para><strong>Minimal Validation Focus:</strong></para>
    /// Business rules focus on essential consistency checks suitable for minimal operations
    /// without the overhead of comprehensive validation required for full document operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Business rule validation automatically checks essential consistency:
    /// var invalidDoc = new DocumentMinimalDto 
    /// { 
    ///     IsDeleted = true,
    ///     IsCheckedOut = true // This would fail business rule validation
    /// };
    /// 
    /// // Professional status combinations are validated
    /// var validDoc = new DocumentMinimalDto 
    /// { 
    ///     IsDeleted = false,
    ///     IsCheckedOut = true // This is valid
    /// };
    /// </code>
    /// </example>
    protected override IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Business rule: Document state consistency - cannot be both deleted and checked out
        if (IsDeleted && IsCheckedOut)
        {
            yield return CreateValidationResult(
                "Document cannot be both deleted and checked out simultaneously. This violates professional document management rules.",
                nameof(IsDeleted), nameof(IsCheckedOut));
        }

        // Business rule: Professional file naming standards for minimal operations
        if (!string.IsNullOrWhiteSpace(FileName))
        {
            // Essential professional naming: should not be only special characters
            if (FileName.All(c => !char.IsLetterOrDigit(c)))
            {
                yield return CreateValidationResult(
                    "File name must contain at least one letter or digit for professional document identification.",
                    nameof(FileName));
            }

            // Professional naming: reasonable length for display
            if (FileName.Length > 80) // Shorter threshold for minimal operations
            {
                yield return CreateValidationResult(
                    "File name is very long and may impact professional display and usability in minimal scenarios.",
                    nameof(FileName));
            }
        }

        // Business rule: Professional file size awareness for minimal operations
        if (FileSize is > 0 and > FileValidationHelper.LargeFileSizeThreshold)
        {
            // Note for minimal operations - less strict than full validation
            yield return CreateValidationResult(
                $"Document size ({FormattedFileSize}) is large and may impact performance in document selection operations.",
                nameof(FileSize));
        }

        // Business rule: Creation date reasonableness for minimal operations
        var documentAge = DocumentAge;
        if (documentAge.TotalDays > 36500) // 100 years - basic sanity check
        {
            yield return CreateValidationResult(
                $"Document age ({documentAge.TotalDays:F0} days) exceeds reasonable limits. Verify creation date accuracy.",
                nameof(CreationDate));
        }
    }

    /// <summary>
    /// Validates cross-property relationships for minimal document consistency and professional standards.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    /// <remarks>
    /// This method implements the third step of the BaseValidationDto validation hierarchy,
    /// validating essential relationships between minimal document properties for consistency
    /// in lightweight operations.
    /// 
    /// <para><strong>Cross-Property Rules for Minimal Operations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Temporal Consistency:</strong> Creation date must be reasonable for document age</item>
    /// <item><strong>Status Logic:</strong> Document status flags must be logically consistent</item>
    /// <item><strong>Professional Standards:</strong> Basic professional standards compliance for minimal scenarios</item>
    /// </list>
    /// 
    /// <para><strong>Minimal Validation Focus:</strong></para>
    /// Cross-property validation focuses on essential consistency without complex relationships,
    /// suitable for performance-critical minimal operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Cross-property validation for minimal operations automatically checks:
    /// // - Creation date is not significantly in the future
    /// // - Document age is reasonable
    /// // - Status combinations are professionally valid
    /// 
    /// var minimalDoc = new DocumentMinimalDto 
    /// { 
    ///     CreationDate = DateTime.UtcNow.AddDays(1), // Would fail cross-property validation
    ///     /* other properties */
    /// };
    /// </code>
    /// </example>
    protected override IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // Cross-property validation: Creation date reasonableness
        if (CreationDate > DateTime.UtcNow.AddMinutes(5)) // Allow small clock skew
        {
            yield return CreateValidationResult(
                "Document creation date cannot be significantly in the future. Verify system clock accuracy.",
                nameof(CreationDate));
        }

        // Cross-property validation: Document age and status consistency
        var documentAge = DocumentAge;
        if (documentAge.TotalMinutes < -5) // Future creation date
        {
            yield return CreateValidationResult(
                "Document creation date is in the future, which is not allowed for professional document management.",
                nameof(CreationDate));
        }

        // Cross-property validation: File size and name consistency for professional standards
        if (FileSize > 0 && !string.IsNullOrWhiteSpace(FileName))
        {
            // Very basic consistency check - large files should have descriptive names
            if (FileSize > FileValidationHelper.LargeFileSizeThreshold && FileName.Length < 5)
            {
                yield return CreateValidationResult(
                    "Large documents should have descriptive file names for professional document management.",
                    nameof(FileName), nameof(FileSize));
            }
        }

        // Cross-property validation: Extension and name consistency
        if (string.IsNullOrWhiteSpace(FileName) || string.IsNullOrWhiteSpace(Extension)) yield break;
        // Ensure file name doesn't already contain the extension
        if (!FileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase)) yield break;
        yield return CreateValidationResult(
            "File name should not include the file extension as it is specified separately.",
            nameof(FileName), nameof(Extension));
    }

    #endregion Standardized Validation Implementation

    #region Enhanced Static Validation Methods

    /// <summary>
    /// Validates a DocumentMinimalDto instance using the standardized BaseValidationDto validation framework.
    /// </summary>
    /// <param name="dto">The DocumentMinimalDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides comprehensive yet lightweight validation using the standardized 
    /// BaseValidationDto framework, ensuring consistent validation patterns for minimal document operations.
    /// 
    /// <para><strong>Validation Completeness:</strong></para>
    /// Performs essential validation hierarchy including data annotations, core properties,
    /// and business rules optimized for minimal document scenarios.
    /// 
    /// <para><strong>Performance Optimization:</strong></para>
    /// Uses lightweight validation suitable for high-throughput minimal document operations
    /// while maintaining professional validation standards.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Essential minimal document validation
    /// var minimalDoc = new DocumentMinimalDto { /* essential properties */ };
    /// var validationResults = DocumentMinimalDto.ValidateModel(minimalDoc);
    /// 
    /// if (BaseValidationDto.HasValidationErrors(validationResults))
    /// {
    ///     var summary = BaseValidationDto.GetValidationSummary(validationResults);
    ///     _logger.LogError("Minimal document validation failed: {ValidationSummary}", summary);
    ///     throw new ValidationException($"Minimal document validation failed: {summary}");
    /// }
    /// 
    /// // Document is guaranteed to be valid for minimal operations
    /// await ProcessValidMinimalDocument(minimalDoc);
    /// </code>
    /// </example>
    public static new IList<ValidationResult> ValidateModel([AllowNull] DocumentMinimalDto? dto)
    {
        return BaseValidationDto.ValidateModel(dto);
    }

    /// <summary>
    /// Creates a DocumentMinimalDto from an ADMS.API.Entities.Document entity with essential validation.
    /// </summary>
    /// <param name="entity">The Document entity to convert. Cannot be null.</param>
    /// <returns>A valid DocumentMinimalDto instance with essential properties populated.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails essential validation.</exception>
    /// <remarks>
    /// This factory method provides optimized entity-to-DTO conversion for minimal operations
    /// while ensuring essential validation using the standardized BaseValidationDto framework.
    /// 
    /// <para><strong>Performance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Fast Conversion:</strong> Minimal property mapping for optimal performance</item>
    /// <item><strong>Essential Validation:</strong> Core validation only, no complex relationship validation</item>
    /// <item><strong>Memory Efficient:</strong> No navigation property loading for minimal footprint</item>
    /// <item><strong>Validation Guarantee:</strong> Ensures essential validation standards</item>
    /// </list>
    /// 
    /// <para><strong>Professional Usage:</strong></para>
    /// Ideal for document lists, search results, selection interfaces, and any scenario
    /// requiring rapid document enumeration with essential validation guarantees.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create minimal document DTO from entity
    /// var entity = await context.Documents.FirstAsync(d => d.Id == documentId);
    /// var minimalDoc = DocumentMinimalDto.FromEntity(entity);
    /// 
    /// // DTO is guaranteed to be valid with essential properties
    /// Console.WriteLine($"Minimal document: {minimalDoc.DisplayText}");
    /// Console.WriteLine($"Status: {minimalDoc.Status}");
    /// Console.WriteLine($"Age: {minimalDoc.DocumentAge.TotalDays:F1} days");
    /// 
    /// // Professional UI integration
    /// documentSelectionList.Add(minimalDoc);
    /// </code>
    /// </example>
    public static DocumentMinimalDto FromEntity([NotNull] Entities.Document entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var dto = new DocumentMinimalDto
        {
            Id = entity.Id,
            FileName = entity.FileName,
            Extension = entity.Extension,
            FileSize = entity.FileSize,
            IsCheckedOut = entity.IsCheckedOut,
            IsDeleted = entity.IsDeleted,
            CreationDate = entity.Matter.CreationDate
        };

        // Perform essential validation using standardized framework
        var validationResults = ValidateModel(dto);
        if (!BaseValidationDto.HasValidationErrors(validationResults)) return dto;
        var summary = BaseValidationDto.GetValidationSummary(validationResults);
        var entityInfo = $"{entity.FileName}{entity.Extension}";
        throw new ValidationException($"Failed to create valid DocumentMinimalDto from entity '{entityInfo}' ({entity.Id}): {summary}");

    }

    /// <summary>
    /// Creates multiple DocumentMinimalDto instances from a collection of entities with comprehensive validation and error handling.
    /// </summary>
    /// <param name="entities">The collection of Document entities to convert. Cannot be null.</param>
    /// <param name="skipInvalidEntities">Whether to skip invalid entities instead of throwing exceptions (default: false).</param>
    /// <returns>A collection of valid DocumentMinimalDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <exception cref="ValidationException">Thrown when entities fail validation and skipInvalidEntities is false.</exception>
    /// <remarks>
    /// This bulk conversion method provides high-performance entity-to-DTO conversion optimized for
    /// minimal document operations with detailed error handling and validation reporting.
    /// 
    /// <para><strong>Processing Modes:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Strict Mode (skipInvalidEntities=false):</strong> Throws exception on first validation failure</item>
    /// <item><strong>Fault-Tolerant Mode (skipInvalidEntities=true):</strong> Logs errors and continues processing</item>
    /// </list>
    /// 
    /// <para><strong>Performance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>High-Throughput Processing:</strong> Optimized for large document collections</item>
    /// <item><strong>Memory Efficient:</strong> Minimal memory usage for bulk operations</item>
    /// <item><strong>Error Resilient:</strong> Continues processing despite individual entity errors</item>
    /// <item><strong>Professional Reporting:</strong> Comprehensive error reporting for data quality assessment</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // High-performance conversion for document lists
    /// var entities = await context.Documents
    ///     .Where(d => !d.IsDeleted)
    ///     .OrderBy(d => d.FileName)
    ///     .ToListAsync();
    /// 
    /// // Fault-tolerant bulk conversion
    /// var minimalDocs = DocumentMinimalDto.FromEntities(entities, skipInvalidEntities: true);
    /// Console.WriteLine($"Processed {entities.Count} entities, got {minimalDocs.Count} valid minimal DTOs");
    /// 
    /// // Professional document selection interface
    /// documentSelectionComboBox.DataSource = minimalDocs;
    /// documentSelectionComboBox.DisplayMember = nameof(DocumentMinimalDto.DisplayText);
    /// documentSelectionComboBox.ValueMember = nameof(DocumentMinimalDto.Id);
    /// </code>
    /// </example>
    public static IList<DocumentMinimalDto> FromEntities(
        [NotNull] IEnumerable<Entities.Document> entities,
        bool skipInvalidEntities = false)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        var result = new List<DocumentMinimalDto>();
        var errors = new List<string>();
        var processedCount = 0;
        var entitiesList = entities.ToList(); // Materialize for count

        foreach (var entity in entitiesList)
        {
            processedCount++;
            try
            {
                var dto = FromEntity(entity);
                result.Add(dto);
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                var entityInfo = $"{entity.FileName}{entity.Extension}";
                var errorMessage = $"Document {entity.Id} ({entityInfo}): {ex.Message}";
                errors.Add(errorMessage);

                if (skipInvalidEntities)
                {
                    // In production, use proper logging framework
                    Console.WriteLine($"Warning: Skipped invalid minimal document entity ({processedCount}/{entitiesList.Count}): {errorMessage}");
                }
                else
                {
                    // In strict mode, throw on first error with enhanced context
                    throw new ValidationException(
                        $"Minimal entity conversion failed on document {processedCount} of {entitiesList.Count}: {errorMessage}",
                        ex);
                }
            }
        }

        // Provide comprehensive processing summary for performance analysis
        if (!errors.Any() || !skipInvalidEntities) return result;
        var successRate = (double)(processedCount - errors.Count) / processedCount * 100;
        Console.WriteLine($"Minimal document entity conversion completed:");
        Console.WriteLine($"  - Total Processed: {processedCount:N0}");
        Console.WriteLine($"  - Successfully Converted: {result.Count:N0}");
        Console.WriteLine($"  - Conversion Errors: {errors.Count:N0}");
        Console.WriteLine($"  - Success Rate: {successRate:F1}%");

        // Log error summary for analysis (limit for readability)
        if (errors.Count <= 5)
        {
            Console.WriteLine($"  - Error Details:");
            foreach (var error in errors)
            {
                Console.WriteLine($"    • {error}");
            }
        }
        else
        {
            Console.WriteLine($"  - Error Details (first 5 of {errors.Count}):");
            foreach (var error in errors.Take(5))
            {
                Console.WriteLine($"    • {error}");
            }
            Console.WriteLine($"    ... and {errors.Count - 5} more conversion errors.");
        }

        return result;
    }

    /// <summary>
    /// Creates a DocumentMinimalDto with essential document information for lightweight operations with standardized validation.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="extension">The file extension.</param>
    /// <param name="fileSize">The file size in bytes.</param>
    /// <param name="creationDate">The creation date.</param>
    /// <param name="isCheckedOut">Whether the document is checked out (default: false).</param>
    /// <param name="isDeleted">Whether the document is deleted (default: false).</param>
    /// <returns>A DocumentMinimalDto with essential information and validation guarantee.</returns>
    /// <exception cref="ArgumentException">Thrown when essential parameters are invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method creates lightweight DocumentMinimalDto instances for scenarios where only
    /// essential document information is needed, optimized for performance while ensuring
    /// essential validation using the standardized BaseValidationDto framework.
    /// 
    /// <para><strong>Performance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Minimal Memory Usage:</strong> Essential properties only for optimal memory efficiency</item>
    /// <item><strong>Fast Creation:</strong> Optimized creation process for high-throughput scenarios</item>
    /// <item><strong>Essential Validation:</strong> Core validation only, minimal validation overhead</item>
    /// <item><strong>Professional Standards:</strong> Maintains professional validation standards</item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document References:</strong> Creating document references for selection and lookup</item>
    /// <item><strong>Performance Operations:</strong> High-speed document enumeration and processing</item>
    /// <item><strong>UI Integration:</strong> Document data for user interface controls and displays</item>
    /// <item><strong>API Responses:</strong> Lightweight document data for performance-critical endpoints</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create essential minimal document
    /// var minimalDoc = DocumentMinimalDto.CreateEssentialDocument(
    ///     Guid.NewGuid(),
    ///     "Legal_Contract_Amendment",
    ///     ".pdf",
    ///     2547832,
    ///     DateTime.UtcNow);
    /// 
    /// // Document is guaranteed to be valid with essential properties
    /// Console.WriteLine($"Created minimal document: {minimalDoc.DisplayText}");
    /// Console.WriteLine($"Status: {minimalDoc.Status}");
    /// Console.WriteLine($"Size: {minimalDoc.FormattedFileSize}");
    /// 
    /// // Professional UI integration
    /// documentSelectionList.Add(minimalDoc);
    /// </code>
    /// </example>
    public static DocumentMinimalDto CreateEssentialDocument(
        Guid documentId,
        [NotNull] string fileName,
        [NotNull] string extension,
        long fileSize,
        DateTime creationDate,
        bool isCheckedOut = false,
        bool isDeleted = false)
    {
        // Validate essential parameters before DTO creation
        if (documentId == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty.", nameof(documentId));

        ArgumentException.ThrowIfNullOrWhiteSpace(fileName, nameof(fileName));
        ArgumentException.ThrowIfNullOrWhiteSpace(extension, nameof(extension));

        if (fileSize < 0)
            throw new ArgumentException("File size cannot be negative.", nameof(fileSize));

        var dto = new DocumentMinimalDto
        {
            Id = documentId,
            FileName = fileName.Trim(),
            Extension = extension.StartsWith('.') ? extension : $".{extension}",
            FileSize = fileSize,
            IsCheckedOut = isCheckedOut,
            IsDeleted = isDeleted,
            CreationDate = creationDate
        };

        // Perform essential validation using standardized framework
        var validationResults = ValidateModel(dto);
        if (!BaseValidationDto.HasValidationErrors(validationResults)) return dto;
        var summary = BaseValidationDto.GetValidationSummary(validationResults);
        throw new ValidationException($"Essential document validation failed: {summary}");

    }

    /// <summary>
    /// Validates multiple DocumentMinimalDto instances and returns comprehensive validation analysis with detailed error reporting.
    /// </summary>
    /// <param name="documents">The collection of DocumentMinimalDto instances to validate. Can be null.</param>
    /// <param name="includeStatistics">Whether to include validation statistics in the analysis (default: true).</param>
    /// <returns>A comprehensive validation analysis including results, statistics, and error summaries.</returns>
    /// <remarks>
    /// This advanced validation method provides comprehensive analysis of minimal document collections
    /// with detailed reporting suitable for batch operations, data quality assessment, and
    /// professional document management oversight optimized for minimal scenarios.
    /// 
    /// <para><strong>Analysis Components:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Individual Results:</strong> Validation results for each document with index tracking</item>
    /// <item><strong>Statistical Summary:</strong> Validation success rates and error distribution</item>
    /// <item><strong>Error Categorization:</strong> Grouped errors by type for analysis</item>
    /// <item><strong>Professional Reporting:</strong> Summary suitable for professional review and action</item>
    /// </list>
    /// 
    /// <para><strong>Performance Focus:</strong></para>
    /// Optimized for minimal document scenarios with lightweight analysis suitable for
    /// large document collections and performance-critical validation operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var minimalDocuments = new List<DocumentMinimalDto> { doc1, doc2, doc3, invalidDoc };
    /// var analysis = DocumentMinimalDto.ValidateDocumentCollection(minimalDocuments, includeStatistics: true);
    /// 
    /// Console.WriteLine($"Minimal Document Validation Summary:");
    /// Console.WriteLine($"Total Documents: {analysis.TotalDocuments}");
    /// Console.WriteLine($"Valid Documents: {analysis.ValidDocuments}");
    /// Console.WriteLine($"Invalid Documents: {analysis.InvalidDocuments}");
    /// Console.WriteLine($"Success Rate: {analysis.SuccessRate:P2}");
    /// 
    /// foreach (var (index, errors) in analysis.ValidationResults)
    /// {
    ///     Console.WriteLine($"Document {index} errors: {string.Join("; ", errors.Select(e => e.ErrorMessage))}");
    /// }
    /// </code>
    /// </example>
    public static DocumentMinimalValidationAnalysis ValidateDocumentCollection(
        [AllowNull] IEnumerable<DocumentMinimalDto>? documents,
        bool includeStatistics = true)
    {
        if (documents is null)
        {
            return new DocumentMinimalValidationAnalysis
            {
                ValidationResults = new Dictionary<int, IList<ValidationResult>>(),
                TotalDocuments = 0,
                ValidDocuments = 0,
                InvalidDocuments = 0,
                SuccessRate = 0.0,
                ErrorSummary = "No minimal documents provided for validation.",
                ValidationTimestamp = DateTime.UtcNow
            };
        }

        var documentList = documents.ToList();
        var validationResults = BaseValidationDto.ValidateModels(documentList);
        var validCount = documentList.Count - validationResults.Count;
        var invalidCount = validationResults.Count;
        var successRate = documentList.Count > 0 ? (double)validCount / documentList.Count : 0.0;

        var analysis = new DocumentMinimalValidationAnalysis
        {
            ValidationResults = validationResults,
            TotalDocuments = documentList.Count,
            ValidDocuments = validCount,
            InvalidDocuments = invalidCount,
            SuccessRate = successRate,
            ValidationTimestamp = DateTime.UtcNow,
            ErrorSummary = string.Empty
        };

        if (includeStatistics && validationResults.Any())
        {
            // Generate detailed error summary for professional analysis
            var errorCategories = new Dictionary<string, int>();
            var allErrors = validationResults.SelectMany(kvp => kvp.Value).ToList();

            foreach (var memberNames in allErrors.Select(error => error.MemberNames.Any() ? string.Join(", ", error.MemberNames) : "General"))
            {
                errorCategories[memberNames] = errorCategories.GetValueOrDefault(memberNames, 0) + 1;
            }

            var errorSummaryBuilder = new StringBuilder();
            errorSummaryBuilder.AppendLine($"Minimal document validation completed: {validCount:N0}/{documentList.Count:N0} documents valid ({successRate:P2} success rate)");

            if (errorCategories.Any())
            {
                errorSummaryBuilder.AppendLine("Error distribution by property:");
                foreach (var (category, count) in errorCategories.OrderByDescending(kvp => kvp.Value))
                {
                    errorSummaryBuilder.AppendLine($"  - {category}: {count:N0} error(s)");
                }

                // Add top error messages for analysis
                var topErrors = allErrors
                    .GroupBy(e => e.ErrorMessage)
                    .OrderByDescending(g => g.Count())
                    .Take(3) // Fewer for minimal operations
                    .ToList();

                if (topErrors.Any())
                {
                    errorSummaryBuilder.AppendLine("Most common error messages:");
                    foreach (var errorGroup in topErrors)
                    {
                        errorSummaryBuilder.AppendLine($"  - \"{errorGroup.Key}\" ({errorGroup.Count()} occurrences)");
                    }
                }
            }

            // Instead of assigning after initialization, set ErrorSummary in the object initializer below
            analysis = new DocumentMinimalValidationAnalysis
            {
                ValidationResults = validationResults,
                TotalDocuments = documentList.Count,
                ValidDocuments = validCount,
                InvalidDocuments = invalidCount,
                SuccessRate = successRate,
                ValidationTimestamp = DateTime.UtcNow,
                ErrorSummary = errorSummaryBuilder.ToString().TrimEnd()
            };
        }
        else
        {
            analysis = new DocumentMinimalValidationAnalysis
            {
                ValidationResults = validationResults,
                TotalDocuments = documentList.Count,
                ValidDocuments = validCount,
                InvalidDocuments = invalidCount,
                SuccessRate = successRate,
                ValidationTimestamp = DateTime.UtcNow,
                ErrorSummary = invalidCount == 0
                    ? "All minimal documents passed validation successfully."
                    : $"{invalidCount:N0} minimal document(s) failed validation out of {documentList.Count:N0} total."
            };
        }

        return analysis;
    }

    #endregion Enhanced Static Validation Methods

    #region Enhanced Business Logic Methods

    /// <summary>
    /// Determines whether the document can be selected for operations based on professional document management rules.
    /// </summary>
    /// <returns>True if the document can be selected; otherwise, false.</returns>
    /// <remarks>
    /// This method provides comprehensive selection availability checking for minimal document operations,
    /// supporting professional workflow decisions and user interface control logic.
    /// 
    /// <para><strong>Selection Availability Criteria:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Valid Status:</strong> Must be valid according to essential validation rules</item>
    /// <item><strong>Not Deleted:</strong> Deleted documents should not be available for normal selection</item>
    /// <item><strong>Professional Standards:</strong> Must meet basic professional document standards</item>
    /// </list>
    /// 
    /// <para><strong>Professional Benefits:</strong></para>
    /// Ensures documents meet essential professional standards before allowing selection operations,
    /// supporting professional workflow optimization and user interface logic.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional document selection logic
    /// var selectableDocuments = documentList.Where(d => d.CanBeSelected()).ToList();
    /// 
    /// // UI control enabling
    /// selectButton.Enabled = minimalDoc.CanBeSelected();
    /// 
    /// // Professional workflow decisions
    /// if (minimalDoc.CanBeSelected())
    /// {
    ///     EnableDocumentSelectionFeatures(minimalDoc);
    /// }
    /// else
    /// {
    ///     ShowDocumentSelectionRestrictions(minimalDoc.Status);
    /// }
    /// </code>
    /// </example>
    public bool CanBeSelected() => IsValid && !IsDeleted;

    /// <summary>
    /// Gets comprehensive minimal document statistics including validation analysis and professional metrics.
    /// </summary>
    /// <returns>A comprehensive dictionary containing essential document statistics and validation status.</returns>
    /// <remarks>
    /// This method provides essential document analysis optimized for minimal scenarios including validation status,
    /// basic metrics, and professional indicators useful for UI display, reporting, and administrative operations.
    /// 
    /// <para><strong>Statistics Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>DocumentInfo:</strong> Essential document identification and metadata</item>
    /// <item><strong>Status:</strong> Current document status and availability information</item>
    /// <item><strong>Metrics:</strong> Basic quantitative analysis including size and age</item>
    /// <item><strong>Timestamps:</strong> Essential temporal analysis optimized for minimal operations</item>
    /// <item><strong>ValidationStatus:</strong> Current validation status and basic error summary</item>
    /// </list>
    /// 
    /// <para><strong>Performance Optimization:</strong></para>
    /// Statistics are optimized for minimal scenarios with lightweight analysis suitable for
    /// high-throughput operations and responsive user interface requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = minimalDoc.GetEssentialStatistics();
    /// 
    /// // Professional minimal document reporting
    /// Console.WriteLine("Minimal Document Analysis:");
    /// Console.WriteLine($"Document: {stats["DocumentInfo"]}");
    /// Console.WriteLine($"Status: {stats["Status"]}");
    /// Console.WriteLine($"Metrics: {stats["Metrics"]}");
    /// Console.WriteLine($"Validation: {stats["ValidationStatus"]}");
    /// 
    /// // UI integration
    /// var statusInfo = (dynamic)stats["Status"];
    /// documentStatusLabel.Text = statusInfo.Status;
    /// documentSizeLabel.Text = statusInfo.FormattedFileSize;
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetEssentialStatistics()
    {
        // Perform validation to get current status
        var validationResults = ValidateModel(this);
        var validationStatus = BaseValidationDto.HasValidationErrors(validationResults)
            ? "Invalid"
            : "Valid";

        // Essential analysis optimized for minimal operations
        var documentAge = DocumentAge;

        return new Dictionary<string, object>
        {
            // Essential document identification and metadata
            ["DocumentInfo"] = new
            {
                Id,
                FileName,
                Extension,
                FullFileName = DisplayText,
                FileSize = FormattedFileSize,
                FileSizeBytes = FileSize
            },

            // Current document status and availability
            ["Status"] = new
            {
                Status,
                IsCheckedOut,
                IsDeleted,
                IsAvailableForEdit,
                CanBeSelected = CanBeSelected(),
                IsRecentDocument,
                IsLargeFile
            },

            // Basic quantitative analysis and measurements
            ["Metrics"] = new
            {
                DocumentAgeInDays = documentAge.TotalDays,
                DocumentAgeInHours = documentAge.TotalHours,
                IsNewDocument = documentAge.TotalDays < 1,
                IsActiveDocument = !IsDeleted && !IsCheckedOut,
                FileSizeCategory = FileSize switch
                {
                    < 1024 => "Small",
                    < 1024 * 1024 => "Medium",
                    < 10 * 1024 * 1024 => "Large",
                    _ => "Very Large"
                }
            },

            // Essential temporal analysis optimized for minimal operations
            ["Timestamps"] = new
            {
                CreationDate,
                CreationDateLocal = CreationDate.ToLocalTime(),
                DocumentAge = $"{documentAge.TotalDays:F1} days",
                CreationDateFormatted = CreationDate.ToString("yyyy-MM-dd HH:mm:ss UTC")
            },

            // Current validation status and basic error analysis
            ["ValidationStatus"] = new
            {
                IsValid,
                ValidationSummary = validationStatus,
                HasValidationErrors = BaseValidationDto.HasValidationErrors(validationResults),
                ValidationErrorCount = validationResults.Count,
                EssentialValidation = "Complete"
            }
        };
    }

    /// <summary>
    /// Gets document information suitable for professional audit reports and compliance documentation in minimal format.
    /// </summary>
    /// <returns>A structured dictionary containing audit-ready minimal document information.</returns>
    /// <remarks>
    /// This method provides minimal document information specifically formatted for professional audit reports,
    /// compliance documentation, and regulatory reporting requirements optimized for lightweight operations.
    /// 
    /// <para><strong>Audit Information Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Identification:</strong> Essential professional document identification data</item>
    /// <item><strong>File Metadata:</strong> Essential file metadata for audit trail completeness</item>
    /// <item><strong>Status Information:</strong> Current document status for audit verification</item>
    /// <item><strong>Temporal Data:</strong> Creation information for legal chronology</item>
    /// <item><strong>Compliance Status:</strong> Basic validation status for audit compliance</item>
    /// </list>
    /// 
    /// <para><strong>Professional Compliance:</strong></para>
    /// Information is structured to support professional responsibility requirements,
    /// legal discovery processes, and regulatory compliance reporting standards in minimal format.
    /// </remarks>
    /// <example>
    /// <code>
    /// var auditInfo = minimalDoc.GetAuditInformation();
    /// 
    /// // Professional audit reporting
    /// foreach (var (category, data) in auditInfo)
    /// {
    ///     Console.WriteLine($"{category}: {data}");
    /// }
    /// 
    /// // Compliance verification
    /// var statusInfo = (dynamic)auditInfo["StatusInformation"];
    /// if (statusInfo.IsAuditCompliant)
    /// {
    ///     Console.WriteLine("Minimal document passes audit compliance checks");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetAuditInformation()
    {
        // Get current validation status for audit
        var validationResults = ValidateModel(this);
        var isAuditCompliant = !BaseValidationDto.HasValidationErrors(validationResults);

        return new Dictionary<string, object>
        {
            // Essential professional document identification
            ["DocumentIdentification"] = new
            {
                DocumentId = Id,
                FileName,
                Extension,
                FullFileName = DisplayText,
                CreationDate,
                CreationDateFormatted = CreationDate.ToString("yyyy-MM-dd HH:mm:ss UTC")
            },

            // Essential file metadata for audit
            ["FileMetadata"] = new
            {
                FileSize,
                FormattedFileSize,
                FileSizeCategory = FileSize switch
                {
                    < 1024 => "Small",
                    < 1024 * 1024 => "Medium",
                    < 10 * 1024 * 1024 => "Large",
                    _ => "Very Large"
                },
                IsLargeFile
            },

            // Current status for audit verification
            ["StatusInformation"] = new
            {
                Status,
                IsCheckedOut,
                IsDeleted,
                IsAvailableForEdit,
                IsAuditCompliant = isAuditCompliant,
                CanBeSelected = CanBeSelected()
            },

            // Essential temporal data for legal chronology
            ["TemporalData"] = new
            {
                CreationDate,
                DocumentAge = DocumentAge.TotalDays,
                DocumentAgeFormatted = $"{DocumentAge.TotalDays:F1} days",
                IsRecentDocument,
                AuditComplianceDate = CreationDate.ToString("yyyy-MM-dd")
            }
        };
    }

    #endregion Enhanced Business Logic Methods

    #region Supporting Data Classes

    /// <summary>
    /// Represents comprehensive validation analysis results for minimal document collections.
    /// </summary>
    /// <remarks>
    /// This class provides structured analysis of minimal document validation operations,
    /// useful for batch processing, data quality assessment, and professional oversight optimized for minimal scenarios.
    /// </remarks>
    public record DocumentMinimalValidationAnalysis
    {
        /// <summary>Gets the validation results keyed by document index.</summary>
        public required IReadOnlyDictionary<int, IList<ValidationResult>> ValidationResults { get; init; }

        /// <summary>Gets the total number of minimal documents analyzed.</summary>
        public required int TotalDocuments { get; init; }

        /// <summary>Gets the number of valid minimal documents.</summary>
        public required int ValidDocuments { get; init; }

        /// <summary>Gets the number of invalid minimal documents.</summary>
        public required int InvalidDocuments { get; init; }

        /// <summary>Gets the validation success rate as a decimal between 0 and 1.</summary>
        public required double SuccessRate { get; init; }

        /// <summary>Gets a comprehensive error summary for professional review.</summary>
        public required string ErrorSummary { get; init; }

        /// <summary>Gets the timestamp when the validation analysis was performed.</summary>
        public required DateTime ValidationTimestamp { get; init; }
    }

    #endregion Supporting Data Classes

    #region Professional Comparison and Equality Implementation

    /// <summary>
    /// Compares the current DocumentMinimalDto with another DocumentMinimalDto for ordering purposes.
    /// </summary>
    /// <param name="other">The DocumentMinimalDto to compare with the current DocumentMinimalDto.</param>
    /// <returns>
    /// A value that indicates the relative order of the documents being compared.
    /// Less than zero: This document precedes the other document.
    /// Zero: This document occurs in the same position as the other document.
    /// Greater than zero: This document follows the other document.
    /// </returns>
    /// <remarks>
    /// Comparison is performed based on display text (filename + extension) for alphabetical ordering,
    /// optimized for professional document display and user interface purposes in minimal scenarios.
    /// 
    /// <para><strong>Comparison Logic:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Primary Sort:</strong> Alphabetical by DisplayText (filename + extension)</item>
    /// <item><strong>Secondary Sort:</strong> By creation date for documents with identical names</item>
    /// <item><strong>Null Handling:</strong> Null documents are considered "less than" non-null documents</item>
    /// <item><strong>Professional Ordering:</strong> Case-insensitive comparison for professional consistency</item>
    /// </list>
    /// 
    /// <para><strong>Performance Optimization:</strong></para>
    /// Comparison logic is optimized for minimal operations with fast comparison suitable for
    /// large document collection sorting and professional user interface operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Sort minimal documents alphabetically for professional display
    /// var sortedDocuments = minimalDocuments.OrderBy(d => d).ToList();
    /// 
    /// // Compare specific minimal documents
    /// if (minimalDoc1.CompareTo(minimalDoc2) < 0)
    /// {
    ///     Console.WriteLine($"Document '{minimalDoc1.DisplayText}' comes before '{minimalDoc2.DisplayText}'");
    /// }
    /// 
    /// // Professional document ordering for UI
    /// var documentList = minimalDocuments
    ///     .Where(d => d.CanBeSelected())
    ///     .OrderBy(d => d)
    ///     .ToList();
    /// 
    /// documentSelectionListBox.DataSource = documentList;
    /// </code>
    /// </example>
    public int CompareTo(DocumentMinimalDto? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary comparison: alphabetical by display text
        var displayComparison = string.Compare(DisplayText, other.DisplayText, StringComparison.OrdinalIgnoreCase);
        if (displayComparison != 0) return displayComparison;

        // Secondary comparison: by creation date for identical names
        var dateComparison = CreationDate.CompareTo(other.CreationDate);
        return dateComparison != 0 ? dateComparison :
            // Final comparison: by ID for complete determinism
            Id.CompareTo(other.Id);
    }

    /// <summary>
    /// Determines whether the specified DocumentMinimalDto is equal to the current DocumentMinimalDto based on document ID.
    /// </summary>
    /// <param name="other">The DocumentMinimalDto to compare with the current DocumentMinimalDto.</param>
    /// <returns>True if the specified DocumentMinimalDto is equal to the current DocumentMinimalDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each document has a unique identifier.
    /// This follows best practices for entity equality comparison and supports efficient collection operations
    /// optimized for minimal document scenarios.
    /// 
    /// <para><strong>Equality Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>ID-based Equality:</strong> Two documents are equal if they have the same non-empty ID</item>
    /// <item><strong>Null Safety:</strong> Handles null comparisons gracefully</item>
    /// <item><strong>Reference Equality:</strong> Returns true for reference equality optimization</item>
    /// <item><strong>Empty ID Handling:</strong> Documents with empty IDs are never considered equal</item>
    /// </list>
    /// 
    /// <para><strong>Performance Benefits:</strong></para>
    /// Equality implementation is optimized for minimal operations with fast comparison suitable for
    /// large collection operations and professional user interface requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// var minimalDoc1 = new DocumentMinimalDto { Id = documentId, /* other properties */ };
    /// var minimalDoc2 = new DocumentMinimalDto { Id = documentId, /* other properties */ };
    /// var minimalDoc3 = new DocumentMinimalDto { Id = Guid.NewGuid(), /* other properties */ };
    /// 
    /// Console.WriteLine(minimalDoc1.Equals(minimalDoc2)); // True - same ID
    /// Console.WriteLine(minimalDoc1.Equals(minimalDoc3)); // False - different ID
    /// Console.WriteLine(minimalDoc1.Equals(null)); // False - null safety
    /// 
    /// // Professional collection operations
    /// var uniqueDocuments = minimalDocuments.Distinct().ToList();
    /// var documentExists = minimalDocuments.Contains(searchDocument);
    /// </code>
    /// </example>
    public virtual bool Equals(DocumentMinimalDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Serves as the default hash function for DocumentMinimalDto instances.
    /// </summary>
    /// <returns>A hash code for the current DocumentMinimalDto based on the document ID.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation, supporting efficient collection operations
    /// optimized for minimal document scenarios.
    /// 
    /// <para><strong>Performance Benefits:</strong></para>
    /// Hash code implementation is optimized for minimal operations with fast hashing suitable for
    /// large collection operations, dictionary lookups, and professional user interface requirements.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Professional Comparison and Equality Implementation

    #region Professional String Representation

    /// <summary>
    /// Returns a professional string representation of the DocumentMinimalDto for logging and debugging purposes.
    /// </summary>
    /// <returns>A string that represents the current DocumentMinimalDto with essential identifying information.</returns>
    /// <remarks>
    /// The string representation includes essential document information in a professional format,
    /// useful for debugging, logging, audit trail entries, and professional communication optimized for minimal scenarios.
    /// 
    /// <para><strong>Information Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Identification:</strong> Display text (filename + extension)</item>
    /// <item><strong>Unique Identifier:</strong> Document ID for precise identification</item>
    /// <item><strong>Status Information:</strong> Current document status for context</item>
    /// <item><strong>File Size:</strong> Formatted file size for professional presentation</item>
    /// <item><strong>Age Information:</strong> Document age for temporal context</item>
    /// </list>
    /// 
    /// <para><strong>Professional Usage:</strong></para>
    /// Ideal for audit logs, error messages, debugging output, and any scenario
    /// requiring concise but informative document identification in minimal format.
    /// 
    /// <para><strong>Performance Optimization:</strong></para>
    /// String representation is optimized for minimal operations with efficient formatting
    /// suitable for high-throughput logging and professional user interface requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional string representation examples:
    /// // "Contract_Amendment.pdf (12345678-1234-5678-9012-123456789012) [Available] - 2.43 MB (5.2 days old)"
    /// // "Meeting_Notes.docx (87654321-4321-8765-2109-876543210987) [Checked Out] - 1.21 MB (0.5 days old)"
    /// // "Financial_Report.xlsx (11111111-2222-3333-4444-555555555555) [Deleted] - 5.67 MB (15.3 days old)"
    /// 
    /// // Usage in logging
    /// _logger.LogInformation("Processing minimal document: {Document}", minimalDoc.ToString());
    /// 
    /// // Usage in error handling
    /// throw new InvalidOperationException($"Cannot select document: {minimalDoc}");
    /// 
    /// // Usage in professional communications
    /// var message = $"Minimal document {minimalDoc} has been successfully processed";
    /// </code>
    /// </example>
    public override string ToString() =>
        $"{DisplayText} ({Id}) [{Status}] - {FormattedFileSize} ({DocumentAge.TotalDays:F1} days old)";

    /// <summary>
    /// Gets a concise summary suitable for professional reports and logs in minimal format.
    /// </summary>
    /// <returns>A professional summary string with essential document information.</returns>
    /// <remarks>
    /// This method provides a concise yet informative summary suitable for professional
    /// reports, audit logs, and business communications requiring minimal document identification.
    /// 
    /// <para><strong>Professional Benefits:</strong></para>
    /// Optimized for minimal scenarios with essential information only, suitable for
    /// high-performance logging and professional user interface requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// var summary = minimalDoc.GetProfessionalSummary();
    /// // Example: "Contract_Amendment.pdf (2.43 MB, Available)"
    /// 
    /// // Usage in professional reporting
    /// Console.WriteLine($"Document Summary: {summary}");
    /// 
    /// // Usage in UI tooltips
    /// documentListItem.ToolTipText = minimalDoc.GetProfessionalSummary();
    /// </code>
    /// </example>
    public string GetProfessionalSummary()
    {
        return $"{DisplayText} ({FormattedFileSize}, {Status})";
    }

    /// <summary>
    /// Gets a detailed description suitable for professional audit reports and compliance documentation in minimal format.
    /// </summary>
    /// <returns>A detailed description with essential document information.</returns>
    /// <remarks>
    /// This method provides essential document information suitable for audit reports,
    /// compliance documentation, and professional communication requiring comprehensive context in minimal format.
    /// 
    /// <para><strong>Professional Usage:</strong></para>
    /// Ideal for audit documentation, compliance reporting, and professional communication
    /// scenarios requiring detailed but concise document information.
    /// </remarks>
    /// <example>
    /// <code>
    /// var description = minimalDoc.GetDetailedDescription();
    /// // Example: "Document 'Contract_Amendment.pdf' (ID: 12345678-1234-5678-9012-123456789012) 
    /// //          created on 2024-01-15, size 2.43 MB, status Available, 5.2 days old"
    /// 
    /// // Usage in audit reports
    /// auditReport.AppendLine(minimalDoc.GetDetailedDescription());
    /// 
    /// // Usage in professional communications
    /// var clientEmail = $"Your document has been processed: {minimalDoc.GetDetailedDescription()}";
    /// </code>
    /// </example>
    public string GetDetailedDescription()
    {
        var ageInfo = $"{DocumentAge.TotalDays:F1} days old";

        return $"Document '{DisplayText}' (ID: {Id}) created on {CreationDate:yyyy-MM-dd}, " +
               $"size {FormattedFileSize}, status {Status}, {ageInfo}";
    }

    #endregion Professional String Representation

    #region Enhanced Utility Methods

    /// <summary>
    /// Determines whether this minimal document matches the specified search criteria with professional search logic.
    /// </summary>
    /// <param name="searchText">The search text to match against. Can be null or empty.</param>
    /// <param name="includeExtension">Whether to include file extension in the search (default: true).</param>
    /// <param name="caseSensitive">Whether the search should be case-sensitive (default: false).</param>
    /// <returns>True if the document matches the search criteria; otherwise, false.</returns>
    /// <remarks>
    /// This method provides professional document search capabilities optimized for minimal document operations,
    /// supporting flexible search logic suitable for document selection and filtering scenarios.
    /// 
    /// <para><strong>Search Capabilities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Filename Matching:</strong> Searches within the document filename</item>
    /// <item><strong>Extension Inclusion:</strong> Optionally includes file extension in search</item>
    /// <item><strong>Case Sensitivity:</strong> Configurable case sensitivity for professional flexibility</item>
    /// <item><strong>Null Safety:</strong> Handles null and empty search text gracefully</item>
    /// </list>
    /// 
    /// <para><strong>Professional Usage:</strong></para>
    /// Ideal for document selection interfaces, search functionality, and filtering operations
    /// in professional legal document management environments.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional document search scenarios
    /// var contractDocuments = minimalDocuments
    ///     .Where(d => d.MatchesSearchCriteria("contract", includeExtension: false))
    ///     .ToList();
    /// 
    /// var pdfDocuments = minimalDocuments
    ///     .Where(d => d.MatchesSearchCriteria(".pdf", includeExtension: true))
    ///     .ToList();
    /// 
    /// // Case-sensitive search
    /// var exactMatches = minimalDocuments
    ///     .Where(d => d.MatchesSearchCriteria("Contract", caseSensitive: true))
    ///     .ToList();
    /// 
    /// // Search with user input
    /// var userSearchResults = minimalDocuments
    ///     .Where(d => d.MatchesSearchCriteria(userSearchText))
    ///     .OrderBy(d => d.DisplayText)
    ///     .ToList();
    /// </code>
    /// </example>
    public bool MatchesSearchCriteria(string? searchText, bool includeExtension = true, bool caseSensitive = false)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return true; // Empty search matches all documents

        var searchTarget = includeExtension ? DisplayText : FileName;
        var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        return searchTarget.Contains(searchText.Trim(), comparison);
    }

    /// <summary>
    /// Gets the file type category for professional document classification and UI display.
    /// </summary>
    /// <returns>A user-friendly file type category for professional document classification.</returns>
    /// <remarks>
    /// This method provides professional file type classification based on file extension,
    /// useful for document categorization, UI icons, and professional workflow organization.
    /// 
    /// <para><strong>Category Classifications:</strong></para>
    /// <list type="bullet">
    /// <item><strong>"PDF Document":</strong> PDF files commonly used for legal documents</item>
    /// <item><strong>"Word Document":</strong> Microsoft Word documents for drafting and editing</item>
    /// <item><strong>"Excel Document":</strong> Microsoft Excel documents for data and calculations</item>
    /// <item><strong>"PowerPoint Document":</strong> Microsoft PowerPoint presentations</item>
    /// <item><strong>"Text Document":</strong> Plain text and RTF documents</item>
    /// <item><strong>"Image Document":</strong> Image files for exhibits and evidence</item>
    /// <item><strong>"Archive Document":</strong> Compressed archive files</item>
    /// <item><strong>"Unknown Document":</strong> Unrecognized or unsupported file types</item>
    /// </list>
    /// 
    /// <para><strong>Professional Usage:</strong></para>
    /// Ideal for document classification, UI icon selection, and professional workflow
    /// organization in legal document management environments.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional document categorization
    /// var fileType = minimalDoc.GetFileTypeCategory();
    /// var documentIcon = GetIconForFileType(fileType);
    /// var documentColor = GetColorForFileType(fileType);
    /// 
    /// // UI integration
    /// documentListItem.Text = $"{minimalDoc.DisplayText} [{fileType}]";
    /// documentListItem.ImageKey = fileType;
    /// 
    /// // Professional reporting
    /// var documentsByType = minimalDocuments
    ///     .GroupBy(d => d.GetFileTypeCategory())
    ///     .ToDictionary(g => g.Key, g => g.Count());
    /// 
    /// foreach (var (category, count) in documentsByType)
    /// {
    ///     Console.WriteLine($"{category}: {count} documents");
    /// }
    /// </code>
    /// </example>
    public string GetFileTypeCategory()
    {
        if (string.IsNullOrWhiteSpace(Extension))
            return "Unknown Document";

        var normalizedExtension = Extension.ToLowerInvariant();

        return normalizedExtension switch
        {
            ".pdf" => "PDF Document",
            ".docx" or ".doc" => "Word Document",
            ".xlsx" or ".xls" => "Excel Document",
            ".pptx" or ".ppt" => "PowerPoint Document",
            ".txt" or ".rtf" => "Text Document",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".tiff" or ".tif" => "Image Document",
            ".zip" or ".rar" or ".7z" => "Archive Document",
            _ => "Unknown Document"
        };
    }

    /// <summary>
    /// Determines whether this minimal document is suitable for a specific operation based on professional criteria.
    /// </summary>
    /// <param name="operation">The operation to check suitability for (e.g., "edit", "view", "transfer", "delete").</param>
    /// <returns>True if the document is suitable for the operation; otherwise, false.</returns>
    /// <remarks>
    /// This method provides operation-specific suitability checking for professional workflow decisions
    /// and user interface control logic optimized for minimal document scenarios.
    /// 
    /// <para><strong>Supported Operations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>"edit":</strong> Document can be edited (available for edit)</item>
    /// <item><strong>"view":</strong> Document can be viewed (not deleted)</item>
    /// <item><strong>"transfer":</strong> Document can be transferred (not deleted, not checked out)</item>
    /// <item><strong>"delete":</strong> Document can be deleted (not already deleted)</item>
    /// <item><strong>"select":</strong> Document can be selected (valid and not deleted)</item>
    /// </list>
    /// 
    /// <para><strong>Professional Benefits:</strong></para>
    /// Enables operation-specific workflow decisions and user interface logic
    /// supporting professional document management practices and user experience optimization.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional operation suitability checking
    /// editButton.Enabled = minimalDoc.IsSuitableForOperation("edit");
    /// viewButton.Enabled = minimalDoc.IsSuitableForOperation("view");
    /// transferButton.Enabled = minimalDoc.IsSuitableForOperation("transfer");
    /// deleteButton.Enabled = minimalDoc.IsSuitableForOperation("delete");
    /// 
    /// // Professional workflow decisions
    /// var operationsMenu = new List<string>();
    /// if (minimalDoc.IsSuitableForOperation("edit")) operationsMenu.Add("Edit");
    /// if (minimalDoc.IsSuitableForOperation("view")) operationsMenu.Add("View");
    /// if (minimalDoc.IsSuitableForOperation("transfer")) operationsMenu.Add("Transfer");
    /// if (minimalDoc.IsSuitableForOperation("delete")) operationsMenu.Add("Delete");
    /// 
    /// contextMenu.Items.Clear();
    /// foreach (var operation in operationsMenu)
    /// {
    ///     contextMenu.Items.Add(operation);
    /// }
    /// </code>
    /// </example>
    public bool IsSuitableForOperation([NotNull] string operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation, nameof(operation));

        return operation.ToLowerInvariant() switch
        {
            "edit" => IsAvailableForEdit && IsValid,
            "view" => !IsDeleted && IsValid,
            "transfer" => !IsDeleted && !IsCheckedOut && IsValid,
            "delete" => !IsDeleted && IsValid,
            "select" => CanBeSelected(),
            _ => false // Unknown operations default to false
        };
    }

    /// <summary>
    /// Creates a copy of this minimal document with updated essential metadata.
    /// </summary>
    /// <param name="newFileName">The new file name for the copied document.</param>
    /// <param name="newId">Optional new ID for the copy (default: generates new GUID).</param>
    /// <returns>A new DocumentMinimalDto instance representing the copied document.</returns>
    /// <remarks>
    /// This method creates a professional document copy optimized for minimal operations with options for
    /// essential metadata updates based on business requirements and performance considerations.
    /// 
    /// <para><strong>Copy Features:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Essential Metadata Copy:</strong> Copies essential file metadata and properties</item>
    /// <item><strong>Status Reset:</strong> Copy starts as available (not checked out or deleted)</item>
    /// <item><strong>New Creation Date:</strong> Copy gets new creation timestamp</item>
    /// <item><strong>Validation Guarantee:</strong> Ensures copied document meets validation standards</item>
    /// </list>
    /// 
    /// <para><strong>Professional Usage:</strong></para>
    /// Useful for document templates, basic copying operations, and professional document management workflows
    /// requiring minimal overhead and fast performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a minimal copy with new name
    /// var templateCopy = minimalDoc.CreateCopy("Template_Contract_v2");
    /// 
    /// // Create a copy with specific ID
    /// var specificCopy = minimalDoc.CreateCopy("Contract_Backup", Guid.NewGuid());
    /// 
    /// // Professional template creation
    /// var template = originalDocument.CreateCopy($"Template_{originalDocument.FileName}");
    /// Console.WriteLine($"Created template: {template.DisplayText}");
    /// </code>
    /// </example>
    public DocumentMinimalDto CreateCopy([NotNull] string newFileName, Guid? newId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newFileName, nameof(newFileName));

        var copy = new DocumentMinimalDto
        {
            Id = newId ?? Guid.NewGuid(), // New unique identifier
            FileName = newFileName.Trim(),
            Extension = Extension,
            FileSize = FileSize,
            IsCheckedOut = false, // Copy starts as available
            IsDeleted = false, // Copy starts as active
            CreationDate = DateTime.UtcNow // New creation time
        };

        // Validate the copy
        var validationResults = ValidateModel(copy);
        if (!BaseValidationDto.HasValidationErrors(validationResults)) return copy;
        var summary = BaseValidationDto.GetValidationSummary(validationResults);
        throw new ValidationException($"Minimal document copy validation failed: {summary}");

    }

    #endregion Enhanced Utility Methods
}