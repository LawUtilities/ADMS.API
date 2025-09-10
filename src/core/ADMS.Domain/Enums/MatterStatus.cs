using System.ComponentModel;

namespace ADMS.Domain.Enums;

/// <summary>
/// Represents the possible status values for a matter in the legal document management system.
/// </summary>
/// <remarks>
/// This enumeration defines the lifecycle states that a matter can be in within the ADMS system.
/// The status values support legal practice workflows and provide clear state management for
/// matter organization and reporting.
/// 
/// <para><strong>Status Lifecycle:</strong></para>
/// <list type="bullet">
/// <item><strong>Active → Archived:</strong> When legal work is complete but documents must be retained</item>
/// <item><strong>Active → Deleted:</strong> When matter is no longer needed (soft delete)</item>
/// <item><strong>Archived → Active:</strong> When archived matter needs to be reactivated</item>
/// <item><strong>Deleted → Active:</strong> When deleted matter needs to be restored</item>
/// </list>
/// 
/// <para><strong>Legal Practice Context:</strong></para>
/// <list type="bullet">
/// <item><strong>Active:</strong> Ongoing legal matters requiring regular access</item>
/// <item><strong>Archived:</strong> Completed matters retained for compliance/reference</item>
/// <item><strong>Deleted:</strong> Matters marked for removal but preserved for audit trails</item>
/// <item><strong>ArchivedAndDeleted:</strong> Dual state for comprehensive lifecycle management</item>
/// </list>
/// </remarks>
public enum MatterStatus
{
    /// <summary>
    /// The matter is active and available for normal operations.
    /// </summary>
    /// <remarks>
    /// Active matters are those currently being worked on or available for regular access.
    /// This is the default state for new matters and represents ongoing legal work.
    /// 
    /// <para><strong>Characteristics:</strong></para>
    /// <list type="bullet">
    /// <item>Fully accessible to authorized users</item>
    /// <item>Can have documents added, modified, or removed</item>
    /// <item>Appears in default matter listings and searches</item>
    /// <item>Subject to all normal business operations</item>
    /// </list>
    /// </remarks>
    [Description("Active")]
    Active = 0,

    /// <summary>
    /// The matter has been archived but is not deleted.
    /// </summary>
    /// <remarks>
    /// Archived matters are those where active work has been completed but the matter
    /// and its documents must be retained for legal, compliance, or reference purposes.
    /// 
    /// <para><strong>Characteristics:</strong></para>
    /// <list type="bullet">
    /// <item>Read-only access to preserve historical integrity</item>
    /// <item>Hidden from default active matter views</item>
    /// <item>Accessible through archive-specific searches and reports</item>
    /// <item>Maintains all audit trails and document associations</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Usage:</strong></para>
    /// <list type="bullet">
    /// <item>Completed client matters with document retention requirements</item>
    /// <item>Concluded legal proceedings requiring historical access</item>
    /// <item>Inactive client relationships with potential future relevance</item>
    /// </list>
    /// </remarks>
    [Description("Archived")]
    Archived = 1,

    /// <summary>
    /// The matter has been deleted but is not archived.
    /// </summary>
    /// <remarks>
    /// Deleted matters are those marked for removal while preserving audit trail integrity
    /// through soft deletion. The matter data remains in the system for compliance and
    /// recovery purposes.
    /// 
    /// <para><strong>Characteristics:</strong></para>
    /// <list type="bullet">
    /// <item>Hidden from normal user interfaces</item>
    /// <item>Accessible only through administrative recovery functions</item>
    /// <item>Preserves all related data for audit trail integrity</item>
    /// <item>Can be restored to active status if needed</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Cannot delete matters with checked-out documents</item>
    /// <item>Deletion activities are tracked in audit trails</item>
    /// <item>Associated documents handled according to deletion policies</item>
    /// </list>
    /// </remarks>
    [Description("Deleted")]
    Deleted = 2,

