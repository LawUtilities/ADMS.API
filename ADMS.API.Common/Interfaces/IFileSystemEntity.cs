namespace ADMS.API.Common.Interfaces;

/// <summary>
/// Marker interface for entities that represent file system resources with metadata.
/// </summary>
/// <remarks>
/// File system integration provides consistent file metadata handling and validation
/// for entities that represent physical or logical file system resources.
/// </remarks>
public interface IFileSystemEntity
{
    /// <summary>
    /// Gets the file name without extension.
    /// </summary>
    string FileName { get; }

    /// <summary>
    /// Gets the file extension including the dot.
    /// </summary>
    string Extension { get; }

    /// <summary>
    /// Gets the complete file name including extension.
    /// </summary>
    string FullFileName => $"{FileName}{Extension}";

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    long FileSize { get; }

    /// <summary>
    /// Gets the formatted file size for display.
    /// </summary>
    string FormattedFileSize { get; }
}