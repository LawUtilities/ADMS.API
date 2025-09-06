using System;

using ADMS.Domain.Common;

namespace ADMS.Domain.ValueObjects
{
    /// <summary>
    /// Represents a document version number in the ADMS legal document management system.
    /// </summary>
    /// <remarks>
    /// This value object encapsulates version numbering for documents, supporting both
    /// major and minor version increments. The version follows semantic versioning
    /// principles with Major.Minor format (e.g., 1.0, 1.1, 2.0).
    /// 
    /// Major versions typically indicate significant changes or complete rewrites,
    /// while minor versions represent incremental updates, corrections, or additions
    /// that maintain compatibility with the previous major version.
    /// 
    /// As a value object, DocumentVersion instances are immutable and compared by value.
    /// This ensures consistency in version tracking and prevents accidental modification
    /// of version information throughout the document lifecycle.
    /// </remarks>
    public sealed record DocumentVersion : IComparable<DocumentVersion>
    {
        /// <summary>
        /// Gets the major version number.
        /// </summary>
        /// <value>
        /// An integer representing the major version number, starting from 1.
        /// </value>
        /// <remarks>
        /// The major version number is incremented for significant changes to the document
        /// that may affect its legal interpretation or require stakeholder review.
        /// Major version changes typically reset the minor version to 0.
        /// </remarks>
        public int Major { get; }

        /// <summary>
        /// Gets the minor version number.
        /// </summary>
        /// <value>
        /// An integer representing the minor version number, starting from 0.
        /// </value>
        /// <remarks>
        /// The minor version number is incremented for small changes, corrections,
        /// or additions that don't fundamentally alter the document's meaning or
        /// require extensive review processes.
        /// </remarks>
        public int Minor { get; }

        /// <summary>
        /// Gets the full version string in "Major.Minor" format.
        /// </summary>
        /// <value>
        /// A string representation of the version (e.g., "1.0", "2.3").
        /// </value>
        /// <remarks>
        /// This property provides a standardized string representation suitable for
        /// display in user interfaces, logging, and document metadata.
        /// </remarks>
        public string VersionString => $"{Major}.{Minor}";

        /// <summary>
        /// Gets a value indicating whether this is the initial version (1.0).
        /// </summary>
        /// <value>
        /// <c>true</c> if this is version 1.0; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// The initial version represents the first release of a document and
        /// is useful for identifying original documents in workflows and reporting.
        /// </remarks>
        public bool IsInitialVersion => Major == 1 && Minor == 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentVersion"/> record.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="major"/> is less than 1 or <paramref name="minor"/> is less than 0.
        /// </exception>
        /// <remarks>
        /// This constructor is private to enforce the use of factory methods for creating instances.
        /// This ensures consistent validation and provides controlled ways to create DocumentVersion instances.
        /// </remarks>
        private DocumentVersion(int major, int minor)
        {
            if (major < 1)
                throw new ArgumentException("Major version must be at least 1", nameof(major));
            if (minor < 0)
                throw new ArgumentException("Minor version cannot be negative", nameof(minor));

            Major = major;
            Minor = minor;
        }

        /// <summary>
        /// Creates the initial document version (1.0).
        /// </summary>
        /// <returns>
        /// A new <see cref="DocumentVersion"/> representing version 1.0.
        /// </returns>
        /// <remarks>
        /// This method should be used when creating the first version of a new document.
        /// It establishes the baseline version from which all subsequent versions will be derived.
        /// 
        /// Example usage:
        /// <code>
        /// var initialVersion = DocumentVersion.Initial();
        /// Console.WriteLine(initialVersion.VersionString); // Outputs: "1.0"
        /// </code>
        /// </remarks>
        public static DocumentVersion Initial() => new(1, 0);

        /// <summary>
        /// Creates a document version with the specified major and minor version numbers.
        /// </summary>
        /// <param name="major">The major version number (must be at least 1).</param>
        /// <param name="minor">The minor version number (must be 0 or greater).</param>
        /// <returns>
        /// A <see cref="Result{DocumentVersion}"/> containing the created version or validation errors.
        /// </returns>
        /// <remarks>
        /// This method provides validation and error handling when creating versions from
        /// external input such as user interfaces or API calls. It returns a Result object
        /// to handle validation failures gracefully.
        /// 
        /// Example usage:
        /// <code>
        /// var versionResult = DocumentVersion.Create(2, 1);
        /// if (versionResult.IsSuccess)
        /// {
        ///     var version = versionResult.Value;
        ///     Console.WriteLine(version.VersionString); // Outputs: "2.1"
        /// }
        /// </code>
        /// </remarks>
        public static Result<DocumentVersion> Create(int major, int minor)
        {
            try
            {
                return Result.Success(new DocumentVersion(major, minor));
            }
            catch (ArgumentException ex)
            {
                return Result.Failure<DocumentVersion>(DomainError.Create("INVALID_VERSION", ex.Message));
            }
        }