    /// <summary>
    /// The matter has been both archived and deleted.
    /// </summary>
    /// <remarks>
    /// This status represents matters that have been archived (indicating completion
    /// of active work) and subsequently deleted (indicating they are no longer needed
    /// for regular access). This dual state provides comprehensive lifecycle management.
    /// 
    /// <para><strong>Characteristics:</strong></para>
    /// <list type="bullet">
    /// <item>Combines aspects of both archived and deleted states</item>
    /// <item>Represents the final stage of matter lifecycle</item>
    /// <item>Maintains complete audit trails for compliance</item>
    /// <item>Accessible only through specialized administrative functions</item>
    /// </list>
    /// 
    /// <para><strong>Use Cases:</strong></para>
    /// <list type="bullet">
    /// <item>Long-term completed matters no longer requiring regular access</item>
    /// <item>Matters archived for compliance then marked for eventual purging</item>
    /// <item>Historical matters maintained for legal discovery purposes only</item>
    /// </list>
    /// </remarks>
    [Description("Archived and Deleted")]
    ArchivedAndDeleted = 3
}

/// <summary>
/// Extension methods for the MatterStatus enumeration.
/// </summary>
/// <remarks>
/// These extensions provide convenient helper methods for working with matter status
/// values, including validation, state transitions, and business rule enforcement.
/// </remarks>
public static class MatterStatusExtensions
{
    /// <summary>
    /// Determines whether the matter status allows normal operations.
    /// </summary>
    /// <param name="status">The matter status to check.</param>
    /// <returns>True if the status allows normal operations; otherwise, false.</returns>
    /// <remarks>
    /// Normal operations include document management, user access, and general matter activities.
    /// Only Active status currently allows full normal operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var matter = GetMatter();
    /// if (matter.GetStatus().AllowsNormalOperations())
    /// {
    ///     // Proceed with document operations
    /// }
    /// </code>
    /// </example>
    public static bool AllowsNormalOperations(this MatterStatus status)
    {
        return status == MatterStatus.Active;
    }

    /// <summary>
    /// Determines whether the matter status allows read-only access.
    /// </summary>
    /// <param name="status">The matter status to check.</param>
    /// <returns>True if the status allows read-only access; otherwise, false.</returns>
    /// <remarks>
    /// Read-only access includes viewing matter details, documents, and audit trails
    /// without the ability to modify content.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matterStatus.AllowsReadOnlyAccess())
    /// {
    ///     // Show matter in read-only mode
    /// }
    /// </code>
    /// </example>
    public static bool AllowsReadOnlyAccess(this MatterStatus status)
    {
        return status is MatterStatus.Active or MatterStatus.Archived;
    }

    /// <summary>
    /// Determines whether the matter is in a deleted state.
    /// </summary>
    /// <param name="status">The matter status to check.</param>
    /// <returns>True if the matter is deleted; otherwise, false.</returns>
    /// <remarks>
    /// Deleted status includes both Deleted and ArchivedAndDeleted states.
    /// </remarks>
    public static bool IsDeleted(this MatterStatus status)
    {
        return status is MatterStatus.Deleted or MatterStatus.ArchivedAndDeleted;
    }

    /// <summary>
    /// Determines whether the matter is in an archived state.
    /// </summary>
    /// <param name="status">The matter status to check.</param>
    /// <returns>True if the matter is archived; otherwise, false.</returns>
    /// <remarks>
    /// Archived status includes both Archived and ArchivedAndDeleted states.
    /// </remarks>
    public static bool IsArchived(this MatterStatus status)
    {
        return status is MatterStatus.Archived or MatterStatus.ArchivedAndDeleted;
    }

    /// <summary>
    /// Determines whether transitioning from one status to another is valid.
    /// </summary>
    /// <param name="fromStatus">The current status.</param>
    /// <param name="toStatus">The target status.</param>
    /// <returns>True if the transition is valid; otherwise, false.</returns>
    /// <remarks>
    /// This method enforces business rules for valid status transitions to maintain
    /// data integrity and support proper matter lifecycle management.
    /// 
    /// <para><strong>Valid Transitions:</strong></para>
    /// <list type="bullet">
    /// <item>Active → Archived, Deleted</item>
    /// <item>Archived → Active, ArchivedAndDeleted</item>
    /// <item>Deleted → Active</item>
    /// <item>ArchivedAndDeleted → Active, Archived</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (MatterStatusExtensions.IsValidTransition(currentStatus, newStatus))
    /// {
    ///     // Proceed with status change
    /// }
    /// else
    /// {
    ///     // Handle invalid transition
    /// }
    /// </code>
    /// </example>
    public static bool IsValidTransition(MatterStatus fromStatus, MatterStatus toStatus)
    {
        // Same status is always valid (no-op)
        if (fromStatus == toStatus)
            return true;

        return fromStatus switch
        {
            MatterStatus.Active => toStatus is MatterStatus.Archived or MatterStatus.Deleted,
            MatterStatus.Archived => toStatus is MatterStatus.Active or MatterStatus.ArchivedAndDeleted,
            MatterStatus.Deleted => toStatus == MatterStatus.Active,
            MatterStatus.ArchivedAndDeleted => toStatus is MatterStatus.Active or MatterStatus.Archived,
            _ => false
        };
    }

