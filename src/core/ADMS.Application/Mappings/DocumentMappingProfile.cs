using AutoMapper;
using ADMS.Application.DTOs;
using ADMS.Domain.Entities;

namespace ADMS.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping between Document entities and DTOs.
/// </summary>
public class DocumentMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentMappingProfile"/> class.
    /// </summary>
    public DocumentMappingProfile()
    {
        CreateMap<Document, DocumentDto>()
            .ForMember(dest => dest.FullFileName, opt => opt.MapFrom(src => $"{src.FileName}.{src.Extension}"))
            .ForMember(dest => dest.CurrentRevision, opt => opt.MapFrom(src =>
                src.Revisions.OrderByDescending(r => r.RevisionNumber).FirstOrDefault()))
            .ForMember(dest => dest.RevisionCount, opt => opt.MapFrom(src => src.Revisions.Count))
            .ForMember(dest => dest.TotalActivityCount, opt => opt.MapFrom(src =>
                src.DocumentActivityUsers.Count +
                src.MatterDocumentActivityUsersFrom.Count +
                src.MatterDocumentActivityUsersTo.Count))
            .ForMember(dest => dest.IsAvailableForEdit, opt => opt.MapFrom(src => !src.IsCheckedOut && !src.IsDeleted))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                src.IsDeleted ? "Deleted" :
                src.IsCheckedOut ? "Checked Out" : "Available"))
            .ForMember(dest => dest.FormattedFileSize, opt => opt.MapFrom(src => FormatFileSize(src.FileSize)));

        CreateMap<DocumentDto, Document>()
            .ForMember(dest => dest.Revisions, opt => opt.Ignore())
            .ForMember(dest => dest.DocumentActivityUsers, opt => opt.Ignore())
            .ForMember(dest => dest.MatterDocumentActivityUsersFrom, opt => opt.Ignore())
            .ForMember(dest => dest.MatterDocumentActivityUsersTo, opt => opt.Ignore());

        CreateMap<DocumentActivity, DocumentActivityDto>()
            .ForMember(dest => dest.UsageCount, opt => opt.MapFrom(src => src.DocumentActivityUsers.Count))
            .ForMember(dest => dest.UniqueUserCount, opt => opt.MapFrom(src =>
                src.DocumentActivityUsers.Select(u => u.UserId).Distinct().Count()))
            .ForMember(dest => dest.NormalizedActivity, opt => opt.MapFrom(src => src.Activity.ToUpperInvariant().Trim()));

        CreateMap<DocumentActivityDto, DocumentActivity>()
            .ForMember(dest => dest.DocumentActivityUsers, opt => opt.Ignore());

        CreateMap<DocumentActivityUser, DocumentActivityUserDto>()
            .ForMember(dest => dest.ActivitySummary, opt => opt.MapFrom(src =>
                $"{src.Document.FileName}.{src.Document.Extension} {src.DocumentActivity.Activity} by {src.User.Name}"))
            .ForMember(dest => dest.FormattedTimestamp, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")))
            .ForMember(dest => dest.IsVersionControlActivity, opt => opt.MapFrom(src =>
                src.DocumentActivity.Activity.ToUpperInvariant() == "CHECKED_IN" ||
                src.DocumentActivity.Activity.ToUpperInvariant() == "CHECKED_OUT"))
            .ForMember(dest => dest.IsCreationActivity, opt => opt.MapFrom(src =>
                src.DocumentActivity.Activity.ToUpperInvariant() == "CREATED"));

        CreateMap<DocumentActivityUserDto, DocumentActivityUser>();

        CreateMap<Revision, RevisionDto>();
        CreateMap<RevisionDto, Revision>()
            .ForMember(dest => dest.Document, opt => opt.Ignore())
            .ForMember(dest => dest.RevisionActivityUsers, opt => opt.Ignore());

        CreateMap<RevisionActivityUser, RevisionActivityUserDto>();
        CreateMap<RevisionActivityUserDto, RevisionActivityUser>();

        CreateMap<User, UserDto>();
        CreateMap<UserDto, User>();

        CreateMap<Matter, MatterDto>();
        CreateMap<MatterDto, Matter>();

        CreateMap<MatterDocumentActivityUserFrom, MatterDocumentActivityUserFromDto>();
        CreateMap<MatterDocumentActivityUserFromDto, MatterDocumentActivityUserFrom>();

        CreateMap<MatterDocumentActivityUserTo, MatterDocumentActivityUserToDto>();
        CreateMap<MatterDocumentActivityUserToDto, MatterDocumentActivityUserTo>();
    }

    private static string FormatFileSize(long fileSize)
    {
        return fileSize switch
        {
            < 1024 => $"{fileSize} bytes",
            < 1024 * 1024 => $"{fileSize / 1024.0:F1} KB",
            < 1024 * 1024 * 1024 => $"{fileSize / (1024.0 * 1024.0):F1} MB",
            _ => $"{fileSize / (1024.0 * 1024.0 * 1024.0):F1} GB"
        };
    }
}