using AutoMapper;
using MSEmail.Application.DTOs;
using MSEmail.Domain.Entities;

namespace MSEmail.Application.Mappings;

/// <summary>
/// Perfil de mapeamento do AutoMapper
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // EmailTemplate mappings
        CreateMap<EmailTemplate, EmailTemplateDto>();
        CreateMap<CreateEmailTemplateDto, EmailTemplate>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateEmailTemplateDto, EmailTemplate>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // EmailLog mappings
        CreateMap<EmailLog, EmailLogDto>()
            .ForMember(dest => dest.RecipientName, opt => opt.MapFrom(src => src.Recipient.Name))
            .ForMember(dest => dest.RecipientEmail, opt => opt.MapFrom(src => src.Recipient.Email))
            .ForMember(dest => dest.EmailTemplateName, opt => opt.MapFrom(src => src.EmailTemplate.Name));
    }
}
