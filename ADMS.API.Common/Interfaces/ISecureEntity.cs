namespace ADMS.API.Common.Interfaces;

/// <summary>
/// Marker interface for entities that include security validation and integrity verification.
/// </summary>
/// <remarks>
/// Security functionality provides cryptographic integrity verification and security
/// compliance features essential for legal document management systems.
/// </remarks>
public interface ISecureEntity
{
    /// <summary>
    /// Gets the cryptographic checksum for integrity verification.
    /// </summary>
    string Checksum { get; }

    /// <summary>
    /// Gets a value indicating whether the entity has a valid checksum.
    /// </summary>
    bool HasValidChecksum { get; }

    /// <summary>
    /// Gets the MIME type for content identification.
    /// </summary>
    string MimeType { get; }
}