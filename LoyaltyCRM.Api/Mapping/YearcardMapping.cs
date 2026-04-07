using AutoMapper;
using LoyaltyCRM.DTOs.Requests.Yearcard;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Domain.DomainPrimitives;

public class YearcardProfile : Profile
{
    public YearcardProfile()
    {
        CreateMap<YearcardCreateRequest, Yearcard>()
            .ConstructUsing(request =>
                new Yearcard(
                    id: null,
                    cardId: null
                )
            )
            .ForMember(Yearcard => Yearcard.UserId, opt => opt.Ignore())
            .ForMember(Yearcard => Yearcard.User, opt => opt.Ignore())
            .ForMember(Yearcard => Yearcard.ValidityIntervals, opt => opt.Ignore())
            .ForMember(Yearcard => Yearcard.Name, opt => opt.MapFrom(request => new Name(request.Name!)));

        CreateMap<Yearcard, YearcardGetResponse>()
            // Simple properties
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId!.GetValue()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name != null ? src.Name.GetValue() : null))

            // User-related fields
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))

            // Validity intervals
            .ForMember(dest => dest.ValidityIntervals, opt => opt.MapFrom(src => src.ValidityIntervals))

            // Derived fields
            .ForMember(dest => dest.ValidTo, opt => opt.MapFrom(src =>
                src.ValidityIntervals.Any()
                    ? src.ValidityIntervals.Max(v => v.EndDate.GetValue())
                    : DateTime.MinValue
            ))
            .ForMember(dest => dest.IsValidForDiscount, opt => opt.MapFrom(src => src.IsYearcardValidForDiscount()));

        CreateMap<YearcardUpdateRequest, Yearcard>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => new CardNumber(src.CardId)))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new Name(src.Name!)))
            
            // User fields are updated through ApplicationUser, not Yearcard
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())

            // Validity intervals are replaced entirely
            .ForMember(dest => dest.ValidityIntervals, opt => opt.MapFrom(src => src.ValidityIntervals))

            // Domain timestamps should not be overwritten by DTO
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        
        // Map ValidityInterval → ValidityIntervalResponseAndRequest
        CreateMap<ValidityInterval, ValidityIntervalResponseAndRequest>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.GetValue()))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.GetValue()));

        CreateMap<ValidityIntervalResponseAndRequest, ValidityInterval>()
            .ConstructUsing(src =>
                new ValidityInterval(
                    startDate: new StartDate(src.StartDate),
                    endDate: new EndDate(src.EndDate),
                    src.Id
                )
            );

        CreateMap<Yearcard, YearcardCreateResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId!.GetValue()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name != null ? src.Name.GetValue() : null))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

    }
}