    /// <summary>
    /// Gets the display-friendly description of the matter status.
    /// </summary>
    /// <param name="status">The matter status to get the description for.</param>
    /// <returns>A human-readable description of the status.</returns>
    /// <remarks>
    /// This method retrieves the description from the Description attribute
    /// or falls back to the enum name if no description is available.
    /// </remarks>
    /// <example>
    /// <code>
    /// var statusDescription = matterStatus.GetDescription();
    /// // Returns: "Active", "Archived", "Deleted", or "Archived and Deleted"
    /// </code>
    /// </example>
    public static string GetDescription(this MatterStatus status)
    {
        var field = status.GetType().GetField(status.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                              .FirstOrDefault() as DescriptionAttribute;

        return attribute?.Description ?? status.ToString();
    }

    /// <summary>
    /// Gets all valid transition targets from the current status.
    /// </summary>
    /// <param name="fromStatus">The current status.</param>
    /// <returns>An enumerable of valid target statuses.</returns>
    /// <remarks>
    /// This method returns all statuses that can be validly transitioned to
    /// from the current status, useful for UI dropdown population and validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var validTargets = currentStatus.GetValidTransitions();
    /// foreach (var target in validTargets)
    /// {
    ///     // Populate UI options
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<MatterStatus> GetValidTransitions(this MatterStatus fromStatus)
    {
        var allStatuses = Enum.GetValues<MatterStatus>();
        return allStatuses.Where(toStatus => IsValidTransition(fromStatus, toStatus) && toStatus != fromStatus);
    }

    /// <summary>
    /// Determines the effective status based on archived and deleted flags.
    /// </summary>
    /// <param name="isArchived">Whether the matter is archived.</param>
    /// <param name="isDeleted">Whether the matter is deleted.</param>
    /// <returns>The corresponding MatterStatus enum value.</returns>
    /// <remarks>
    /// This method converts boolean flags (as used in the Matter entity) to the
    /// corresponding enum value for type-safe status operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var status = MatterStatusExtensions.FromFlags(matter.IsArchived, matter.IsDeleted);
    /// </code>
    /// </example>
    public static MatterStatus FromFlags(bool isArchived, bool isDeleted)
    {
        return (isArchived, isDeleted) switch
        {
            (false, false) => MatterStatus.Active,
            (true, false) => MatterStatus.Archived,
            (false, true) => MatterStatus.Deleted,
            (true, true) => MatterStatus.ArchivedAndDeleted
        };
    }

    /// <summary>
    /// Converts a MatterStatus to boolean flags.
    /// </summary>
    /// <param name="status">The status to convert.</param>
    /// <returns>A tuple containing the archived and deleted flags.</returns>
    /// <remarks>
    /// This method converts the enum value back to boolean flags for compatibility
    /// with the Matter entity's flag-based approach.
    /// </remarks>
    /// <example>
    /// <code>
    /// var (isArchived, isDeleted) = status.ToFlags();
    /// matter.IsArchived = isArchived;
    /// matter.IsDeleted = isDeleted;
    /// </code>
    /// </example>
    public static (bool IsArchived, bool IsDeleted) ToFlags(this MatterStatus status)
    {
        return status switch
        {
            MatterStatus.Active => (false, false),
            MatterStatus.Archived => (true, false),
            MatterStatus.Deleted => (false, true),
            MatterStatus.ArchivedAndDeleted => (true, true),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown matter status")
        };
    }
}