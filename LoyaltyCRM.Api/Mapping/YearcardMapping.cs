using System;
using System.Linq;
using Mapster;
using LoyaltyCRM.DTOs.Dtos.FileImport;
using LoyaltyCRM.DTOs.Requests.Yearcard;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;

namespace LoyaltyCRM.Api.Mapping
{
    public class YearcardMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ImportRowDto, ApplicationUser>()
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.UserName, src => src.UserName ?? src.Email ?? src.PhoneNumber ?? src.Name)
                .Ignore(dest => dest.Yearcard);

            config.NewConfig<ImportRowDto, Yearcard>()
                .ConstructUsing(src => new Yearcard(
                    id: null,
                    cardId: string.IsNullOrWhiteSpace(src.CardId) ? null : new CardNumber(int.Parse(src.CardId!))
                ))
                .Ignore(dest => dest.CardId)
                .Map(dest => dest.Name, src => string.IsNullOrWhiteSpace(src.Name) ? null : new Name(src.Name!))
                .Ignore(dest => dest.ValidityIntervals)
                .Map(dest => dest.User, src => src.Adapt<ApplicationUser>())
                .Ignore(dest => dest.UserId);

            config.NewConfig<YearcardCreateRequest, Yearcard>()
                .ConstructUsing(_ => new Yearcard(id: null, cardId: null))
                .Map(dest => dest.User, src => src.Adapt<ApplicationUser>());

            config.NewConfig<YearcardCreateRequest, ApplicationUser>()
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.UserName, src => src.UserName ?? src.Email ?? src.PhoneNumber ?? src.Name);

            config.NewConfig<Yearcard, YearcardCreateResponse>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.CardId, src => src.CardId!.Value)
                .Map(dest => dest.Name, src => src.Name != null ? src.Name.Value : null)
                .Map(dest => dest.Email, src => src.User.Email)
                .Map(dest => dest.PhoneNumber, src => src.User.PhoneNumber)
                .Map(dest => dest.UserName, src => src.User.UserName);

            config.NewConfig<Yearcard, YearcardGetResponse>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.CardId, src => src.CardId!.Value)
                .Map(dest => dest.Name, src => src.Name != null ? src.Name.Value : null)
                .Map(dest => dest.PhoneNumber, src => src.User.PhoneNumber)
                .Map(dest => dest.UserName, src => src.User.UserName)
                .Map(dest => dest.Email, src => src.User.Email)
                .Map(dest => dest.ValidityIntervals, src => src.ValidityIntervals)
                .Map(dest => dest.ValidTo, src =>
                    src.ValidityIntervals.Any()
                        ? src.ValidityIntervals.Max(v => v.EndDate.Value)
                        : DateTime.MinValue
                );

            config.NewConfig<YearcardUpdateRequest, Yearcard>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.CardId, src => new CardNumber(src.CardId))
                .Map(dest => dest.Name, src => new Name(src.Name!))
                .Map(dest => dest.User, src => src.Adapt<ApplicationUser>())
                .Map(dest => dest.ValidityIntervals, src => src.ValidityIntervals)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt);

            config.NewConfig<YearcardUpdateRequest, ApplicationUser>()
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.UserName, src => src.UserName ?? src.Email ?? src.PhoneNumber ?? src.Name);

            config.NewConfig<ValidityInterval, ValidityIntervalResponseAndRequest>()
                .Map(dest => dest.StartDate, src => src.StartDate.Value)
                .Map(dest => dest.EndDate, src => src.EndDate.Value);

            config.NewConfig<ValidityIntervalResponseAndRequest, ValidityInterval>()
                .ConstructUsing(src => new ValidityInterval(
                    new StartDate(src.StartDate),
                    new EndDate(src.EndDate),
                    src.Id
                ));
        }
    }

    public static class MapsterConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig.GlobalSettings.Scan(typeof(YearcardMapping).Assembly);
        }
    }
}
