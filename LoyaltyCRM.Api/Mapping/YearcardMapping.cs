using AutoMapper;
using LoyaltyCRM.DTOs.Requests.Yearcard;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Domain.DomainPrimitives;

public class YearcardProfile : Profile
{
    public YearcardProfile()
    {

        //CREATE MAPPING
        CreateMap<YearcardCreateRequest, Yearcard>()
            .ConstructUsing(request =>
                new Yearcard(
                    id: null,
                    cardId: null
                )
            )
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src));

        CreateMap<YearcardCreateRequest, ApplicationUser>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName ?? src.Email));

        CreateMap<Yearcard, YearcardCreateResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId!.Value))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name != null ? src.Name.Value : null))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

        //GET MAPPING
        CreateMap<Yearcard, YearcardGetResponse>()
            // Simple properties
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId!.Value))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name != null ? src.Name.Value : null))

            // User-related fields
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))

            // Validity intervals
            .ForMember(dest => dest.ValidityIntervals, opt => opt.MapFrom(src => src.ValidityIntervals))

            // Derived fields
            .ForMember(dest => dest.ValidTo, opt => opt.MapFrom(src =>
                src.ValidityIntervals.Any()
                    ? src.ValidityIntervals.Max(v => v.EndDate.Value)
                    : DateTime.MinValue
            ))
            .ForMember(dest => dest.IsValidForDiscount, opt => opt.MapFrom(src => src.IsYearcardValidForDiscount()));

        //UPDATE MAPPING
        CreateMap<YearcardUpdateRequest, Yearcard>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => new CardNumber(src.CardId)))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new Name(src.Name!)))
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src))
            
            // Validity intervals are replaced entirely
            .ForMember(dest => dest.ValidityIntervals, opt => opt.MapFrom(src => src.ValidityIntervals))

            // Domain timestamps should not be overwritten by DTO
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    
        CreateMap<YearcardUpdateRequest, ApplicationUser>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName ?? src.Email));

        // Map ValidityInterval → ValidityIntervalResponseAndRequest
        CreateMap<ValidityInterval, ValidityIntervalResponseAndRequest>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.Value))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.Value));

        CreateMap<ValidityIntervalResponseAndRequest, ValidityInterval>()
            .ConstructUsing(src =>
                new ValidityInterval(
                    startDate: new StartDate(src.StartDate),
                    endDate: new EndDate(src.EndDate),
                    src.Id
                )
            );


    }
}
