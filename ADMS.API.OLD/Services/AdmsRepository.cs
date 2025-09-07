using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Helpers;
using ADMS.API.Models;
using ADMS.API.ResourceParameters;

using MapsterMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Linq.Dynamic.Core;

using Document = ADMS.API.Entities.Document;

namespace ADMS.API.Services;

/// <summary>
///     Adms Repository containing implementation details.
/// </summary>
/// <summary>
/// Initializes a new instance of the <see cref="AdmsRepository"/> class.
/// </summary>
/// <param name="logger">The logger used for logging repository operations and errors.</param>
/// <param name="context">The database context for accessing ADMS entities.</param>
/// <param name="mapper">The AutoMapper instance for mapping between entities and DTOs.</param>
/// <param name="propertyMappingService">The service used for property mapping and dynamic sorting.</param>
/// <param name="validationService">The service providing centralized validation logic for repository operations.</param>
public class AdmsRepository(
    ILogger<AdmsRepository> logger,
    AdmsContext context,
    IMapper mapper,
    IPropertyMappingService propertyMappingService,
    IValidationService validationService) : IAdmsRepository
{
    private static readonly string ServerFilesPath = Environment.GetEnvironmentVariable("ADMSServerFilesPath")
                                                     ?? throw new InvalidOperationException("The environment variable 'ADMSServerFilesPath' is not set.");
    private const string UserName = "rbrown";

    //Tests Created

    #region Matter Activity

    /// <summary>
    /// Retrieves a <see cref="MatterActivity"/> by its name.
    /// Returns <c>null</c> if the activity name is invalid, not found, not unique, or if an unexpected error occurs.
    /// All error conditions are logged.
    /// </summary>
    /// <param name="activityName">The name of the activity to retrieve.</param>
    /// <returns>
    /// The requested <see cref="MatterActivity"/>, or <c>null</c> if not found or if an error occurs.
    /// </returns>
    public async Task<MatterActivity?> GetMatterActivityByActivityNameAsync(string activityName)
    {
        // Validate input
        if (!validationService.ValidateStringNotEmpty(activityName, nameof(activityName)))
        {
            logger.LogWarning("Activity name cannot be null or empty.");
            return null;
        }

        try
        {
            var matterActivity = await context
                .MatterActivities
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.Activity == activityName);

            if (matterActivity == null)
            {
                logger.LogWarning("No MatterActivity found with activity name: {ActivityName}", activityName);
            }
            else
            {
                logger.LogInformation("MatterActivity '{ActivityName}' retrieved successfully.", activityName);
            }

            return matterActivity;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Multiple MatterActivity records found for activity name: {ActivityName}. Ensure the database contains unique activity names.", activityName);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request for activity name: {ActivityName}", activityName);
            return null;
        }
    }

    #endregion Matter Activity

    //Tests Created
    #region User Actions

    /// <summary>
    /// Retrieves a user by their username, using the validation service for input validation and consistent logging.
    /// </summary>
    /// <param name="username">The username of the user to retrieve.</param>
    /// <returns>The requested user, or null if not found.</returns>
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        // Centralized validation using the validation service
        if (!validationService.ValidateStringNotEmpty(username, nameof(username)))
        {
            logger.LogWarning("Username cannot be null or empty.");
            return null;
        }

        try
        {
            var user = await context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Name == username);

            if (user == null)
            {
                logger.LogWarning("No user found with username: {Username}", username);
            }
            else
            {
                logger.LogInformation("User '{Username}' retrieved successfully.", username);
            }

            return user;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "An InvalidOperationException occurred. Context: {Username}", username);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request.");
            return null;
        }
    }

    #endregion User Actions

    #region Helper Methods

    //Tests Created
    /// <summary>
    /// Persists all changes made in this context to the database, using centralized validation and consistent logging.
    /// </summary>
    /// <returns>True if the changes were successfully saved, false otherwise.</returns>
    public async Task<bool> SaveChangesAsync()
    {
        // Example: Centralized validation before saving (if needed)
        // If you want to validate the context or entities before saving, add logic here.
        // For example, you could validate that there are changes to save:
        if (!context.ChangeTracker.HasChanges())
        {
            logger.LogInformation("No changes detected in the context. SaveChangesAsync was not executed.");
            return true;
        }

        try
        {
            var result = await context.SaveChangesAsync();
            logger.LogInformation("Database changes saved successfully. Rows affected: {RowsAffected}", result);
            return result >= 0;
        }
        catch (DbUpdateException dbEx)
        {
            logger.LogError(dbEx, "A database update error occurred while saving changes. Context: {ContextInfo}", "SaveChangesAsync");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request.");
            return false;
        }
    }

    //Tests Created

    /// <summary>
    /// Retrieves an extended audit history for a specified matter and document, using centralized validation and consistent logging.
    /// </summary>
    /// <param name="matterId">The ID of the matter to retrieve data for.</param>
    /// <param name="documentId">The ID of the document to retrieve data for.</param>
    /// <param name="direction">The direction of the operation (From/To).</param>
    /// <returns>A queryable collection of audit history records.</returns>
    /// <exception cref="ArgumentException">Thrown if both <paramref name="matterId"/> and <paramref name="documentId"/> are empty, or if either does not exist.</exception>
    public async Task<IQueryable<MatterDocumentActivityUserMinimalDto>> GetExtendedAuditsAsync(
        Guid matterId,
        Guid documentId,
        AuditEnums.AuditDirection direction)
    {
        // Validate direction using helper
        direction = ValidateAndProcessAuditDirection(direction);

        // Centralized validation using the validation service
        if (matterId == Guid.Empty && documentId == Guid.Empty)
            throw new ArgumentException("At least one of matterId or documentId must be provided.");

        if (matterId != Guid.Empty)
        {
            var matterValidation = await validationService.ValidateMatterExistsAsync(matterId);
            if (matterValidation != null)
            {
                logger.LogWarning("Matter with ID: {MatterId} does not exist.", matterId);
                throw new ArgumentException($"Matter with ID {matterId} does not exist.", nameof(matterId));
            }
        }

        if (documentId != Guid.Empty)
        {
            var documentValidation = await validationService.ValidateDocumentExistsAsync(documentId);
            if (documentValidation != null)
            {
                logger.LogWarning("Document with ID: {DocumentId} does not exist.", documentId);
                throw new ArgumentException($"Document with ID {documentId} does not exist.", nameof(documentId));
            }
        }

        try
        {
            IQueryable<MatterDocumentActivityUserMinimalDto> query = direction switch
            {
                AuditEnums.AuditDirection.From => context.MatterDocumentActivityUsersFrom
                    .AsNoTracking()
                    .Select(audit => new MatterDocumentActivityUserMinimalDto
                    {
                        MatterId = audit.MatterId,
                        DocumentId = audit.DocumentId,
                        MatterDocumentActivityId = audit.MatterDocumentActivityId,
                        UserId = audit.UserId,
                        CreatedAt = audit.CreatedAt
                    }),
                AuditEnums.AuditDirection.To => context.MatterDocumentActivityUsersTo
                    .AsNoTracking()
                    .Select(audit => new MatterDocumentActivityUserMinimalDto
                    {
                        MatterId = audit.MatterId,
                        DocumentId = audit.DocumentId,
                        MatterDocumentActivityId = audit.MatterDocumentActivityId,
                        UserId = audit.UserId,
                        CreatedAt = audit.CreatedAt
                    }),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), $"Unhandled AuditDirection value: {direction}")
            };

            if (matterId != Guid.Empty)
                query = query.Where(audit => audit.MatterId == matterId);

            if (documentId != Guid.Empty)
                query = query.Where(audit => audit.DocumentId == documentId);

            logger.LogInformation("Extended audits query built for MatterId: {MatterId}, DocumentId: {DocumentId}, Direction: {Direction}", matterId, documentId, direction);
            return query;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request.");
            throw new InvalidOperationException("An unexpected error occurred. See inner exception for details.", ex);
        }
    }

    /// <summary>
    /// Validates and processes the specified audit direction, logging the result.
    /// </summary>
    /// <param name="direction">The audit direction to validate and process.</param>
    /// <returns>The validated <see cref="AuditEnums.AuditDirection"/> value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the direction is not a valid value.</exception>
    private AuditEnums.AuditDirection ValidateAndProcessAuditDirection(AuditEnums.AuditDirection direction)
    {
        // Enum validation using the generic overload
        if (!Enum.IsDefined(direction))
        {
            logger.LogWarning("Invalid AuditDirection value: {Direction}", direction);
            throw new ArgumentOutOfRangeException(nameof(direction), $"Invalid AuditDirection value: {direction}");
        }

        logger.LogInformation("AuditDirection '{Direction}' validated and processed.", direction);
        return direction;
    }


    /// <summary>
    /// Retrieves a user by their username, using the validation service for input validation and consistent logging.
    /// </summary>
    /// <param name="username">The username of the user to retrieve.</param>
    /// <returns>The requested user, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="username"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if multiple users are found with the same username.</exception>
    private async Task<User?> GetUserAsync(string username)
    {
        // Centralized validation using the validation service
        if (!validationService.ValidateStringNotEmpty(username, nameof(username)))
            throw new ArgumentNullException(nameof(username), "Username cannot be null or empty.");

        try
        {
            return await GetUserByUsernameAsync(username);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "An InvalidOperationException occurred. Context: {ActivityName}", username);
            throw new InvalidOperationException($"An error occurred while processing the username: {username}. See inner exception for details.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request.");
            throw new InvalidOperationException("An unexpected error occurred. See inner exception for details.", ex);
        }
    }

    /// <summary>
    /// Retrieves a DocumentActivity by its name, using the validation service for input validation and consistent logging.
    /// </summary>
    /// <param name="activityName">The name of the activity to retrieve.</param>
    /// <returns>The requested DocumentActivity, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="activityName"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if multiple activities are found with the same name.</exception>
    private async Task<DocumentActivity?> GetDocumentActivityAsync(string activityName)
    {
        // Centralized validation using the validation service
        if (!validationService.ValidateStringNotEmpty(activityName, nameof(activityName)))
            throw new ArgumentNullException(nameof(activityName), "Activity name cannot be null or empty.");

        try
        {
            var documentActivity = await context.DocumentActivities
                .AsNoTracking()
                .SingleOrDefaultAsync(a => a.Activity == activityName);

            if (documentActivity == null)
            {
                logger.LogWarning("No DocumentActivity found with activity name: {ActivityName}", activityName);
            }
            else
            {
                logger.LogInformation("DocumentActivity '{ActivityName}' retrieved successfully.", activityName);
            }

            return documentActivity;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "An InvalidOperationException occurred. Context: {ActivityName}", activityName);
            throw new InvalidOperationException($"An error occurred while processing the activity name: {activityName}. See inner exception for details.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request.");
            throw new InvalidOperationException("An unexpected error occurred. See inner exception for details.", ex);
        }
    }

    //Tests Created

    /// <summary>
    /// Retrieves a RevisionActivity by its name, using the validation service for input validation and consistent logging.
    /// </summary>
    /// <param name="activityName">The name of the revision activity to retrieve.</param>
    /// <returns>The requested RevisionActivity, or null if not found.</returns>
    public async Task<RevisionActivity?> GetRevisionActivityByActivityNameAsync(string activityName)
    {
        // Centralized validation using the validation service
        if (!validationService.ValidateStringNotEmpty(activityName, nameof(activityName)))
        {
            logger.LogWarning("Activity name cannot be null or empty.");
            return null;
        }

        try
        {
            var revisionActivity = await context.RevisionActivities
                .AsNoTracking()
                .SingleOrDefaultAsync(a => a.Activity == activityName);

            if (revisionActivity == null)
            {
                logger.LogWarning("No RevisionActivity found with activity name: {ActivityName}", activityName);
            }
            else
            {
                logger.LogInformation("RevisionActivity '{ActivityName}' retrieved successfully.", activityName);
            }

            return revisionActivity;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "An InvalidOperationException occurred. Context: {ActivityName}", activityName);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request.");
            return null;
        }
    }

    /// <summary>
    /// Adds an audit log entry for the specified entity, activity, and user, using centralized validation and consistent logging.
    /// </summary>
    /// <typeparam name="TActivity">The type of the activity entity (e.g., DocumentActivity, MatterActivity, RevisionActivity).</typeparam>
    /// <typeparam name="TEntity">The type of the main entity (e.g., Document, Matter, Revision).</typeparam>
    /// <param name="entity">The entity being audited.</param>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="activity">The activity performed.</param>
    /// <param name="activityId">The unique identifier of the activity.</param>
    /// <param name="user">The user performing the action.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any required argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown if any required Guid is empty.</exception>
    private async Task AddAuditLogAsync<TActivity, TEntity>(
        TEntity entity,
        Guid entityId,
        TActivity activity,
        Guid activityId,
        User user,
        Guid userId)
        where TActivity : class
        where TEntity : class
    {
        // Centralized validation using the validation service
        if (validationService.ValidateNotNull(entity, nameof(entity)) != null)
            throw new ArgumentNullException(nameof(entity));
        if (validationService.ValidateNotNull(activity, nameof(activity)) != null)
            throw new ArgumentNullException(nameof(activity));
        if (validationService.ValidateNotNull(user, nameof(user)) != null)
            throw new ArgumentNullException(nameof(user));
        if (validationService.ValidateGuid(entityId, nameof(entityId)) != null)
            throw new ArgumentException("EntityId cannot be empty.", nameof(entityId));
        if (validationService.ValidateGuid(activityId, nameof(activityId)) != null)
            throw new ArgumentException("ActivityId cannot be empty.", nameof(activityId));
        if (validationService.ValidateGuid(userId, nameof(userId)) != null)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        try
        {
            switch (entity)
            {
                case Matter matterEntity when activity is MatterActivity matterActivity:
                {
                    context.Attach(matterActivity);
                    context.Attach(user);
                    // Always use the tracked entity from the context
                    var trackedMatter = await context.Matters
                        .SingleOrDefaultAsync(m => m.Id == matterEntity.Id);

                    if (trackedMatter == null)
                    {
                        logger.LogError(
                            "Audit log could not be created: Matter not found (Id: {EntityId}), activity {ActivityType} (Id: {ActivityId}), user (Id: {UserId}).",
                            entityId, typeof(TActivity).Name, activityId, userId);
                        break;
                    }

                    var auditEntry = new MatterActivityUser
                    {
                        Matter = trackedMatter,
                        MatterId = trackedMatter.Id,
                        MatterActivity = matterActivity,
                        MatterActivityId = matterActivity.Id,
                        User = user,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                    };

                    await context.MatterActivityUsers.AddAsync(auditEntry);

                    var saved = await SaveChangesAsync();
                    if (!saved)
                    {
                        logger.LogError(
                            "Audit log could not be persisted for entity {EntityType} (Id: {EntityId}), activity {ActivityType} (Id: {ActivityId}), user (Id: {UserId}) at {CreatedAt}.",
                            typeof(TEntity).Name, entityId, typeof(TActivity).Name, activityId, userId,
                            auditEntry.CreatedAt);
                        throw new InvalidOperationException("Failed to persist Matter audit log entry.");
                    }

                    logger.LogInformation(
                        "Audit log entry added for entity {EntityType} (Id: {EntityId}), activity {ActivityType} (Id: {ActivityId}), user (Id: {UserId}) at {CreatedAt}.",
                        typeof(TEntity).Name, entityId, typeof(TActivity).Name, activityId, userId,
                        auditEntry.CreatedAt);

                    break;
                }
                case Document documentEntity when activity is DocumentActivity documentActivity:
                {
                    context.Attach(documentActivity);
                    context.Attach(user);
                    var trackedDocument = await context.Documents
                        .FirstOrDefaultAsync(m => m.Id == documentEntity.Id);
                    if (trackedDocument == null)
                    {
                        logger.LogError(
                            "Audit log could not be created for entity {EntityType} (Id: {EntityId}), activity {ActivityType} (Id: {ActivityId}), user (Id: {UserId}).",
                            typeof(TEntity).Name, entityId, typeof(TActivity).Name, activityId, userId);
                        break;
                    }

                    var auditEntry = new DocumentActivityUser
                    {
                        Document = trackedDocument,
                        DocumentId = trackedDocument.Id,
                        DocumentActivity = documentActivity,
                        DocumentActivityId = documentActivity.Id,
                        User = user,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                    };

                    await context.DocumentActivityUsers.AddAsync(auditEntry);

                    var saved = await SaveChangesAsync();
                    if (!saved)
                    {
                        logger.LogError(
                            "Audit log could not be persisted for entity {EntityType} (Id: {EntityId}), activity {ActivityType} (Id: {ActivityId}), user (Id: {UserId}) at {CreatedAt}.",
                            typeof(TEntity).Name, entityId, typeof(TActivity).Name, activityId, userId,
                            auditEntry.CreatedAt);
                        throw new InvalidOperationException("Failed to persist Matter audit log entry.");
                    }

                    logger.LogInformation(
                        "Audit log entry added for entity {EntityType} (Id: {EntityId}), activity {ActivityType} (Id: {ActivityId}), user (Id: {UserId}) at {CreatedAt}.",
                        typeof(TEntity).Name, entityId, typeof(TActivity).Name, activityId, userId,
                        auditEntry.CreatedAt);

                    break;
                }
                case Revision revisionEntity when activity is RevisionActivity revisionActivity:
                {
                    context.Attach(revisionActivity);
                    context.Attach(user);
                    var trackedRevision = await context.Revisions
                        .FirstOrDefaultAsync(m => m.Id == revisionEntity.Id);
                    if (trackedRevision == null)
                    {
                        logger.LogError(
                            "Audit log could not be created for entity {EntityType} (Id: {EntityId}), activity {ActivityType} (Id: {ActivityId}), user (Id: {UserId}).",
                            typeof(TEntity).Name, entityId, typeof(TActivity).Name, activityId, userId);
                        break;
                    }

                    var auditEntry = new RevisionActivityUser
                    {
                        Revision = trackedRevision,
                        RevisionId = trackedRevision.Id,
                        RevisionActivity = revisionActivity,
                        RevisionActivityId = revisionActivity.Id,
                        User = user,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                    };

                    await context.RevisionActivityUsers.AddAsync(auditEntry);

                    var saved = await SaveChangesAsync();
                    if (!saved)
                    {
                        logger.LogError(
                            "Audit log could not be persisted for entity {EntityType} (Id: {EntityId}), activity {ActivityType} (Id: {ActivityId}), user (Id: {UserId}) at {CreatedAt}.",
                            typeof(TEntity).Name, entityId, typeof(TActivity).Name, activityId, userId,
                            auditEntry.CreatedAt);
                        throw new InvalidOperationException("Failed to persist Matter audit log entry.");
                    }

                    logger.LogInformation(
                        "Audit log entry added for entity {EntityType} (Id: {EntityId}), activity {ActivityType} (Id: {ActivityId}), user (Id: {UserId}) at {CreatedAt}.",
                        typeof(TEntity).Name, entityId, typeof(TActivity).Name, activityId, userId,
                        auditEntry.CreatedAt);

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request.");
            throw new InvalidOperationException("An unexpected error occurred. See inner exception for details.", ex);
        }
    }

    /// <summary>
    /// Adds an audit log entry for the specified document Move / Copy entity, activity, and user, using centralized validation and consistent logging.
    /// </summary>
    /// <typeparam name="TActivity">The type of the activity entity (e.g., MatterDocumentActivity).</typeparam>
    /// <typeparam name="TEntity">The type of the main entity (e.g., Document, Matter, Revision).</typeparam>
    /// <param name="entity">The entity being audited.</param>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="activity">The activity performed.</param>
    /// <param name="activityId">The unique identifier of the activity.</param>
    /// <param name="user">The user performing the action.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="direction">copy / Move FROM or To.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any required argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown if any required Guid is empty.</exception>
    private async Task AddMoveCopyAuditLogAsync<TActivity, TEntity>(
        TEntity entity,
        Guid entityId,
        TActivity activity,
        Guid activityId,
        User user,
        Guid userId,
        string direction)
        where TActivity : class
        where TEntity : class
    {
        // Centralized validation using the validation service
        if (validationService.ValidateNotNull(entity, nameof(entity)) != null)
            throw new ArgumentNullException(nameof(entity));
        if (validationService.ValidateNotNull(activity, nameof(activity)) != null)
            throw new ArgumentNullException(nameof(activity));
        if (validationService.ValidateNotNull(user, nameof(user)) != null)
            throw new ArgumentNullException(nameof(user));
        if (validationService.ValidateGuid(entityId, nameof(entityId)) != null)
            throw new ArgumentException("EntityId cannot be empty.", nameof(entityId));
        if (validationService.ValidateGuid(activityId, nameof(activityId)) != null)
            throw new ArgumentException("ActivityId cannot be empty.", nameof(activityId));
        if (validationService.ValidateGuid(userId, nameof(userId)) != null)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        try
        {
            switch (entity)
            {
                case Matter matterEntity when activity is MatterDocumentActivity matterDocumentActivity:
                {
                    context.Attach(matterDocumentActivity);
                    context.Attach(user);
                    var trackedMatter = await context.Matters
                        .SingleOrDefaultAsync(m => m.Id == matterEntity.Id);

                    if ("to".Contains(direction, StringComparison.OrdinalIgnoreCase))
                    {
                        var auditEntry = new MatterDocumentActivityUserTo
                        {
                            Matter = trackedMatter,
                            MatterId = trackedMatter!.Id,
                            MatterDocumentActivity = matterDocumentActivity,
                            MatterDocumentActivityId = matterDocumentActivity.Id,
                            User = user,
                            UserId = userId,
                            CreatedAt = DateTime.UtcNow,
                        };
                        await context.MatterDocumentActivityUsersTo.AddAsync(auditEntry);

                        var saved = await SaveChangesAsync();
                        if (!saved)
                        {
                            logger.LogError(
                                "Audit log could not be persisted for entity {EntityType} (Id: {EntityId}), activity {ActivityType} (Id: {ActivityId}), user (Id: {UserId}) at {CreatedAt}.",
                                typeof(TEntity).Name, entityId, typeof(TActivity).Name, activityId, userId,
                                auditEntry.CreatedAt);
                            throw new InvalidOperationException("Failed to persist Matter audit log entry.");
                        }

                        logger.LogInformation(
                            "Audit log entry added for entity {EntityType} (Id: {EntityId}), activity {ActivityType} (Id: {ActivityId}), user (Id: {UserId}) at {CreatedAt}.",
                            typeof(TEntity).Name, entityId, typeof(TActivity).Name, activityId, userId,
                            auditEntry.CreatedAt);
                    }
                    else if ("from".Contains(direction, StringComparison.OrdinalIgnoreCase))
                    {
                        var auditEntry = new MatterDocumentActivityUserFrom
                        {
                            Matter = trackedMatter,
                            MatterId = trackedMatter!.Id,
                            MatterDocumentActivity = matterDocumentActivity,
                            MatterDocumentActivityId = matterDocumentActivity.Id,
                            User = user,
                            UserId = userId,
                            CreatedAt = DateTime.UtcNow,
                        };
                        await context.MatterDocumentActivityUsersFrom.AddAsync(auditEntry);

                        var saved = await SaveChangesAsync();
                        if (!saved)
                        {
                            logger.LogError(
                                "Audit log could not be persisted for entity {EntityType} (Id: {EntityId}), activity {ActivityType} (Id: {ActivityId}), user (Id: {UserId}) at {CreatedAt}.",
                                typeof(TEntity).Name, entityId, typeof(TActivity).Name, activityId, userId,
                                auditEntry.CreatedAt);
                            throw new InvalidOperationException("Failed to persist Matter audit log entry.");
                        }

                        logger.LogInformation(
                            "Audit log entry added for entity {EntityType} (Id: {EntityId}), activity {ActivityType} (Id: {ActivityId}), user (Id: {UserId}) at {CreatedAt}.",
                            typeof(TEntity).Name, entityId, typeof(TActivity).Name, activityId, userId,
                            auditEntry.CreatedAt);
                    }

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request.");
            throw new InvalidOperationException("An unexpected error occurred. See inner exception for details.", ex);
        }
    }

    /// <summary>
    /// Creates a new revision for a specified document, using centralized validation and consistent logging.
    /// </summary>
    /// <param name="documentId">The ID of the document to add the revision to.</param>
    /// <param name="revisionToAdd">The revision DTO to be added to the specified document.</param>
    /// <returns>The created <see cref="Revision"/> entity, or null if the operation fails.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="documentId"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="revisionToAdd"/> is null.</exception>
    private async Task<Revision?> CreateRevisionAsync(Guid documentId, RevisionDto revisionToAdd)
    {
        // Centralized validation using the validation service
        var guidValidation = validationService.ValidateGuid(documentId, nameof(documentId));
        if (guidValidation != null)
            throw new ArgumentException("Invalid documentId.", nameof(documentId));
        if (validationService.ValidateNotNull(revisionToAdd, nameof(revisionToAdd)) != null)
            throw new ArgumentNullException(nameof(revisionToAdd));

        // Optionally: Validate the document exists
        var documentExists = await validationService.ValidateDocumentExistsAsync(documentId);
        if (documentExists != null)
        {
            logger.LogWarning("Document not found with ID: {DocumentId}", documentId);
            return null;
        }

        try
        {
            // Retrieve the document to add the revision to
            var documentToAddTo = await context.Documents.AsNoTracking().SingleOrDefaultAsync(d => d.Id == documentId);
            if (documentToAddTo == null)
            {
                logger.LogWarning("Document not found with ID: {DocumentId}", documentId);
                return null;
            }

            // Map the DTO to the Revision entity and set its properties
            var dbRevision = mapper.Map<Revision>(revisionToAdd);
            dbRevision.DocumentId = documentId;
            dbRevision.Document = documentToAddTo;

            // Add the revision to the context
            var createdRevision = await context.Revisions.AddAsync(dbRevision);

            logger.LogInformation("Revision created for DocumentId: {DocumentId} with RevisionNumber: {RevisionNumber}", documentId, createdRevision.Entity.Id);

            return createdRevision.Entity;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request.");
            throw new InvalidOperationException("An unexpected error occurred. See inner exception for details.", ex);
        }
    }

    #endregion Helper Methods

    //Tests Created

    #region Revision Activities

    //Tests Created

    /// <summary>
    /// Updates a specified revision and logs the update as an audit activity, using centralized validation and consistent logging.
    /// </summary>
    /// <param name="matterId">The ID of the matter containing the revision to update.</param>
    /// <param name="documentId">The ID of the document containing the revision to update.</param>
    /// <param name="revisionId">The ID of the revision to update.</param>
    /// <param name="revision">The updated revision data.</param>
    /// <returns>The updated revision, or null if the operation fails.</returns>
    public async Task<Revision?> UpdateRevisionAsync(
        Guid matterId,
        Guid documentId,
        Guid revisionId,
        Revision revision)
    {
        // Centralized validation using the validation service
        if (validationService.ValidateNotNull(revision, nameof(revision)) != null)
        {
            logger.LogWarning("Revision cannot be null.");
            return null;
        }

        var matterValidation = await validationService.ValidateMatterExistsAsync(matterId);
        if (matterValidation != null)
        {
            logger.LogWarning("Matter with ID: {MatterId} does not exist.", matterId);
            return null;
        }

        var documentValidation = await validationService.ValidateDocumentExistsAsync(documentId);
        if (documentValidation != null)
        {
            logger.LogWarning("Document with ID: {DocumentId} does not exist.", documentId);
            return null;
        }

        var revisionValidation = await validationService.ValidateRevisionExistsAsync(revisionId);
        if (revisionValidation != null)
        {
            logger.LogWarning("Revision with ID: {RevisionNumber} does not exist.", revisionId);
            return null;
        }

        try
        {
            // Map the updated revision data and update the entity in the context
            var updatedRevision = context.Revisions.Update(mapper.Map<Revision>(revision)).Entity;

            // Retrieve the current user and the "SAVED" revision activity
            var userProcessing = await GetUserAsync(UserName);
            var revisionActivity = await GetRevisionActivityByActivityNameAsync("SAVED");

            if (userProcessing == null || revisionActivity == null)
            {
                logger.LogWarning("User or RevisionActivity not found. User: {UserName}, Activity: SAVED", UserName);
                return null;
            }

            await AddAuditLogAsync(
                entity: updatedRevision,
                entityId: updatedRevision.Id,
                activity: revisionActivity,
                activityId: revisionActivity.Id,
                user: userProcessing,
                userId: userProcessing.Id
            );

            // Save changes and return the updated revision
            var saved = await SaveChangesAsync();
            if (saved)
            {
                logger.LogInformation("Revision with ID: {RevisionNumber} updated successfully.", revisionId);
                return updatedRevision;
            }
            else
            {
                logger.LogWarning("Failed to save changes after updating revision with ID: {RevisionNumber}.", revisionId);
                return null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating revision. MatterId: {MatterId}, DocumentId: {DocumentId}, RevisionNumber: {RevisionNumber}",
                matterId, documentId, revisionId);
            return null;
        }
    }

    #endregion Revision Activities

    #region Documents

    /// <summary>
    /// Adds a new document to the specified matter, using centralized validation and consistent logging.
    /// </summary>
    /// <param name="matterId">The ID of the matter to add the document to.</param>
    /// <param name="document">The document DTO to add.</param>
    /// <returns>The created document, or null if the operation fails.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="matterId"/> is invalid or the matter does not exist.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="document"/> is null.</exception>
    public async Task<Document?> AddDocumentAsync(Guid matterId, DocumentForCreationDto? document)
    {
        // Centralized validation using the validation service
        var matterValidation = await validationService.ValidateMatterExistsAsync(matterId);
        if (matterValidation != null)
            throw new ArgumentException($"Matter with ID {matterId} does not exist.", nameof(matterId));
        if (validationService.ValidateNotNull(document, nameof(document)) != null)
            throw new ArgumentNullException(nameof(document));

        // Additional document creation validation
        var validationResults = await validationService.ValidateDocumentForCreationAsync(matterId, document);
        if (validationResults.Any())
        {
            logger.LogWarning("Document creation validation returned null or empty results: {ValidationErrors}", string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            return null;
        }

        try
        {
            var newDocument = mapper.Map<Document>(document ?? throw new ArgumentNullException(nameof(document)));

            var user = await GetUserAsync(UserName);
            var documentActivity = await GetDocumentActivityAsync("CREATED");
            var revisionActivity = await GetRevisionActivityByActivityNameAsync("CREATED");

            if (user == null || documentActivity == null || revisionActivity == null)
            {
                logger.LogWarning("User or activity not found. User: {UserName}, DocumentActivity: {DocumentActivity}, RevisionActivity: {RevisionActivity}",
                    UserName, documentActivity?.Id, revisionActivity?.Id);
                return null;
            }

            // Set matter reference
            newDocument.MatterId = matterId;

            var documentAdditionResult = await context.Documents.AddAsync(newDocument);

            if (documentAdditionResult.State != EntityState.Added)
            {
                logger.LogWarning("Failed to add new document for matter {MatterId}.", matterId);
                return null;
            }

            var createdDocument = documentAdditionResult.Entity;

            var documentSaved = await SaveChangesAsync();
            if (!documentSaved)
            {
                logger.LogWarning("Failed to save new document for matter {MatterId}.", matterId);
                return null;
            }

            // Create initial revision
            var newRevision = new RevisionForCreationDto(false)
            {
                CreationDate = DateTime.UtcNow,
                ModificationDate = DateTime.UtcNow,
                IsDeleted = false,
                RevisionNumber = 1
            };

            var RevisionAdditionResult= await context.Revisions.AddAsync(mapper.Map<Revision>(newRevision));

            if (RevisionAdditionResult.State != EntityState.Added)
            {
                logger.LogWarning("Failed to save new revision for document {createdDocumentId}.", createdDocument.Id);
                return null;
            }

            var createdRevision = RevisionAdditionResult.Entity;
            createdRevision.Document = createdDocument;
            createdRevision.DocumentId = createdDocument.Id;

            createdDocument.Revisions.Add(createdRevision);

            var documentUpdate = context.Documents.Update(createdDocument);

            if (documentUpdate.State != EntityState.Modified)
            {
                logger.LogWarning("Failed to update document with new revision for matter {MatterId}.", matterId);
                return null;
            }
            else
            {
                await SaveChangesAsync();
            }

            // Create document audit log
            await AddAuditLogAsync(
                entity: createdDocument,
                entityId: createdDocument.Id,
                activity: documentActivity,
                activityId: documentActivity.Id,
                user: user,
                userId: user.Id
            );

            // Create a revision audit log
            await AddAuditLogAsync(
                entity: createdRevision,
                entityId: createdRevision.Id,
                activity: revisionActivity,
                activityId: revisionActivity.Id,
                user: user,
                userId: user.Id
            );

            logger.LogInformation("Successfully added document with ID: {DocumentId} to matter {MatterId}.", createdDocument.Id, matterId);
            return createdDocument;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding document to matter with ID: {MatterId}", matterId);
            return null;
        }
    }

    /// <summary>
    /// Performs a move or copy operation on a document, using centralized validation and consistent logging.
    /// </summary>
    /// <param name="sourceMatterId">The ID of the source matter containing the document.</param>
    /// <param name="targetMatterId">The ID of the target matter to move or copy the document to.</param>
    /// <param name="document">The document to move or copy.</param>
    /// <param name="operationType">The type of operation to perform ("MOVED" or "COPIED").</param>
    /// <returns>True if the operation was successful, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if any matter or document does not exist.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="document"/> is null.</exception>
    public async Task<bool> PerformDocumentOperationAsync(
        Guid sourceMatterId,
        Guid targetMatterId,
        DocumentWithoutRevisionsDto? document,
        string operationType)
    {
        // Centralized validation using the validation service
        var sourceMatterValidation = await validationService.ValidateMatterExistsAsync(sourceMatterId);
        if (sourceMatterValidation != null)
            throw new ArgumentException($"Source matter with ID {sourceMatterId} does not exist.", nameof(sourceMatterId));
        var targetMatterValidation = await validationService.ValidateMatterExistsAsync(targetMatterId);
        if (targetMatterValidation != null)
            throw new ArgumentException($"Target matter with ID {targetMatterId} does not exist.", nameof(targetMatterId));
        if (validationService.ValidateNotNull(document, nameof(document)) != null)
            throw new ArgumentNullException(nameof(document));
        if (document != null)
        {
            var documentValidation = await validationService.ValidateDocumentExistsAsync(document.Id);
            if (documentValidation != null)
                throw new ArgumentException($"Document with ID {document.Id} does not exist.", nameof(document));
        }

        try
        {
            var newFileName = string.Empty;
            var newMatter = await context.Matters.SingleOrDefaultAsync(matter => matter.Id == targetMatterId);
            var oldDocument = await context.Documents.Include(d => d.Revisions)
                .SingleOrDefaultAsync(doc => document != null && doc.Id == document.Id);
            var oldMatter = oldDocument != null
                ? await context.Matters.SingleOrDefaultAsync(matter => matter.Id == oldDocument.MatterId)
                : null;

            if (oldDocument != null && operationType == "MOVED" && oldDocument.Revisions.Count == 0)
            {
                operationType = "COPIED";
            }

            var lastRevision = oldDocument?.Revisions.OrderBy(r => r.RevisionNumber).LastOrDefault();
            if (lastRevision == null)
            {
                logger.LogWarning("Last Revision is invalid: {LastRevision}", lastRevision);
                return false;
            }

            var matterDocumentActivity =
                await context.MatterDocumentActivities.SingleOrDefaultAsync(activity =>
                    activity.Activity == operationType);
            var user = await GetUserAsync(UserName);

            if (matterDocumentActivity == null || user == null)
            {
                logger.LogWarning(
                    "MatterDocumentActivity or User not found. OperationType: {OperationType}, User: {UserName}",
                    operationType, UserName);
                return false;
            }

            switch (operationType)
            {
                case "MOVED":
                    if (oldDocument != null && newMatter != null)
                    {
                        await AddMoveCopyAuditLogAsync(
                            entity: oldDocument.Matter,
                            entityId: oldDocument.MatterId,
                            activity: matterDocumentActivity,
                            activityId: matterDocumentActivity.Id,
                            user: user,
                            userId: user.Id,
                            "from"
                        );

                        oldDocument.Matter = newMatter;
                        oldDocument.MatterId = newMatter.Id;

                        await AddMoveCopyAuditLogAsync(
                            entity: oldDocument.Matter,
                            entityId: oldDocument.MatterId,
                            activity: matterDocumentActivity,
                            activityId: matterDocumentActivity.Id,
                            user: user,
                            userId: user.Id,
                            "to"
                        );
                        context.Documents.Update(oldDocument);

                        newFileName = $"{oldDocument.Id}R{lastRevision.RevisionNumber}.{oldDocument.Extension}";
                    }
                    break;
                case "COPIED":
                    if (oldDocument != null && newMatter != null)
                    {
                        var newDocument = new DocumentForCreationDto()
                        {
                            FileName = oldDocument.FileName,
                            Extension = oldDocument.Extension,
                            IsCheckedOut = false
                        };

                        var createdDocument = await AddDocumentAsync(newMatter.Id, newDocument);

                        if (createdDocument != null)
                        {
                            await AddMoveCopyAuditLogAsync(
                                entity: oldDocument.Matter,
                                entityId: oldDocument.MatterId,
                                activity: matterDocumentActivity,
                                activityId: matterDocumentActivity.Id,
                                user: user,
                                userId: user.Id,
                                "from"
                            );

                            await AddMoveCopyAuditLogAsync(
                                entity: newMatter,
                                entityId: newMatter.Id,
                                activity: matterDocumentActivity,
                                activityId: matterDocumentActivity.Id,
                                user: user,
                                userId: user.Id,
                                "to"
                            );

                            newFileName = $"{oldDocument.Id}R{lastRevision.RevisionNumber + 1}.{oldDocument.Extension}";
                        }
                    }

                    break;
            }

            if (!await SaveChangesAsync())
                return false;

            if (oldDocument != null && newMatter != null && oldMatter != null)
            {
                var originalFolderPath = Path.Combine(ServerFilesPath, "matters", $"{oldMatter.Id}");
                var newFolderPath = Path.Combine(ServerFilesPath, "matters", $"{newMatter.Id}");

                if (!Directory.Exists(originalFolderPath)) Directory.CreateDirectory(originalFolderPath);
                if (!Directory.Exists(newFolderPath)) Directory.CreateDirectory(newFolderPath);

                var originalPath = Path.Combine(originalFolderPath, newFileName);
                var newPath = Path.Combine(newFolderPath, newFileName);

                switch (operationType)
                {
                    case "MOVED":
                        File.Move(originalPath, newPath);
                        break;
                    case "COPIED":
                        File.Copy(originalPath, newPath);
                        break;
                }

                return await SaveChangesAsync();
            }
            else
            {
                logger.LogWarning("Either oldDocument, newMatter, or oldMatter is null after operation. OperationType: {OperationType}", operationType);
                return false;
            }
        }
        catch (Exception ex)
        {
            if (document != null)
                logger.LogError(ex,
                    "Error performing document operation. SourceMatterId: {SourceMatterId}, TargetMatterId: {TargetMatterId}, DocumentId: {DocumentId}, OperationType: {OperationType}",
                    sourceMatterId, targetMatterId, document.Id, operationType);
            return false;
        }
    }

    /// <summary>
    /// Sets the check-in or check-out state of a document, using centralized validation and consistent logging.
    /// </summary>
    /// <param name="documentId">The ID of the document to update.</param>
    /// <param name="isCheckedOut">True to check out the document, false to check it in.</param>
    /// <returns>True if the operation was successful, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if the document does not exist.</exception>
    public async Task<bool> SetDocumentCheckStateAsync(Guid documentId, bool isCheckedOut)
    {
        // Centralized validation using the validation service
        var documentValidation = await validationService.ValidateDocumentExistsAsync(documentId);
        if (documentValidation != null)
            throw new ArgumentException($"Document with ID {documentId} does not exist.", nameof(documentId));

        try
        {
            // Retrieve the document
            var document = await context.Documents.SingleOrDefaultAsync(d => d.Id == documentId);
            if (document == null)
            {
                logger.LogWarning("Document not found with ID: {DocumentId}", documentId);
                return false;
            }

            // Retrieve the current user and the appropriate activity
            var user = await GetUserAsync(UserName);
            var activityName = isCheckedOut ? "CHECKED OUT" : "CHECKED IN";
            var documentActivity = await GetDocumentActivityAsync(activityName);

            if (user == null || documentActivity == null)
            {
                logger.LogWarning("User or DocumentActivity not found. User: {UserName}, Activity: {ActivityName}", UserName, activityName);
                return false;
            }

            // Update the document's check-out state
            document.IsCheckedOut = isCheckedOut;
            context.Documents.Update(document);

            await AddAuditLogAsync(
                entity: document,
                entityId: document.Id,
                activity: documentActivity,
                activityId: documentActivity.Id,
                user: user,
                userId: user.Id
            );

            // Save changes
            var saved = await SaveChangesAsync();
            if (saved)
            {
                logger.LogInformation("Document check state updated. DocumentId: {DocumentId}, IsCheckedOut: {IsCheckedOut}", documentId, isCheckedOut);
                return true;
            }
            else
            {
                logger.LogWarning("Failed to save check state update for document with ID: {DocumentId}", documentId);
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting check state for document with ID: {DocumentId}", documentId);
            return false;
        }
    }

    /// <summary>
    /// Deletes a specified document by marking it as deleted, using centralized validation and consistent logging.
    /// </summary>
    /// <param name="document">The document to be deleted.</param>
    /// <returns>True if the document was successfully deleted, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="document"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the document does not exist.</exception>
    public async Task<bool> DeleteDocumentAsync(DocumentDto document)
    {
        // Centralized validation using the validation service
        if (validationService.ValidateNotNull(document, nameof(document)) != null)
            throw new ArgumentNullException(nameof(document));
        var documentValidation = await validationService.ValidateDocumentExistsAsync(document.Id);
        if (documentValidation != null)
            throw new ArgumentException($"Document with ID {document.Id} does not exist.", nameof(document));

        try
        {
            // Retrieve the document from the database
            var dbDocument = await context.Documents.SingleOrDefaultAsync(databaseDocument => databaseDocument.Id == document.Id);
            if (dbDocument == null)
            {
                logger.LogWarning("Document not found with ID: {DocumentId}", document.Id);
                return false;
            }

            // Mark the document as deleted
            dbDocument.IsDeleted = true;
            context.Documents.Update(dbDocument);

            // Retrieve the user and the "DELETED" activity
            var user = await GetUserAsync(UserName);
            var documentActivity = await GetDocumentActivityAsync("DELETED");

            if (user == null || documentActivity == null)
            {
                logger.LogWarning("User or DocumentActivity not found. User: {UserName}, Activity: DELETED", UserName);
                return false;
            }

            await AddAuditLogAsync(
                entity: dbDocument,
                entityId: dbDocument.Id,
                activity: documentActivity,
                activityId: documentActivity.Id,
                user: user,
                userId: user.Id
            );

            // Save changes
            var saved = await SaveChangesAsync();
            if (saved)
            {
                logger.LogInformation("Successfully deleted document with ID: {DocumentId}.", document.Id);
                return true;
            }
            else
            {
                logger.LogWarning("Failed to save changes after deleting document with ID: {DocumentId}.", document.Id);
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting document with ID: {DocumentId}", document.Id);
            return false;
        }
    }

    /// <summary>
    /// Checks if a document exists in the database, using centralized validation and consistent logging.
    /// </summary>
    /// <param name="documentId">The ID of the document to check.</param>
    /// <returns>True if the document exists, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="documentId"/> is invalid.</exception>
    public async Task<bool> DocumentExistsAsync(Guid documentId)
    {
        // Centralized validation using the validation service
        var validationResult = validationService.ValidateGuid(documentId, nameof(documentId));
        if (validationResult != null)
            throw new ArgumentException($"Invalid documentId: {documentId}", nameof(documentId));

        try
        {
            var exists = await context.Documents.AsNoTracking().AnyAsync(document => document.Id == documentId);

            if (exists)
            {
                logger.LogInformation("Document with ID: {DocumentId} exists in the database.", documentId);
            }
            else
            {
                logger.LogWarning("Document with ID: {DocumentId} does not exist in the database.", documentId);
            }

            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if document exists with ID: {DocumentId}", documentId);
            return false;
        }
    }

    /// <summary>
    /// Checks if a document with the specified file name already exists in the given matter,
    /// using centralized validation and consistent logging.
    /// </summary>
    /// <param name="matterId">The ID of the matter containing the document.</param>
    /// <param name="fileName">The file name to check.</param>
    /// <returns>True if the file name exists, false otherwise.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="matterId"/> is invalid or <paramref name="fileName"/> is null or empty.
    /// </exception>
    public async Task<bool> FileNameExists(Guid matterId, string fileName)
    {
        // Centralized validation using the validation service
        var matterValidation = await validationService.ValidateMatterExistsAsync(matterId);
        if (matterValidation != null)
        {
            logger.LogWarning("Matter with ID: {MatterId} does not exist.", matterId);
            throw new ArgumentException($"Matter with ID {matterId} does not exist.", nameof(matterId));
        }
        if (!validationService.ValidateStringNotEmpty(fileName, nameof(fileName)))
        {
            logger.LogWarning("File name cannot be null or empty.");
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
        }

        try
        {
            var exists = await context.Documents
                .AsNoTracking()
                .AnyAsync(d => d.MatterId == matterId && d.FileName == fileName);

            if (exists)
            {
                logger.LogInformation("A document with file name '{FileName}' exists in matter {MatterId}.", fileName, matterId);
            }
            else
            {
                logger.LogInformation("No document with file name '{FileName}' exists in matter {MatterId}.", fileName, matterId);
            }

            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request.");
            throw new InvalidOperationException("An unexpected error occurred. See inner exception for details.", ex);
        }
    }


    /// <summary>
    /// Retrieves a document by its ID, optionally including revisions and history, using centralized validation and consistent logging.
    /// </summary>
    /// <param name="documentId">The ID of the document to retrieve.</param>
    /// <param name="includeRevisions">Whether to include revisions in the result.</param>
    /// <param name="includeHistory">Whether to include history in the result.</param>
    /// <returns>The requested document, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="documentId"/> is empty or the document does not exist.</exception>
    public async Task<Document?> GetDocumentAsync(Guid documentId, bool includeRevisions, bool includeHistory)
    {
        // Centralized validation using the validation service
        var documentValidation = await validationService.ValidateDocumentExistsAsync(documentId);
        if (documentValidation != null)
            throw new ArgumentException($"Document with ID {documentId} does not exist.", nameof(documentId));

        try
        {
            // Base query
            var query = context.Documents.AsNoTracking().Where(document => document.Id == documentId);

            // Include revisions if requested
            if (includeRevisions)
                query = query.Include(document => document.Revisions);

            // Include history if requested
            if (includeHistory)
                query = query.Include(document => document.DocumentActivityUsers)
                    .ThenInclude(activityUser => activityUser.DocumentActivity)
                    .Include(document => document.DocumentActivityUsers)
                    .ThenInclude(activityUser => activityUser.User);

            // Execute the query
            var result = await query.AsSplitQuery().SingleOrDefaultAsync();

            if (result == null)
            {
                logger.LogWarning("No document found with ID: {DocumentId}", documentId);
            }
            else
            {
                logger.LogInformation("Successfully retrieved document with ID: {DocumentId}", documentId);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving document with ID: {DocumentId}", documentId);
            return null;
        }
    }

    /// <summary>
    /// Retrieves a paginated list of documents for a specified matter, using centralized validation and consistent logging.
    /// </summary>
    /// <param name="matterId">The ID of the matter containing the documents.</param>
    /// <param name="parameters">The parameters for pagination and filtering.</param>
    /// <returns>A paginated list of documents.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameters"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="matterId"/> is invalid or the matter does not exist.</exception>
    public async Task<Helpers.PagedList<Document>> GetPaginatedDocumentsAsync(
        Guid matterId,
        DocumentsResourceParameters parameters)
    {
        // Centralized validation using the validation service
        if (validationService.ValidateNotNull(parameters, nameof(parameters)) != null)
            throw new ArgumentNullException(nameof(parameters));
        var matterValidation = await validationService.ValidateMatterExistsAsync(matterId);
        if (matterValidation != null)
            throw new ArgumentException($"Matter with ID {matterId} does not exist.", nameof(matterId));
        if (parameters.PageNumber <= 0)
            throw new ArgumentException("PageNumber must be greater than 0.", nameof(parameters));
        if (parameters.PageSize <= 0)
            throw new ArgumentException("PageSize must be greater than 0.", nameof(parameters));

        try
        {
            var query = context.Documents
                .Where(d => d.MatterId == matterId);

            // Filtering
            if (!string.IsNullOrWhiteSpace(parameters.FileName))
                query = query.Where(d => d.FileName.Contains(parameters.FileName));

            if (!string.IsNullOrWhiteSpace(parameters.SearchQuery))
                query = query.Where(d => d.FileName.Contains(parameters.SearchQuery));

            // Sorting
            if (!string.IsNullOrWhiteSpace(parameters.OrderBy))
                query = query.ApplySort(parameters.OrderBy, propertyMappingService.GetPropertyMapping<DocumentDto, Document>());

            // Data shaping: include revisions if requested
            if (!string.IsNullOrWhiteSpace(parameters.Fields) &&
                parameters.Fields.Contains("Revisions", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Include(document => document.Revisions);
            }

            // Paging
            var pagedList = await Helpers.PagedList<Document>.CreateAsync(
                query.Include(document => document.Revisions).AsNoTracking(),
                parameters.PageNumber,
                parameters.PageSize);

            logger.LogInformation("Paginated documents retrieved for MatterId: {MatterId}, Page: {PageNumber}, PageSize: {PageSize}, TotalCount: {TotalCount}",
                matterId, pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount);

            return pagedList;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving paginated documents for MatterId: {MatterId}", matterId);
            return new Helpers.PagedList<Document>([], 0, parameters.PageNumber, parameters.PageSize);
        }
    }

    /// <summary>
    /// Updates a specified document with new data from a DTO, using centralized validation and consistent logging.
    /// </summary>
    /// <param name="documentId">The ID of the document to update.</param>
    /// <param name="documentForUpdate">The DTO containing updated document data.</param>
    /// <returns>The updated document, or null if the operation fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="documentForUpdate"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the document does not exist or validation fails.</exception>
    public async Task<Document?> UpdateDocumentAsync(Guid documentId, DocumentForUpdateDto documentForUpdate)
    {
        // Centralized validation using the validation service
        if (validationService.ValidateNotNull(documentForUpdate, nameof(documentForUpdate)) != null)
            throw new ArgumentNullException(nameof(documentForUpdate));

        var documentValidation = await validationService.ValidateDocumentExistsAsync(documentId);
        if (documentValidation != null)
            throw new ArgumentException($"Document with ID {documentId} does not exist.", nameof(documentId));

        var validationResults = validationService.ValidateDocumentForUpdate(documentForUpdate);
        var resultsList = validationResults.ToList();
        if (resultsList.Count > 0)
        {
            logger.LogWarning("Document update validation failed: {ValidationErrors}", string.Join("; ", resultsList.Select(v => v.ErrorMessage)));
            return null;
        }

        try
        {
            // Retrieve the existing document entity
            var dbDocument = await context.Documents.SingleOrDefaultAsync(d => d.Id == documentId);
            if (dbDocument == null)
            {
                logger.LogWarning("Document not found with ID: {DocumentId}", documentId);
                return null;
            }

            // Map the DTO to the entity
            mapper.Map(documentForUpdate, dbDocument);

            // Update the document in the context
            context.Documents.Update(dbDocument);

            // Retrieve the current user and the "SAVED" document activity
            var user = await GetUserAsync(UserName);
            var documentActivity = await GetDocumentActivityAsync("SAVED");

            if (user == null || documentActivity == null)
            {
                logger.LogWarning("User or DocumentActivity not found. User: {UserName}, Activity: SAVED", UserName);
                return null;
            }

            await AddAuditLogAsync(
                entity: dbDocument,
                entityId: dbDocument.Id,
                activity: documentActivity,
                activityId: documentActivity.Id,
                user: user,
                userId: user.Id
            );

            // Save changes and return the updated document
            var saved = await SaveChangesAsync();
            if (saved)
            {
                logger.LogInformation("Successfully updated document with ID: {DocumentId}.", dbDocument.Id);
                return dbDocument;
            }
            else
            {
                logger.LogWarning("Failed to save changes after updating document with ID: {DocumentId}.", dbDocument.Id);
                return null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating document with ID: {DocumentId}", documentId);
            return null;
        }
    }

    #endregion Documents

    #region Document Activity

    //Tests Created

    /// <summary>
    /// Retrieves a DocumentActivity by its name, using the validation service for input validation and consistent logging.
    /// </summary>
    /// <param name="activityName">The name of the activity to retrieve.</param>
    /// <returns>The requested DocumentActivity, or null if not found.</returns>
    public async Task<DocumentActivity?> GetDocumentActivityByActivityNameAsync(string activityName)
    {
        // Centralized validation using the validation service
        if (!validationService.ValidateStringNotEmpty(activityName, nameof(activityName)))
        {
            logger.LogWarning("Activity name cannot be null or empty.");
            return null;
        }

        try
        {
            return await GetDocumentActivityAsync(activityName);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "An InvalidOperationException occurred. Context: {ActivityName}", activityName);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request.");
            return null;
        }
    }

    //Tests Created

    /// <summary>
    /// Retrieves document audit records for a specified document, returning appropriate error responses.
    /// </summary>
    /// <param name="documentId">The ID of the document to retrieve audits for.</param>
    /// <returns>
    /// <para><see cref="OkObjectResult"/> with a list of document audit records if successful.</para>
    /// <para><see cref="BadRequestObjectResult"/> if the documentId is invalid.</para>
    /// <para><see cref="NotFoundObjectResult"/> if the document does not exist.</para>
    /// <para><see cref="ObjectResult"/> with status code 500 if an unexpected error occurs.</para>
    /// </returns>
    public async Task<ActionResult<IEnumerable<DocumentActivityUserMinimalDto>>> GetDocumentAuditsAsync(Guid documentId)
    {
        // Validate documentId
        if (documentId == Guid.Empty)
        {
            logger.LogWarning("Invalid documentId: {DocumentId}", documentId);
            return new BadRequestObjectResult($"Invalid documentId: {documentId}");
        }

        // Validate document existence
        ActionResult? documentValidation;
        try
        {
            documentValidation = await validationService.ValidateDocumentExistsAsync(documentId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating document existence for documentId: {DocumentId}", documentId);
            return new ObjectResult("An error occurred while validating document existence.")
            {
                StatusCode = 500
            };
        }

        if (documentValidation != null)
        {
            logger.LogWarning("Document with ID: {DocumentId} does not exist.", documentId);
            return new NotFoundObjectResult($"Document with ID {documentId} does not exist.");
        }

        try
        {
            var query = context.DocumentActivityUsers
                .AsNoTracking()
                .Where(audit => audit.DocumentId == documentId)
                .OrderBy(audit => audit.CreatedAt)
                .Include(audit => audit.User)
                .Include(audit => audit.DocumentActivity);

            var auditList = await query.ToListAsync();

            logger.LogInformation("Retrieved {Count} audit records for DocumentId: {DocumentId}", auditList.Count, documentId);
            var result = mapper.Map<IEnumerable<DocumentActivityUserMinimalDto>>(auditList);
            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving document audit records for DocumentId: {DocumentId}", documentId);
            return new ObjectResult("An unexpected error occurred. See logs for details.")
            {
                StatusCode = 500
            };
        }
    }

    //Tests Created

    /// <summary>
    /// Retrieves a paginated list of document activity audit records (create, save, delete, restore, etc.) for a specified document,
    /// returning appropriate error responses for all error conditions.
    /// </summary>
    /// <param name="documentId">The ID of the document to retrieve audits for.</param>
    /// <param name="resourceParameters">Pagination and sorting information.</param>
    /// <returns>
    /// <para><see cref="OkObjectResult"/> with a paged list of document activity audit records if successful.</para>
    /// <para><see cref="BadRequestObjectResult"/> if the documentId or resource parameters are invalid.</para>
    /// <para><see cref="NotFoundObjectResult"/> if the document does not exist.</para>
    /// <para><see cref="ObjectResult"/> with status code 500 if an unexpected error occurs.</para>
    /// </returns>
    public async Task<ActionResult<Helpers.PagedList<DocumentActivityUserMinimalDto>>> GetDocumentActivityAuditsAsync(
        Guid documentId, DocumentAuditsResourceParameters resourceParameters)
    {
        logger.LogInformation("Fetching document activity audits for DocumentId: {DocumentId} with parameters: {@ResourceParameters}", documentId, resourceParameters);

        // Validate input parameters
        if (documentId == Guid.Empty)
        {
            logger.LogWarning("Invalid documentId: {DocumentId}", documentId);
            return new BadRequestObjectResult($"Invalid documentId: {documentId}");
        }

        if (resourceParameters.PageNumber <= 0)
        {
            logger.LogWarning("PageNumber must be greater than 0. Provided: {PageNumber}", resourceParameters.PageNumber);
            return new BadRequestObjectResult("PageNumber must be greater than 0.");
        }
        if (resourceParameters.PageSize <= 0)
        {
            logger.LogWarning("PageSize must be greater than 0. Provided: {PageSize}", resourceParameters.PageSize);
            return new BadRequestObjectResult("PageSize must be greater than 0.");
        }

        // Validate document existence
        ActionResult? documentValidation;
        try
        {
            documentValidation = await validationService.ValidateDocumentExistsAsync(documentId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating document existence for documentId: {DocumentId}", documentId);
            return new ObjectResult("An error occurred while validating document existence.")
            {
                StatusCode = 500
            };
        }

        if (documentValidation != null)
        {
            logger.LogWarning("Document with ID: {DocumentId} does not exist.", documentId);
            return new NotFoundObjectResult($"Document with ID {documentId} does not exist.");
        }

        try
        {
            // Base query: filter by documentId
            var query = context.DocumentActivityUsers
                .AsNoTracking()
                .Where(audit => audit.DocumentId == documentId);

            // Optional: Sorting
            if (!string.IsNullOrWhiteSpace(resourceParameters.OrderBy))
            {
                query = resourceParameters.OrderBy.Trim().ToLower() switch
                {
                    "createdat" => query.OrderByDescending(audit => audit.CreatedAt),
                    "activity" => query.OrderBy(audit => audit.DocumentActivity.Activity),
                    _ => query.OrderByDescending(audit => audit.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(audit => audit.CreatedAt);
            }

//            var results = await query.ToListAsync();

            // Project to minimal DTO
            var minimalQuery = query.Select(audit => new DocumentActivityUserMinimalDto
            {
                DocumentId = audit.DocumentId,
                DocumentActivity = new DocumentActivityMinimalDto
                {
                    Activity = audit.DocumentActivity.Activity
                },
                User = new UserMinimalDto
                {
                    Id = audit.UserId,
                    Name = audit.User.Name
                },
                CreatedAt = audit.CreatedAt
            });

            // Paging
            var pagedList = await Helpers.PagedList<DocumentActivityUserMinimalDto>.CreateAsync(
                minimalQuery,
                resourceParameters.PageNumber,
                resourceParameters.PageSize);

            logger.LogInformation("Successfully fetched {Count} document activity audit records for DocumentId: {DocumentId}.", pagedList.Count, documentId);

            return new OkObjectResult(pagedList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching document activity audits for DocumentId: {DocumentId}.", documentId);
            return new ObjectResult("An unexpected error occurred. See logs for details.")
            {
                StatusCode = 500
            };
        }
    }

    //Tests Created

    /// <summary>
    /// Retrieves a paginated list of "move/copy FROM" audit records for a specified document,
    /// returning appropriate error responses for all error conditions.
    /// </summary>
    /// <param name="documentId">The ID of the document to retrieve move/copy FROM audits for.</param>
    /// <param name="resourceParameters">Pagination and sorting information.</param>
    /// <returns>
    /// <para>A paged list of move/copy FROM audit records if successful.</para>
    /// <para><see cref="BadRequestObjectResult"/> if the input is invalid.</para>
    /// <para><see cref="NotFoundObjectResult"/> if the document does not exist.</para>
    /// <para><see cref="ObjectResult"/> with status code 500 if an unexpected error occurs.</para>
    /// </returns>
    public async Task<Helpers.PagedList<MatterDocumentActivityUserMinimalDto>> GetPaginatedDocumentMoveFromAuditsAsync(
        Guid documentId, DocumentAuditsResourceParameters resourceParameters)
    {
        logger.LogInformation("Fetching move/copy FROM audits for DocumentId: {DocumentId} with parameters: {@ResourceParameters}", documentId, resourceParameters);

        // Validate input parameters
        if (documentId == Guid.Empty)
        {
            logger.LogWarning("Invalid documentId: {DocumentId}", documentId);
            return new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>([], 0, 1, 10);
        }
        if (resourceParameters == null)
        {
            logger.LogWarning("Resource parameters for move/copy FROM audits are null.");
            return new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>([], 0, 1, 10);
        }
        if (resourceParameters.PageNumber <= 0)
        {
            logger.LogWarning("PageNumber must be greater than 0. Provided: {PageNumber}", resourceParameters.PageNumber);
            return new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>([], 0, 1, 10);
        }
        if (resourceParameters.PageSize <= 0)
        {
            logger.LogWarning("PageSize must be greater than 0. Provided: {PageSize}", resourceParameters.PageSize);
            return new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>([], 0, 1, 10);
        }

        // Validate document existence
        ActionResult? documentValidation;
        try
        {
            documentValidation = await validationService.ValidateDocumentExistsAsync(documentId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating document existence for documentId: {DocumentId}", documentId);
            return new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>([], 0, 1, 10);
        }

        if (documentValidation != null)
        {
            logger.LogWarning("Document with ID: {DocumentId} does not exist.", documentId);
            return new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>([], 0, 1, 10);
        }

        try
        {
            // Base query: filter by documentId
            var query = context.MatterDocumentActivityUsersFrom
                .AsNoTracking()
                .Where(audit => audit.DocumentId == documentId);

            // Optional: Sorting
            if (!string.IsNullOrWhiteSpace(resourceParameters.OrderBy))
            {
                query = resourceParameters.OrderBy.Trim().ToLower() switch
                {
                    "createdat" => query.OrderByDescending(audit => audit.CreatedAt),
                    "activity" => query.OrderBy(audit => audit.MatterDocumentActivity != null ? audit.MatterDocumentActivity.Activity : string.Empty),
                    _ => query.OrderByDescending(audit => audit.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(audit => audit.CreatedAt);
            }

            // Project to minimal DTO
            var minimalQuery = query.Select(audit => new MatterDocumentActivityUserMinimalDto
            {
                MatterId = audit.MatterId,
                DocumentId = audit.DocumentId,
                MatterDocumentActivityId = audit.MatterDocumentActivityId,
                MatterDocumentActivity = audit.MatterDocumentActivity == null
                    ? null
                    : new MatterDocumentActivityMinimalDto
                    {
                        Id = audit.MatterDocumentActivity.Id,
                        Activity = audit.MatterDocumentActivity.Activity
                    },
                UserId = audit.UserId,
                User = audit.User == null
                    ? null
                    : new UserMinimalDto
                    {
                        Id = audit.User.Id,
                        Name = audit.User.Name
                    },
                CreatedAt = audit.CreatedAt
            });

            // Paging
            var pagedList = await Helpers.PagedList<MatterDocumentActivityUserMinimalDto>.CreateAsync(
                minimalQuery,
                resourceParameters.PageNumber,
                resourceParameters.PageSize);

            logger.LogInformation("Successfully fetched {Count} move/copy FROM audit records for DocumentId: {DocumentId}.", pagedList.Count, documentId);

            return pagedList;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching move/copy FROM audits for DocumentId: {DocumentId}.", documentId);
            return new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>([], 0, 1, 10);
        }
    }

    //Tests Created

    /// <summary>
    /// Retrieves a paginated list of "move/copy TO" audit records for a specified document,
    /// returning appropriate error responses for all error conditions.
    /// </summary>
    /// <param name="documentId">The ID of the document to retrieve move/copy TO audits for.</param>
    /// <param name="resourceParameters">Pagination and sorting information.</param>
    /// <returns>
    /// <para>A paged list of move/copy TO audit records if successful.</para>
    /// <para><see cref="BadRequestObjectResult"/> if the input is invalid.</para>
    /// <para><see cref="NotFoundObjectResult"/> if the document does not exist.</para>
    /// <para><see cref="ObjectResult"/> with status code 500 if an unexpected error occurs.</para>
    /// </returns>
    public async Task<Helpers.PagedList<MatterDocumentActivityUserMinimalDto>> GetPaginatedDocumentMoveToAuditsAsync(
        Guid documentId, DocumentAuditsResourceParameters resourceParameters)
    {
        logger.LogInformation("Fetching move/copy TO audits for DocumentId: {DocumentId} with parameters: {@ResourceParameters}", documentId, resourceParameters);

        // Validate input parameters
        if (documentId == Guid.Empty)
        {
            logger.LogWarning("Invalid documentId: {DocumentId}", documentId);
            return new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>([], 0, 1, 10);
        }
        if (resourceParameters == null)
        {
            logger.LogWarning("Resource parameters for move/copy TO audits are null.");
            return new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>([], 0, 1, 10);
        }
        if (resourceParameters.PageNumber <= 0)
        {
            logger.LogWarning("PageNumber must be greater than 0. Provided: {PageNumber}", resourceParameters.PageNumber);
            return new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>([], 0, 1, 10);
        }
        if (resourceParameters.PageSize <= 0)
        {
            logger.LogWarning("PageSize must be greater than 0. Provided: {PageSize}", resourceParameters.PageSize);
            return new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>([], 0, 1, 10);
        }

        // Validate document existence
        ActionResult? documentValidation;
        try
        {
            documentValidation = await validationService.ValidateDocumentExistsAsync(documentId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating document existence for documentId: {DocumentId}", documentId);
            return new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>([], 0, 1, 10);
        }

        if (documentValidation != null)
        {
            logger.LogWarning("Document with ID: {DocumentId} does not exist.", documentId);
            return new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>([], 0, 1, 10);
        }

        try
        {
            // Base query: filter by documentId
            var query = context.MatterDocumentActivityUsersTo
                .AsNoTracking()
                .Where(audit => audit.DocumentId == documentId);

            // Optional: Sorting
            if (!string.IsNullOrWhiteSpace(resourceParameters.OrderBy))
            {
                query = resourceParameters.OrderBy.Trim().ToLower() switch
                {
                    "createdat" => query.OrderByDescending(audit => audit.CreatedAt),
                    "activity" => query.OrderBy(audit => audit.MatterDocumentActivity != null ? audit.MatterDocumentActivity.Activity : string.Empty),
                    _ => query.OrderByDescending(audit => audit.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(audit => audit.CreatedAt);
            }

            // Project to minimal DTO
            var minimalQuery = query.Select(audit => new MatterDocumentActivityUserMinimalDto
            {
                MatterId = audit.MatterId,
                DocumentId = audit.DocumentId,
                MatterDocumentActivityId = audit.MatterDocumentActivityId,
                MatterDocumentActivity = audit.MatterDocumentActivity == null
                    ? null
                    : new MatterDocumentActivityMinimalDto
                    {
                        Id = audit.MatterDocumentActivity.Id,
                        Activity = audit.MatterDocumentActivity.Activity
                    },
                UserId = audit.UserId,
                User = audit.User == null
                    ? null
                    : new UserMinimalDto
                    {
                        Id = audit.User.Id,
                        Name = audit.User.Name
                    },
                CreatedAt = audit.CreatedAt
            });

            // Paging
            var pagedList = await Helpers.PagedList<MatterDocumentActivityUserMinimalDto>.CreateAsync(
                minimalQuery,
                resourceParameters.PageNumber,
                resourceParameters.PageSize);

            logger.LogInformation("Successfully fetched {Count} move/copy TO audit records for DocumentId: {DocumentId}.", pagedList.Count, documentId);

            return pagedList;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching move/copy TO audits for DocumentId: {DocumentId}.", documentId);
            return new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>([], 0, 1, 10);
        }
    }

    #endregion Document Activity

    #region Matters

    //Tests Created

    /// <summary>
    /// Adds a new matter to the repository and logs the creation as an audit activity.
    /// Returns appropriate error responses for all error conditions.
    /// </summary>
    /// <param name="matter">The matter to be added.</param>
    /// <returns>
    /// <para><see cref="OkObjectResult"/> with the created matter if successful.</para>
    /// <para><see cref="BadRequestObjectResult"/> if the input is invalid.</para>
    /// <para><see cref="ConflictObjectResult"/> if a matter with the same description already exists.</para>
    /// <para><see cref="ObjectResult"/> with status code 500 if an unexpected error occurs.</para>
    /// </returns>
    public async Task<ActionResult<Matter>> AddMatterAsync(MatterForCreationDto matter)
    {
        // Validate input
        if (!validationService.ValidateStringNotEmpty(matter.Description, nameof(matter.Description)))
        {
            logger.LogWarning("Matter description cannot be null or empty.");
            return new BadRequestObjectResult("Matter description cannot be null or empty.");
        }

        // Validate model
        var validationResults = MatterForCreationDto.ValidateModel(matter);
        if (validationResults is { Count: > 0 })
        {
            logger.LogWarning("Matter DTO validation failed: {ValidationErrors}", string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            return new BadRequestObjectResult(validationResults.Select(v => v.ErrorMessage));
        }

        try
        {
            var matterNameExists = await MatterNameExistsAsync(matter.Description);
            // Check if a matter with the same description already exists
            if (matterNameExists.Value)
            {
                logger.LogWarning("A matter with the description '{Description}' already exists.", matter.Description);
                return new ConflictObjectResult($"A matter with the description '{matter.Description}' already exists.");
            }

            // Retrieve the current user and the "CREATED" matter activity
            var user = await GetUserAsync(UserName);
            var matterActivity = await context.MatterActivities.AsNoTracking()
                .FirstOrDefaultAsync(activity => activity.Activity == "CREATED");

            if (user == null || matterActivity == null)
            {
                logger.LogWarning("User or MatterActivity not found. User: {UserName}, Activity: CREATED", UserName);
                return new ObjectResult("User or MatterActivity not found.") { StatusCode = 500 };
            }

            // Map the DTO to the Matter entity and add it to the context
            var newMatter = mapper.Map<Matter>(matter);
            var matterAdditionResult = await context.Matters.AddAsync(newMatter);

            if (matterAdditionResult.State != EntityState.Added)
            {
                logger.LogWarning("Failed to add new matter {matterDescription}.", matter.Description);
                return new ObjectResult("Could not save Matter.") { StatusCode = 500 };
            }

            var createdMatter = matterAdditionResult.Entity;

            var matterSaved = await SaveChangesAsync();
            if (!matterSaved)
            {
                logger.LogWarning("Failed to save new matter {matterDescription}.", matter.Description);
                return new ObjectResult("Could not save Matter.") { StatusCode = 500 };
            }

            await AddAuditLogAsync(
                entity: createdMatter,
                entityId: createdMatter.Id,
                activity: matterActivity,
                activityId: matterActivity.Id,
                user: user,
                userId: user.Id
            );

            logger.LogInformation("Successfully added a new matter with ID: {MatterId} and description: {Description}.",
                createdMatter.Id, createdMatter.Description);

            var result = await context.Matters
                .Include(m => m.MatterActivityUsers)
                .Include(m => m.MatterDocumentActivityUsersFrom)
                .Include(m => m.MatterDocumentActivityUsersTo)
                .SingleOrDefaultAsync(m => m.Id == createdMatter.Id);

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding matter with description: {Description}", matter.Description);
            return new ObjectResult("An unexpected error occurred. See logs for details.") { StatusCode = 500 };
        }
    }

    //Tests Created

    /// <summary>
    /// Checks if a matter exists in the database, using centralized validation and consistent logging.
    /// Returns appropriate error responses for all error conditions.
    /// </summary>
    /// <param name="matterId">The ID of the matter to check.</param>
    /// <returns>
    /// <para><see cref="OkObjectResult"/> with true if the matter exists, false otherwise.</para>
    /// <para><see cref="BadRequestObjectResult"/> if the input is invalid.</para>
    /// <para><see cref="ObjectResult"/> with status code 500 if an unexpected error occurs.</para>
    /// </returns>
    public async Task<ActionResult<bool>> MatterExistsAsync(Guid matterId)
    {
        // Validate input
        if (matterId == Guid.Empty)
        {
            logger.LogWarning("Invalid matterId: {MatterId}", matterId);
            return new BadRequestObjectResult($"Invalid matterId: {matterId}");
        }

        try
        {
            var exists = await context.Matters
                .AsNoTracking()
                .AnyAsync(matter => matter.Id == matterId);

            if (exists)
            {
                logger.LogInformation("Matter with ID: {MatterId} exists in the database.", matterId);
            }
            else
            {
                logger.LogWarning("Matter with ID: {MatterId} does not exist in the database.", matterId);
            }

            return new OkObjectResult(exists);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if matter exists with ID: {MatterId}", matterId);
            return new ObjectResult("An unexpected error occurred. See logs for details.")
            {
                StatusCode = 500
            };
        }
    }

    //Tests Created

    /// <summary>
    /// Checks if a matter with the specified name exists in the database, using centralized validation and consistent logging.
    /// Returns appropriate error responses for all error conditions.
    /// </summary>
    /// <param name="matterName">The name of the matter to check.</param>
    /// <returns>
    /// <para><see cref="OkObjectResult"/> with true if a matter with the specified name exists, false otherwise.</para>
    /// <para><see cref="BadRequestObjectResult"/> if the input is invalid.</para>
    /// <para><see cref="ObjectResult"/> with status code 500 if an unexpected error occurs.</para>
    /// </returns>
    public async Task<ActionResult<bool>> MatterNameExistsAsync(string matterName)
    {
        // Validate input
        if (!validationService.ValidateStringNotEmpty(matterName, nameof(matterName)))
        {
            logger.LogWarning("Matter name cannot be null or empty.");
            return new BadRequestObjectResult("Matter name cannot be null or empty.");
        }

        try
        {
            var exists = await context.Matters
                .AsNoTracking()
                .AnyAsync(matter => matter.Description == matterName);

            if (exists)
            {
                logger.LogInformation("A matter with the name '{MatterName}' exists in the database.", matterName);
            }
            else
            {
                logger.LogInformation("No matter with the name '{MatterName}' exists in the database.", matterName);
            }

            return new OkObjectResult(exists);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if a matter with the name '{MatterName}' exists.", matterName);
            return new ObjectResult("An unexpected error occurred. See logs for details.") { StatusCode = 500 };
        }
    }

    //Tests Created

    /// <summary>
    /// Deletes a specified matter by marking it as deleted and logs the deletion as an audit activity.
    /// Returns <c>true</c> if the matter was successfully deleted, <c>false</c> otherwise.
    /// All error conditions are logged; no exceptions are thrown.
    /// </summary>
    /// <param name="matterToDelete">The matter to be deleted.</param>
    /// <returns>
    /// <c>true</c> if the matter was successfully deleted; <c>false</c> if validation fails, the matter does not exist, or an error occurs.
    /// </returns>
    public async Task<bool> DeleteMatterAsync(MatterDto matterToDelete)
    {
        // Validate input
        if (validationService.ValidateNotNull(matterToDelete, nameof(matterToDelete)) != null)
        {
            logger.LogWarning("Matter to delete is null.");
            return false;
        }

        var matterId = matterToDelete.Id;
        var matterValidation = await validationService.ValidateMatterExistsAsync(matterId);
        if (matterValidation != null)
        {
            logger.LogWarning("Matter with ID: {MatterId} does not exist.", matterId);
            return false;
        }

        try
        {
            // Retrieve the matter from the database
            var dbMatter = await context.Matters.SingleOrDefaultAsync(matter => matter.Id == matterId);
            if (dbMatter == null)
            {
                logger.LogWarning("Matter not found with ID: {MatterId}", matterId);
                return false;
            }

            // Mark the matter as deleted
            dbMatter.IsDeleted = true;
            context.Matters.Update(dbMatter);

            // Retrieve the current user and the "DELETED" matter activity
            var user = await GetUserAsync(UserName);
            var matterActivity = await context.MatterActivities.AsNoTracking()
                .FirstOrDefaultAsync(activity => activity.Activity == "DELETED");

            if (user == null || matterActivity == null)
            {
                logger.LogWarning("User or MatterActivity not found. User: {UserName}, Activity: DELETED", UserName);
                return false;
            }

            await AddAuditLogAsync(
                entity: dbMatter,
                entityId: dbMatter.Id,
                activity: matterActivity,
                activityId: matterActivity.Id,
                user: user,
                userId: user.Id
            );

            // Save changes and return the result
            var saved = await SaveChangesAsync();
            if (saved)
            {
                logger.LogInformation("Successfully deleted matter with ID: {MatterId}.", matterId);
                return true;
            }
            else
            {
                logger.LogWarning("Failed to save changes after deleting matter with ID: {MatterId}.", matterId);
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting matter with ID: {MatterId}", matterId);
            return false;
        }
    }

    //Tests Created

    /// <summary>
    /// Retrieves a paginated list of matters based on the specified resource parameters,
    /// using centralized validation and consistent logging. Returns an empty paged list if validation fails or an error occurs.
    /// </summary>
    /// <param name="resourceParameters">The parameters for pagination, filtering, and sorting.</param>
    /// <returns>
    /// A paginated list of matters. If validation fails or an error occurs, returns an empty paged list.
    /// </returns>
    public async Task<Helpers.PagedList<Matter>> GetPaginatedMattersAsync(MattersResourceParameters? resourceParameters)
    {
        // Centralized validation using the validation service
        if (validationService.ValidateNotNull(resourceParameters, nameof(resourceParameters)) != null)
        {
            logger.LogWarning("Resource parameters for paginated matters are null.");
            return new Helpers.PagedList<Matter>([], 0, 1, 10);
        }
        switch (resourceParameters)
        {
            case { PageNumber: <= 0 }:
                logger.LogWarning("PageNumber must be greater than 0. Provided: {PageNumber}", resourceParameters.PageNumber);
                return new Helpers.PagedList<Matter>([], 0, 1, 10);
            case { PageSize: <= 0 }:
                logger.LogWarning("PageSize must be greater than 0. Provided: {PageSize}", resourceParameters.PageSize);
                return new Helpers.PagedList<Matter>([], 0, 1, 10);
            default:
                try
                {
                    // Start building the query and include extended data
                    IQueryable<Matter> query = context.Matters
                        .AsNoTracking()
                        .Include(matter => matter.MatterActivityUsers)
                        .Include(matter => matter.MatterDocumentActivityUsersFrom)
                        .Include(matter => matter.MatterDocumentActivityUsersTo);

                    // Filter by description if provided
                    if (!string.IsNullOrWhiteSpace(resourceParameters?.Description))
                    {
                        var desc = resourceParameters.Description.Trim();
                        query = query.Where(matter => matter.Description.Contains(desc));
                    }

                    // Filter by archived
                    if (resourceParameters is { IsArchived: false })
                        query = query.Where(matter => !matter.IsArchived);

                    // Filter by deleted
                    if (resourceParameters is { IsDeleted: false })
                        query = query.Where(matter => !matter.IsDeleted);

                    // Sorting
                    query = !string.IsNullOrWhiteSpace(resourceParameters?.OrderBy)
                        ? query.ApplySort(resourceParameters.OrderBy, propertyMappingService.GetPropertyMapping<MatterDto, Matter>())
                        : query.OrderBy(matter => matter.Description);

                    // Paging
                    if (resourceParameters != null)
                    {
                        var pagedList = await Helpers.PagedList<Matter>.CreateAsync(
                            query,
                            resourceParameters.PageNumber,
                            resourceParameters.PageSize
                        );

                        logger.LogInformation("Paginated matters retrieved. Page: {PageNumber}, PageSize: {PageSize}, TotalCount: {TotalCount}",
                            pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount);

                        return pagedList;
                    }
                }
                catch (Exception ex)
                {
                    if (resourceParameters == null)
                        return new Helpers.PagedList<Matter>([], 0, resourceParameters!.PageNumber, resourceParameters.PageSize);
                    logger.LogError(ex, "Error retrieving paginated matters. Page: {PageNumber}, PageSize: {PageSize}",
                        resourceParameters.PageNumber, resourceParameters.PageSize);
                    return new Helpers.PagedList<Matter>([], 0, resourceParameters.PageNumber, resourceParameters.PageSize);
                }

                break;
        }
        return new Helpers.PagedList<Matter>([], 0, 1, 10);
    }

    //Tests Created

    /// <summary>
    /// Retrieves a matter by its ID, optionally including documents and history, using centralized validation and consistent logging.
    /// Returns <c>null</c> if the matter does not exist, the ID is invalid, or an error occurs.
    /// </summary>
    /// <param name="matterId">The ID of the matter to retrieve.</param>
    /// <param name="includeDocuments">Whether to include documents in the result.</param>
    /// <param name="includeHistory">Whether to include history in the result.</param>
    /// <returns>
    /// The requested <see cref="Matter"/>, or <c>null</c> if not found or if an error occurs.
    /// </returns>
    public async Task<Matter?> GetMatterAsync(Guid matterId, bool includeDocuments, bool includeHistory = false)
    {
        // Validate input
        if (matterId == Guid.Empty)
        {
            logger.LogWarning("Invalid matterId: {MatterId}", matterId);
            return null;
        }

        var matterValidation = await validationService.ValidateMatterExistsAsync(matterId);
        if (matterValidation != null)
        {
            logger.LogWarning("Matter with ID: {MatterId} does not exist.", matterId);
            return null;
        }

        try
        {
            // Start building the query
            var query = context.Matters.AsNoTracking().Where(matter => matter.Id == matterId);

            // Include documents if requested
            if (includeDocuments)
            {
                query = query.Include(matter => matter.Documents.OrderBy(document => document.FileName));
            }

            // Include history if requested
            if (includeHistory)
            {
                query = query
                    .Include(matter => matter.MatterActivityUsers)
                    .ThenInclude(mau => mau.MatterActivity)
                    .Include(matter => matter.MatterActivityUsers)
                    .ThenInclude(mau => mau.User)
                    .Include(matter => matter.MatterDocumentActivityUsersFrom)
                    .ThenInclude(mduf => mduf.MatterDocumentActivity)
                    .Include(matter => matter.MatterDocumentActivityUsersFrom)
                    .ThenInclude(mduf => mduf.User)
                    .Include(matter => matter.MatterDocumentActivityUsersTo)
                    .ThenInclude(mdut => mdut.MatterDocumentActivity)
                    .Include(matter => matter.MatterDocumentActivityUsersTo)
                    .ThenInclude(mdut => mdut.User);
            }

            // Execute the query and return the result
            var result = await query.AsSplitQuery().SingleOrDefaultAsync();

            if (result == null)
            {
                logger.LogWarning("No matter found with ID: {MatterId}", matterId);
                return null;
            }

            logger.LogInformation("Successfully retrieved matter with ID: {MatterId}", matterId);

            // Audit the view action
            var user = await GetUserAsync(UserName);
            var matterActivity = await context.MatterActivities.AsNoTracking()
                .FirstOrDefaultAsync(activity => activity.Activity == "VIEWED");

            if (user == null || matterActivity == null)
            {
                logger.LogWarning("User or MatterActivity not found. User: {UserName}, Activity: VIEWED", UserName);
                return result;
            }

            await AddAuditLogAsync(
                entity: result,
                entityId: result.Id,
                activity: matterActivity,
                activityId: matterActivity.Id,
                user: user,
                userId: user.Id
            );

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving matter with ID: {MatterId}", matterId);
            return null;
        }
    }

    //Tests Created

    /// <summary>
    /// Restores a deleted matter by marking it as not deleted and logs the restoration as an audit activity.
    /// Returns <c>true</c> if the matter was successfully restored; <c>false</c> if validation fails, the matter does not exist, or an error occurs.
    /// All error conditions are logged; no exceptions are thrown.
    /// </summary>
    /// <param name="matterId">The ID of the matter to restore.</param>
    /// <returns>
    /// <c>true</c> if the matter was successfully restored; <c>false</c> if validation fails, the matter does not exist, or an error occurs.
    /// </returns>
    public async Task<bool> RestoreMatterAsync(Guid matterId)
    {
        // Validate input
        if (matterId == Guid.Empty)
        {
            logger.LogWarning("Invalid matterId: {MatterId}", matterId);
            return false;
        }

        var matterValidation = await validationService.ValidateMatterExistsAsync(matterId);
        if (matterValidation != null)
        {
            logger.LogWarning("Matter with ID: {MatterId} does not exist.", matterId);
            return false;
        }

        try
        {
            // Retrieve the matter from the database
            var dbMatter = await context.Matters.SingleOrDefaultAsync(m => m.Id == matterId);
            if (dbMatter == null)
            {
                logger.LogWarning("Matter not found with ID: {MatterId}", matterId);
                return false;
            }

            // Mark the matter as not deleted
            dbMatter.IsDeleted = false;
            context.Matters.Update(dbMatter);

            // Retrieve the current user and the "RESTORED" matter activity
            var user = await GetUserAsync(UserName);
            var matterActivity = await context.MatterActivities.AsNoTracking()
                .FirstOrDefaultAsync(activity => activity.Activity == "RESTORED");

            if (user == null || matterActivity == null)
            {
                logger.LogWarning("User or MatterActivity not found. User: {UserName}, Activity: RESTORED", UserName);
                return false;
            }

            await AddAuditLogAsync(
                entity: dbMatter,
                entityId: dbMatter.Id,
                activity: matterActivity,
                activityId: matterActivity.Id,
                user: user,
                userId: user.Id
            );

            // Save changes and return the result
            var saved = await SaveChangesAsync();
            if (saved)
            {
                logger.LogInformation("Successfully restored matter with ID: {MatterId}.", matterId);
                return true;
            }
            else
            {
                logger.LogWarning("Failed to save changes after restoring matter with ID: {MatterId}.", matterId);
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error restoring matter with ID: {MatterId}", matterId);
            return false;
        }
    }

    //Tests Created

    /// <summary>
    /// Updates a specified matter with new data and logs the update as an audit activity.
    /// Returns the updated <see cref="Matter"/> if successful; otherwise, returns <c>null</c>.
    /// All error conditions are logged; no exceptions are thrown.
    /// </summary>
    /// <param name="matterId">The ID of the matter to update.</param>
    /// <param name="matterToUpdate">The updated matter data.</param>
    /// <returns>
    /// The updated <see cref="Matter"/> if the operation succeeds; <c>null</c> if validation fails, the matter does not exist, or an error occurs.
    /// </returns>
    public async Task<Matter?> UpdateMatterAsync(Guid matterId, MatterForUpdateDto? matterToUpdate)
    {
        // Validate input
        if (matterToUpdate == null)
        {
            logger.LogWarning("MatterForUpdateDto is null.");
            return null;
        }

        // Validate model using static method if available
        var validationResults = MatterForUpdateDto.ValidateModel(matterToUpdate);
        if (validationResults is { Count: > 0 })
        {
            logger.LogWarning("MatterForUpdateDto validation failed: {ValidationErrors}", string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            return null;
        }

        // Validate matter existence
        var matterValidation = await validationService.ValidateMatterExistsAsync(matterId);
        if (matterValidation != null)
        {
            logger.LogWarning("Matter with ID: {MatterId} does not exist.", matterId);
            return null;
        }

        try
        {
            // Retrieve the matter from the database
            var dbMatter = await context.Matters.SingleOrDefaultAsync(m => m.Id == matterId);
            if (dbMatter == null)
            {
                logger.LogWarning("Matter not found with ID: {MatterId}", matterId);
                return null;
            }

            // Map the updated fields from the DTO to the entity
            mapper.Map(matterToUpdate, dbMatter);

            // Retrieve the current user and the "SAVED" matter activity
            var user = await GetUserAsync(UserName);
            var matterActivity = await context.MatterActivities.AsNoTracking()
                .FirstOrDefaultAsync(activity => activity.Activity == "SAVED");

            if (user == null || matterActivity == null)
            {
                logger.LogWarning("User or MatterActivity not found. User: {UserName}, Activity: SAVED", UserName);
                return null;
            }

            // Add audit log
            await AddAuditLogAsync(
                entity: dbMatter,
                entityId: dbMatter.Id,
                activity: matterActivity,
                activityId: matterActivity.Id,
                user: user,
                userId: user.Id
            );

            // Save changes and return the updated matter
            var saved = await SaveChangesAsync();
            if (saved)
            {
                logger.LogInformation("Successfully updated matter with ID: {MatterId}.", matterId);
                return dbMatter;
            }
            else
            {
                logger.LogWarning("Failed to save changes after updating matter with ID: {MatterId}.", matterId);
                return null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating matter with ID: {MatterId}", matterId);
            return null;
        }
    }

    #endregion Matters

    #region Revisions

    //Tests Created

    /// <summary>
    /// Adds a new revision to a specified document and logs the addition as an audit activity.
    /// Centralizes all validation using the <see cref="IValidationService"/>.
    /// Returns <c>null</c> if validation fails or an error occurs.
    /// </summary>
    /// <param name="documentId">The ID of the document to add the revision to.</param>
    /// <param name="revision">The revision to add.</param>
    /// <returns>The created revision, or <c>null</c> if the operation fails.</returns>
    public async Task<Revision?> AddRevisionAsync(Guid documentId, RevisionDto revision)
    {
        // Validate document existence
        var documentValidation = await validationService.ValidateDocumentExistsAsync(documentId);
        if (documentValidation != null)
        {
            logger.LogWarning("Document with ID: {DocumentId} does not exist.", documentId);
            return null;
        }

        // Validate revision DTO is not null
        if (validationService.ValidateNotNull(revision, nameof(revision)) != null)
        {
            logger.LogWarning("Revision DTO is null or invalid.");
            return null;
        }

        // Validate revision DTO model
        var validationResults = RevisionDto.ValidateModel(revision);
        if (validationResults is { Count: > 0 })
        {
            logger.LogWarning("Revision DTO validation failed: {ValidationErrors}", string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            return null;
        }

        try
        {
            var newRevision = await CreateRevisionAsync(documentId, revision);
            if (newRevision == null)
            {
                logger.LogWarning("Failed to create a new revision for DocumentId: {DocumentId}", documentId);
                return null;
            }

            var user = await GetUserAsync(UserName);
            var revisionActivity = await GetRevisionActivityByActivityNameAsync("CREATED");

            if (user == null || revisionActivity == null)
            {
                logger.LogWarning("User or RevisionActivity not found. User: {UserName}, Activity: CREATED", UserName);
                return null;
            }

            await AddAuditLogAsync(
                entity: newRevision,
                entityId: newRevision.Id,
                activity: revisionActivity,
                activityId: revisionActivity.Id,
                user: user,
                userId: user.Id
            );

            // Save changes and return the created revision
            var saved = await SaveChangesAsync();
            if (saved)
            {
                logger.LogInformation("Successfully added revision with ID: {RevisionNumber} to document with ID: {DocumentId}.",
                    newRevision.Id, documentId);
                return newRevision;
            }
            else
            {
                logger.LogWarning("Failed to save new revision for document {DocumentId}.", documentId);
                return null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding revision to document with ID: {DocumentId}", documentId);
            return null;
        }
    }

    //Tests Created

    /// <summary>
    /// Checks if a revision exists in the database, using centralized validation and consistent logging.
    /// </summary>
    /// <param name="revisionId">The ID of the revision to check.</param>
    /// <returns>True if the revision exists, false otherwise.</returns>
    public async Task<bool> RevisionExistsAsync(Guid revisionId)
    {
        // Centralized validation using the validation service
        var validationResult = validationService.ValidateGuid(revisionId, nameof(revisionId));
        if (validationResult != null)
        {
            logger.LogWarning("Invalid revisionId: {revisionId}", nameof(revisionId));
            return false;
        }

        try
        {
            var exists = await context.Revisions
                .AsNoTracking()
                .AnyAsync(revision => revision.Id == revisionId);

            if (exists)
            {
                logger.LogInformation("Revision with ID: {RevisionNumber} exists in the database.", revisionId);
            }
            else
            {
                logger.LogWarning("Revision with ID: {RevisionNumber} does not exist in the database.", revisionId);
            }

            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if revision exists with ID: {RevisionNumber}", revisionId);
            return false;
        }
    }

    //Tests Created

    /// <summary>
    /// Deletes a specified revision by marking it as deleted and logs the deletion as an audit activity.
    /// Centralizes all validation using the <see cref="IValidationService"/>.
    /// </summary>
    /// <param name="revision">The revision to be deleted.</param>
    /// <returns>True if the revision was successfully deleted, false otherwise.</returns>
    public async Task<bool> DeleteRevisionAsync(RevisionDto revision)
    {
        // Centralized validation using the validation service
        if (validationService.ValidateNotNull(revision, nameof(revision)) != null)
        {
            logger.LogWarning("revision must be provided.");
            return false;
        }

        if (!revision.Id.HasValue)
        {
            logger.LogWarning("Revision ID must be provided.");
            return false;

        }

        var revisionId = revision.Id.Value;
        var revisionValidation = await validationService.ValidateRevisionExistsAsync(revisionId);
        if (revisionValidation != null)
        {
            logger.LogWarning("Revision with ID {revisionId} does not exist.", nameof(revision));
            return false;
        }

        try
        {
            // Retrieve the revision from the database
            var dbRevision = await context.Revisions.SingleOrDefaultAsync(r => r.Id == revisionId);
            if (dbRevision == null)
            {
                logger.LogWarning("Revision not found with ID: {RevisionNumber}", revisionId);
                return false;
            }

            // Mark the revision as deleted
            dbRevision.IsDeleted = true;
            context.Revisions.Update(dbRevision);

            // Retrieve the current user and the "DELETED" revision activity
            var user = await GetUserAsync(UserName);
            var revisionActivity = await GetRevisionActivityByActivityNameAsync("DELETED");

            if (user == null || revisionActivity == null)
            {
                logger.LogWarning("User or RevisionActivity not found. User: {UserName}, Activity: DELETED", UserName);
                return false;
            }

            // Add audit log
            await AddAuditLogAsync(
                entity: dbRevision,
                entityId: dbRevision.Id,
                activity: revisionActivity,
                activityId: revisionActivity.Id,
                user: user,
                userId: user.Id
            );

            // Save changes and return the result
            var saved = await SaveChangesAsync();
            if (saved)
            {
                logger.LogInformation("Successfully deleted revision with ID: {RevisionNumber}.", revisionId);
                return true;
            }
            else
            {
                logger.LogWarning("Failed to save changes after deleting revision with ID: {RevisionNumber}.", revisionId);
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting revision with ID: {RevisionNumber}", revisionId);
            return false;
        }
    }

    //Tests Created

    /// <summary>
    /// Retrieves a revision by its ID, optionally including its history, using centralized validation and consistent logging.
    /// </summary>
    /// <param name="revisionId">The ID of the revision to retrieve.</param>
    /// <param name="includeHistory">Whether to include history in the result. Default is false.</param>
    /// <returns>The requested revision, or null if not found.</returns>
    public async Task<Revision?> GetRevisionByIdAsync(Guid revisionId, bool includeHistory = false)
    {
        // Centralized validation using the validation service
        var revisionValidation = await validationService.ValidateRevisionExistsAsync(revisionId);
        if (revisionValidation != null)
        {
            logger.LogWarning("Revision with ID: {RevisionNumber} does not exist.", revisionId);
            return null;
        }

        try
        {
            // Base query
            var query = context.Revisions.AsNoTracking().Where(revision => revision.Id == revisionId);

            // Include history if requested
            if (includeHistory)
            {
                query = query
                    .Include(revision => revision.RevisionActivityUsers)
                        .ThenInclude(activityUser => activityUser.RevisionActivity)
                    .Include(revision => revision.RevisionActivityUsers)
                        .ThenInclude(activityUser => activityUser.User);
            }

            // Execute the query and return the result
            var result = await query.SingleOrDefaultAsync();

            if (result == null)
            {
                logger.LogWarning("No revision found with ID: {RevisionNumber}", revisionId);
            }
            else
            {
                logger.LogInformation("Successfully retrieved revision with ID: {RevisionNumber}", revisionId);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving revision with ID: {RevisionNumber}", revisionId);
            return null;
        }
    }

    //Tests Created

    /// <summary>
    /// Retrieves a list of revisions for a specified document, optionally including deleted revisions,
    /// and returns an appropriate error response for all error conditions.
    /// </summary>
    /// <param name="documentId">The ID of the document to retrieve revisions for.</param>
    /// <param name="includeDeleted">Whether to include deleted revisions in the result.</param>
    /// <param name="orderBy">Optional order by clause.</param>
    /// <returns>
    /// <para>
    /// <see cref="OkObjectResult"/> with a queryable list of revisions for the specified document if successful.
    /// </para>
    /// <para>
    /// <see cref="BadRequestObjectResult"/> if the documentId is invalid.
    /// </para>
    /// <para>
    /// <see cref="NotFoundObjectResult"/> if the document does not exist.
    /// </para>
    /// <para>
    /// <see cref="ObjectResult"/> with status code 500 if an unexpected error occurs.
    /// </para>
    /// </returns>
    /// <remarks>
    /// This method uses centralized validation and consistent logging.
    /// </remarks>
    public async Task<ActionResult<IQueryable<Revision>>> GetRevisionsAsync(
        Guid documentId,
        bool includeDeleted,
        string? orderBy = null)
    {
        // Validate documentId
        var validationResult = validationService.ValidateGuid(documentId, nameof(documentId));
        if (validationResult != null)
        {
            logger.LogWarning("Invalid documentId: {DocumentId}", documentId);
            return new BadRequestObjectResult($"Invalid documentId: {documentId}");
        }

        // Validate document existence
        ActionResult? documentValidation;
        try
        {
            documentValidation = await validationService.ValidateDocumentExistsAsync(documentId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating document existence for documentId: {DocumentId}", documentId);
            return new ObjectResult("An error occurred while validating document existence.")
            {
                StatusCode = 500
            };
        }

        if (documentValidation != null)
        {
            logger.LogWarning("Document with ID: {DocumentId} does not exist.", documentId);
            return new NotFoundObjectResult($"Document with ID {documentId} does not exist.");
        }

        try
        {
            var query = context.Revisions
                .Where(r => r.DocumentId == documentId && (includeDeleted || !r.IsDeleted));

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                query = query.ApplySort(orderBy, propertyMappingService.GetPropertyMapping<RevisionDto, Revision>());
            }

            logger.LogInformation("Retrieved revisions for DocumentId: {DocumentId} (IncludeDeleted: {IncludeDeleted}, OrderBy: {OrderBy})",
                documentId, includeDeleted, orderBy);

            return new OkObjectResult(query);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request.");
            return new ObjectResult("An unexpected error occurred. See logs for details.")
            {
                StatusCode = 500
            };
        }
    }

    //Tests Created

    /// <summary>
    /// Retrieves a paginated list of revisions for a specified document,
    /// returning appropriate HTTP responses for all error conditions.
    /// </summary>
    /// <param name="documentId">The ID of the document containing the revisions.</param>
    /// <param name="resourceParameters">Parameters for pagination and sorting.</param>
    /// <returns>
    /// <para>
    /// <see cref="OkObjectResult"/> with a paginated list of revisions if successful.
    /// </para>
    /// <para>
    /// <see cref="BadRequestObjectResult"/> if the input is invalid.
    /// </para>
    /// <para>
    /// <see cref="NotFoundObjectResult"/> if the document does not exist.
    /// </para>
    /// <para>
    /// <see cref="ObjectResult"/> with status code 500 if an unexpected error occurs.
    /// </para>
    /// </returns>
    /// <remarks>
    /// This method uses centralized validation and consistent logging.
    /// </remarks>
    public async Task<ActionResult<Helpers.PagedList<Revision>>> GetPaginatedRevisionsAsync(
        Guid documentId,
        RevisionsResourceParameters resourceParameters)
    {
        // Validate input parameters
        if (documentId == Guid.Empty)
        {
            logger.LogWarning("Invalid documentId: {DocumentId}", documentId);
            return new BadRequestObjectResult($"Invalid documentId: {documentId}");
        }
        if (resourceParameters == null)
        {
            logger.LogWarning("Resource parameters for paginated revisions are null.");
            return new BadRequestObjectResult("Resource parameters must be provided.");
        }
        if (resourceParameters.PageNumber <= 0)
        {
            logger.LogWarning("PageNumber must be greater than 0. Provided: {PageNumber}", resourceParameters.PageNumber);
            return new BadRequestObjectResult("PageNumber must be greater than 0.");
        }
        if (resourceParameters.PageSize <= 0)
        {
            logger.LogWarning("PageSize must be greater than 0. Provided: {PageSize}", resourceParameters.PageSize);
            return new BadRequestObjectResult("PageSize must be greater than 0.");
        }

        // Validate document existence
        ActionResult? documentValidation;
        try
        {
            documentValidation = await validationService.ValidateDocumentExistsAsync(documentId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating document existence for documentId: {DocumentId}", documentId);
            return new ObjectResult("An error occurred while validating document existence.")
            {
                StatusCode = 500
            };
        }

        if (documentValidation != null)
        {
            logger.LogWarning("Document with ID: {DocumentId} does not exist.", documentId);
            return new NotFoundObjectResult($"Document with ID {documentId} does not exist.");
        }

        try
        {
            var query = context.Revisions.Where(r => r.DocumentId == documentId);

            if (!resourceParameters.IncludeDeleted)
                query = query.Where(r => !r.IsDeleted);

            // Sorting
            if (!string.IsNullOrWhiteSpace(resourceParameters.OrderBy))
                query = query.ApplySort(resourceParameters.OrderBy, propertyMappingService.GetPropertyMapping<RevisionDto, Revision>());

            // Paging
            var pagedList = await Helpers.PagedList<Revision>.CreateAsync(query, resourceParameters.PageNumber, resourceParameters.PageSize);

            logger.LogInformation("Paginated revisions retrieved for DocumentId: {DocumentId}, Page: {PageNumber}, PageSize: {PageSize}, TotalCount: {TotalCount}",
                documentId, pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount);

            return new OkObjectResult(pagedList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving paginated revisions for DocumentId: {DocumentId}", documentId);
            return new ObjectResult("An unexpected error occurred. See logs for details.")
            {
                StatusCode = 500
            };
        }
    }

    #endregion Revisions
}