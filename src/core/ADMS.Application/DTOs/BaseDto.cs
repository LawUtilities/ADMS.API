using ADMS.API.Helpers;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ADMS.Application.DTOs;

/// <summary>
/// Base class for all DTOs with common validation and metadata.
/// </summary>
public abstract class BaseDto
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    [JsonPropertyOrder(-1)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    [JsonPropertyOrder(1000)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    [JsonPropertyOrder(1001)]
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who created this entity.
    /// </summary>
    [JsonPropertyOrder(1002)]
    [StringLength(100, ErrorMessage = "Created by field cannot exceed 100 characters.")]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified this entity.
    /// </summary>
    [JsonPropertyOrder(1003)]
    [StringLength(100, ErrorMessage = "Modified by field cannot exceed 100 characters.")]
    public string? ModifiedBy { get; set; }
}