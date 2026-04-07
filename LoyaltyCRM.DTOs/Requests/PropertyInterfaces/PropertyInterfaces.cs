using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PapasCRM_API.Requests.PropertyInterfaces
{
    public interface IHasId
    {
        Guid? Id { get; set; }
    }

    public interface IHasCardId
    {
        int CardId { get; set; }
    }

    public interface IHasPhone
    {
        string Phone { get; set; }
    }

    public interface IHasName
    {
        string? Name { get; set; }
    }

    public interface IHasFirstName
    {
        string? FirstName { get; set; }
    }

    public interface IHasLastName
    {
        string? LastName { get; set; }
    }

    public interface IHasEmail
    {
        string? Email { get; set; }
    }
    public interface IHasPhoneNumber
    {
        string? PhoneNumber { get; set; }
    }
    public interface IHasUserName
    {
        string? UserName { get; set; }
    }
    public interface IHasValidTo
    {
        DateTime ValidTo { get; set; }
    }
    public interface IHasIsValidForDiscount
    {
        bool IsValidForDiscount { get; set; }
    }

    public interface IHasIsConfirmed
    {
        bool IsConfirmed { get; set; }
    }

    public interface IHasStartDate
    {
        public DateTime StartDate { get; set; }
    }

    public interface IHasExposedIdentification
    {
        string ExposedIdentification { get; set; }
    }
}