        /// <summary>
        /// Parses a version string in "Major.Minor" format.
        /// </summary>
        /// <param name="versionString">The version string to parse (e.g., "1.0", "2.3").</param>
        /// <returns>
        /// A <see cref="Result{DocumentVersion}"/> containing the parsed version or parsing errors.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="versionString"/> is null.
        /// </exception>
        /// <remarks>
        /// This method accepts version strings in the format "Major.Minor" where both
        /// Major and Minor are valid integers. Leading and trailing whitespace is ignored.
        /// 
        /// Example usage:
        /// <code>
        /// var versionResult = DocumentVersion.Parse("2.1");
        /// if (versionResult.IsSuccess)
        /// {
        ///     var version = versionResult.Value;
        ///     Console.WriteLine($"Parsed version: {version.Major}.{version.Minor}");
        /// }
        /// </code>
        /// </remarks>
        public static Result<DocumentVersion> Parse(string versionString)
        {
            if (versionString == null)
                throw new ArgumentNullException(nameof(versionString));

            var trimmed = versionString.Trim();

            if (string.IsNullOrEmpty(trimmed))
                return Result.Failure<DocumentVersion>(DomainError.Create("EMPTY_VERSION_STRING", "Version string cannot be empty"));

            var parts = trimmed.Split('.');

            if (parts.Length != 2)
                return Result.Failure<DocumentVersion>(DomainError.Create("INVALID_VERSION_FORMAT", "Version string must be in 'Major.Minor' format"));

            if (!int.TryParse(parts[0], out var major))
                return Result.Failure<DocumentVersion>(DomainError.Create("INVALID_MAJOR_VERSION", "Major version must be a valid integer"));

            if (!int.TryParse(parts[1], out var minor))
                return Result.Failure<DocumentVersion>(DomainError.Create("INVALID_MINOR_VERSION", "Minor version must be a valid integer"));

            return Create(major, minor);
        }

        /// <summary>
        /// Creates the next minor version by incrementing the minor version number.
        /// </summary>
        /// <returns>
        /// A new <see cref="DocumentVersion"/> with the minor version incremented by 1.
        /// </returns>
        /// <remarks>
        /// This method is used to create incremental updates to a document that don't
        /// warrant a major version change. The major version remains the same while
        /// the minor version is incremented.
        /// 
        /// Example usage:
        /// <code>
        /// var currentVersion = DocumentVersion.Parse("1.2").Value;
        /// var nextMinor = currentVersion.NextMinor();
        /// Console.WriteLine(nextMinor.VersionString); // Outputs: "1.3"
        /// </code>
        /// </remarks>
        public DocumentVersion NextMinor() => new(Major, Minor + 1);

        /// <summary>
        /// Creates the next major version by incrementing the major version and resetting minor to 0.
        /// </summary>
        /// <returns>
        /// A new <see cref="DocumentVersion"/> with the major version incremented by 1 and minor version set to 0.
        /// </returns>
        /// <remarks>
        /// This method is used to create significant updates to a document that warrant
        /// a major version change. The minor version is reset to 0 to indicate a fresh
        /// start for the new major version.
        /// 
        /// Example usage:
        /// <code>
        /// var currentVersion = DocumentVersion.Parse("1.3").Value;
        /// var nextMajor = currentVersion.NextMajor();
        /// Console.WriteLine(nextMajor.VersionString); // Outputs: "2.0"
        /// </code>
        /// </remarks>
        public DocumentVersion NextMajor() => new(Major + 1, 0);

        /// <summary>
        /// Compares this version with another version for ordering.
        /// </summary>
        /// <param name="other">The other version to compare with.</param>
        /// <returns>
        /// A negative integer if this version is less than the other;
        /// zero if they are equal; a positive integer if this version is greater.
        /// </returns>
        /// <remarks>
        /// Version comparison follows semantic versioning rules:
        /// - Major versions are compared first
        /// - If major versions are equal, minor versions are compared
        /// - Version 2.0 is greater than 1.9
        /// - Version 1.5 is greater than 1.4
        /// </remarks>
        public int CompareTo(DocumentVersion other)
        {
            if (other is null) return 1;

            var majorComparison = Major.CompareTo(other.Major);
            return majorComparison != 0 ? majorComparison : Minor.CompareTo(other.Minor);
        }

        /// <summary>
        /// Determines whether this version is greater than another version.
        /// </summary>
        /// <param name="left">The first version to compare.</param>
        /// <param name="right">The second version to compare.</param>
        /// <returns><c>true</c> if the first version is greater than the second; otherwise, <c>false</c>.</returns>
        public static bool operator >(DocumentVersion left, DocumentVersion right) =>
            left is not null && left.CompareTo(right) > 0;

        /// <summary>
        /// Determines whether this version is less than another version.
        /// </summary>
        /// <param name="left">The first version to compare.</param>
        /// <param name="right">The second version to compare.</param>
        /// <returns><c>true</c> if the first version is less than the second; otherwise, <c>false</c>.</returns>
        public static bool operator <(DocumentVersion left, DocumentVersion right) =>
            right is not null && left?.CompareTo(right) < 0;

        /// <summary>
        /// Determines whether this version is greater than or equal to another version.
        /// </summary>
        /// <param name="left">The first version to compare.</param>
        /// <param name="right">The second version to compare.</param>
        /// <returns><c>true</c> if the first version is greater than or equal to the second; otherwise, <c>false</c>.</returns>
        public static bool operator >=(DocumentVersion left, DocumentVersion right) =>
            left is null ? right is null : left.CompareTo(right) >= 0;

        /// <summary>
        /// Determines whether this version is less than or equal to another version.
        /// </summary>
        /// <param name="left">The first version to compare.</param>
        /// <param name="right">The second version to compare.</param>
        /// <returns><c>true</c> if the first version is less than or equal to the second; otherwise, <c>false</c>.</returns>
        public static bool operator <=(DocumentVersion left, DocumentVersion right) =>
            right is null ? left is null : left?.CompareTo(right) <= 0;

        /// <summary>
        /// Returns a string representation of this document version.
        /// </summary>
        /// <returns>
        /// A string in "Major.Minor" format representing this version.
        /// </returns>
        /// <remarks>
        /// This method returns the same value as the <see cref="VersionString"/> property
        /// and is suitable for display, logging, and serialization purposes.
        /// </remarks>
        public override string ToString() => VersionString;
    }
}